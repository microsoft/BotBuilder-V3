"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Library_1 = require("./Library");
const Session_1 = require("../Session");
const DefaultLocalizer_1 = require("../DefaultLocalizer");
const BotStorage_1 = require("../storage/BotStorage");
const SessionLogger_1 = require("../SessionLogger");
const RemoteSessionLogger_1 = require("../RemoteSessionLogger");
const consts = require("../consts");
const utils = require("../utils");
const async = require("async");
class UniversalBot extends Library_1.Library {
    constructor(connector, defaultDialog, libraryName) {
        super(libraryName || consts.Library.default);
        this.settings = {
            processLimit: 4,
            persistUserData: true,
            persistConversationData: true
        };
        this.connectors = {};
        this.mwReceive = [];
        this.mwSend = [];
        this.mwSession = [];
        this.localePath('./locale/');
        this.library(Library_1.systemLib);
        if (defaultDialog) {
            if (typeof defaultDialog === 'function' || Array.isArray(defaultDialog)) {
                this.dialog('/', defaultDialog);
            }
            else {
                var settings = defaultDialog;
                for (var name in settings) {
                    if (settings.hasOwnProperty(name)) {
                        this.set(name, settings[name]);
                    }
                }
            }
        }
        if (connector) {
            this.connector(consts.defaultConnector, connector);
        }
    }
    clone(copyTo, newName) {
        var obj = copyTo || new UniversalBot(null, null, newName || this.name);
        for (var name in this.settings) {
            if (this.settings.hasOwnProperty(name)) {
                this.set(name, this.settings[name]);
            }
        }
        for (var channel in this.connectors) {
            obj.connector(channel, this.connectors[channel]);
        }
        obj.mwReceive = this.mwReceive.slice(0);
        obj.mwSession = this.mwSession.slice(0);
        obj.mwSend = this.mwSend.slice(0);
        return super.clone(obj);
    }
    set(name, value) {
        this.settings[name] = value;
        if (value && name === 'localizerSettings') {
            var settings = value;
            if (settings.botLocalePath) {
                this.localePath(settings.botLocalePath);
            }
        }
        return this;
    }
    get(name) {
        return this.settings[name];
    }
    connector(channelId, connector) {
        var c;
        if (connector) {
            this.connectors[channelId || consts.defaultConnector] = c = connector;
            c.onEvent((events, cb) => this.receive(events, cb));
            var asStorage = connector;
            if (!this.settings.storage &&
                typeof asStorage.getData === 'function' &&
                typeof asStorage.saveData === 'function') {
                this.settings.storage = asStorage;
            }
        }
        else if (this.connectors.hasOwnProperty(channelId)) {
            c = this.connectors[channelId];
        }
        else if (this.connectors.hasOwnProperty(consts.defaultConnector)) {
            c = this.connectors[consts.defaultConnector];
        }
        return c;
    }
    use(...args) {
        args.forEach((mw) => {
            var added = 0;
            if (mw.receive) {
                Array.prototype.push.apply(this.mwReceive, Array.isArray(mw.receive) ? mw.receive : [mw.receive]);
                added++;
            }
            if (mw.send) {
                Array.prototype.push.apply(this.mwSend, Array.isArray(mw.send) ? mw.send : [mw.send]);
                added++;
            }
            if (mw.botbuilder) {
                Array.prototype.push.apply(this.mwSession, Array.isArray(mw.botbuilder) ? mw.botbuilder : [mw.botbuilder]);
                added++;
            }
            if (added < 1) {
                console.warn('UniversalBot.use: no compatible middleware hook found to install.');
            }
        });
        return this;
    }
    receive(events, done) {
        var list = Array.isArray(events) ? events : [events];
        async.eachLimit(list, this.settings.processLimit, (message, cb) => {
            message.agent = consts.agent;
            message.type = message.type || consts.messageType;
            this.lookupUser(message.address, (user) => {
                if (user) {
                    message.user = user;
                }
                this.emit('receive', message);
                this.eventMiddleware(message, this.mwReceive, () => {
                    if (this.isMessage(message)) {
                        this.emit('incoming', message);
                        var userId = message.user.id;
                        var conversationId = message.address.conversation ? message.address.conversation.id : null;
                        var storageCtx = {
                            userId: userId,
                            conversationId: conversationId,
                            address: message.address,
                            persistUserData: this.settings.persistUserData,
                            persistConversationData: this.settings.persistConversationData
                        };
                        this.dispatch(storageCtx, message, this.settings.defaultDialogId || '/', this.settings.defaultDialogArgs, cb);
                    }
                    else {
                        this.emit(message.type, message);
                        cb(null);
                    }
                }, cb);
            }, cb);
        }, this.errorLogger(done));
    }
    beginDialog(address, dialogId, dialogArgs, done) {
        this.lookupUser(address, (user) => {
            var msg = {
                type: consts.messageType,
                agent: consts.agent,
                source: address.channelId,
                sourceEvent: {},
                address: utils.clone(address),
                text: '',
                user: user
            };
            this.ensureConversation(msg.address, (adr) => {
                msg.address = adr;
                var conversationId = msg.address.conversation ? msg.address.conversation.id : null;
                var storageCtx = {
                    userId: msg.user.id,
                    conversationId: conversationId,
                    address: msg.address,
                    persistUserData: this.settings.persistUserData,
                    persistConversationData: this.settings.persistConversationData
                };
                this.dispatch(storageCtx, msg, dialogId, dialogArgs, this.errorLogger(done), true);
            }, this.errorLogger(done));
        }, this.errorLogger(done));
    }
    send(messages, done) {
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
        async.eachLimit(list, this.settings.processLimit, (message, cb) => {
            this.ensureConversation(message.address, (adr) => {
                message.address = adr;
                this.emit('send', message);
                this.eventMiddleware(message, this.mwSend, () => {
                    this.emit('outgoing', message);
                    cb(null);
                }, cb);
            }, cb);
        }, this.errorLogger((err) => {
            if (!err && list.length > 0) {
                this.tryCatch(() => {
                    var channelId = list[0].address.channelId;
                    var connector = this.connector(channelId);
                    if (!connector) {
                        throw new Error("Invalid channelId='" + channelId + "'");
                    }
                    connector.send(list, this.errorLogger(done));
                }, this.errorLogger(done));
            }
            else if (done) {
                done(err, null);
            }
        }));
    }
    isInConversation(address, cb) {
        this.lookupUser(address, (user) => {
            var conversationId = address.conversation ? address.conversation.id : null;
            var storageCtx = {
                userId: user.id,
                conversationId: conversationId,
                address: address,
                persistUserData: false,
                persistConversationData: false
            };
            this.getStorageData(storageCtx, (data) => {
                var lastAccess;
                if (data && data.privateConversationData && data.privateConversationData.hasOwnProperty(consts.Data.SessionState)) {
                    var ss = data.privateConversationData[consts.Data.SessionState];
                    if (ss && ss.lastAccess) {
                        lastAccess = new Date(ss.lastAccess);
                    }
                }
                cb(null, lastAccess);
            }, this.errorLogger(cb));
        }, this.errorLogger(cb));
    }
    onDisambiguateRoute(handler) {
        this._onDisambiguateRoute = handler;
    }
    loadSession(address, done) {
        this.loadSessionWithOptionalDispatch(address, true, done);
    }
    loadSessionWithoutDispatching(address, done) {
        this.loadSessionWithOptionalDispatch(address, false, done);
    }
    loadSessionWithOptionalDispatch(address, shouldDispatch, done) {
        const newStack = false;
        this.lookupUser(address, (user) => {
            var msg = {
                type: consts.messageType,
                agent: consts.agent,
                source: address.channelId,
                sourceEvent: {},
                address: utils.clone(address),
                text: '',
                user: user
            };
            this.ensureConversation(msg.address, (adr) => {
                msg.address = adr;
                var conversationId = msg.address.conversation ? msg.address.conversation.id : null;
                var storageCtx = {
                    userId: msg.user.id,
                    conversationId: conversationId,
                    address: msg.address,
                    persistUserData: this.settings.persistUserData,
                    persistConversationData: this.settings.persistConversationData
                };
                this.createSession(storageCtx, msg, this.settings.defaultDialogId || '/', this.settings.defaultDialogArgs, done, newStack, shouldDispatch);
            }, this.errorLogger(done));
        }, this.errorLogger(done));
    }
    dispatch(storageCtx, message, dialogId, dialogArgs, done, newStack = false) {
        this.createSession(storageCtx, message, dialogId, dialogArgs, (err, session) => {
            if (!err) {
                this.emit('routing', session);
                this.routeMessage(session, done);
            }
            else {
                done(err);
            }
        }, newStack);
    }
    createSession(storageCtx, message, dialogId, dialogArgs, done, newStack = false, shouldDispatch = true) {
        var loadedData;
        this.getStorageData(storageCtx, (data) => {
            if (!this.localizer) {
                var defaultLocale = this.settings.localizerSettings ? this.settings.localizerSettings.defaultLocale : null;
                this.localizer = new DefaultLocalizer_1.DefaultLocalizer(this, defaultLocale);
            }
            let logger;
            if (message.source == consts.emulatorChannel) {
                logger = new RemoteSessionLogger_1.RemoteSessionLogger(this.connector(consts.emulatorChannel), message.address, message.address);
            }
            else if (data.privateConversationData && data.privateConversationData.hasOwnProperty(consts.Data.DebugAddress)) {
                var debugAddress = data.privateConversationData[consts.Data.DebugAddress];
                logger = new RemoteSessionLogger_1.RemoteSessionLogger(this.connector(consts.emulatorChannel), debugAddress, message.address);
            }
            else {
                logger = new SessionLogger_1.SessionLogger();
            }
            var session = new Session_1.Session({
                localizer: this.localizer,
                logger: logger,
                autoBatchDelay: this.settings.autoBatchDelay,
                connector: this.connector(message.address.channelId),
                library: this,
                middleware: this.mwSession,
                dialogId: dialogId,
                dialogArgs: dialogArgs,
                dialogErrorMessage: this.settings.dialogErrorMessage,
                onSave: (cb) => {
                    var finish = this.errorLogger(cb);
                    loadedData.userData = utils.clone(session.userData);
                    loadedData.conversationData = utils.clone(session.conversationData);
                    loadedData.privateConversationData = utils.clone(session.privateConversationData);
                    loadedData.privateConversationData[consts.Data.SessionState] = session.sessionState;
                    this.saveStorageData(storageCtx, loadedData, finish, finish);
                },
                onSend: (messages, cb) => {
                    this.send(messages, cb);
                }
            });
            session.on('error', (err) => this.emitError(err));
            var sessionState;
            session.userData = data.userData || {};
            session.conversationData = data.conversationData || {};
            session.privateConversationData = data.privateConversationData || {};
            if (session.privateConversationData.hasOwnProperty(consts.Data.SessionState)) {
                sessionState = newStack ? null : session.privateConversationData[consts.Data.SessionState];
                delete session.privateConversationData[consts.Data.SessionState];
            }
            loadedData = data;
            if (shouldDispatch) {
                session.dispatch(sessionState, message, () => done(null, session));
            }
            else {
                done(null, session);
            }
        }, done);
    }
    routeMessage(session, done) {
        var entry = 'UniversalBot("' + this.name + '") routing ';
        if (session.message.text) {
            entry += '"' + session.message.text + '"';
        }
        else if (session.message.attachments && session.message.attachments.length > 0) {
            entry += session.message.attachments.length + ' attachment(s)';
        }
        else {
            entry += '<null>';
        }
        entry += ' from "' + session.message.source + '"';
        session.logger.log(null, entry);
        var context = session.toRecognizeContext();
        this.recognize(context, (err, topIntent) => {
            if (session.message.entities) {
                session.message.entities.forEach((entity) => {
                    if (entity.type === consts.intentEntityType &&
                        entity.score > topIntent.score) {
                        topIntent = entity;
                    }
                });
            }
            context.intent = topIntent;
            context.libraryName = this.name;
            var results = Library_1.Library.addRouteResult({ score: 0.0, libraryName: this.name });
            async.each(this.libraryList(), (lib, cb) => {
                lib.findRoutes(context, (err, routes) => {
                    if (!err && routes) {
                        routes.forEach((r) => results = Library_1.Library.addRouteResult(r, results));
                    }
                    cb(err);
                });
            }, (err) => {
                if (!err) {
                    var disambiguateRoute = (session, routes) => {
                        var route = Library_1.Library.bestRouteResult(results, session.dialogStack(), this.name);
                        if (route) {
                            this.library(route.libraryName).selectRoute(session, route);
                        }
                        else {
                            session.routeToActiveDialog();
                        }
                    };
                    if (this._onDisambiguateRoute) {
                        disambiguateRoute = this._onDisambiguateRoute;
                    }
                    disambiguateRoute(session, results);
                    done(null);
                }
                else {
                    session.error(err);
                    done(err);
                }
            });
        });
    }
    eventMiddleware(event, middleware, done, error) {
        var i = -1;
        var _that = this;
        function next() {
            if (++i < middleware.length) {
                _that.tryCatch(() => {
                    middleware[i](event, next);
                }, () => next());
            }
            else {
                _that.tryCatch(() => done(), error);
            }
        }
        next();
    }
    isMessage(message) {
        return (message && message.type && message.type.toLowerCase() == consts.messageType);
    }
    ensureConversation(address, done, error) {
        this.tryCatch(() => {
            if (!address.conversation) {
                var connector = this.connector(address.channelId);
                if (!connector) {
                    throw new Error("Invalid channelId='" + address.channelId + "'");
                }
                connector.startConversation(address, (err, adr) => {
                    if (!err) {
                        this.tryCatch(() => done(adr), error);
                    }
                    else if (error) {
                        error(err);
                    }
                });
            }
            else {
                this.tryCatch(() => done(address), error);
            }
        }, error);
    }
    lookupUser(address, done, error) {
        this.tryCatch(() => {
            this.emit('lookupUser', address);
            if (this.settings.lookupUser) {
                this.settings.lookupUser(address, (err, user) => {
                    if (!err) {
                        this.tryCatch(() => done(user || address.user), error);
                    }
                    else if (error) {
                        error(err);
                    }
                });
            }
            else {
                this.tryCatch(() => done(address.user), error);
            }
        }, error);
    }
    getStorageData(storageCtx, done, error) {
        this.tryCatch(() => {
            this.emit('getStorageData', storageCtx);
            var storage = this.getStorage();
            storage.getData(storageCtx, (err, data) => {
                if (!err) {
                    this.tryCatch(() => done(data || {}), error);
                }
                else if (error) {
                    error(err);
                }
            });
        }, error);
    }
    saveStorageData(storageCtx, data, done, error) {
        this.tryCatch(() => {
            this.emit('saveStorageData', storageCtx);
            var storage = this.getStorage();
            storage.saveData(storageCtx, data, (err) => {
                if (!err) {
                    if (done) {
                        this.tryCatch(() => done(), error);
                    }
                }
                else if (error) {
                    error(err);
                }
            });
        }, error);
    }
    getStorage() {
        if (!this.settings.storage) {
            this.settings.storage = new BotStorage_1.MemoryBotStorage();
        }
        return this.settings.storage;
    }
    tryCatch(fn, error) {
        try {
            fn();
        }
        catch (e) {
            try {
                if (error) {
                    error(e, null);
                }
            }
            catch (e2) {
                this.emitError(e2);
            }
        }
    }
    errorLogger(done) {
        return (err, result) => {
            if (err) {
                this.emitError(err);
            }
            if (done) {
                done(err, result);
                done = null;
            }
        };
    }
    emitError(err) {
        var m = err.toString();
        var e = err instanceof Error ? err : new Error(m);
        if (this.listenerCount('error') > 0) {
            this.emit('error', e);
        }
        else {
            console.error(e.stack);
        }
    }
}
exports.UniversalBot = UniversalBot;
