var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var collection = require('../dialogs/DialogCollection');
var session = require('../Session');
var storage = require('../storage/Storage');
var uuid = require('node-uuid');
var readline = require('readline');
var TextBot = (function (_super) {
    __extends(TextBot, _super);
    function TextBot(options) {
        _super.call(this);
        this.options = {
            maxSessionAge: 14400000,
            defaultDialogId: '/',
            minSendDelay: 1000
        };
        this.configure(options);
    }
    TextBot.prototype.configure = function (options) {
        if (options) {
            for (var key in options) {
                if (options.hasOwnProperty(key)) {
                    this.options[key] = options[key];
                }
            }
        }
    };
    TextBot.prototype.beginDialog = function (address, dialogId, dialogArgs) {
        if (!this.hasDialog(dialogId)) {
            throw new Error('Invalid dialog passed to TextBot.beginDialog().');
        }
        var message = address || {};
        var userId = message.to ? message.to.address : 'user';
        this.dispatchMessage(userId, message, null, dialogId, dialogArgs, true);
    };
    TextBot.prototype.processMessage = function (message, callback) {
        this.emit('message', message);
        if (!message.id) {
            message.id = uuid.v1();
        }
        if (!message.from) {
            message.from = { channelId: 'text', address: 'user' };
        }
        this.dispatchMessage(message.from.address, message, callback, this.options.defaultDialogId, this.options.defaultDialogArgs);
    };
    TextBot.prototype.listenStdin = function () {
        var _this = this;
        function onMessage(message) {
            console.log(message.text);
        }
        this.on('reply', onMessage);
        this.on('send', onMessage);
        this.on('quit', function () {
            rl.close();
            process.exit();
        });
        var rl = readline.createInterface({ input: process.stdin, output: process.stdout, terminal: false });
        rl.on('line', function (line) {
            _this.processMessage({ text: line || '' });
        });
    };
    TextBot.prototype.dispatchMessage = function (userId, message, callback, dialogId, dialogArgs, newSessionState) {
        var _this = this;
        if (newSessionState === void 0) { newSessionState = false; }
        var ses = new session.Session({
            localizer: this.options.localizer,
            minSendDelay: this.options.minSendDelay,
            dialogs: this,
            dialogId: dialogId,
            dialogArgs: dialogArgs
        });
        ses.on('send', function (reply) {
            _this.saveData(userId, ses.userData, ses.sessionState, function () {
                if (reply && reply.text) {
                    if (callback) {
                        callback(null, reply);
                        callback = null;
                    }
                    else if (message.id || message.conversationId) {
                        reply.from = message.to;
                        reply.to = reply.replyTo || reply.to;
                        reply.conversationId = message.conversationId;
                        reply.language = message.language;
                        _this.emit('reply', reply);
                    }
                    else {
                        _this.emit('send', reply);
                    }
                }
            });
        });
        ses.on('error', function (err) {
            if (callback) {
                callback(err, null);
                callback = null;
            }
            else {
                _this.emit('error', err, message);
            }
        });
        ses.on('quit', function () {
            _this.emit('quit', message);
        });
        this.getData(userId, function (err, userData, sessionState) {
            if (!err) {
                ses.userData = userData || {};
                ses.dispatch(newSessionState ? null : sessionState, message);
            }
            else {
                _this.emit('error', err, message);
            }
        });
    };
    TextBot.prototype.getData = function (userId, callback) {
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
                    callback(null, userData, sessionState);
                }
            }
            else {
                callback(err, null, null);
            }
        });
        this.options.sessionStore.get(userId, function (err, data) {
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
    TextBot.prototype.saveData = function (userId, userData, sessionState, callback) {
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
    return TextBot;
})(collection.DialogCollection);
exports.TextBot = TextBot;
