var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var da = require('../dialogs/DialogAction');
var dc = require('../dialogs/DialogCollection');
var sd = require('../dialogs/SimpleDialog');
var ses = require('../Session');
var bs = require('../storage/BotStorage');
var consts = require('../consts');
var utils = require('../utils');
var events = require('events');
var async = require('async');
var UniversalBot = (function (_super) {
    __extends(UniversalBot, _super);
    function UniversalBot(connector, settings) {
        _super.call(this);
        this.settings = {
            processLimit: 4,
            persistUserData: true,
            persistConversationData: false
        };
        this.connectors = {};
        this.dialogs = new dc.DialogCollection();
        this.mwReceive = [];
        this.mwAnalyze = [];
        this.mwSend = [];
        if (settings) {
            for (var name in settings) {
                this.set(name, settings[name]);
            }
        }
        if (connector) {
            this.connector('*', connector);
            var asStorage = connector;
            if (!this.settings.storage &&
                typeof asStorage.getData === 'function' &&
                typeof asStorage.saveData === 'function') {
                this.settings.storage = asStorage;
            }
        }
    }
    UniversalBot.prototype.set = function (name, value) {
        this.settings[name] = value;
        return this;
    };
    UniversalBot.prototype.get = function (name) {
        return this.settings[name];
    };
    UniversalBot.prototype.connector = function (channelId, connector) {
        var _this = this;
        var c;
        if (connector) {
            this.connectors[channelId || '*'] = c = connector;
            c.onMessage(function (messages, cb) { return _this.receive(messages, cb); });
        }
        else if (this.connectors.hasOwnProperty(channelId)) {
            c = this.connectors[channelId];
        }
        else if (this.connectors.hasOwnProperty('*')) {
            c = this.connectors['*'];
        }
        return c;
    };
    UniversalBot.prototype.dialog = function (id, dialog) {
        var d;
        if (dialog) {
            if (Array.isArray(dialog)) {
                d = new sd.SimpleDialog(da.waterfall(dialog));
            }
            if (typeof dialog == 'function') {
                d = new sd.SimpleDialog(da.waterfall([dialog]));
            }
            else {
                d = dialog;
            }
            this.dialogs.add(id, d);
        }
        else {
            d = this.dialogs.getDialog(id);
        }
        return d;
    };
    UniversalBot.prototype.use = function (middleware) {
        if (middleware.receive) {
            this.mwReceive.push(middleware.receive);
        }
        if (middleware.analyze) {
            this.mwAnalyze.push(middleware.analyze);
        }
        if (middleware.dialog) {
            this.dialogs.use(middleware.dialog);
        }
        if (middleware.send) {
            this.mwSend.push(middleware.send);
        }
        return this;
    };
    UniversalBot.prototype.receive = function (messages, done) {
        var _this = this;
        var list = Array.isArray(messages) ? messages : [messages];
        async.eachLimit(list, this.settings.processLimit, function (message, cb) {
            message.type = message.type || 'message';
            _this.lookupUser(message.address, function (user) {
                if (user) {
                    message.user = user;
                }
                _this.emit('receive', message);
                _this.messageMiddleware(message, _this.mwReceive, function () {
                    if (_this.isMessage(message)) {
                        _this.emit('analyze', message);
                        _this.analyzeMiddleware(message, function () {
                            _this.emit('incoming', message);
                            var userId = message.user.id;
                            var conversationId = message.address.conversation ? message.address.conversation.id : null;
                            var storageCtx = {
                                userId: userId,
                                conversationId: conversationId,
                                address: message.address,
                                persistUserData: _this.settings.persistUserData,
                                persistConversationData: _this.settings.persistConversationData
                            };
                            _this.route(storageCtx, message, _this.settings.defaultDialogId || '/', _this.settings.defaultDialogArgs, cb);
                        }, cb);
                    }
                    else {
                        _this.emit(message.type, message);
                        cb(null);
                    }
                }, cb);
            }, cb);
        }, this.errorLogger(done));
    };
    UniversalBot.prototype.beginDialog = function (message, dialogId, dialogArgs, done) {
        var _this = this;
        var msg = message && message.toMessage ? message.toMessage() : message;
        if (!msg || !msg.address) {
            throw new Error('Invalid message passed to UniversalBot.beginDialog().');
        }
        msg.text = msg.text || '';
        msg.type = 'message';
        this.lookupUser(msg.address, function (user) {
            if (user) {
                msg.user = user;
            }
            _this.ensureConversation(msg.address, function (adr) {
                msg.address = adr;
                var storageCtx = {
                    userId: msg.user.id,
                    address: msg.address,
                    persistUserData: _this.settings.persistUserData,
                    persistConversationData: _this.settings.persistConversationData
                };
                _this.route(storageCtx, msg, dialogId, dialogArgs, _this.errorLogger(done));
            }, _this.errorLogger(done));
        }, this.errorLogger(done));
    };
    UniversalBot.prototype.send = function (messages, done) {
        var _this = this;
        var list;
        if (Array.isArray(messages)) {
            list = messages;
        }
        else if (messages.toMessage) {
            list = [messages.toMessage()];
        }
        else {
            list = [messages];
        }
        async.eachLimit(list, this.settings.processLimit, function (message, cb) {
            _this.ensureConversation(message.address, function (adr) {
                message.address = adr;
                _this.emit('send', message);
                _this.messageMiddleware(message, _this.mwSend, function () {
                    _this.emit('outgoing', message);
                    cb(null);
                }, cb);
            }, cb);
        }, this.errorLogger(function (err) {
            if (!err) {
                _this.tryCatch(function () {
                    var channelId = list[0].address.channelId;
                    var connector = _this.connector(channelId);
                    if (!connector) {
                        throw new Error("Invalid channelId='" + channelId + "'");
                    }
                    connector.send(list, _this.errorLogger(done));
                }, _this.errorLogger(done));
            }
            else if (done) {
                done;
            }
        }));
    };
    UniversalBot.prototype.isInConversation = function (address, cb) {
        var _this = this;
        this.lookupUser(address, function (user) {
            var conversationId = address.conversation ? address.conversation.id : null;
            var storageCtx = {
                userId: user.id,
                conversationId: conversationId,
                address: address,
                persistUserData: false,
                persistConversationData: false
            };
            _this.getStorageData(storageCtx, function (data) {
                var lastAccess;
                if (data && data.privateConversationData && data.privateConversationData.hasOwnProperty(consts.Data.SessionState)) {
                    var ss = data.privateConversationData[consts.Data.SessionState];
                    if (ss && ss.lastAccess) {
                        lastAccess = new Date(ss.lastAccess);
                    }
                }
                cb(null, lastAccess);
            }, _this.errorLogger(cb));
        }, this.errorLogger(cb));
    };
    UniversalBot.prototype.route = function (storageCtx, message, dialogId, dialogArgs, done) {
        var _this = this;
        var loadedData;
        this.getStorageData(storageCtx, function (data) {
            var session = new ses.Session({
                localizer: _this.settings.localizer,
                autoBatchDelay: _this.settings.autoBatchDelay,
                dialogs: _this.dialogs,
                dialogId: dialogId,
                dialogArgs: dialogArgs,
                dialogErrorMessage: _this.settings.dialogErrorMessage,
                onSave: function (cb) {
                    var finish = _this.errorLogger(cb);
                    loadedData.userData = utils.clone(session.userData);
                    loadedData.conversationData = utils.clone(session.conversationData);
                    loadedData.privateConversationData = utils.clone(session.privateConversationData);
                    loadedData.privateConversationData[consts.Data.SessionState] = session.sessionState;
                    _this.saveStorageData(storageCtx, loadedData, finish, finish);
                },
                onSend: function (messages, cb) {
                    _this.send(messages, cb);
                }
            });
            session.on('error', function (err) { return _this.emitError(err); });
            var sessionState;
            session.userData = data.userData || {};
            session.conversationData = data.conversationData || {};
            session.privateConversationData = data.privateConversationData || {};
            if (session.privateConversationData.hasOwnProperty(consts.Data.SessionState)) {
                sessionState = session.privateConversationData[consts.Data.SessionState];
                delete session.privateConversationData[consts.Data.SessionState];
            }
            loadedData = data;
            _this.emit('routing', session);
            session.dispatch(sessionState, message);
            done(null);
        }, done);
    };
    UniversalBot.prototype.messageMiddleware = function (message, middleware, done, error) {
        var i = -1;
        var _this = this;
        function next() {
            if (++i < middleware.length) {
                _this.tryCatch(function () {
                    middleware[i](message, next);
                }, function () { return next(); });
            }
            else {
                _this.tryCatch(function () { return done(); }, error);
            }
        }
        next();
    };
    UniversalBot.prototype.analyzeMiddleware = function (message, done, error) {
        var cnt = this.mwAnalyze.length;
        var _this = this;
        function analyze(fn) {
            _this.tryCatch(function () {
                fn(message, function (analysis) {
                    if (analysis && typeof analysis == 'object') {
                        for (var prop in analysis) {
                            if (analysis.hasOwnProperty(prop)) {
                                message[prop] = analysis[prop];
                            }
                        }
                    }
                    finish();
                });
            }, function () { return finish(); });
        }
        function finish() {
            _this.tryCatch(function () {
                if (--cnt <= 0) {
                    done();
                }
            }, error);
        }
        if (cnt > 0) {
            for (var i = 0; i < this.mwAnalyze.length; i++) {
                analyze(this.mwAnalyze[i]);
            }
        }
        else {
            finish();
        }
    };
    UniversalBot.prototype.isMessage = function (message) {
        return (message && message.type && message.type.toLowerCase().indexOf('message') == 0);
    };
    UniversalBot.prototype.ensureConversation = function (address, done, error) {
        var _this = this;
        this.tryCatch(function () {
            if (!address.conversation) {
                var connector = _this.connector(address.channelId);
                if (!connector) {
                    throw new Error("Invalid channelId='" + address.channelId + "'");
                }
                connector.startConversation(address, function (err, adr) {
                    if (!err) {
                        _this.tryCatch(function () { return done(adr); }, error);
                    }
                    else if (error) {
                        error(err);
                    }
                });
            }
            else {
                _this.tryCatch(function () { return done(address); }, error);
            }
        }, error);
    };
    UniversalBot.prototype.lookupUser = function (address, done, error) {
        var _this = this;
        this.tryCatch(function () {
            _this.emit('lookupUser', address);
            if (_this.settings.lookupUser) {
                _this.settings.lookupUser(address, function (err, user) {
                    if (!err) {
                        _this.tryCatch(function () { return done(user || address.user); }, error);
                    }
                    else if (error) {
                        error(err);
                    }
                });
            }
            else {
                _this.tryCatch(function () { return done(address.user); }, error);
            }
        }, error);
    };
    UniversalBot.prototype.getStorageData = function (storageCtx, done, error) {
        var _this = this;
        this.tryCatch(function () {
            _this.emit('getStorageData', storageCtx);
            var storage = _this.getStorage();
            storage.getData(storageCtx, function (err, data) {
                if (!err) {
                    _this.tryCatch(function () { return done(data || {}); }, error);
                }
                else if (error) {
                    error(err);
                }
            });
        }, error);
    };
    UniversalBot.prototype.saveStorageData = function (storageCtx, data, done, error) {
        var _this = this;
        this.tryCatch(function () {
            _this.emit('saveStorageData', storageCtx);
            var storage = _this.getStorage();
            storage.saveData(storageCtx, data, function (err) {
                if (!err) {
                    if (done) {
                        _this.tryCatch(function () { return done(); }, error);
                    }
                }
                else if (error) {
                    error(err);
                }
            });
        }, error);
    };
    UniversalBot.prototype.getStorage = function () {
        if (!this.settings.storage) {
            this.settings.storage = new bs.MemoryBotStorage();
        }
        return this.settings.storage;
    };
    UniversalBot.prototype.tryCatch = function (fn, error) {
        try {
            fn();
        }
        catch (e) {
            try {
                if (error) {
                    error(e);
                }
            }
            catch (e2) {
                this.emitError(e2);
            }
        }
    };
    UniversalBot.prototype.errorLogger = function (done) {
        var _this = this;
        return function (err) {
            if (err) {
                _this.emitError;
            }
            if (done) {
                done(err);
                done = null;
            }
        };
    };
    UniversalBot.prototype.emitError = function (err) {
        this.emit("error", err instanceof Error ? err : new Error(err.toString()));
    };
    return UniversalBot;
})(events.EventEmitter);
exports.UniversalBot = UniversalBot;
