var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var collection = require('../dialogs/DialogCollection');
var session = require('../Session');
var consts = require('../consts');
var utils = require('../utils');
var SlackBot = (function (_super) {
    __extends(SlackBot, _super);
    function SlackBot(controller, bot, options) {
        var _this = this;
        _super.call(this);
        this.controller = controller;
        this.bot = bot;
        this.options = {
            maxSessionAge: 14400000,
            defaultDialogId: '/',
            ambientMentionDuration: 300000,
            minSendDelay: 1500,
            sendIsTyping: true
        };
        this.configure(options);
        ['message_received', 'bot_channel_join', 'user_channel_join', 'bot_group_join', 'user_group_join'].forEach(function (type) {
            _this.controller.on(type, function (bot, msg) {
                _this.emit(type, bot, msg);
            });
        });
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
        dialogId = dialogId || this.options.defaultDialogId;
        dialogArgs = dialogArgs || this.options.defaultDialogArgs;
        types.forEach(function (type) {
            _this.controller.on(type, function (bot, msg) {
                bot.identifyTeam(function (err, teamId) {
                    msg.team = teamId;
                    _this.dispatchMessage(bot, msg, dialogId, dialogArgs);
                });
            });
        });
        return this;
    };
    SlackBot.prototype.listenForMentions = function (dialogId, dialogArgs) {
        var _this = this;
        var sessions = {};
        var dispatch = function (bot, msg, ss) {
            bot.identifyTeam(function (err, teamId) {
                msg.team = teamId;
                _this.dispatchMessage(bot, msg, dialogId, dialogArgs, ss);
            });
        };
        dialogId = dialogId || this.options.defaultDialogId;
        dialogArgs = dialogArgs || this.options.defaultDialogArgs;
        this.controller.on('direct_message', function (bot, msg) {
            dispatch(bot, msg);
        });
        ['direct_mention', 'mention'].forEach(function (type) {
            _this.controller.on(type, function (bot, msg) {
                var key = msg.channel + ':' + msg.user;
                var ss = sessions[key] = { callstack: [], lastAccess: new Date().getTime() };
                dispatch(bot, msg, ss);
            });
        });
        this.controller.on('ambient', function (bot, msg) {
            var key = msg.channel + ':' + msg.user;
            if (sessions.hasOwnProperty(key)) {
                var ss = sessions[key];
                if (ss.callstack && ss.callstack.length > 0 && (new Date().getTime() - ss.lastAccess) <= _this.options.ambientMentionDuration) {
                    dispatch(bot, msg, ss);
                }
                else {
                    delete sessions[key];
                }
            }
        });
        return this;
    };
    SlackBot.prototype.beginDialog = function (address, dialogId, dialogArgs) {
        if (!address.channel) {
            throw new Error('Invalid address passed to SlackBot.beginDialog().');
        }
        if (!this.hasDialog(dialogId)) {
            throw new Error('Invalid dialog passed to SlackBot.beginDialog().');
        }
        this.dispatchMessage(null, address, dialogId, dialogArgs);
        return this;
    };
    SlackBot.prototype.dispatchMessage = function (bot, msg, dialogId, dialogArgs, smartState) {
        var _this = this;
        var onError = function (err) {
            if (err) {
                _this.emit('error', err, msg);
            }
        };
        var ses = new SlackSession({
            localizer: this.options.localizer,
            minSendDelay: this.options.minSendDelay,
            dialogs: this,
            dialogId: dialogId || this.options.defaultDialogId,
            dialogArgs: dialogArgs || this.options.defaultDialogArgs
        });
        ses.on('send', function (reply) {
            var teamData = ses.teamData && ses.teamData.id ? utils.clone(ses.teamData) : null;
            var channelData = ses.channelData && ses.channelData.id ? utils.clone(ses.channelData) : null;
            var userData = ses.userData && ses.userData.id ? utils.clone(ses.userData) : null;
            if (channelData && !smartState) {
                channelData[consts.Data.SessionState] = ses.sessionState;
            }
            _this.saveData(teamData, channelData, userData, function () {
                if (reply && (reply.text || reply.channelData)) {
                    var slackReply = _this.toSlackMessage(reply);
                    if (bot) {
                        if (slackReply.channel && slackReply.channel != msg.channel) {
                            _this.emit('send', slackReply);
                            bot.say(slackReply, onError);
                        }
                        else {
                            _this.emit('reply', slackReply);
                            bot.reply(msg, slackReply, onError);
                        }
                    }
                    else {
                        if (!slackReply.channel) {
                            slackReply.channel = msg.channel;
                        }
                        _this.emit('send', slackReply);
                        _this.bot.say(slackReply, onError);
                    }
                }
            });
        });
        ses.on('error', function (err) {
            _this.emit('error', err, msg);
        });
        ses.on('quit', function () {
            _this.emit('quit', msg);
        });
        ses.on('typing', function () {
            _this.emit('typing', msg);
            _this.bot.say({ id: 1, type: 'typing', channel: msg.channel }, onError);
        });
        this.bot.say({ id: 1, type: 'typing', channel: msg.channel }, onError);
        var sessionState;
        var message = this.fromSlackMessage(msg);
        this.getData(msg, function (err, data) {
            if (!err) {
                if (!data.teamData && msg.team) {
                    data.teamData = { id: msg.team };
                }
                if (!data.channelData && msg.channel) {
                    data.channelData = { id: msg.channel };
                }
                if (!data.userData && msg.user) {
                    data.userData = { id: msg.user };
                }
                if (smartState) {
                    sessionState = smartState;
                }
                else if (data.channelData && data.channelData.hasOwnProperty(consts.Data.SessionState)) {
                    sessionState = data.channelData[consts.Data.SessionState];
                    delete data.channelData[consts.Data.SessionState];
                }
                ses.teamData = data.teamData;
                ses.channelData = data.channelData;
                ses.userData = data.userData;
                ses.dispatch(sessionState, message);
            }
            else {
                _this.emit('error', err, msg);
            }
        });
    };
    SlackBot.prototype.getData = function (msg, callback) {
        var ops = 3;
        var data = {};
        function load(store, id, field) {
            data[field] = null;
            if (id && id.length > 0) {
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
        load(this.controller.storage.teams, msg.team, 'teamData');
        load(this.controller.storage.channels, msg.channel, 'channelData');
        load(this.controller.storage.users, msg.user, 'userData');
    };
    SlackBot.prototype.saveData = function (teamData, channelData, userData, callback) {
        var ops = 3;
        function save(store, data) {
            if (data) {
                store.save(data, function (err) {
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
            else if (callback && --ops == 0) {
                callback(null);
            }
        }
        save(this.controller.storage.teams, teamData);
        save(this.controller.storage.channels, channelData);
        save(this.controller.storage.users, userData);
    };
    SlackBot.prototype.fromSlackMessage = function (msg) {
        var attachments = [];
        if (msg.attachments) {
            msg.attachments.forEach(function (value) {
                var contentType = value.image_url ? 'image' : 'text/plain';
                var a = { contentType: contentType, fallbackText: value.fallback };
                if (value.image_url) {
                    a.contentUrl = value.image_url;
                }
                if (value.thumb_url) {
                    a.thumbnailUrl = value.thumb_url;
                }
                if (value.text) {
                    a.text = value.text;
                }
                if (value.title) {
                    a.title = value.title;
                }
                if (value.title_link) {
                    a.titleLink;
                }
                attachments.push(a);
            });
        }
        return {
            type: msg.type,
            id: msg.ts,
            text: msg.text,
            attachments: attachments,
            from: {
                channelId: 'slack',
                address: msg.user
            },
            channelConversationId: msg.channel,
            channelData: msg
        };
    };
    SlackBot.prototype.toSlackMessage = function (msg) {
        var attachments = [];
        if (msg.attachments && !msg.channelData) {
            msg.attachments.forEach(function (value) {
                var a = {};
                if (value.fallbackText) {
                    a.fallback = value.fallbackText;
                }
                else {
                    a.fallback = value.contentUrl ? value.contentUrl : value.text || '<attachment>';
                }
                if (value.contentUrl && /^image/i.test(value.contentType)) {
                    a.image_url = value.contentUrl;
                }
                if (value.thumbnailUrl) {
                    a.thumb_url = value.thumbnailUrl;
                }
                if (value.text) {
                    a.text = value.text;
                }
                if (value.title) {
                    a.title = value.title;
                }
                if (value.titleLink) {
                    a.title_link = value.titleLink;
                }
                attachments.push(a);
            });
        }
        return msg.channelData || {
            event: 'direct_message',
            type: msg.type,
            ts: msg.id,
            text: msg.text,
            attachments: attachments,
            user: msg.to ? msg.to.address : (msg.from ? msg.from.address : null),
            channel: msg.channelConversationId
        };
    };
    return SlackBot;
})(collection.DialogCollection);
exports.SlackBot = SlackBot;
var SlackSession = (function (_super) {
    __extends(SlackSession, _super);
    function SlackSession() {
        _super.apply(this, arguments);
    }
    SlackSession.prototype.isTyping = function () {
        this.emit('typing');
    };
    SlackSession.prototype.escapeText = function (text) {
        if (text) {
            text = text.replace(/&/g, '&amp;');
            text = text.replace(/</g, '&lt;');
            text = text.replace(/>/g, '&gt;');
        }
        return text;
    };
    SlackSession.prototype.unescapeText = function (text) {
        if (text) {
            text = text.replace(/&amp;/g, '&');
            text = text.replace(/&lt;/g, '<');
            text = text.replace(/&gt;/g, '>');
        }
        return text;
    };
    return SlackSession;
})(session.Session);
exports.SlackSession = SlackSession;
