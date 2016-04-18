var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var collection = require('../dialogs/DialogCollection');
var session = require('../Session');
var consts = require('../consts');
var request = require('request');
var uuid = require('node-uuid');
var BotConnectorBot = (function (_super) {
    __extends(BotConnectorBot, _super);
    function BotConnectorBot(options) {
        _super.call(this);
        this.options = {
            endpoint: process.env['endpoint'] || 'https://api.botframework.com',
            appId: process.env['appId'] || '',
            appSecret: process.env['appSecret'] || '',
            defaultDialogId: '/',
            minSendDelay: 1000
        };
        this.configure(options);
    }
    BotConnectorBot.prototype.configure = function (options) {
        if (options) {
            for (var key in options) {
                if (options.hasOwnProperty(key)) {
                    this.options[key] = options[key];
                }
            }
        }
    };
    BotConnectorBot.prototype.verifyBotFramework = function (options) {
        var _this = this;
        this.configure(options);
        return function (req, res, next) {
            var authorized;
            var isSecure = req.headers['x-forwarded-proto'] === 'https' || req.headers['x-arr-ssl'];
            if (isSecure && _this.options.appId && _this.options.appSecret) {
                if (req.headers.hasOwnProperty('authorization')) {
                    var tmp = req.headers['authorization'].split(' ');
                    var buf = new Buffer(tmp[1], 'base64');
                    var cred = buf.toString().split(':');
                    if (cred[0] == _this.options.appId && cred[1] == _this.options.appSecret) {
                        authorized = true;
                    }
                    else {
                        authorized = false;
                    }
                }
                else {
                    authorized = false;
                }
            }
            else {
                authorized = true;
            }
            if (authorized) {
                next();
            }
            else {
                res.send(403);
            }
        };
    };
    BotConnectorBot.prototype.listen = function (dialogId, dialogArgs) {
        var _this = this;
        return function (req, res) {
            if (req.body) {
                _this.dispatchMessage(null, req.body, { dialogId: dialogId, dialogArgs: dialogArgs }, res);
            }
            else {
                var requestData = '';
                req.on('data', function (chunk) {
                    requestData += chunk;
                });
                req.on('end', function () {
                    try {
                        var msg = JSON.parse(requestData);
                        _this.dispatchMessage(null, msg, { dialogId: dialogId, dialogArgs: dialogArgs }, res);
                    }
                    catch (e) {
                        _this.emit('error', new Error('Invalid Bot Framework Message'));
                        res.send(400);
                    }
                });
            }
        };
    };
    BotConnectorBot.prototype.beginDialog = function (address, dialogId, dialogArgs) {
        var message = address;
        message.type = 'Message';
        if (!message.from) {
            message.from = this.options.defaultFrom;
        }
        if (!message.to || !message.from) {
            throw new Error('Invalid address passed to BotConnectorBot.beginDialog().');
        }
        if (!this.hasDialog(dialogId)) {
            throw new Error('Invalid dialog passed to BotConnectorBot.beginDialog().');
        }
        this.dispatchMessage(message.to.id, message, { dialogId: dialogId, dialogArgs: dialogArgs });
    };
    BotConnectorBot.prototype.dispatchMessage = function (userId, message, options, res) {
        var _this = this;
        try {
            if (!message || !message.type) {
                this.emit('error', new Error('Invalid Bot Framework Message'));
                return res ? res.send(400) : null;
            }
            if (!userId) {
                if (message.from && message.from.id) {
                    userId = message.from.id;
                }
                else {
                    this.emit('error', new Error('Invalid Bot Framework Message'));
                    return res ? res.send(400) : null;
                }
            }
            var sessionId;
            if (message.botConversationData && message.botConversationData[consts.Data.SessionId]) {
                sessionId = message.botConversationData[consts.Data.SessionId];
            }
            else {
                sessionId = uuid.v1();
                message.botConversationData = message.botConversationData || {};
                message.botConversationData[consts.Data.SessionId] = sessionId;
            }
            this.emit(message.type, message);
            if (message.type == 'Message') {
                var ses = new BotConnectorSession({
                    localizer: this.options.localizer,
                    minSendDelay: this.options.minSendDelay,
                    dialogs: this,
                    dialogId: options.dialogId || this.options.defaultDialogId,
                    dialogArgs: options.dialogArgs || this.options.defaultDialogArgs
                });
                ses.on('send', function (reply) {
                    reply = reply || {};
                    reply.botConversationData = message.botConversationData;
                    if (reply.text && !reply.language && message.language) {
                        reply.language = message.language;
                    }
                    var data = {
                        userData: ses.userData,
                        conversationData: ses.conversationData,
                        perUserConversationData: ses.perUserInConversationData
                    };
                    data.perUserConversationData[consts.Data.SessionState] = ses.sessionState;
                    _this.saveData(userId, sessionId, data, reply, function (err) {
                        if (res) {
                            _this.emit('reply', reply);
                            res.send(200, reply);
                            res = null;
                        }
                        else if (ses.message.conversationId) {
                            reply.from = ses.message.to;
                            reply.to = ses.message.replyTo ? ses.message.replyTo : ses.message.from;
                            reply.replyToMessageId = ses.message.id;
                            reply.conversationId = ses.message.conversationId;
                            reply.channelConversationId = ses.message.channelConversationId;
                            reply.channelMessageId = ses.message.channelMessageId;
                            reply.participants = ses.message.participants;
                            reply.totalParticipants = ses.message.totalParticipants;
                            _this.emit('reply', reply);
                            post(_this.options, '/bot/v1.0/messages', reply, function (err) {
                                if (err) {
                                    _this.emit('error', err);
                                }
                            });
                        }
                        else {
                            reply.from = ses.message.from;
                            reply.to = ses.message.to;
                            _this.emit('send', reply);
                            post(_this.options, '/bot/v1.0/messages', reply, function (err) {
                                if (err) {
                                    _this.emit('error', err);
                                }
                            });
                        }
                    });
                });
                ses.on('error', function (err) {
                    _this.emit('error', err, ses.message);
                    if (res) {
                        res.send(500);
                    }
                });
                ses.on('quit', function () {
                    _this.emit('quit', ses.message);
                });
                this.getData(userId, sessionId, message, function (err, data) {
                    if (!err) {
                        var sessionState;
                        ses.userData = data.userData || {};
                        ses.conversationData = data.conversationData || {};
                        ses.perUserInConversationData = data.perUserConversationData || {};
                        if (ses.perUserInConversationData.hasOwnProperty(consts.Data.SessionState)) {
                            sessionState = ses.perUserInConversationData[consts.Data.SessionState];
                            delete ses.perUserInConversationData[consts.Data.SessionState];
                        }
                        if (options.replyToDialogId) {
                            if (sessionState && sessionState.callstack[sessionState.callstack.length - 1].id == options.replyToDialogId) {
                                ses.dispatch(sessionState, message);
                            }
                        }
                        else {
                            ses.dispatch(sessionState, message);
                        }
                    }
                    else {
                        _this.emit('error', err, message);
                    }
                });
            }
            else if (res) {
                var msg;
                switch (message.type) {
                    case "botAddedToConversation":
                        msg = this.options.groupWelcomeMessage;
                        break;
                    case "userAddedToConversation":
                        msg = this.options.userWelcomeMessage;
                        break;
                    case "endOfConversation":
                        msg = this.options.goodbyeMessage;
                        break;
                }
                res.send(msg ? { type: message.type, text: msg } : {});
            }
        }
        catch (e) {
            this.emit('error', e instanceof Error ? e : new Error(e.toString()));
            res.send(500);
        }
    };
    BotConnectorBot.prototype.getData = function (userId, sessionId, msg, callback) {
        var botPath = '/' + this.options.appId;
        var userPath = botPath + '/users/' + userId;
        var convoPath = botPath + '/conversations/' + sessionId;
        var perUserConvoPath = botPath + '/conversations/' + sessionId + '/users/' + userId;
        var ops = 3;
        var data = {};
        function load(id, field, store, botData) {
            data[field] = botData;
            if (store) {
                store.get(id, function (err, item) {
                    if (callback) {
                        if (!err) {
                            data[field] = item;
                            if (--ops == 0) {
                                callback(null, data);
                            }
                        }
                        else {
                            callback(err, null);
                            callback = null;
                        }
                    }
                });
            }
            else if (callback && --ops == 0) {
                callback(null, data);
            }
        }
        load(userPath, 'userData', this.options.userStore, msg.botUserData);
        load(convoPath, 'conversationData', this.options.conversationStore, msg.botConversationData);
        load(perUserConvoPath, 'perUserConversationData', this.options.perUserInConversationStore, msg.botPerUserInConversationData);
    };
    BotConnectorBot.prototype.saveData = function (userId, sessionId, data, msg, callback) {
        var botPath = '/' + this.options.appId;
        var userPath = botPath + '/users/' + userId;
        var convoPath = botPath + '/conversations/' + sessionId;
        var perUserConvoPath = botPath + '/conversations/' + sessionId + '/users/' + userId;
        var ops = 3;
        function save(id, field, store, botData) {
            if (store) {
                store.save(id, botData, function (err) {
                    if (callback) {
                        if (!err && --ops == 0) {
                            callback(null);
                        }
                        else {
                            callback(err);
                            callback = null;
                        }
                    }
                });
            }
            else {
                msg[field] = botData;
                if (callback && --ops == 0) {
                    callback(null);
                }
            }
        }
        save(userPath, 'botUserData', this.options.userStore, data.userData);
        save(convoPath, 'botConversationData', this.options.conversationStore, data.conversationData);
        save(perUserConvoPath, 'botPerUserInConversationData', this.options.perUserInConversationStore, data.perUserConversationData);
    };
    return BotConnectorBot;
})(collection.DialogCollection);
exports.BotConnectorBot = BotConnectorBot;
var BotConnectorSession = (function (_super) {
    __extends(BotConnectorSession, _super);
    function BotConnectorSession() {
        _super.apply(this, arguments);
    }
    return BotConnectorSession;
})(session.Session);
exports.BotConnectorSession = BotConnectorSession;
function post(settings, path, body, callback) {
    var options = {
        method: 'POST',
        url: settings.endpoint + path,
        body: body,
        json: true
    };
    if (settings.appId && settings.appSecret) {
        options.auth = {
            username: settings.appId,
            password: settings.appSecret
        };
        options.headers = {
            'Ocp-Apim-Subscription-Key': settings.appSecret
        };
    }
    request(options, callback);
}
