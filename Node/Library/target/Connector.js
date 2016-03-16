var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var request = require('request');
var session = require('./Session');
var consts = require('./Consts');
var utils = require('./Utils');
var ConnectorSession = (function (_super) {
    __extends(ConnectorSession, _super);
    function ConnectorSession(dialogs, options, callback) {
        _super.call(this, dialogs, options.defaultDialogId, options.defaultDialogArgs);
        this.options = options;
        this.callback = callback;
        if (options.localizer) {
            this.localizer = options.localizer;
        }
    }
    ConnectorSession.prototype.dispatch = function (message) {
        if (message.botUserData) {
            this.userData = message.botUserData;
            delete message.botUserData;
        }
        else {
            this.userData = {};
        }
        if (message.botConversationData) {
            this.conversationData = message.botConversationData;
            delete message.botConversationData;
        }
        else {
            this.conversationData = {};
        }
        if (message.botPerUserInConversationData) {
            if (message.botPerUserInConversationData.hasOwnProperty(consts.Data.SessionState)) {
                this.ss = message.botPerUserInConversationData[consts.Data.SessionState];
                delete message.botPerUserInConversationData[consts.Data.SessionState];
            }
            this.perUserInConversationData = message.botPerUserInConversationData;
            delete message.botPerUserInConversationData;
        }
        else {
            this.perUserInConversationData = {};
        }
        if (!this.ss || !this.ss.callstack) {
            this.ss = { callstack: [] };
        }
        return _super.prototype.dispatch.call(this, message);
    };
    ConnectorSession.prototype.routeMessage = function (session) {
        var handled = false;
        if (!session.messageSent()) {
            switch (session.message.type.toLowerCase()) {
                case "botaddedtoconversation":
                    if (this.options.groupWelcomeMessage) {
                        handled = true;
                        var msg = this.createMessage(this.options.groupWelcomeMessage);
                        msg.type = session.message.type;
                        session.send(msg);
                    }
                    break;
                case "useraddedtoconversation":
                    if (this.options.userWelcomeMessage) {
                        handled = true;
                        var msg = this.createMessage(this.options.userWelcomeMessage);
                        msg.type = session.message.type;
                        session.send(msg);
                    }
                    break;
                case "endofconversation":
                    if (this.options.goodbyeMessage) {
                        handled = true;
                        var msg = this.createMessage(this.options.goodbyeMessage);
                        msg.type = session.message.type;
                        session.send(msg);
                    }
                    break;
            }
        }
        if (!handled) {
            _super.prototype.routeMessage.call(this, session);
        }
    };
    ConnectorSession.prototype.composeMessage = function (status, message) {
        if (status < 400) {
            var reply = message || {};
            reply.botUserData = utils.clone(this.userData);
            reply.botConversationData = utils.clone(this.conversationData);
            reply.botPerUserInConversationData = utils.clone(this.perUserInConversationData);
            reply.botPerUserInConversationData[consts.Data.SessionState] = this.ss;
            if (this.callback) {
                this.callback(status, reply);
                this.callback = null;
            }
            else {
                reply.from = this.message.from;
                reply.to = this.message.to;
                this.post('/bot/v1.0/messages', reply, function (err) {
                    if (err) {
                        console.error(err.toString());
                    }
                });
            }
        }
        else if (this.callback) {
            this.callback(status);
            this.callback == null;
        }
    };
    ConnectorSession.prototype.getSessionState = function () {
        return this.ss;
    };
    ConnectorSession.prototype.post = function (path, body, callback) {
        var settings = this.options;
        var options = {
            url: settings.endpoint + path,
            body: body
        };
        if (settings.appId && settings.appSecret) {
            options.auth = {
                username: 'Bot_' + settings.appId,
                password: 'Bot_' + settings.appSecret
            };
            options.headers = {
                'Ocp-Apim-Subscription-Key': settings.subscriptionKey || settings.appSecret
            };
        }
        request.post(options, callback);
    };
    ConnectorSession.processMessage = function (dialogs, message, options, callback) {
        var session = new ConnectorSession(dialogs, options, callback);
        session.dispatch(message);
    };
    return ConnectorSession;
})(session.Session);
exports.ConnectorSession = ConnectorSession;
