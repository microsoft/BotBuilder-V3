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
        this.settings = { processLimit: 4 };
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
            _this.lookupUser(message.address, function (user) {
                message.user = user;
                _this.emit('receive', message);
                _this.messageMiddleware(message, _this.mwReceive, function () {
                    _this.emit('analyze', message);
                    _this.analyzeMiddleware(message, function () {
                        _this.emit('incoming', message);
                        var userId = message.user.id;
                        var conversationId = message.address.conversation ? message.address.conversation.id : null;
                        var storageCtx = { userId: userId, conversationId: conversationId, address: message.address };
                        _this.route(storageCtx, message, _this.settings.defaultDialogId || '/', _this.settings.defaultDialogArgs, cb);
                    }, cb);
                }, cb);
            }, cb);
        }, done);
    };
    UniversalBot.prototype.beginDialog = function (message, dialogId, dialogArgs, done) {
        var _this = this;
        this.lookupUser(message.address, function (user) {
            message.user = user;
            var storageCtx = { userId: message.user.id, address: message.address };
            _this.route(storageCtx, message, dialogId, dialogArgs, function (err) {
                if (done) {
                    done(err);
                }
            });
        }, done);
    };
    UniversalBot.prototype.send = function (messages, done) {
        var _this = this;
        var list = Array.isArray(messages) ? messages : [messages];
        async.eachLimit(list, this.settings.processLimit, function (message, cb) {
            _this.emit('send', message);
            _this.messageMiddleware(message, _this.mwSend, function () {
                _this.emit('outgoing', message);
                cb(null);
            }, cb);
        }, function (err) {
            if (!err) {
                _this.tryCatch(function () {
                    var channelId = list[0].address.channelId;
                    var connector = _this.connector(channelId);
                    if (!connector) {
                        throw new Error("Invalid channelId='" + channelId + "'");
                    }
                    connector.send(list, function (err, conversationId) {
                        if (done) {
                            if (err) {
                                _this.emitError(err);
                            }
                            done(err, conversationId);
                        }
                    });
                }, done);
            }
            else if (done) {
                done(err);
            }
        });
    };
    UniversalBot.prototype.isInConversation = function (address, cb) {
        var _this = this;
        this.lookupUser(address, function (user) {
            var conversationId = address.conversation ? address.conversation.id : null;
            var storageCtx = { userId: user.id, conversationId: conversationId, address: address };
            _this.getStorageData(storageCtx, function (data) {
                var lastAccess;
                if (data && data.conversationData && data.conversationData.hasOwnProperty(consts.Data.SessionState)) {
                    var ss = data.conversationData[consts.Data.SessionState];
                    if (ss && ss.lastAccess) {
                        lastAccess = new Date(ss.lastAccess);
                    }
                }
                cb(null, lastAccess);
            }, cb);
        }, cb);
    };
    UniversalBot.prototype.route = function (storageCtx, message, dialogId, dialogArgs, done) {
        var _this = this;
        var _that = this;
        function saveSessionData(session, cb) {
            if (storageCtx.conversationId) {
                loadedData.userData = utils.clone(session.userData);
                loadedData.conversationData = utils.clone(session.conversationData);
                loadedData.conversationData[consts.Data.SessionState] = session.sessionState;
                _that.saveStorageData(storageCtx, loadedData, cb, cb);
            }
            else if (cb) {
                cb(null);
            }
        }
        var loadedData;
        this.getStorageData(storageCtx, function (data) {
            var session = new ses.Session({
                localizer: _this.settings.localizer,
                autoBatchDelay: _this.settings.autoBatchDelay,
                dialogs: _this.dialogs,
                dialogId: dialogId,
                dialogArgs: dialogArgs,
                onSave: function (cb) {
                    saveSessionData(session, cb);
                },
                onSend: function (messages, cb) {
                    _this.send(messages, function (err, conversationId) {
                        if (!err && conversationId && !storageCtx.conversationId) {
                            storageCtx.conversationId = conversationId;
                            saveSessionData(session, cb);
                        }
                        else if (cb) {
                            cb(err);
                        }
                    });
                }
            });
            session.on('error', function (err) { return _this.emitError(err); });
            var sessionState;
            session.userData = data.userData || {};
            session.conversationData = data.conversationData || {};
            if (session.conversationData.hasOwnProperty(consts.Data.SessionState)) {
                sessionState = session.conversationData[consts.Data.SessionState];
                delete session.conversationData[consts.Data.SessionState];
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
        if (this.mwAnalyze.length > 0) {
            for (var i = 0; i < this.mwAnalyze.length; i++) {
                analyze(this.mwAnalyze[i]);
            }
        }
        else {
            finish();
        }
    };
    UniversalBot.prototype.lookupUser = function (address, done, error) {
        var _this = this;
        this.tryCatch(function () {
            _this.emit('lookupUser', address.user);
            if (_this.settings.lookupUser) {
                _this.settings.lookupUser(address.user, function (err, user) {
                    if (!err) {
                        _this.tryCatch(function () { return done(user || address.user); }, error);
                    }
                    else {
                        _this.emitError(err);
                        if (error) {
                            error(err);
                        }
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
                else {
                    _this.emitError(err);
                    if (error) {
                        error(err);
                    }
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
                else {
                    _this.emitError(err);
                    if (error) {
                        error(err);
                    }
                }
            });
        }, error);
    };
    UniversalBot.prototype.getStorage = function () {
        if (!this.settings.storage) {
            this.settings.storage = new bs.MemoryBotStorage();
            console.warn('UniversalBot using memory based storage. ALL DATA WILL BE LOST ON RESTART. Configure an IBotStorage provider.');
        }
        return this.settings.storage;
    };
    UniversalBot.prototype.tryCatch = function (fn, error) {
        try {
            fn();
        }
        catch (e) {
            this.emitError(e);
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
    UniversalBot.prototype.emitError = function (err) {
        this.emit("error", err instanceof Error ? err : new Error(err.toString()));
    };
    return UniversalBot;
})(events.EventEmitter);
exports.UniversalBot = UniversalBot;
