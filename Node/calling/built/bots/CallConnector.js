var request = require('request');
var async = require('async');
var url = require('url');
var utils = require('../utils');
var Busboy = require('busboy');
var CallConnector = (function () {
    function CallConnector(settings) {
        this.settings = settings;
        this.responses = {};
        if (!this.settings.endpoint) {
            this.settings.endpoint = {
                refreshEndpoint: 'https://login.microsoftonline.com/common/oauth2/v2.0/token',
                refreshScope: 'https://graph.microsoft.com/.default',
                verifyEndpoint: 'https://api.botframework.com/api/.well-known/OpenIdConfiguration',
                verifyIssuer: 'https://api.botframework.com',
                stateEndpoint: this.settings.stateUri || 'https://state.botframework.com'
            };
        }
    }
    CallConnector.prototype.listen = function () {
        var _this = this;
        return function (req, res) {
            var callback = _this.responseCallback(req, res);
            if (req.is('application/json')) {
                _this.parseBody(req, function (err, body) {
                    if (!err) {
                        _this.dispatch(body, callback);
                    }
                    else {
                        callback(err);
                    }
                });
            }
            else if (req.is('multipart/form-data')) {
                _this.parseFormData(req, function (err, body) {
                    if (!err) {
                        _this.dispatch(body, callback);
                    }
                    else {
                        callback(err);
                    }
                });
            }
            else {
                callback(new Error('Invalid content type.'));
            }
        };
    };
    CallConnector.prototype.verifyBotFramework = function () {
        return function (req, res, next) {
            next();
        };
    };
    CallConnector.prototype.onMessage = function (handler) {
        this.handler = handler;
    };
    CallConnector.prototype.send = function (message, cb) {
        if (message.type == 'workflow' && message.address) {
            if (this.responses.hasOwnProperty(message.address.id)) {
                var callback = this.responses[message.address.id];
                delete this.responses[message.address.id];
                var response = utils.clone(message);
                response.links = { 'callback': this.settings.callbackUri };
                response.appState = JSON.stringify(response.address);
                delete response.type;
                delete response.address;
                callback(null, response);
            }
        }
        else {
            cb(new Error('Invalid message sent to CallConnector.send().'));
        }
    };
    CallConnector.prototype.getData = function (context, callback) {
        var _this = this;
        try {
            var root = this.getStoragePath();
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
    CallConnector.prototype.saveData = function (context, data, callback) {
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
            var root = this.getStoragePath();
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
    CallConnector.prototype.dispatch = function (body, response) {
        var _this = this;
        var msg;
        this.responses[body.id] = response;
        if (body.hasOwnProperty('participants')) {
            msg = body;
            msg.type = 'conversation';
            msg.address = {};
            moveFields(body, msg.address, toAddress);
        }
        else {
            msg = body;
            msg.type = 'conversationResult';
            msg.address = JSON.parse(body.appState);
            if (msg.id !== msg.address.id) {
                console.warn("CallConnector received a 'conversationResult' with an invalid conversation id.");
            }
            delete msg.id;
            delete msg.appState;
        }
        this.handler(body, function (err) {
            if (err && _this.responses.hasOwnProperty(body.id)) {
                delete _this.responses[body.id];
                response(err);
            }
        });
    };
    CallConnector.prototype.authenticatedRequest = function (options, callback, refresh) {
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
    CallConnector.prototype.getAccessToken = function (cb) {
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
    CallConnector.prototype.addAccessToken = function (options, cb) {
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
    CallConnector.prototype.getStoragePath = function () {
        return url.resolve(this.settings.endpoint.stateEndpoint, '/v3/botstate/skype/');
    };
    CallConnector.prototype.parseBody = function (req, cb) {
        if (typeof req.body === 'undefined') {
            var data = '';
            req.on('data', function (chunk) { return data += chunk; });
            req.on('end', function () {
                var err;
                var body;
                try {
                    body = JSON.parse(data);
                }
                catch (e) {
                    err = e;
                }
                cb(err, body);
            });
        }
        else {
            cb(null, req.body);
        }
    };
    CallConnector.prototype.parseFormData = function (req, cb) {
        var busboy = new Busboy({ headers: req.headers, defCharset: 'binary' });
        var result;
        var recordedAudio;
        busboy.on('field', function (fieldname, val, fieldnameTruncated, valTruncated, encoding, mimetype) {
            if (fieldname === 'recordedAudio') {
                recordedAudio = new Buffer(val, 'binary');
            }
            else if (fieldname === 'conversationResult') {
                result = JSON.parse(val);
            }
        });
        busboy.on('finish', function () {
            if (result && recordedAudio) {
                result.recordedAudio = recordedAudio;
            }
            cb(null, result);
        });
        req.pipe(busboy);
    };
    CallConnector.prototype.responseCallback = function (req, res) {
        return function (err, response) {
            if (err) {
                res.status(500);
                res.end();
            }
            else {
                res.status(200);
                res.send(response);
            }
        };
    };
    return CallConnector;
})();
exports.CallConnector = CallConnector;
var toAddress = {
    'id': 'id',
    'participants': 'participants',
    'isMultiParty': 'isMultiParty',
    'threadId': 'threadId',
    'subject': 'subject'
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
