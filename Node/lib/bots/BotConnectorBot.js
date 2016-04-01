var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var collection = require('../dialogs/DialogCollection');
var session = require('../Session');
var consts = require('../consts');
var utils = require('../utils');
var request = require('request');
var BotConnectorBot = (function (_super) {
    __extends(BotConnectorBot, _super);
    function BotConnectorBot(options) {
        _super.call(this);
        this.options = {
            endpoint: process.env['endpoint'] || 'https://api.botframework.com',
            appId: process.env['appId'] || '',
            appSecret: process.env['appSecret'] || '',
            subscriptionKey: process.env['subscriptionKey'] || '',
            defaultDialogId: '/'
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
                if (req.headers && req.headers.hasOwnProperty('authorization')) {
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
    BotConnectorBot.prototype.listen = function (options) {
        var _this = this;
        this.configure(options);
        return function (req, res) {
            if (req.body) {
                _this.processMessage(req.body, _this.options.defaultDialogId, _this.options.defaultDialogArgs, res);
            }
            else {
                var requestData = '';
                req.on('data', function (chunk) {
                    requestData += chunk;
                });
                req.on('end', function () {
                    try {
                        var msg = JSON.parse(requestData);
                        _this.processMessage(msg, _this.options.defaultDialogId, _this.options.defaultDialogArgs, res);
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
        var msg = address;
        msg.type = 'Message';
        if (!msg.from) {
            msg.from = this.options.defaultFrom;
        }
        if (!msg.to || !msg.from) {
            throw new Error('Invalid address passed to BotConnectorBot.beginDialog().');
        }
        if (!this.hasDialog(dialogId)) {
            throw new Error('Invalid dialog passed to BotConnectorBot.beginDialog().');
        }
        this.processMessage(msg, dialogId, dialogArgs);
    };
    BotConnectorBot.prototype.processMessage = function (message, dialogId, dialogArgs, res) {
        var _this = this;
        try {
            if (!message || !message.type) {
                this.emit('error', new Error('Invalid Bot Framework Message'));
                return res.send(400);
            }
            this.emit(message.type, message);
            if (message.type == 'Message') {
                var ses = new BotConnectorSession({
                    localizer: this.options.localizer,
                    dialogs: this,
                    dialogId: dialogId,
                    dialogArgs: dialogArgs
                });
                ses.on('send', function (message) {
                    var reply = message || {};
                    reply.botUserData = utils.clone(ses.userData);
                    reply.botConversationData = utils.clone(ses.conversationData);
                    reply.botPerUserInConversationData = utils.clone(ses.perUserInConversationData);
                    reply.botPerUserInConversationData[consts.Data.SessionState] = ses.sessionState;
                    if (reply.text && !reply.language) {
                        reply.language = ses.message.language;
                    }
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
                        _this.post('/bot/v1.0/messages', reply, function (err) {
                            _this.emit('error', err);
                        });
                    }
                    else {
                        reply.from = ses.message.from;
                        reply.to = ses.message.to;
                        _this.emit('send', reply);
                        _this.post('/bot/v1.0/messages', reply, function (err) {
                            _this.emit('error', err);
                        });
                    }
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
                var sessionState;
                if (message.botUserData) {
                    ses.userData = message.botUserData;
                    delete message.botUserData;
                }
                else {
                    ses.userData = {};
                }
                if (message.botConversationData) {
                    ses.conversationData = message.botConversationData;
                    delete message.botConversationData;
                }
                else {
                    ses.conversationData = {};
                }
                if (message.botPerUserInConversationData) {
                    if (message.botPerUserInConversationData.hasOwnProperty(consts.Data.SessionState)) {
                        sessionState = message.botPerUserInConversationData[consts.Data.SessionState];
                        delete message.botPerUserInConversationData[consts.Data.SessionState];
                    }
                    ses.perUserInConversationData = message.botPerUserInConversationData;
                    delete message.botPerUserInConversationData;
                }
                else {
                    ses.perUserInConversationData = {};
                }
                ses.dispatch(sessionState, message);
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
    BotConnectorBot.prototype.post = function (path, body, callback) {
        var settings = this.options;
        var options = {
            url: settings.endpoint + path,
            body: body
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
        request.post(options, callback);
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
