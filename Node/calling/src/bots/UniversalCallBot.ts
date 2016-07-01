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

import dlg = require('../dialogs/Dialog');
import da = require('../dialogs/DialogAction');
import dc = require('../dialogs/DialogCollection');
import sd = require('../dialogs/SimpleDialog');
import ses = require('../CallSession');
import bs = require('../storage/BotStorage');
import consts = require('../consts');
import utils = require('../utils');
import events = require('events');
import async = require('async');

export interface IUniversalCallBotSettings {
    defaultDialogId?: string;
    defaultDialogArgs?: any;
    localizer?: ILocalizer;
    lookupUser?: ILookupUser;
    processLimit?: number;
    autoBatchDelay?: number;
    storage?: bs.IBotStorage;
    persistUserData?: boolean;
    persistConversationData?: boolean;
    dialogErrorMessage?: string|string[];
    promptDefaults?: IPrompt;
    recognizeDefaults?: IRecognizeAction;
    recordDefaults?: IRecordAction;
}

export interface ICallConnector {
    onMessage(handler: (message: IMessage, cb?: (err: Error) => void) => void): void;
    send(message: IMessage, cb: (err: Error) => void): void;
}

export interface IMiddlewareMap {
    receive?: IMessageMiddleware;
    analyze?: IAnalysisMiddleware;
    dialog?: IDialogMiddleware;
    send?: IMessageMiddleware;
}

export interface IMessageMiddleware {
    (message: IMessage, next: Function): void;
}

export interface IAnalysisMiddleware {
    (message: IMessage, done: (analysis: any) => void): void;
}

export interface IDialogMiddleware {
    (session: ses.CallSession, next: Function): void;
}

export interface ILookupUser {
    (address: IConversationAddress, done: (err: Error, user: IParticipant) => void): void;
}

export class UniversalCallBot extends events.EventEmitter {
    private settings = <IUniversalCallBotSettings>{ 
        processLimit: 4, 
        persistUserData: true, 
        persistConversationData: false 
    };
    private dialogs = new dc.DialogCollection();
    private mwReceive = <IMessageMiddleware[]>[];
    private mwAnalyze = <IAnalysisMiddleware[]>[];
    private mwSend = <IMessageMiddleware[]>[];
    
    constructor(private connector: ICallConnector, settings?: IUniversalCallBotSettings) {
        super();
        if (settings) {
            for (var name in settings) {
                this.set(name, (<any>settings)[name]);
            }
        }
        var asStorage: bs.IBotStorage = <any>connector;
        if (!this.settings.storage && 
            typeof asStorage.getData === 'function' &&
            typeof asStorage.saveData === 'function') {
            this.settings.storage = asStorage;
        }
    }
    
    //-------------------------------------------------------------------------
    // Settings
    //-------------------------------------------------------------------------
    
    public set(name: string, value: any): this {
        (<any>this.settings)[name] = value;
        return this;
    }
    
    public get(name: string): any {
        return (<any>this.settings)[name];
    }
    
    //-------------------------------------------------------------------------
    // Dialogs
    //-------------------------------------------------------------------------

    public dialog(id: string, dialog?: dlg.IDialog | da.IDialogWaterfallStep[] | da.IDialogWaterfallStep): dlg.Dialog {
        var d: dlg.Dialog;
        if (dialog) {
            if (Array.isArray(dialog)) {
                d = new sd.SimpleDialog(da.waterfall(dialog));
            } if (typeof dialog == 'function') {
                d = new sd.SimpleDialog(da.waterfall([<any>dialog]));
            } else {
                d = <any>dialog;
            }
            this.dialogs.add(id, d);
        } else {
            d = this.dialogs.getDialog(id);
        }
        return d;
    }

    //-------------------------------------------------------------------------
    // Middleware
    //-------------------------------------------------------------------------
    
    public use(middleware: IMiddlewareMap): this {
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
    }
    
    //-------------------------------------------------------------------------
    // Messaging
    //-------------------------------------------------------------------------
    
    private receive(message: IMessage, done?: (err: Error) => void): void {
        var logger = this.errorLogger(done);
        this.lookupUser(message.address, (user) => {
            if (user) {
                message.user = user;
            }
            this.emit('receive', message);
            this.messageMiddleware(message, this.mwReceive, () => {
                // Analyze message and route to dialog system
                this.emit('analyze', message);
                this.analyzeMiddleware(message, () => {
                    this.emit('incoming', message);
                    var userId = message.user.identity;
                    var conversationId = message.address.id;
                    var storageCtx: bs.IBotStorageContext = { 
                        userId: userId, 
                        conversationId: conversationId, 
                        address: message.address,
                        persistUserData: this.settings.persistUserData,
                        persistConversationData: this.settings.persistConversationData 
                    };
                    this.route(storageCtx, message, this.settings.defaultDialogId || '/', this.settings.defaultDialogArgs, logger);
                }, logger);
            }, logger);
        }, logger);
    }
    
    private send(message: IIsMessage|IMessage, done?: (err: Error) => void): void {
        var logger = this.errorLogger(done);
        var msg = (<IIsMessage>message).toMessage ? (<IIsMessage>message).toMessage() : <IMessage>message; 
        this.emit('send', msg);
        this.messageMiddleware(msg, this.mwSend, () => {
            this.emit('outgoing', msg);
            this.connector.send(msg, logger);
        }, logger);
    }

    //-------------------------------------------------------------------------
    // Helpers
    //-------------------------------------------------------------------------
    
    private route(storageCtx: bs.IBotStorageContext, message: IMessage, dialogId: string, dialogArgs: any, done: (err: Error) => void): void {
        // --------------------------------------------------------------------
        // Theory of Operation
        // --------------------------------------------------------------------
        // The route() function is called for both reactive & pro-active 
        // messages and while they generally work the same there are some 
        // differences worth noting.
        //
        // REACTIVE:
        // * The passed in storageKey will have the normalized userId and the
        //   conversationId for the incoming message. These are used as keys to
        //   load the persisted userData and conversationData objects.
        // * After loading data from storage we create a new Session object and
        //   dispatch the incoming message to the active dialog.
        // * As part of the normal dialog flow the session will call onSave() 1 
        //   or more times before each call to onSend().  Anytime onSave() is 
        //   called we'll save the current userData & conversationData objects
        //   to storage.
        //
        // PROACTIVE:
        // * Proactive follows essentially the same flow but the difference is 
        //   the passed in storageKey will only have a userId and not a 
        //   conversationId as this is a new conversation.  This will cause use
        //   to load userData but conversationData will be set to {}.
        // * When onSave() is called for a proactive message we don't know the
        //   conversationId yet so we can't actually save anything. The first
        //   call to this.send() results in a conversationId being assigned and
        //   that's the point at which we can actually save state. So we'll update
        //   the storageKey with the new conversationId and then manually trigger
        //   saving the userData & conversationData to storage.
        // * After the first call to onSend() for the conversation everything 
        //   follows the same flow as for reactive messages.
        var loadedData: bs.IBotStorageData;
        this.getStorageData(storageCtx, (data) => {
            // Initialize session
            var session = new ses.CallSession({
                localizer: this.settings.localizer,
                autoBatchDelay: this.settings.autoBatchDelay,
                dialogs: this.dialogs,
                dialogId: dialogId,
                dialogArgs: dialogArgs,
                dialogErrorMessage: this.settings.dialogErrorMessage,
                promptDefaults: this.settings.promptDefaults || <any>{},
                recognizeDefaults: this.settings.recognizeDefaults || <any>{},
                recordDefaults: this.settings.recordDefaults || <any>{},
                onSave: (cb) => {
                    var finish = this.errorLogger(cb);
                    loadedData.userData = utils.clone(session.userData);
                    loadedData.conversationData = utils.clone(session.conversationData);
                    loadedData.privateConversationData = utils.clone(session.privateConversationData);
                    loadedData.privateConversationData[consts.Data.SessionState] = session.sessionState;
                    this.saveStorageData(storageCtx, loadedData, finish, finish);
                },
                onSend: (workflow, cb) => {
                    this.send(workflow, cb);
                }
            });
            session.on('error', (err: Error) => this.emitError(err));
            
            // Initialize session data
            var sessionState: ISessionState;
            session.userData = data.userData || {};
            session.conversationData = data.conversationData || {};
            session.privateConversationData = data.privateConversationData || {};
            if (session.privateConversationData.hasOwnProperty(consts.Data.SessionState)) {
                sessionState = session.privateConversationData[consts.Data.SessionState];
                delete session.privateConversationData[consts.Data.SessionState];
            }
            loadedData = data;  // We'll clone it when saving data later
            
            // Dispatch message
            this.emit('routing', session);
            session.dispatch(sessionState, message);
            done(null);
        }, done);
    }

    private messageMiddleware(message: IMessage, middleware: IMessageMiddleware[], done: Function, error?: (err: Error) => void): void {
        var i = -1;
        var _this = this;
        function next() {
            if (++i < middleware.length) {
                _this.tryCatch(() => {
                    middleware[i](message, next);
                }, () => next());
            } else {
                _this.tryCatch(() => done(), error);
            }
        }
        next();
    }
    
    private analyzeMiddleware(message: IMessage, done: Function, error?: (err: Error) => void): void {
        var cnt = this.mwAnalyze.length;
        var _this = this;
        function analyze(fn: IAnalysisMiddleware) {
            _this.tryCatch(() => {
                fn(message, function (analysis) {
                    if (analysis && typeof analysis == 'object') {
                        // Copy analysis to message
                        for (var prop in analysis) {
                            if (analysis.hasOwnProperty(prop)) {
                                (<any>message)[prop] = analysis[prop];
                            }
                        }
                    }
                    finish();
                });
            }, () => finish());
        }
        function finish() {
            _this.tryCatch(() => {
                if (--cnt <= 0) {
                    done();
                }
            }, error);
        }
        if (cnt > 0) {
            for (var i = 0; i < this.mwAnalyze.length; i++) {
                analyze(this.mwAnalyze[i]);
            }
        } else {
            finish();
        }
    }
    
    private lookupUser(address: IConversationAddress, done: (user: IParticipant) => void, error?: (err: Error) => void): void {
        this.tryCatch(() => {
            this.emit('lookupUser', address);
            // Find originator
            var originator: IParticipant;
            address.participants.forEach((participant) => {
                if (participant.originator) {
                    originator = participant;
                }
            });

            // Map to user object
            if (this.settings.lookupUser) {
                this.settings.lookupUser(address, (err, user) => {
                    if (!err) {
                        this.tryCatch(() => done(user || originator), error);
                    } else if (error) {
                        error(err);
                    }
                });
            } else {
                this.tryCatch(() => done(originator), error);
            }
        }, error);
    }
    
    private getStorageData(storageCtx: bs.IBotStorageContext, done: (data: bs.IBotStorageData) => void, error?: (err: Error) => void): void {
        this.tryCatch(() => {
            this.emit('getStorageData', storageCtx);
            var storage = this.getStorage();
            storage.getData(storageCtx, (err, data) => {
                if (!err) {
                    this.tryCatch(() => done(data || {}), error);
                } else if (error) {
                    error(err);
                } 
            });  
        }, error);
    }
    
    private saveStorageData(storageCtx: bs.IBotStorageContext, data: bs.IBotStorageData, done?: Function, error?: (err: Error) => void): void {
        this.tryCatch(() => {
            this.emit('saveStorageData', storageCtx);
            var storage = this.getStorage();
            storage.saveData(storageCtx, data, (err) => {
                if (!err) {
                    if (done) {
                        this.tryCatch(() => done(), error);
                    }
                } else if (error) {
                    error(err);
                } 
            });  
        }, error);
    }

    private getStorage(): bs.IBotStorage {
        if (!this.settings.storage) {
            this.settings.storage = new bs.MemoryBotStorage();
        }
        return this.settings.storage;
    }
    
    private tryCatch(fn: Function, error?: (err?: Error) => void): void {
        try {
            fn();
        } catch (e) {
            try {
                if (error) {
                    error(e);
                }
            } catch (e2) {
                this.emitError(e2);
            }
        }
    }

    private errorLogger(done?: (err: Error) => void): (err: Error) => void {
        return (err: Error) => {
            if (err) {
                this.emitError;
            }
            if (done) {
                done(err);
                done = null;
            }
        };
    }
     
    private emitError(err: Error): void {
        this.emit("error", err instanceof Error ? err : new Error(err.toString()));
    }
}