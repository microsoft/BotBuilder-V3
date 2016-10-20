"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var dl = require('./Library');
var ses = require('../CallSession');
var bs = require('../storage/BotStorage');
var consts = require('../consts');
var utils = require('../utils');
var events = require('events');
var UniversalCallBot = (function (_super) {
    __extends(UniversalCallBot, _super);
    function UniversalCallBot(connector, settings) {
        var _this = this;
        _super.call(this);
        this.connector = connector;
        this.settings = {
            processLimit: 4,
            persistUserData: true,
            persistConversationData: false
        };
        this.lib = new dl.Library(consts.Library.default);
        this.mwReceive = [];
        this.mwSend = [];
        this.mwSession = [];
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
        this.lib.library(dl.systemLib);
        this.connector.onEvent(function (event, cb) { return _this.receive(event, cb); });
    }
    UniversalCallBot.prototype.set = function (name, value) {
        this.settings[name] = value;
        return this;
    };
    UniversalCallBot.prototype.get = function (name) {
        return this.settings[name];
    };
    UniversalCallBot.prototype.dialog = function (id, dialog) {
        return this.lib.dialog(id, dialog);
    };
    UniversalCallBot.prototype.library = function (lib) {
        return this.lib.library(lib);
    };
    UniversalCallBot.prototype.use = function () {
        var _this = this;
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i - 0] = arguments[_i];
        }
        args.forEach(function (mw) {
            var added = 0;
            if (mw.receive) {
                Array.prototype.push.apply(_this.mwReceive, Array.isArray(mw.receive) ? mw.receive : [mw.receive]);
                added++;
            }
            if (mw.send) {
                Array.prototype.push.apply(_this.mwSend, Array.isArray(mw.send) ? mw.send : [mw.send]);
                added++;
            }
            if (mw.botbuilder) {
                Array.prototype.push.apply(_this.mwSession, Array.isArray(mw.botbuilder) ? mw.botbuilder : [mw.botbuilder]);
                added++;
            }
            if (added < 1) {
                console.warn('UniversalBot.use: no compatible middleware hook found to install.');
            }
        });
        return this;
    };
    UniversalCallBot.prototype.receive = function (event, done) {
        var _this = this;
        var logger = this.errorLogger(done);
        this.lookupUser(event.address, function (user) {
            if (user) {
                event.user = user;
            }
            _this.emit('receive', event);
            _this.eventMiddleware(event, _this.mwReceive, function () {
                _this.emit('incoming', event);
                var userId = event.user.id;
                var storageCtx = {
                    userId: userId,
                    conversationId: event.address.conversation.id,
                    address: event.address,
                    persistUserData: _this.settings.persistUserData,
                    persistConversationData: _this.settings.persistConversationData
                };
                _this.route(storageCtx, event, _this.settings.defaultDialogId || '/', _this.settings.defaultDialogArgs, logger);
            }, logger);
        }, logger);
    };
    UniversalCallBot.prototype.send = function (event, done) {
        var _this = this;
        var logger = this.errorLogger(done);
        var evt = event.toEvent ? event.toEvent() : event;
        this.emit('send', evt);
        this.eventMiddleware(evt, this.mwSend, function () {
            _this.emit('outgoing', evt);
            _this.connector.send(evt, logger);
        }, logger);
    };
    UniversalCallBot.prototype.route = function (storageCtx, event, dialogId, dialogArgs, done) {
        var _this = this;
        var loadedData;
        this.getStorageData(storageCtx, function (data) {
            var session = new ses.CallSession({
                localizer: _this.settings.localizer,
                autoBatchDelay: _this.settings.autoBatchDelay,
                library: _this.lib,
                middleware: _this.mwSession,
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
            session.dispatch(sessionState, event);
            done(null);
        }, done);
    };
    UniversalCallBot.prototype.eventMiddleware = function (event, middleware, done, error) {
        var i = -1;
        var _that = this;
        function next() {
            if (++i < middleware.length) {
                _that.tryCatch(function () {
                    middleware[i](event, next);
                }, function () { return next(); });
            }
            else {
                _that.tryCatch(function () { return done(); }, error);
            }
        }
        next();
    };
    UniversalCallBot.prototype.lookupUser = function (address, done, error) {
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
        var msg = err.toString();
        this.emit("error", err instanceof Error ? err : new Error(msg));
    };
    return UniversalCallBot;
}(events.EventEmitter));
exports.UniversalCallBot = UniversalCallBot;
