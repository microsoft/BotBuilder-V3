var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var collection = require('../dialogs/DialogCollection');
var session = require('../Session');
var storage = require('../storage/Storage');
var SlackBot = (function (_super) {
    __extends(SlackBot, _super);
    function SlackBot(controller, bot, options) {
        _super.call(this);
        this.controller = controller;
        this.bot = bot;
        this.options = {
            maxSessionAge: 14400000,
            defaultDialogId: '/'
        };
        this.configure(options);
    }
    SlackBot.prototype.configure = function (options) {
        if (options) {
            for (var key in options) {
                if (options.hasOwnProperty(key)) {
                    this.options[key] = options[key];
                }
            }
        }
        return this;
    };
    SlackBot.prototype.listen = function (types, dialogId, dialogArgs) {
        var _this = this;
        types.forEach(function (type) {
            _this.controller.on(type, function (bot, msg) {
                _this.emit(type, msg);
                _this.dispatchMessage(bot, msg, dialogId || _this.options.defaultDialogId, dialogArgs || _this.options.defaultDialogArgs);
            });
        });
        return this;
    };
    SlackBot.prototype.beginDialog = function (address, dialogId, dialogArgs) {
        // Validate args
        if (!this.bot) {
            throw new Error('Spawned BotKit Bot not passed to constructor.');
        }
        if (!address.to) {
            throw new Error('Invalid address passed to SlackBot.beginDialog().');
        }
        if (!this.hasDialog(dialogId)) {
            throw new Error('Invalid dialog passed to SlackBot.beginDialog().');
        }
        // Dispatch message
        this.dispatchMessage(null, this.toSlackMessage(address), dialogId, dialogArgs);
        return this;
    };
    SlackBot.prototype.dispatchMessage = function (bot, data, dialogId, dialogArgs) {
        var _this = this;
        var onError = function (err) {
            _this.emit('error', err, data);
        };
        // Initialize session
        var sessionId = data.event == 'direct_message' ? data.user : data.channel;
        var ses = new session.Session({
            localizer: this.options.localizer,
            dialogs: this,
            dialogId: this.options.defaultDialogId,
            dialogArgs: this.options.defaultDialogArgs
        });
        ses.on('send', function (reply) {
            _this.saveData(data.user, sessionId, ses.userData, ses.sessionState, function () {
                // If we have no message text then we're just saving state.
                if (reply && reply.text) {
                    var slackReply = _this.toSlackMessage(reply);
                    if (bot) {
                        // Check for a different TO address
                        if (slackReply.user && slackReply.user != data.user) {
                            _this.emit('send', slackReply);
                            bot.say(slackReply, onError);
                        }
                        else {
                            _this.emit('reply', slackReply);
                            bot.reply(data, slackReply.text);
                        }
                    }
                    else {
                        slackReply.user = ses.message.to.address;
                        _this.emit('send', slackReply);
                        _this.bot.say(slackReply, onError);
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
        // Dispatch message
        var message = this.fromSlackMessage(data);
        this.getData(data.user, sessionId, function (err, userData, sessionState) {
            ses.userData = userData || {};
            ses.dispatch(sessionState, message);
        });
    };
    SlackBot.prototype.getData = function (userId, sessionId, callback) {
        var _this = this;
        // Ensure stores specified
        if (!this.options.userStore) {
            this.options.userStore = new storage.MemoryStorage();
        }
        if (!this.options.sessionStore) {
            this.options.sessionStore = new storage.MemoryStorage();
        }
        // Load data
        var ops = 2;
        var userData, sessionState;
        this.options.userStore.get(userId, function (err, data) {
            if (!err) {
                userData = data;
                if (--ops == 0) {
                    callback(null, userData, sessionState);
                }
            }
            else {
                callback(err, null, null);
            }
        });
        this.options.sessionStore.get(sessionId, function (err, data) {
            if (!err) {
                if (data && (new Date().getTime() - data.lastAccess) < _this.options.maxSessionAge) {
                    sessionState = data;
                }
                if (--ops == 0) {
                    callback(null, userData, sessionState);
                }
            }
            else {
                callback(err, null, null);
            }
        });
    };
    SlackBot.prototype.saveData = function (userId, sessionId, userData, sessionState, callback) {
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
        this.options.sessionStore.save(sessionId, sessionState, onComplete);
    };
    SlackBot.prototype.fromSlackMessage = function (msg) {
        return {
            type: msg.type,
            id: msg.ts,
            text: msg.text,
            from: {
                channelId: 'slack',
                address: msg.user
            },
            channelConversationId: msg.channel,
            channelData: msg
        };
    };
    SlackBot.prototype.toSlackMessage = function (msg) {
        return {
            event: msg.channelData ? msg.channelData.event : 'direct_message',
            type: msg.type,
            ts: msg.id,
            text: msg.text,
            user: msg.to ? msg.to.address : (msg.from ? msg.from.address : undefined),
            channel: msg.channelConversationId
        };
    };
    return SlackBot;
})(collection.DialogCollection);
exports.SlackBot = SlackBot;
