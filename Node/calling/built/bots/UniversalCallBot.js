var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var da = require('../dialogs/DialogAction');
var dc = require('../dialogs/DialogCollection');
var sd = require('../dialogs/SimpleDialog');
var ses = require('../CallSession');
var bs = require('../storage/BotStorage');
var consts = require('../consts');
var utils = require('../utils');
var events = require('events');
var UniversalCallBot = (function (_super) {
    __extends(UniversalCallBot, _super);
    function UniversalCallBot(connector, settings) {
        _super.call(this);
        this.connector = connector;
        this.settings = {
            processLimit: 4,
            persistUserData: true,
            persistConversationData: false
        };
        this.dialogs = new dc.DialogCollection();
        this.mwReceive = [];
        this.mwAnalyze = [];
        this.mwSend = [];
        if (settings) {
            for (var name in settings) {
                this.set(name, settings[name]);
            }
        }
        var asStorage = connector;
        if (!this.settings.storage &&
            typeof asStorage.getData === 'function' &&
            typeof asStorage.saveData === 'function') {
            this.settings.storage = asStorage;
        }
    }
    UniversalCallBot.prototype.set = function (name, value) {
        this.settings[name] = value;
        return this;
    };
    UniversalCallBot.prototype.get = function (name) {
        return this.settings[name];
    };
    UniversalCallBot.prototype.dialog = function (id, dialog) {
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
    UniversalCallBot.prototype.use = function (middleware) {
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
    UniversalCallBot.prototype.receive = function (message, done) {
        var _this = this;
        var logger = this.errorLogger(done);
        this.lookupUser(message.address, function (user) {
            if (user) {
                message.user = user;
            }
            _this.emit('receive', message);
            _this.messageMiddleware(message, _this.mwReceive, function () {
                _this.emit('analyze', message);
                _this.analyzeMiddleware(message, function () {
                    _this.emit('incoming', message);
                    var userId = message.user.identity;
                    var conversationId = message.address.id;
                    var storageCtx = {
                        userId: userId,
                        conversationId: conversationId,
                        address: message.address,
                        persistUserData: _this.settings.persistUserData,
                        persistConversationData: _this.settings.persistConversationData
                    };
                    _this.route(storageCtx, message, _this.settings.defaultDialogId || '/', _this.settings.defaultDialogArgs, logger);
                }, logger);
            }, logger);
        }, logger);
    };
    UniversalCallBot.prototype.send = function (message, done) {
        var _this = this;
        var logger = this.errorLogger(done);
        var msg = message.toMessage ? message.toMessage() : message;
        this.emit('send', msg);
        this.messageMiddleware(msg, this.mwSend, function () {
            _this.emit('outgoing', msg);
            _this.connector.send(msg, logger);
        }, logger);
    };
    UniversalCallBot.prototype.route = function (storageCtx, message, dialogId, dialogArgs, done) {
        var _this = this;
        var loadedData;
        this.getStorageData(storageCtx, function (data) {
            var session = new ses.CallSession({
                localizer: _this.settings.localizer,
                autoBatchDelay: _this.settings.autoBatchDelay,
                dialogs: _this.dialogs,
                dialogId: dialogId,
                dialogArgs: dialogArgs,
                dialogErrorMessage: _this.settings.dialogErrorMessage,
                promptDefaults: _this.settings.promptDefaults || {},
                recognizeDefaults: _this.settings.recognizeDefaults || {},
                recordDefaults: _this.settings.recordDefaults || {},
                onSave: function (cb) {
                    var finish = _this.errorLogger(cb);
                    loadedData.userData = utils.clone(session.userData);
                    loadedData.conversationData = utils.clone(session.conversationData);
                    loadedData.privateConversationData = utils.clone(session.privateConversationData);
                    loadedData.privateConversationData[consts.Data.SessionState] = session.sessionState;
                    _this.saveStorageData(storageCtx, loadedData, finish, finish);
                },
                onSend: function (workflow, cb) {
                    _this.send(workflow, cb);
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
    UniversalCallBot.prototype.messageMiddleware = function (message, middleware, done, error) {
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
    UniversalCallBot.prototype.analyzeMiddleware = function (message, done, error) {
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
    UniversalCallBot.prototype.lookupUser = function (address, done, error) {
        var _this = this;
        this.tryCatch(function () {
            _this.emit('lookupUser', address);
            var originator;
            address.participants.forEach(function (participant) {
                if (participant.originator) {
                    originator = participant;
                }
            });
            if (_this.settings.lookupUser) {
                _this.settings.lookupUser(address, function (err, user) {
                    if (!err) {
                        _this.tryCatch(function () { return done(user || originator); }, error);
                    }
                    else if (error) {
                        error(err);
                    }
                });
            }
            else {
                _this.tryCatch(function () { return done(originator); }, error);
            }
        }, error);
    };
    UniversalCallBot.prototype.getStorageData = function (storageCtx, done, error) {
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
    UniversalCallBot.prototype.saveStorageData = function (storageCtx, data, done, error) {
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
    UniversalCallBot.prototype.getStorage = function () {
        if (!this.settings.storage) {
            this.settings.storage = new bs.MemoryBotStorage();
        }
        return this.settings.storage;
    };
    UniversalCallBot.prototype.tryCatch = function (fn, error) {
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
    UniversalCallBot.prototype.errorLogger = function (done) {
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
    UniversalCallBot.prototype.emitError = function (err) {
        this.emit("error", err instanceof Error ? err : new Error(err.toString()));
    };
    return UniversalCallBot;
})(events.EventEmitter);
exports.UniversalCallBot = UniversalCallBot;
