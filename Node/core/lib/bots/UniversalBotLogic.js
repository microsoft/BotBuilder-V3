"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const botbuilder_core_1 = require("botbuilder-core");
const Library_1 = require("./Library");
const Session_1 = require("../Session");
const DefaultLocalizer_1 = require("../DefaultLocalizer");
const SessionLogger_1 = require("../SessionLogger");
const consts = require("../consts");
const logger = require("../logger");
const utils = require("../utils");
const async = require("async");
class UniversalBotLogic extends Library_1.Library {
    constructor(settings, libraryName) {
        super(libraryName || consts.Library.default);
        this.mwReceive = [];
        this.mwSend = [];
        this.mwSession = [];
        this.settings = Object.assign({
            processLimit: 4,
            routingTimeout: 10000
        }, settings);
        this.localePath('./locale/');
        this.library(Library_1.systemLib);
    }
    clone(copyTo, newName) {
        var obj = copyTo || new UniversalBotLogic(this.settings, newName || this.name);
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
    receive(context) {
        try {
            return Promise.resolve(UniversalBotLogic.toEvent(context.request || {})).then((message) => {
                message.agent = consts.agent;
                message.type = message.type || consts.messageType;
                return this.lookupUser(message.address).then((user) => {
                    message.user = user;
                    this.emit('receive', message);
                    return this.eventMiddleware(message, this.mwReceive).then(() => {
                        if (this.isMessage(message)) {
                            this.emit('incoming', message);
                            return this.getStorageData(context).then((data) => {
                                return this.dispatch(context, data, message);
                            });
                        }
                        else {
                            return false;
                        }
                    });
                });
            });
        }
        catch (err) {
            return Promise.reject(err);
        }
    }
    send(context, messages) {
        return new Promise((resolve, reject) => {
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
                this.emit('send', message);
                this.eventMiddleware(message, this.mwSend).then(() => {
                    this.emit('outgoing', message);
                    cb(null);
                }, (err) => cb(err));
            }, (err) => {
                try {
                    const activities = list.map((event) => UniversalBotLogic.toActivity(event));
                    botbuilder_core_1.BotContext.prototype.sendActivity.apply(context, activities).then((results) => {
                        const addresses = (results || []).map((body, index) => {
                            if (index < list.length) {
                                const clone = Object.assign({}, list[index].address);
                                if (body.id) {
                                    clone.id = body.id;
                                }
                                return clone;
                            }
                        });
                        resolve(addresses);
                    }, (err) => reject(err));
                }
                catch (err) {
                    reject(err);
                }
            });
        });
    }
    onDisambiguateRoute(handler) {
        this._onDisambiguateRoute = handler;
    }
    static toEvent(activity) {
        const msg = Object.assign({}, activity);
        utils.moveFieldsTo(msg, msg, {
            'locale': 'textLocale',
            'channelData': 'sourceEvent'
        });
        msg.text = msg.text || '';
        msg.attachments = msg.attachments || [];
        msg.entities = msg.entities || [];
        const address = {};
        utils.moveFieldsTo(msg, address, {
            'id': 'id',
            'channelId': 'channelId',
            'from': 'user',
            'conversation': 'conversation',
            'recipient': 'bot',
            'serviceUrl': 'serviceUrl'
        });
        msg.address = address;
        msg.source = address.channelId;
        if (msg.source == 'facebook' && msg.sourceEvent && msg.sourceEvent.message && msg.sourceEvent.message.quick_reply) {
            msg.text = msg.sourceEvent.message.quick_reply.payload;
        }
        return msg;
    }
    static toActivity(event) {
        const activity = Object.assign({}, event);
        var address = event.address;
        activity.channelId = address.channelId;
        activity.serviceUrl = address.serviceUrl;
        activity.from = address.bot;
        activity.recipient = address.user;
        activity.conversation = address.conversation;
        if (address.id) {
            activity.replyToId = address.id;
        }
        delete activity['address'];
        if (activity.attachments) {
            const attachments = [];
            for (var i = 0; i < activity.attachments.length; i++) {
                var a = activity.attachments[i];
                switch (a.contentType) {
                    case 'application/vnd.microsoft.keyboard':
                        if (activity.channelId == 'facebook') {
                            activity.channelData = { quick_replies: [] };
                            a.content.buttons.forEach((action) => {
                                switch (action.type) {
                                    case 'imBack':
                                    case 'postBack':
                                        activity.channelData.quick_replies.push({
                                            content_type: 'text',
                                            title: action.title,
                                            payload: action.value
                                        });
                                        break;
                                    default:
                                        logger.warn(address, "Invalid keyboard '%s' button sent to facebook.", action.type);
                                        break;
                                }
                            });
                        }
                        else {
                            a.contentType = 'application/vnd.microsoft.card.hero';
                            attachments.push(a);
                        }
                        break;
                    default:
                        attachments.push(a);
                        break;
                }
            }
            activity.attachments = attachments;
        }
        return activity;
    }
    dispatch(context, data, message) {
        return this.createSession(context, data, message).then((session) => {
            this.emit('routing', session);
            return new Promise((resolve, reject) => {
                let listening = true;
                context.onSendActivity((context, activities, next) => {
                    if (!listening) {
                        return next();
                    }
                    return next().then((results) => {
                        let ended = false;
                        activities.forEach((a) => {
                            if (a.type === 'endOfConversation' || a.inputHint === 'expectingInput') {
                                ended = true;
                            }
                        });
                        if (ended) {
                            clearTimeout(hTimeout);
                            listening = false;
                            resolve(true);
                        }
                        return results;
                    }, (err) => {
                        reject(err);
                        throw err;
                    });
                });
                const hTimeout = setTimeout(() => {
                    listening = false;
                    resolve(true);
                });
                this.routeMessage(session).then((routed) => {
                    listening = routed;
                    if (!listening) {
                        clearTimeout(hTimeout);
                        resolve(false);
                    }
                });
            });
        });
    }
    createSession(context, data, message, newStack = false, shouldDispatch = true) {
        return new Promise((resolve, reject) => {
            if (!this.localizer) {
                var defaultLocale = this.settings.localizerSettings ? this.settings.localizerSettings.defaultLocale : null;
                this.localizer = new DefaultLocalizer_1.DefaultLocalizer(this, defaultLocale);
            }
            let logger = new SessionLogger_1.SessionLogger();
            var session = new Session_1.Session({
                localizer: this.localizer,
                logger: logger,
                autoBatchDelay: this.settings.autoBatchDelay,
                connector: undefined,
                library: this,
                middleware: this.mwSession,
                dialogId: '/',
                dialogErrorMessage: this.settings.dialogErrorMessage,
                onSave: (cb) => {
                    if (data.userData) {
                        syncState(session.userData, data.userData);
                    }
                    if (data.conversationData) {
                        syncState(session.conversationData, data.conversationData);
                    }
                    if (data.privateConversationData) {
                        syncState(session.privateConversationData, data.privateConversationData);
                        data.privateConversationData[consts.Data.SessionState] = session.sessionState;
                    }
                },
                onSend: (messages, cb) => {
                    this.send(context, messages).then((results) => cb(null, results), (err) => cb(err));
                }
            });
            session.on('error', (err) => this.emitError(err));
            let sessionState = null;
            session.userData = Object.assign({}, data.userData);
            session.conversationData = Object.assign({}, data.conversationData);
            session.privateConversationData = Object.assign({}, data.privateConversationData);
            if (session.privateConversationData.hasOwnProperty(consts.Data.SessionState)) {
                sessionState = newStack ? null : session.privateConversationData[consts.Data.SessionState];
                delete session.privateConversationData[consts.Data.SessionState];
            }
            if (shouldDispatch) {
                session.dispatch(sessionState, message, () => resolve(session));
            }
            else {
                resolve(session);
            }
        });
    }
    routeMessage(session) {
        return new Promise((resolve, reject) => {
            var entry = 'UniversalBotLogic("' + this.name + '") routing ';
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
                        let disambiguateRoute = (session, routes) => {
                            const route = Library_1.Library.bestRouteResult(results, session.dialogStack(), this.name);
                            if (route) {
                                this.library(route.libraryName).selectRoute(session, route);
                            }
                            else {
                                if (session.dialogStack().length > 0) {
                                    session.routeToActiveDialog();
                                    resolve(true);
                                }
                                else {
                                    resolve(false);
                                }
                            }
                        };
                        if (this._onDisambiguateRoute) {
                            this._onDisambiguateRoute(session, results);
                            resolve(true);
                        }
                        else {
                            disambiguateRoute(session, results);
                        }
                    }
                    else {
                        session.error(err);
                        reject(err);
                    }
                });
            });
        });
    }
    eventMiddleware(event, middleware) {
        return new Promise((resolve, reject) => {
            function next(i) {
                if (i < middleware.length) {
                    try {
                        middleware[i](event, () => next(i + 1));
                    }
                    catch (err) {
                        reject(err);
                    }
                }
                else {
                    resolve();
                }
            }
            next(0);
        });
    }
    isMessage(message) {
        return (message && message.type && message.type.toLowerCase() == consts.messageType);
    }
    lookupUser(address) {
        return new Promise((resolve, reject) => {
            this.emit('lookupUser', address);
            if (this.settings.lookupUser) {
                this.settings.lookupUser(address, (err, user) => {
                    if (!err) {
                        resolve(user || address.user);
                    }
                    else {
                        reject(err);
                    }
                });
            }
            else {
                resolve(address.user);
            }
        });
    }
    getStorageData(context) {
        const data = {};
        const promises = [];
        promises.push(this.settings.conversationState.read(context).then((state) => { data.conversationData = state; }));
        if (this.settings.userState) {
            promises.push(this.settings.userState.read(context).then((state) => { data.userData = state; }));
        }
        if (this.settings.privateConversationState) {
            promises.push(this.settings.privateConversationState.read(context).then((state) => { data.privateConversationData = state; }));
        }
        return Promise.all(promises).then(() => data);
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
exports.UniversalBotLogic = UniversalBotLogic;
function syncState(from, to) {
    if (!from) {
        from = {};
    }
    for (const key in from) {
        if (from.hasOwnProperty(key)) {
            to[key] = from[key];
        }
    }
    for (const key in to) {
        if (!from.hasOwnProperty(key)) {
            delete to[key];
        }
    }
}
