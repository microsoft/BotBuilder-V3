var request = require('request');
var async = require('async');
var url = require('url');
var utils = require('../utils');
var logger = require('../logger');
var jwt = require('jsonwebtoken');
var getPem = require('rsa-pem-from-mod-exp');
var base64url = require('base64url');
var keysLastFetched = 0;
var cachedKeys;
var issuer;
var ChatConnector = (function () {
    function ChatConnector(settings) {
        if (settings === void 0) { settings = {}; }
        this.settings = settings;
        if (!this.settings.endpoint) {
            this.settings.endpoint = {
                refreshEndpoint: 'https://login.microsoftonline.com/common/oauth2/v2.0/token',
                refreshScope: 'https://graph.microsoft.com/.default',
                verifyEndpoint: 'https://api.aps.skype.com/v1/.well-known/openidconfiguration',
                verifyIssuer: 'https://api.botframework.com',
                stateEndpoint: this.settings.stateEndpoint || 'https://state.botframework.com'
            };
        }
    }
    ChatConnector.prototype.listen = function () {
        var _this = this;
        return function (req, res) {
            if (req.body) {
                _this.verifyBotFramework(req, res);
            }
            else {
                var requestData = '';
                req.on('data', function (chunk) {
                    requestData += chunk;
                });
                req.on('end', function () {
                    req.body = JSON.parse(requestData);
                    _this.verifyBotFramework(req, res);
                });
            }
        };
    };
    ChatConnector.prototype.ensureCachedKeys = function (cb) {
        var now = new Date().getTime();
        if (keysLastFetched < (now - 1000 * 60 * 60 * 24)) {
            var options = {
                method: 'GET',
                url: this.settings.endpoint.verifyEndpoint,
                json: true
            };
            request(options, function (err, response, body) {
                if (!err && (response.statusCode >= 400 || !body)) {
                    err = new Error("Failed to load openID config: " + response.statusCode);
                }
                if (err) {
                    cb(err, null);
                }
                else {
                    var openIdConfig = body;
                    issuer = openIdConfig.issuer;
                    var options = {
                        method: 'GET',
                        url: openIdConfig.jwks_uri,
                        json: true
                    };
                    request(options, function (err, response, body) {
                        if (!err && (response.statusCode >= 400 || !body)) {
                            err = new Error("Failed to load Keys: " + response.statusCode);
                        }
                        if (!err) {
                            keysLastFetched = now;
                        }
                        cachedKeys = body.keys;
                        cb(err, cachedKeys);
                    });
                }
            });
        }
        else {
            cb(null, cachedKeys);
        }
    };
    ChatConnector.prototype.getSecretForKey = function (keyId) {
        for (var i = 0; i < cachedKeys.length; i++) {
            if (cachedKeys[i].kid == keyId) {
                var jwt = cachedKeys[i];
                var modulus = base64url.toBase64(jwt.n);
                var exponent = jwt.e;
                return getPem(modulus, exponent);
            }
        }
        return null;
    };
    ChatConnector.prototype.verifyEmulatorToken = function (decodedPayload) {
        var now = new Date().getTime() / 1000;
        return decodedPayload.appid == this.settings.appId &&
            decodedPayload.iss == "https://sts.windows.net/72f988bf-86f1-41af-91ab-2d7cd011db47/" &&
            now < decodedPayload.exp && now > decodedPayload.nbf;
    };
    ChatConnector.prototype.verifyBotFramework = function (req, res) {
        var _this = this;
        var token;
        var isEmulator = req.body['channelId'] === 'emulator';
        if (req.headers && req.headers.hasOwnProperty('authorization')) {
            var auth = req.headers['authorization'].trim().split(' ');
            ;
            if (auth.length == 2 && auth[0].toLowerCase() == 'bearer') {
                token = auth[1];
            }
        }
        if (token) {
            req.body['useAuth'] = true;
            this.ensureCachedKeys(function (err, keys) {
                if (!err) {
                    var decoded = jwt.decode(token, { complete: true });
                    var now = new Date().getTime() / 1000;
                    if (decoded.payload.aud != _this.settings.appId || decoded.payload.iss != issuer ||
                        now > decoded.payload.exp || now < decoded.payload.nbf) {
                        if (_this.verifyEmulatorToken(decoded.payload)) {
                            _this.dispatch(req.body, res);
                        }
                        else {
                            logger.error('ChatConnector: receive - invalid token. Check bots app ID & Password.');
                            res.status(403);
                            res.end();
                        }
                    }
                    else {
                        var keyId = decoded.header.kid;
                        var secret = _this.getSecretForKey(keyId);
                        try {
                            decoded = jwt.verify(token, secret);
                            _this.dispatch(req.body, res);
                        }
                        catch (err) {
                            logger.error('ChatConnector: receive - invalid token. Check bots app ID & Password.');
                            res.status(403);
                            res.end();
                        }
                    }
                }
                else {
                    logger.error('ChatConnector: receive - error loading openId config: %s', err.toString());
                    res.status(500);
                    res.end();
                }
            });
        }
        else if (isEmulator && !this.settings.appId && !this.settings.appPassword) {
            logger.warn(req.body, 'ChatConnector: receive - emulator running without security enabled.');
            req.body['useAuth'] = false;
            this.dispatch(req.body, res);
        }
        else {
            logger.error('ChatConnector: receive - no security token sent. Ensure emulator configured with bots app ID & Password.');
            res.status(401);
            res.end();
        }
    };
    ChatConnector.prototype.onEvent = function (handler) {
        this.handler = handler;
    };
    ChatConnector.prototype.send = function (messages, done) {
        var _this = this;
        async.eachSeries(messages, function (msg, cb) {
            try {
                if (msg.address && msg.address.serviceUrl) {
                    _this.postMessage(msg, cb);
                }
                else {
                    logger.error('ChatConnector: send - message is missing address or serviceUrl.');
                    cb(new Error('Message missing address or serviceUrl.'));
                }
            }
            catch (e) {
                cb(e);
            }
        }, done);
    };
    ChatConnector.prototype.startConversation = function (address, done) {
        if (address && address.user && address.bot && address.serviceUrl) {
            var options = {
                method: 'POST',
                url: url.resolve(address.serviceUrl, '/v3/conversations'),
                body: {
                    bot: address.bot,
                    members: [address.user]
                },
                json: true
            };
            this.authenticatedRequest(options, function (err, response, body) {
                var adr;
                if (!err) {
                    try {
                        var obj = typeof body === 'string' ? JSON.parse(body) : body;
                        if (obj && obj.hasOwnProperty('id')) {
                            adr = utils.clone(address);
                            adr.conversation = { id: obj['id'] };
                            if (adr.id) {
                                delete adr.id;
                            }
                        }
                        else {
                            err = new Error('Failed to start conversation: no conversation ID returned.');
                        }
                    }
                    catch (e) {
                        err = e instanceof Error ? e : new Error(e.toString());
                    }
                }
                if (err) {
                    logger.error('ChatConnector: startConversation - error starting conversation.');
                }
                done(err, adr);
            });
        }
        else {
            logger.error('ChatConnector: startConversation - address is invalid.');
            done(new Error('Invalid address.'));
        }
    };
    ChatConnector.prototype.getData = function (context, callback) {
        var _this = this;
        try {
            var root = this.getStoragePath(context.address);
            var list = [];
            if (context.userId) {
                if (context.persistUserData) {
                    list.push({
                        field: 'userData',
                        url: root + '/users/' + encodeURIComponent(context.userId)
                    });
                }
                if (context.conversationId) {
                    list.push({
                        field: 'privateConversationData',
                        url: root + '/conversations/' + encodeURIComponent(context.conversationId) +
                            '/users/' + encodeURIComponent(context.userId)
                    });
                }
            }
            if (context.persistConversationData && context.conversationId) {
                list.push({
                    field: 'conversationData',
                    url: root + '/conversations/' + encodeURIComponent(context.conversationId)
                });
            }
            var data = {};
            async.each(list, function (entry, cb) {
                var options = {
                    method: 'GET',
                    url: entry.url,
                    json: true
                };
                _this.authenticatedRequest(options, function (err, response, body) {
                    if (!err && body) {
                        try {
                            var botData = body.data ? body.data : {};
                            data[entry.field + 'Hash'] = JSON.stringify(botData);
                            data[entry.field] = botData;
                        }
                        catch (e) {
                            err = e;
                        }
                    }
                    cb(err);
                });
            }, function (err) {
                if (!err) {
                    callback(null, data);
                }
                else {
                    callback(err instanceof Error ? err : new Error(err.toString()), null);
                }
            });
        }
        catch (e) {
            callback(e instanceof Error ? e : new Error(e.toString()), null);
        }
    };
    ChatConnector.prototype.saveData = function (context, data, callback) {
        var _this = this;
        var list = [];
        function addWrite(field, botData, url) {
            var hashKey = field + 'Hash';
            var hash = JSON.stringify(botData);
            if (!data[hashKey] || hash !== data[hashKey]) {
                data[hashKey] = hash;
                list.push({ botData: botData, url: url });
            }
        }
        try {
            var root = this.getStoragePath(context.address);
            if (context.userId) {
                if (context.persistUserData) {
                    addWrite('userData', data.userData || {}, root + '/users/' + encodeURIComponent(context.userId));
                }
                if (context.conversationId) {
                    var url = root + '/conversations/' + encodeURIComponent(context.conversationId) +
                        '/users/' + encodeURIComponent(context.userId);
                    addWrite('privateConversationData', data.privateConversationData || {}, url);
                }
            }
            if (context.persistConversationData && context.conversationId) {
                addWrite('conversationData', data.conversationData || {}, root + '/conversations/' + encodeURIComponent(context.conversationId));
            }
            async.each(list, function (entry, cb) {
                var options = {
                    method: 'POST',
                    url: entry.url,
                    body: { eTag: '*', data: entry.botData },
                    json: true
                };
                _this.authenticatedRequest(options, function (err, response, body) {
                    cb(err);
                });
            }, function (err) {
                if (callback) {
                    if (!err) {
                        callback(null);
                    }
                    else {
                        callback(err instanceof Error ? err : new Error(err.toString()));
                    }
                }
            });
        }
        catch (e) {
            if (callback) {
                callback(e instanceof Error ? e : new Error(e.toString()));
            }
        }
    };
    ChatConnector.prototype.dispatch = function (messages, res) {
        var _this = this;
        var list = Array.isArray(messages) ? messages : [messages];
        list.forEach(function (msg) {
            try {
                var address = {};
                utils.moveFieldsTo(msg, address, toAddress);
                msg.address = address;
                msg.source = address.channelId;
                logger.info(address, 'ChatConnector: message received.');
                if (address.serviceUrl) {
                    try {
                        var u = url.parse(address.serviceUrl);
                        address.serviceUrl = u.protocol + '//' + u.host;
                    }
                    catch (e) {
                        console.error("ChatConnector error parsing '" + address.serviceUrl + "': " + e.toString());
                    }
                }
                utils.moveFieldsTo(msg, msg, {
                    'locale': 'textLocale',
                    'channelData': 'sourceEvent'
                });
                msg.text = msg.text || '';
                msg.attachments = msg.attachments || [];
                msg.entities = msg.entities || [];
                _this.handler([msg]);
            }
            catch (e) {
                console.error(e.toString());
            }
        });
        res.status(202);
        res.end();
    };
    ChatConnector.prototype.postMessage = function (msg, cb) {
        var address = msg.address;
        msg['from'] = address.bot;
        msg['recipient'] = address.user;
        delete msg.address;
        utils.moveFieldsTo(msg, msg, {
            'textLocale': 'locale',
            'sourceEvent': 'channelData'
        });
        delete msg.agent;
        delete msg.source;
        var path = '/v3/conversations/' + encodeURIComponent(address.conversation.id) + '/activities';
        if (address.id && address.channelId !== 'skype') {
            path += '/' + encodeURIComponent(address.id);
        }
        logger.info(address, 'ChatConnector: sending message.');
        var options = {
            method: 'POST',
            url: url.resolve(address.serviceUrl, path),
            body: msg,
            json: true
        };
        if (address.useAuth) {
            this.authenticatedRequest(options, function (err, response, body) { return cb(err); });
        }
        else {
            request(options, function (err, response, body) {
                if (!err && response.statusCode >= 400) {
                    var txt = "Request to '" + options.url + "' failed: [" + response.statusCode + "] " + response.statusMessage;
                    err = new Error(txt);
                }
                cb(err);
            });
        }
    };
    ChatConnector.prototype.authenticatedRequest = function (options, callback, refresh) {
        var _this = this;
        if (refresh === void 0) { refresh = false; }
        if (refresh) {
            this.accessToken = null;
        }
        this.addAccessToken(options, function (err) {
            if (!err) {
                request(options, function (err, response, body) {
                    if (!err) {
                        switch (response.statusCode) {
                            case 401:
                            case 403:
                                if (!refresh) {
                                    _this.authenticatedRequest(options, callback, true);
                                }
                                else {
                                    callback(null, response, body);
                                }
                                break;
                            default:
                                if (response.statusCode < 400) {
                                    callback(null, response, body);
                                }
                                else {
                                    var txt = "Request to '" + options.url + "' failed: [" + response.statusCode + "] " + response.statusMessage;
                                    callback(new Error(txt), response, null);
                                }
                                break;
                        }
                    }
                    else {
                        callback(err, null, null);
                    }
                });
            }
            else {
                callback(err, null, null);
            }
        });
    };
    ChatConnector.prototype.getAccessToken = function (cb) {
        var _this = this;
        if (!this.accessToken || new Date().getTime() >= this.accessTokenExpires) {
            var opt = {
                method: 'POST',
                url: this.settings.endpoint.refreshEndpoint,
                form: {
                    grant_type: 'client_credentials',
                    client_id: this.settings.appId,
                    client_secret: this.settings.appPassword,
                    scope: this.settings.endpoint.refreshScope
                }
            };
            request(opt, function (err, response, body) {
                if (!err) {
                    if (body && response.statusCode < 300) {
                        var oauthResponse = JSON.parse(body);
                        _this.accessToken = oauthResponse.access_token;
                        _this.accessTokenExpires = new Date().getTime() + ((oauthResponse.expires_in - 300) * 1000);
                        cb(null, _this.accessToken);
                    }
                    else {
                        cb(new Error('Refresh access token failed with status code: ' + response.statusCode), null);
                    }
                }
                else {
                    cb(err, null);
                }
            });
        }
        else {
            cb(null, this.accessToken);
        }
    };
    ChatConnector.prototype.addAccessToken = function (options, cb) {
        if (this.settings.appId && this.settings.appPassword) {
            this.getAccessToken(function (err, token) {
                if (!err && token) {
                    options.headers = {
                        'Authorization': 'Bearer ' + token
                    };
                    cb(null);
                }
                else {
                    cb(err);
                }
            });
        }
        else {
            cb(null);
        }
    };
    ChatConnector.prototype.getStoragePath = function (address) {
        var path;
        switch (address.channelId) {
            case 'emulator':
                if (address.serviceUrl) {
                    path = address.serviceUrl;
                }
                else {
                    throw new Error('ChatConnector.getStoragePath() missing address.serviceUrl.');
                }
                break;
            default:
                path = this.settings.endpoint.stateEndpoint;
                break;
        }
        return path + '/v3/botstate/' + encodeURIComponent(address.channelId);
    };
    return ChatConnector;
})();
exports.ChatConnector = ChatConnector;
var toAddress = {
    'id': 'id',
    'channelId': 'channelId',
    'from': 'user',
    'conversation': 'conversation',
    'recipient': 'bot',
    'serviceUrl': 'serviceUrl',
    'useAuth': 'useAuth'
};
