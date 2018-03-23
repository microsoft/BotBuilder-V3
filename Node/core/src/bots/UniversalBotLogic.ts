//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// Microsoft Bot Framework: http://botframework.com
//
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
//
// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

import { BotContext, Activity, Attachment, ResourceResponse } from 'botbuilder-core';
import { BotState } from 'botbuilder-core-extensions';
import { IUniversalBotSettings, IMiddlewareMap, IEventMiddleware, ILookupUser, IDisambiguateRouteHandler } from './UniversalBot';
import { IChatConnectorAddress } from './ChatConnector';
import { Library, systemLib, IRouteResult } from './Library';
import { IDialogWaterfallStep } from '../dialogs/WaterfallDialog';
import { Session, ISessionMiddleware, IConnector } from '../Session';
import { DefaultLocalizer } from '../DefaultLocalizer';
import { IBotStorage, IBotStorageContext, IBotStorageData, MemoryBotStorage } from '../storage/BotStorage';
import { IIntentRecognizerResult } from '../dialogs/IntentRecognizer';
import { SessionLogger } from '../SessionLogger';
import { RemoteSessionLogger } from '../RemoteSessionLogger';
import * as consts from '../consts';
import * as logger from '../logger';
import * as utils from '../utils';
import * as async from 'async';

export interface IUniversalBotLogicSettings {
    conversationState: BotState;
    userState?: BotState;
    privateConversationState?: BotState;
    localizerSettings?: IDefaultLocalizerSettings;    
    lookupUser?: ILookupUser;
    processLimit?: number;
    autoBatchDelay?: number;
    routingTimeout?: number;
    dialogErrorMessage?: string|string[]|IMessage|IIsMessage;
}

export class UniversalBotLogic extends Library {
    private readonly settings: IUniversalBotLogicSettings;
    private mwReceive = <IEventMiddleware[]>[];
    private mwSend = <IEventMiddleware[]>[];
    private mwSession = <ISessionMiddleware[]>[]; 
    private localizer: DefaultLocalizer;
    private _onDisambiguateRoute: IDisambiguateRouteHandler;
    
    constructor(settings: IUniversalBotLogicSettings, libraryName?: string) {
        super(libraryName || consts.Library.default);
        this.settings = Object.assign({ 
            processLimit: 4,
            routingTimeout: 10000 
        }, settings);
        this.localePath('./locale/');
        this.library(systemLib);
    }

    public clone(copyTo?: UniversalBotLogic, newName?: string): UniversalBotLogic {
        var obj = copyTo || new UniversalBotLogic(this.settings, newName || this.name);
        obj.mwReceive = this.mwReceive.slice(0);
        obj.mwSession = this.mwSession.slice(0);
        obj.mwSend = this.mwSend.slice(0);
        // obj.localizer is automatically created based on settings
        return super.clone(obj) as UniversalBotLogic;
    }
    
    //-------------------------------------------------------------------------
    // Settings
    //-------------------------------------------------------------------------
    
    public set(name: string, value: any): this {
        (<any>this.settings)[name] = value;
        if (value && name === 'localizerSettings') {
            var settings = <IDefaultLocalizerSettings>value;
            if (settings.botLocalePath) {
                this.localePath(settings.botLocalePath);
            }
        }
        return this;
    }
    
    public get(name: string): any {
        return (<any>this.settings)[name];
    }
    
    //-------------------------------------------------------------------------
    // v3 style Middleware
    //-------------------------------------------------------------------------
    
    public use(...args: IMiddlewareMap[]): this {
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
                console.warn('UniversalBot.use: no compatible middleware hook found to install.')
            }
        });
        return this;    
    }
    
    //-------------------------------------------------------------------------
    // Messaging
    //-------------------------------------------------------------------------

    public receive(context: BotContext): Promise<boolean> {
        try {
            return Promise.resolve(UniversalBotLogic.toEvent(context.request || {} as Activity)).then((message) => {
                message.agent = consts.agent;
                message.type = message.type || consts.messageType;
                return this.lookupUser(message.address).then((user) => {
                    message.user = user;
                    this.emit('receive', message);
                    return this.eventMiddleware(message, this.mwReceive).then(() => {
                        if (this.isMessage(message)) {
                            this.emit('incoming', message);
                            return this.getStorageData(context).then((data) => {
                                return this.dispatch(context, data, message as IMessage);
                            });
                        } else {
                            return false;
                        }
                    });
    
                });
            });
        } catch (err) {
            return Promise.reject(err);
        }
    }

    public send(context: BotContext, messages: IIsMessage|IMessage|IMessage[]): Promise<IAddress[]> {
        return new Promise((resolve, reject) => {
            var list: IMessage[];
            if (Array.isArray(messages)) {
                list = messages;
            } else if ((<IIsMessage>messages).toMessage) {
                list = [(<IIsMessage>messages).toMessage()];
            } else {
                list = [<IMessage>messages];
            }
            async.eachLimit(list, this.settings.processLimit, (message, cb) => {
                this.emit('send', message);
                this.eventMiddleware(message, this.mwSend).then(() => {
                    this.emit('outgoing', message);
                    cb(null);
                }, (err) => cb(err));
            }, (err) => {
                try {
                    // Map to activities
                    const activities = list.map((event) => UniversalBotLogic.toActivity(event));
                    BotContext.prototype.sendActivity.apply(context, activities).then((results: ResourceResponse[]) => {
                        // Map results to address objects
                        const addresses = (results || []).map((body, index) => {
                            if (index < list.length) {
                                const clone = Object.assign({}, list[index].address) as IChatConnectorAddress;
                                if (body.id) { clone.id = body.id }
                                return clone;
                            }
                        });
                        resolve(addresses);
                    }, (err: Error) => reject(err));
                } catch (err) {
                    reject(err);
                }
            });
        });
    }
    

    /** Lets a developer override the bots default route disambiguation logic. */
    public onDisambiguateRoute(handler: IDisambiguateRouteHandler): void {
        this._onDisambiguateRoute = handler;
    }

    //-------------------------------------------------------------------------
    // Activity <--> IEvent Mapping
    //-------------------------------------------------------------------------

    static toEvent(activity: Activity): IEvent {
        // Patch locale and channelData
        const msg = Object.assign({}, activity as any) as IMessage;
        utils.moveFieldsTo(msg, msg, {
            'locale': 'textLocale',
            'channelData': 'sourceEvent'
        });

        // Ensure basic fields are there
        msg.text = msg.text || '';
        msg.attachments = msg.attachments || [];
        msg.entities = msg.entities || [];

        // Break out address fields
        const address = {} as IAddress;
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

        // Check for facebook quick replies
        if (msg.source == 'facebook' && msg.sourceEvent && msg.sourceEvent.message && msg.sourceEvent.message.quick_reply) {
            msg.text = msg.sourceEvent.message.quick_reply.payload;
        }
        return msg;        
    }

    static toActivity(event: IEvent): Partial<Activity> {
        const activity = Object.assign({}, event as any) as Partial<Activity>;

        // Apply address fields
        var address = event.address as IChatConnectorAddress;
        activity.channelId = address.channelId;
        activity.serviceUrl = address.serviceUrl;
        activity.from = address.bot as any;
        activity.recipient = address.user as any;
        activity.conversation = address.conversation as any;
        if (address.id) { activity.replyToId = address.id }
        delete (activity as any)['address'];

        // Convert attachments
        if (activity.attachments) {
            const attachments: Attachment[] = [];
            for (var i = 0; i < activity.attachments.length; i++) {
                var a = activity.attachments[i];
                switch (a.contentType) {
                    case 'application/vnd.microsoft.keyboard':
                        if (activity.channelId == 'facebook') {
                            // Convert buttons
                            activity.channelData = { quick_replies: [] };
                            (<IKeyboard>a.content).buttons.forEach((action) => {
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
                        } else {
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

    //-------------------------------------------------------------------------
    // Helpers
    //-------------------------------------------------------------------------
    
    private dispatch(context: BotContext, data: IBotStorageData, message: IMessage): Promise<boolean> {
        // Create session object
        return this.createSession(context, data, message).then((session) => {
            // Route request
            this.emit('routing', session);
            return new Promise<boolean>((resolve, reject) => {
                // Listen for outgoing activities
                let listening = true;
                context.onSendActivity((context, activities, next) => {
                    if (!listening) {
                        return next();
                    }

                    // Send activities and check for indication that the turn is over                        
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

                // Start routing timeout
                const hTimeout = setTimeout(() => {
                    listening = false;
                    resolve(true);
                });

                
                // Route message
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

    private createSession(context: BotContext, data: IBotStorageData, message: IMessage, newStack = false, shouldDispatch = true): Promise<Session> {
        return new Promise((resolve, reject) => {
            // Create localizer on first access
            if (!this.localizer) {
                var defaultLocale = this.settings.localizerSettings ? this.settings.localizerSettings.defaultLocale : null;
                this.localizer = new DefaultLocalizer(this, defaultLocale);
            }

            // Create logger
            let logger = new SessionLogger();

            // Initialize session
            var session = new Session({
                localizer: this.localizer,
                logger: logger,
                autoBatchDelay: this.settings.autoBatchDelay,
                connector: undefined,
                library: this,
                middleware: this.mwSession,
                dialogId: '/',
                dialogErrorMessage: this.settings.dialogErrorMessage,
                onSave: (cb) => {
                    // Sync sessions state changes 
                    if (data.userData) { syncState(session.userData, data.userData) }
                    if (data.conversationData) { syncState(session.conversationData, data.conversationData) }
                    if (data.privateConversationData) { 
                        syncState(session.privateConversationData, data.privateConversationData);
                        data.privateConversationData[consts.Data.SessionState] = session.sessionState;
                    }
                },
                onSend: (messages, cb) => {
                    this.send(context, messages).then((results) => cb(null, results), (err) => cb(err));
                }
            });
            session.on('error', (err: Error) => this.emitError(err));
            
            // Initialize session data
            let sessionState: any = null;
            session.userData = Object.assign({}, data.userData);
            session.conversationData = Object.assign({}, data.conversationData);
            session.privateConversationData = Object.assign({}, data.privateConversationData);
            if (session.privateConversationData.hasOwnProperty(consts.Data.SessionState)) {
                sessionState = newStack ? null : session.privateConversationData[consts.Data.SessionState];
                delete session.privateConversationData[consts.Data.SessionState];
            }
            
            if (shouldDispatch) {
                session.dispatch(sessionState, message, () => resolve(session));
            } else {
                resolve(session);
            }
        });
    }

    private routeMessage(session: Session): Promise<boolean> {
        return new Promise((resolve, reject) => {
            // Log start of routing
            var entry = 'UniversalBotLogic("' + this.name + '") routing ';
            if (session.message.text) {
                entry += '"' + session.message.text + '"';
            } else if (session.message.attachments && session.message.attachments.length > 0) {
                entry += session.message.attachments.length + ' attachment(s)';
            } else {
                entry += '<null>';
            }
            entry += ' from "' + session.message.source + '"';
            session.logger.log(null, entry);
            
            // Run the root libraries recognizers
            var context = session.toRecognizeContext();
            this.recognize(context, (err, topIntent) => {
                // Check for forwarded intent
                if (session.message.entities) {
                    session.message.entities.forEach((entity) => {
                        if (entity.type === consts.intentEntityType && 
                            (<IIntentRecognizerResult>entity).score > topIntent.score) {
                            topIntent = entity;
                        } 
                    });
                }

                // This intent will be automatically inherited by child libraries
                // that don't implement their own recognizers.
                // - We're passing along the library name to avoid running our own
                //   recognizer twice.
                context.intent = topIntent;
                context.libraryName = this.name;

                // Federate across all libraries to find the best route to trigger. 
                var results = Library.addRouteResult({ score: 0.0, libraryName: this.name });
                async.each(this.libraryList(), (lib, cb) => {
                    lib.findRoutes(context, (err, routes) => {
                        if (!err && routes) {
                            routes.forEach((r) => results = Library.addRouteResult(r, results));
                        }
                        cb(err);
                    });
                }, (err) => {
                    if (!err) {
                        // Default disambiguation handler
                        let disambiguateRoute: IDisambiguateRouteHandler = (session, routes) => {
                            const route = Library.bestRouteResult(results, session.dialogStack(), this.name);
                            if (route) {
                                this.library(route.libraryName).selectRoute(session, route);
                            } else {
                                // Route to the active dialog
                                if (session.dialogStack().length > 0) {
                                    session.routeToActiveDialog();
                                    resolve(true);
                                } else {
                                    resolve(false);
                                }
                            }
                        };

                        // Select best route and dispatch message.
                        if (this._onDisambiguateRoute) {
                            this._onDisambiguateRoute(session, results);
                            resolve(true);
                        } else {
                            disambiguateRoute(session, results);
                        }
                    } else {
                        // Let the session process the error
                        session.error(err);
                        reject(err);
                    }
                });
            });
        });
    }

    private eventMiddleware(event: IEvent, middleware: IEventMiddleware[]): Promise<void> {
        return new Promise((resolve, reject) => {
            function next(i: number) {
                if (i < middleware.length) {
                    try {
                        middleware[i](event, () => next(i + 1));
                    } catch (err) {
                        reject(err);
                    }
                } else {
                    resolve();
                }
            }
            next(0);
        });
    }

    private isMessage(message: IEvent): boolean {
        return (message && message.type && message.type.toLowerCase() == consts.messageType);
    }
    
    private lookupUser(address: IAddress): Promise<IIdentity> {
        return new Promise((resolve, reject) => {
            this.emit('lookupUser', address);
            if (this.settings.lookupUser) {
                this.settings.lookupUser(address, (err, user) => {
                    if (!err) {
                        resolve(user || address.user);
                    } else {
                        reject(err);
                    }
                });
            } else {
                resolve(address.user);
            }
        });
    }
    
    private getStorageData(context: BotContext): Promise<IBotStorageData> {
        // Read data
        const data = {} as IBotStorageData;
        const promises: Promise<any>[] = [];
        promises.push(this.settings.conversationState.read(context).then((state) => { data.conversationData = state }));
        if (this.settings.userState) {
            promises.push(this.settings.userState.read(context).then((state) => { data.userData = state }));
        }
        if (this.settings.privateConversationState) {
            promises.push(this.settings.privateConversationState.read(context).then((state) => { data.privateConversationData = state }));
        }

        // Wait for all reads to complete.
        return Promise.all(promises).then(() => data);
    }
     
    private emitError(err: Error): void {
        var m = err.toString();
        var e = err instanceof Error ? err : new Error(m);
        if (this.listenerCount('error') > 0) {
            this.emit('error', e);
        } else {
            console.error(e.stack);
        }
    }
}

function syncState(from: any, to: any) {
    // Copy "from" values to "to" 
    if (!from) { from = {} }
    for (const key in from) {
        if (from.hasOwnProperty(key)) {
            to[key] = from[key];
        }
    }

    // Prune values in "to" not in "from"
    for (const key in to) {
        if (!from.hasOwnProperty(key)) {
            delete to[key];
        }
    }
}