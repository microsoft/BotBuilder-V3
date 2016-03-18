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
                bot.identifyTeam(function (err, teamId) {
                    msg.team = teamId;
                    _this.emit(type, msg);
                    _this.dispatchMessage(bot, msg, dialogId || _this.options.defaultDialogId, dialogArgs || _this.options.defaultDialogArgs);
                });
            });
        });
        return this;
    };
    SlackBot.prototype.beginDialog = function (address, dialogId, dialogArgs) {
        // Validate args
        if (!address.user && !address.channel) {
            throw new Error('Invalid address passed to SlackBot.beginDialog().');
        }
        if (!this.hasDialog(dialogId)) {
            throw new Error('Invalid dialog passed to SlackBot.beginDialog().');
        }
        // Dispatch message
        this.dispatchMessage(null, address, dialogId, dialogArgs);
        return this;
    };
    SlackBot.prototype.dispatchMessage = function (bot, msg, dialogId, dialogArgs) {
        var _this = this;
        var onError = function (err) {
            _this.emit('error', err, msg);
        };
        // Initialize session
        var ses = new SlackSession({
            localizer: this.options.localizer,
            dialogs: this,
            dialogId: this.options.defaultDialogId,
            dialogArgs: this.options.defaultDialogArgs
        });
        ses.on('send', function (reply) {
            // Clone data fields & store session state
            var teamData = ses.teamData && ses.teamData.id ? utils.clone(ses.teamData) : null;
            var channelData = ses.channelData && ses.channelData.id ? utils.clone(ses.channelData) : null;
            var userData = ses.userData && ses.userData.id ? utils.clone(ses.userData) : null;
            if (channelData) {
                channelData[consts.Data.SessionState] = ses.sessionState;
            }
            // Save data
            _this.saveData(teamData, channelData, userData, function () {
                // If we have no message text then we're just saving state.
                if (reply && reply.text) {
                    var slackReply = _this.toSlackMessage(reply);
                    if (bot) {
                        // Check for a different TO address
                        if (slackReply.user && slackReply.user != msg.user) {
                            _this.emit('send', slackReply);
                            bot.say(slackReply, onError);
                        }
                        else {
                            _this.emit('reply', slackReply);
                            bot.reply(msg, slackReply.text);
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
            _this.emit('error', err, msg);
        });
        ses.on('quit', function () {
            _this.emit('quit', msg);
        });
        // Load data from storage
        var sessionState;
        var message = this.fromSlackMessage(msg);
        this.getData(msg, function (err, data) {
            if (!err) {
                // Init data
                if (!data.teamData && msg.team) {
                    data.teamData = { id: msg.team };
                }
                if (!data.channelData && msg.channel) {
                    data.channelData = { id: msg.channel };
                }
                if (!data.userData && msg.user) {
                    data.userData = { id: msg.user };
                }
                // Unpack session state
                if (data.channelData && data.channelData.hasOwnProperty(consts.Data.SessionState)) {
                    sessionState = data.channelData[consts.Data.SessionState];
                    delete data.channelData[consts.Data.SessionState];
                }
                // Dispatch message
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
        var teamId, event;
        if (msg.channelData) {
            teamId = msg.channelData.teamId;
            event = msg.channelData.event;
        }
        return {
            team: teamId,
            event: event || 'direct_message',
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
var SlackSession = (function (_super) {
    __extends(SlackSession, _super);
    function SlackSession() {
        _super.apply(this, arguments);
    }
    return SlackSession;
})(session.Session);
exports.SlackSession = SlackSession;
