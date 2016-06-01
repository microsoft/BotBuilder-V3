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
import ses = require('../Session');
import bs = require('../storage/BotStorage');
import consts = require('../consts');
import utils = require('../utils');
import events = require('events');
import async = require('async');

export interface ISettings {
    storage?: bs.IBotStorage;
    localizer?: ILocalizer;
    processLimit?: number;
    defaultDialogId?: string;
    defaultDialogArgs?: any;
}

export interface IConnector {
    onMessage(handler: (message: IMessage, cb?: (err: Error) => void) => void): void;
    send(messages: IMessage[], cb: (err: Error) => void): void;
}
interface IConnectorMap {
    [channel: string]: IConnector;    
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
    (session: ses.Session, next: Function): void;
}

export class UniversalBot extends events.EventEmitter {
    private settings = <ISettings>{ processLimit: 4 };
    private connectors = <IConnectorMap>{}; 
    private dialogs = new dc.DialogCollection();
    private mwReceive = <IMessageMiddleware[]>[];
    private mwAnalyze = <IAnalysisMiddleware[]>[];
    private mwSend = <IMessageMiddleware[]>[];
    
    constructor(connector?: IConnector, settings?: ISettings) {
        super();
        if (connector) {
            this.connector('*', connector);
        }
        if (settings) {
            for (var name in settings) {
                this.set(name, (<any>settings)[name]);
            }
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
    // Connectors
    //-------------------------------------------------------------------------
    
    public connector(channelId: string, connector?: IConnector): IConnector {
        var c: IConnector;
        if (connector) {
            this.connectors[channelId || '*'] = c = connector;
            c.onMessage((message, cb) => this.receive(message, cb));
        } else if (this.connectors.hasOwnProperty(channelId)) {
            c = this.connectors[channelId];
        } else if (this.connectors.hasOwnProperty('*')) {
            c = this.connectors['*'];
        }
        return c;
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
    
    public receive(messages: IMessage|IMessage[], done?: (err: Error) => void): void {
        var list: IMessage[] = Array.isArray(messages) ? messages : [messages]; 
        async.eachLimit(list, this.settings.processLimit, (message, cb) => {
            message.userId = message.from.id;
            this.emit('receive', message);
            this.messageMiddleware(message, this.mwReceive, () => {
                this.emit('analyze', message);
                this.analyzeMiddleware(message, () => {
                    this.emit('incoming', message);
                    this.route(message, this.settings.defaultDialogId || '/', this.settings.defaultDialogArgs, cb);
                }, cb);
            }, cb);
        }, done);
    }
 
    public beginDialog(message: IMessage, dialogId: string, dialogArgs?: any, done?: (err: Error) => void): void {
        message.userId = message.to.id;
        this.route(message, dialogId, dialogArgs, (err) => {
            if (done) {
                done(err);
            }    
        });
    }
    
    public send(messages: IMessage|IMessage[], done?: (err: Error) => void): void {
        var list: IMessage[] = Array.isArray(messages) ? messages : [messages]; 
        async.eachLimit(list, this.settings.processLimit, (message, cb) => {
            this.emit('send', message);
            this.messageMiddleware(message, this.mwSend, () => {
                this.emit('outgoing', message);
                cb(null);
            }, cb);
        }, (err) => {
            if (!err) {
                this.tryCatch(() => {
                    // All messages should be targeted at the same channel.
                    var channelId = list[0].to.channelId;
                    var connector = this.connector(channelId);
                    if (!connector) {
                        throw new Error("Invalid channelId='" + channelId + "'");
                    }
                    connector.send(list, (err) => {
                        if (done) {
                            done(err);
                        }    
                    });
                }, done);
            } else if (done) {
                done(err);
            }
        });
    }

    //-------------------------------------------------------------------------
    // Helpers
    //-------------------------------------------------------------------------
    
    private route(message: IMessage, dialogId: string, dialogArgs: any, done: (err: Error) => void): void {
        var address: bs.IBotStorageAddress = { userId: message.userId, conversationId: message.conversationId };
        this.getStorageData(address, (data) => {
            // Initialize session
            var session = new ses.Session({
                localizer: this.settings.localizer,
                dialogs: this.dialogs,
                dialogId: dialogId,
                dialogArgs: dialogArgs,
                onSave: (cb) => {
                    var data: bs.IBotStorageData = {
                        userData: utils.clone(session.userData),
                        conversationData: utils.clone(session.conversationData)    
                    };
                    data.conversationData[consts.Data.SessionState] = session.sessionState;
                    this.saveStorageData(address, data, cb, cb);
                },
                onSend: (messages, cb) => {
                    this.send(messages, cb);
                }
            });
            session.on('error', (err: Error) => this.emitError(err));
            
            // Initialize session data
            var sessionState: ISessionState;
            session.userData = data.userData || {};
            session.conversationData = data.conversationData || {};
            if (session.conversationData.hasOwnProperty(consts.Data.SessionState)) {
                sessionState = session.conversationData[consts.Data.SessionState];
                delete session.conversationData[consts.Data.SessionState];
            }
            
            // Dispatch message
            this.emit('routing', session);
            session.dispatch(sessionState, message);
            done(null);
        }, done);
    }

    private messageMiddleware(message: IMessage, middleware: IMessageMiddleware[], done: Function, error: (err: Error) => void): void {
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
    
    private analyzeMiddleware(message: IMessage, done: Function, error: (err: Error) => void): void {
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
        if (this.mwAnalyze.length > 0) {
            for (var i = 0; i < this.mwAnalyze.length; i++) {
                analyze(this.mwAnalyze[i]);
            }
        } else {
            finish();
        }
    }
    
    private getStorageData(address: bs.IBotStorageAddress, done: (data: bs.IBotStorageData) => void, error: (err: Error) => void): void {
        this.tryCatch(() => {
            this.emit('getStorageData', address);
            var storage = this.getStorage();
            storage.get(address, (err, data) => {
                if (!err) {
                    this.tryCatch(() => done(data || {}), error);
                } else {
                    this.emitError(err);
                    error(err);
                } 
            });  
        }, error);
    }
    
    private saveStorageData(address: bs.IBotStorageAddress, data: bs.IBotStorageData, done: Function, error: (err: Error) => void): void {
        this.tryCatch(() => {
            this.emit('saveStorageData', address);
            var storage = this.getStorage();
            storage.save(address, data, (err) => {
                if (!err) {
                    this.tryCatch(() => done(), error);
                } else {
                    this.emitError(err);
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
            this.emitError(e);
            try {
                if (error) {
                    error(e);
                }
            } catch (e2) {
                this.emitError(e2);
            }
        }
    }
    
    private emitError(err: Error): void {
        this.emit("error", err instanceof Error ? err : new Error(err.toString()));
    }
}