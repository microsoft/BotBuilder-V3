var request = require('request');
var async = require('async');
var url = require('url');
var utils = require('../utils');
var ChatConnector = (function () {
    function ChatConnector(settings) {
        this.settings = settings;
        if (!this.settings.endpoint) {
            this.settings.endpoint = {
                refreshEndpoint: 'https://login.microsoftonline.com/common/oauth2/v2.0/token',
                refreshScope: 'https://graph.microsoft.com/.default',
                verifyEndpoint: 'https://api.botframework.com/api/.well-known/OpenIdConfiguration',
                verifyIssuer: 'https://api.botframework.com'
            };
        }
    }
    ChatConnector.prototype.listen = function () {
        var _this = this;
        return function (req, res) {
            if (req.body) {
                _this.dispatch(req.body, res);
            }
            else {
                var requestData = '';
                req.on('data', function (chunk) {
                    requestData += chunk;
                });
                req.on('end', function () {
                    var body = JSON.parse(requestData);
                    _this.dispatch(body, res);
                });
            }
        };
    };
    ChatConnector.prototype.verifyBotFramework = function () {
        return function (req, res, next) {
            next();
        };
    };
    ChatConnector.prototype.onMessage = function (handler) {
        this.handler = handler;
    };
    ChatConnector.prototype.send = function (messages, done) {
        var _this = this;
        var conversationId;
        async.eachSeries(messages, function (msg, cb) {
            try {
                var address = msg.address;
                if (address && address.serviceUrl) {
                    delete msg.address;
                    if (!address.conversation && conversationId) {
                        address.conversation = { id: conversationId };
                    }
                    _this.postMessage(address, msg, cb);
                }
                else {
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
                done(err, adr);
            });
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
                moveFields(msg, address, toAddress);
                msg.address = address;
                if (address.serviceUrl) {
                    try {
                        var u = url.parse(address.serviceUrl);
                        address.serviceUrl = u.protocol + '//' + u.host;
                    }
                    catch (e) {
                        console.error("ChatConnector error parsing '" + address.serviceUrl + "': " + e.toString());
                    }
                }
                _this.handler([msg]);
            }
            catch (e) {
                console.error(e.toString());
            }
        });
        res.status(202);
        res.end();
    };
    ChatConnector.prototype.postMessage = function (address, msg, cb) {
        var path = '/v3/conversations/' + encodeURIComponent(address.conversation.id) + '/activities';
        if (address.id && address.channelId !== 'skype') {
            path += '/' + encodeURIComponent(address.id);
        }
        msg['from'] = address.bot;
        msg['recipient'] = address.user;
        var options = {
            method: 'POST',
            url: url.resolve(address.serviceUrl, path),
            body: msg,
            json: true
        };
        this.authenticatedRequest(options, function (err, response, body) {
            cb(err);
        });
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
    ChatConnector.prototype.addAccessToken = function (options, cb) {
        var _this = this;
        if (this.settings.appId && this.settings.appPassword) {
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
                            options.headers = {
                                'Authorization': 'Bearer ' + _this.accessToken
                            };
                            cb(null);
                        }
                        else {
                            cb(new Error('Refresh access token failed with status code: ' + response.statusCode));
                        }
                    }
                    else {
                        cb(err);
                    }
                });
            }
            else {
                options.headers = {
                    'Authorization': 'Bearer ' + this.accessToken
                };
                cb(null);
            }
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
                path = 'https://api.botframework.com';
                break;
        }
        return path + '/v3/botstate/' +
            encodeURIComponent(this.settings.botId) + '/' +
            encodeURIComponent(address.channelId);
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
    'serviceUrl': 'serviceUrl'
};
function moveFields(frm, to, map) {
    if (frm && to) {
        for (var key in map) {
            if (frm.hasOwnProperty(key)) {
                to[map[key]] = frm[key];
                delete frm[key];
            }
        }
    }
}
