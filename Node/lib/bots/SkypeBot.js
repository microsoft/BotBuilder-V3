var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var collection = require('../dialogs/DialogCollection');
var session = require('../Session');
var storage = require('../storage/Storage');
var SkypeBot = (function (_super) {
    __extends(SkypeBot, _super);
    function SkypeBot(botService, options) {
        var _this = this;
        _super.call(this);
        this.botService = botService;
        this.options = {
            maxSessionAge: 14400000,
            defaultDialogId: '/',
            minSendDelay: 1000
        };
        this.configure(options);
        var events = 'message|personalMessage|groupMessage|attachment|threadBotAdded|threadAddMember|threadBotRemoved|threadRemoveMember|contactAdded|threadTopicUpdated|threadHistoryDisclosedUpdate'.split('|');
        events.forEach(function (value) {
            botService.on(value, function (bot, data) {
                _this.emit(value, bot, data);
                _this.handleEvent(value, bot, data);
            });
        });
    }
    SkypeBot.prototype.configure = function (options) {
        if (options) {
            for (var key in options) {
                if (options.hasOwnProperty(key)) {
                    this.options[key] = options[key];
                }
            }
        }
    };
    SkypeBot.prototype.beginDialog = function (address, dialogId, dialogArgs) {
        if (!address.to) {
            throw new Error('Invalid address passed to SkypeBot.beginDialog().');
        }
        if (!this.hasDialog(dialogId)) {
            throw new Error('Invalid dialog passed to SkypeBot.beginDialog().');
        }
        this.dispatchMessage(null, this.toSkypeMessage(address), dialogId, dialogArgs);
    };
    SkypeBot.prototype.handleEvent = function (event, bot, data) {
        var _this = this;
        var onError = function (err) {
            _this.emit('error', err, data);
        };
        switch (event) {
            case 'personalMessage':
                this.dispatchMessage(bot, data, this.options.defaultDialogId, this.options.defaultDialogArgs);
                break;
            case 'threadBotAdded':
                if (this.options.botAddedMessage) {
                    bot.reply(this.options.botAddedMessage, onError);
                }
                break;
            case 'threadAddMember':
                if (this.options.memberAddedMessage) {
                    bot.reply(this.options.memberAddedMessage, onError);
                }
                break;
            case 'threadBotRemoved':
                if (this.options.botRemovedMessage) {
                    bot.reply(this.options.botRemovedMessage, onError);
                }
                break;
            case 'threadRemoveMember':
                if (this.options.memberRemovedMessage) {
                    bot.reply(this.options.memberRemovedMessage, onError);
                }
                break;
            case 'contactAdded':
                if (this.options.contactAddedmessage) {
                    bot.reply(this.options.contactAddedmessage, onError);
                }
                break;
        }
    };
    SkypeBot.prototype.dispatchMessage = function (bot, data, dialogId, dialogArgs) {
        var _this = this;
        var onError = function (err) {
            _this.emit('error', err, data);
        };
        var ses = new SkypeSession({
            localizer: this.options.localizer,
            minSendDelay: this.options.minSendDelay,
            dialogs: this,
            dialogId: dialogId,
            dialogArgs: dialogArgs
        });
        ses.on('send', function (reply) {
            _this.saveData(msg.from.address, ses.userData, ses.sessionState, function () {
                if (reply && reply.text) {
                    var skypeReply = _this.toSkypeMessage(reply);
                    if (bot) {
                        if (skypeReply.to && skypeReply.to != data.from) {
                            _this.emit('send', skypeReply);
                            bot.send(skypeReply.to, skypeReply.content, onError);
                        }
                        else {
                            _this.emit('reply', skypeReply);
                            bot.reply(skypeReply.content, onError);
                        }
                    }
                    else {
                        skypeReply.to = ses.message.to.address;
                        _this.emit('send', skypeReply);
                        _this.botService.send(skypeReply.to, skypeReply.content, onError);
                    }
                }
            });
        });
        ses.on('error', function (err) {
            _this.emit('error', err, data);
        });
        ses.on('quit', function () {
            _this.emit('quit', data);
        });
        var msg = this.fromSkypeMessage(data);
        this.getData(msg.from.address, function (userData, sessionState) {
            ses.userData = userData || {};
            ses.dispatch(sessionState, msg);
        });
    };
    SkypeBot.prototype.getData = function (userId, callback) {
        var _this = this;
        if (!this.options.userStore) {
            this.options.userStore = new storage.MemoryStorage();
        }
        if (!this.options.sessionStore) {
            this.options.sessionStore = new storage.MemoryStorage();
        }
        var ops = 2;
        var userData, sessionState;
        this.options.userStore.get(userId, function (err, data) {
            if (!err) {
                userData = data;
                if (--ops == 0) {
                    callback(userData, sessionState);
                }
            }
            else {
                _this.emit('error', err);
            }
        });
        this.options.sessionStore.get(userId, function (err, data) {
            if (!err) {
                if (data && (new Date().getTime() - data.lastAccess) < _this.options.maxSessionAge) {
                    sessionState = data;
                }
                if (--ops == 0) {
                    callback(userData, sessionState);
                }
            }
            else {
                _this.emit('error', err);
            }
        });
    };
    SkypeBot.prototype.saveData = function (userId, userData, sessionState, callback) {
        var ops = 2;
        function onComplete(err) {
            if (!err) {
                if (--ops == 0) {
                    callback(null);
                }
            }
            else {
                callback(err);
            }
        }
        this.options.userStore.save(userId, userData, onComplete);
        this.options.sessionStore.save(userId, sessionState, onComplete);
    };
    SkypeBot.prototype.fromSkypeMessage = function (msg) {
        return {
            type: msg.type,
            id: msg.messageId.toString(),
            from: {
                channelId: 'skype',
                address: msg.from
            },
            to: {
                channelId: 'skype',
                address: msg.to
            },
            text: msg.content,
            channelData: msg
        };
    };
    SkypeBot.prototype.toSkypeMessage = function (msg) {
        return {
            type: msg.type,
            from: msg.from ? msg.from.address : '',
            to: msg.to ? msg.to.address : '',
            content: msg.text,
            messageId: msg.id ? Number(msg.id) : Number.NaN,
            contentType: "RichText",
            eventTime: msg.channelData ? msg.channelData.eventTime : new Date().getTime()
        };
    };
    return SkypeBot;
})(collection.DialogCollection);
exports.SkypeBot = SkypeBot;
var SkypeSession = (function (_super) {
    __extends(SkypeSession, _super);
    function SkypeSession() {
        _super.apply(this, arguments);
    }
    SkypeSession.prototype.escapeText = function (text) {
        if (text) {
            text = text.replace(/&/g, '&amp;');
            text = text.replace(/</g, '&lt;');
            text = text.replace(/>/g, '&gt;');
        }
        return text;
    };
    SkypeSession.prototype.unescapeText = function (text) {
        if (text) {
            text = text.replace(/&amp;/g, '&');
            text = text.replace(/&lt;/g, '<');
            text = text.replace(/&gt;/g, '>');
        }
        return text;
    };
    return SkypeSession;
})(session.Session);
exports.SkypeSession = SkypeSession;
