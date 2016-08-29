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

import dl = require('./bots/Library');
import dlg = require('./dialogs/Dialog');
import actions = require('./dialogs/ActionSet');
import consts = require('./consts');
import sprintf = require('sprintf-js');
import events = require('events');
import utils = require('./utils');
import msg = require('./Message');
import logger = require('./logger');
import async = require('async');
import dfLoc = require('./DefaultLocalizer');

export interface ISessionOptions {
    onSave: (done: (err: Error) => void) => void;
    onSend: (messages: IMessage[], done: (err: Error) => void) => void;
    library: dl.Library;
    middleware: ISessionMiddleware[];
    dialogId: string;
    dialogArgs?: any;
    localizer?: ILocalizer;
    localizerSettings?: ILocalizerSettings;
    autoBatchDelay?: number;
    dialogErrorMessage?: string|string[]|IMessage|IIsMessage;
    actions?: actions.ActionSet;
}

export interface ISessionMiddleware {
    (session: Session, next: Function): void;
}

export class Session extends events.EventEmitter implements ISession {
    private msgSent = false;
    private _isReset = false;
    private lastSendTime = new Date().getTime();
    private batch: IMessage[] = [];
    private batchTimer: NodeJS.Timer;
    private batchStarted = false;
    private sendingBatch = false;
    private inMiddleware = false;
    private _locale:string = null;

    constructor(protected options: ISessionOptions) {
        super();
        this.library = options.library;

        if (!options.localizer) {
            this.localizer = new dfLoc.DefaultLocalizer();           
        } else {
            this.localizer = options.localizer;
        }
        this.localizer.initialize(options.localizerSettings);
        
        if (typeof this.options.autoBatchDelay !== 'number') {
            this.options.autoBatchDelay = 250;  // 250ms delay
        }
    }

    public dispatch(sessionState: ISessionState, message: IMessage): ISession {
        var index = 0;
        var session = this;
        var now = new Date().getTime();
        var middleware = this.options.middleware || [];
        var next = () => {
            var handler = index < middleware.length ? middleware[index] : null;
            if (handler) {
                index++;
                handler(session, next);
            } else {
                this.inMiddleware = false;
                this.sessionState.lastAccess = now; // Set after middleware runs so you can expire old sessions.
                this.routeMessage();
            }
        };

        // Make sure dialogData is properly initialized
        this.sessionState = sessionState || { callstack: [], lastAccess: now, version: 0.0 };
        var cur = this.curDialog();
        if (cur) {
            this.dialogData = cur.state;
        }

        // Dispatch message
        this.inMiddleware = true;
        this.message = <IMessage>(message || { text: '' });
        if (!this.message.type) {
            this.message.type = consts.messageType;
        }

        // Localize message, then invoke middleware
        logger.debug("loading localizer for: " + this.message.textLocale)
        this.localizer.load(this.message.textLocale, (err:Error) => {
            if (err) {
                    this.error(err);
            } else {
                next();
        }});
        return this;
    }

    public library: dl.Library;
    public sessionState: ISessionState;
    public message: IMessage;
    public userData: any;
    public conversationData: any;
    public privateConversationData: any;
    public dialogData: any;
    public localizer:ILocalizer = null;

    public error(err: Error): ISession {
        err = err instanceof Error ? err : new Error(err.toString());
        logger.info(this, 'session.error()');
        this.endConversation(this.options.dialogErrorMessage || 'Oops. Something went wrong and we need to start over.');
        this.emit('error', err);
        return this;
    }

    public preferredLocale(locale?: string, callback?: ErrorCallback): string {
        if (locale) {
            this._locale = locale;
            if (this.localizer) {
                this.localizer.load(locale, callback);
            }
        } else {
            if (!this._locale) {
                if (this.message && this.message.textLocale) {
                    this._locale = this.message.textLocale;
                } else if (this.localizer) {
                    this._locale = this.localizer.defaultLocale();
                }
            }
            return this._locale;
        }        
    }

    public gettext(messageid: string, ...args: any[]): string {
        return this.vgettext(messageid, args);
    }

    public ngettext(messageid: string, messageid_plural: string, count: number): string {
        var tmpl: string;
        if (this.localizer && this.message) {
            tmpl = this.localizer.ngettext(this.message.textLocale || '', messageid, messageid_plural, count);
        } else if (count == 1) {
            tmpl = messageid;
        } else {
            tmpl = messageid_plural;
        }
        return sprintf.sprintf(tmpl, count);
    }
    
    public save(): this {
        logger.info(this, 'session.save()');            
        this.startBatch();
        return this;
    }

    public send(message: string|string[]|IMessage|IIsMessage, ...args: any[]): this {
        this.msgSent = true;
        if (message) {
            var m: IMessage;
            if (typeof message == 'string' || Array.isArray(message)) {
                m = this.createMessage(<any>message, args);
            } else if ((<IIsMessage>message).toMessage) {
                m = (<IIsMessage>message).toMessage();
            } else {
                m = <IMessage>message;
            }
            this.prepareMessage(m);
            this.batch.push(m);
            logger.info(this, 'session.send()');            
        }
        this.startBatch();
        return this;
    }

    public sendTyping(): this {
        this.msgSent = true;
        var m = <IMessage>{ type: 'typing' };
        this.prepareMessage(m);
        this.batch.push(m);
        logger.info(this, 'session.sendTyping()');            
        this.sendBatch();
        return this;        
    }

    public messageSent(): boolean {
        return this.msgSent;
    }

    public beginDialog(id: string, args?: any): ISession {
        // Find dialog
        logger.info(this, 'session.beginDialog(%s)', id);            
        var id = this.resolveDialogId(id);
        var dialog = this.findDialog(id);
        if (!dialog) {
            throw new Error('Dialog[' + id + '] not found.');
        }
        
        // Push dialog onto stack and start it
        // - Removed the call to save() here as an optimization. In the case of prompts
        //   we end up saving state twice, once here and again after they save off all of
        //   there params before sending the message.  This chnage does mean a dialog needs
        //   to either send a message or manually call session.save() when started but given
        //   most dialogs should always prompt the user is some way that seems reasonable and
        //   can save a number of intermediate calls to save.
        this.pushDialog({ id: id, state: {} });
        this.startBatch();
        dialog.begin(this, args);
        return this;
    }

    public replaceDialog(id: string, args?: any): ISession {
        // Find dialog
        logger.info(this, 'session.replaceDialog(%s)', id);            
        var id = this.resolveDialogId(id);
        var dialog = this.findDialog(id);
        if (!dialog) {
            throw new Error('Dialog[' + id + '] not found.');
        }
        
        // Update the stack and start dialog
        this.popDialog();
        this.pushDialog({ id: id, state: {} });
        this.startBatch();
        dialog.begin(this, args);
        return this;
    }

    public endConversation(message?: string|string[]|IMessage|IIsMessage, ...args: any[]): ISession {
        // Unpack message
        var m: IMessage;
        if (message) {
            if (typeof message == 'string' || Array.isArray(message)) {
                m = this.createMessage(<any>message, args);
            } else if ((<IIsMessage>message).toMessage) {
                m = (<IIsMessage>message).toMessage();
            } else {
                m = <IMessage>message;
            }
            this.msgSent = true;
            this.prepareMessage(m);
            this.batch.push(m);
        }

        // Clear private conversation data
        this.privateConversationData = {};
                
        // Clear stack and save.
        logger.info(this, 'session.endConversation()');            
        var ss = this.sessionState;
        ss.callstack = [];
        this.sendBatch();
        return this;
    }

    public endDialog(message?: string|string[]|IMessage|IIsMessage, ...args: any[]): ISession {
        // Check for result being passed
        if (typeof message === 'object' && (message.hasOwnProperty('response') || message.hasOwnProperty('resumed') || message.hasOwnProperty('error'))) {
            console.warn('Returning results via Session.endDialog() is deprecated. Use Session.endDialogWithResult() instead.')            
            return this.endDialogWithResult(<any>message);
        }

        // Validate callstack
        var cur = this.curDialog();
        if (cur) {
            // Unpack message
            var m: IMessage;
            if (message) {
                if (typeof message == 'string' || Array.isArray(message)) {
                    m = this.createMessage(<any>message, args);
                } else if ((<IIsMessage>message).toMessage) {
                    m = (<IIsMessage>message).toMessage();
                } else {
                    m = <IMessage>message;
                }
                this.msgSent = true;
                this.prepareMessage(m);
                this.batch.push(m);
            }
                    
            // Pop dialog off the stack and then resume parent.
            logger.info(this, 'session.endDialog()');            
            var childId = cur.id;
            cur = this.popDialog();
            this.startBatch();
            if (cur) {
                var dialog = this.findDialog(cur.id);
                if (dialog) {
                    dialog.dialogResumed(this, { resumed: dlg.ResumeReason.completed, response: true, childId: childId });
                } else {
                    // Bad dialog on the stack so just end it.
                    // - Because of the stack validation we should never actually get here.
                    this.error(new Error("Can't resume missing parent dialog '" + cur.id + "'."));
                }
            }
        }
        return this;
    }

    public endDialogWithResult(result?: dlg.IDialogResult<any>): ISession {
        // Validate callstack
        var cur = this.curDialog();
        if (cur) {
            // Validate result
            result = result || <any>{};
            if (!result.hasOwnProperty('resumed')) {
                result.resumed = dlg.ResumeReason.completed;
            }
            result.childId = cur.id;
                    
            // Pop dialog off the stack and resume parent dlg.
            logger.info(this, 'session.endDialogWithResult()');            
            cur = this.popDialog();
            this.startBatch();
            if (cur) {
                var dialog = this.findDialog(cur.id);
                if (dialog) {
                    dialog.dialogResumed(this, result);
                } else {
                    // Bad dialog on the stack so just end it.
                    // - Because of the stack validation we should never actually get here.
                    this.error(new Error("Can't resume missing parent dialog '" + cur.id + "'."));
                }
            }
        }
        return this;
    }

    public cancelDialog(dialogId: string|number, replaceWithId?: string, replaceWithArgs?: any): ISession {
        // Delete dialog(s)
        var childId = typeof dialogId === 'number' ? this.sessionState.callstack[<number>dialogId].id : <string>dialogId;
        var cur = this.deleteDialogs(dialogId);
        if (replaceWithId) {
            logger.info(this, 'session.cancelDialog(%s)', replaceWithId);            
            var id = this.resolveDialogId(replaceWithId);
            var dialog = this.findDialog(id);
            this.pushDialog({ id: id, state: {} });
            this.startBatch();
            dialog.begin(this, replaceWithArgs);
        } else {
            logger.info(this, 'session.cancelDialog()');            
            this.startBatch();
            if (cur) {
                var dialog = this.findDialog(cur.id);
                if (dialog) {
                    dialog.dialogResumed(this, { resumed: dlg.ResumeReason.canceled, response: null, childId: childId });
                } else {
                    // Bad dialog on the stack so just end it.
                    // - Because of the stack validation we should never actually get here.
                    this.error(new Error("Can't resume missing parent dialog '" + cur.id + "'."));
                }
            }
        }
        return this;
    }

    public reset(dialogId?: string, dialogArgs?: any): ISession {
        logger.info(this, 'session.reset()');            
        this._isReset = true;
        this.sessionState.callstack = [];
        if (!dialogId) {
            dialogId = this.options.dialogId;
            dialogArgs = this.options.dialogArgs;
        }
        this.beginDialog(dialogId, dialogArgs);
        return this;
    }

    public isReset(): boolean {
        return this._isReset;
    }

    public sendBatch(callback?: (err: Error) => void): void {
        logger.info(this, 'session.sendBatch() sending %d messages', this.batch.length);            
        if (this.sendingBatch) {
            return;
        }
        if (this.batchTimer) {
            clearTimeout(this.batchTimer);
            this.batchTimer = null;
        }
        this.batchTimer = null;
        var batch = this.batch;
        this.batch = [];
        this.batchStarted = false;
        this.sendingBatch = true;
        var cur = this.curDialog();
        if (cur) {
            cur.state = this.dialogData;
        }
        this.options.onSave((err) => {
            if (!err) {
                if (batch.length) {
                    this.options.onSend(batch, (err) => {
                        this.sendingBatch = false;
                        if (this.batchStarted) {
                            this.startBatch();
                        }
                        if (callback) {
                            callback(err);
                        }
                    });
                } else {
                    this.sendingBatch = false;
                    if (this.batchStarted) {
                        this.startBatch();
                    }
                    if (callback) {
                        callback(err);
                    }
                }
            } else {
                this.sendingBatch = false;
                switch ((<any>err).code || '') {
                    case consts.Errors.EBADMSG:
                    case consts.Errors.EMSGSIZE:
                        // Something wrong with state so reset everything 
                        this.userData = {};
                        this.batch = [];
                        this.endConversation(this.options.dialogErrorMessage || 'Oops. Something went wrong and we need to start over.');
                        break;
                } 
                if (callback) {
                    callback(err);
                }
            }
        });
    }

    //-----------------------------------------------------
    // PRIVATE HELPERS
    //-----------------------------------------------------

    private startBatch(): void {
        this.batchStarted = true;
        if (!this.sendingBatch) {
            if (this.batchTimer) {
                clearTimeout(this.batchTimer);
            }
            this.batchTimer = setTimeout(() => {
                this.sendBatch();
            }, this.options.autoBatchDelay);
        }
    }

    private createMessage(text: string|string[], args?: any[]): IMessage {
        args.unshift(text);
        var message = new msg.Message(this);
        msg.Message.prototype.text.apply(message, args);
        return message.toMessage();
    }
    
    private prepareMessage(msg: IMessage): void {
        if (!msg.type) {
            msg.type = 'message';
        }
        if (!msg.address) {
            msg.address = this.message.address;
        }
        if (!msg.textLocale && this.message.textLocale) {
            msg.textLocale = this.message.textLocale;
        }
    }

    private routeMessage(): void {
        var _that = this;
        function routeToDialog(recognizeResult: dlg.IRecognizeResult) {
            var cur = _that.curDialog();
            if (!cur) {
                _that.beginDialog(_that.options.dialogId, _that.options.dialogArgs);
            } else {
                var dialog = _that.findDialog(cur.id);
                _that.dialogData = cur.state;
                dialog.replyReceived(_that, recognizeResult);
            }    
        }

        // Validate callstack
        if (this.validateCallstack()) {
            // Check current dialogs confidence that it understands utterance 
            this.recognizeCurDialog((err, dialogResult) => {
                if (err) {
                    this.error(err);
                } else if (dialogResult.score < 1.0) {
                    // Check the confidence of any actions on the callstack.
                    this.recognizeCallstackActions((err, actionResult) => {
                        if (err) {
                            this.error(err);
                        } else if (actionResult.score > dialogResult.score) {
                            // Route to action
                            if (actionResult.dialogId) {
                                // Action is on the stack
                                var dialog = this.findDialog(actionResult.dialogId);
                                dialog.invokeAction(this, actionResult);
                            } else {
                                // Global action
                                this.options.actions.invokeAction(this, actionResult);
                            }
                        } else {
                            routeToDialog(dialogResult);
                        }
                    });
                } else {
                    routeToDialog(dialogResult);
                }
            });      
        } else {
            logger.warn(this, 'Callstack is invalid, resetting session.');
            this.reset(this.options.dialogId, this.options.dialogArgs);
        }
    }

    private recognizeCurDialog(done: (err: Error, result: actions.IRecognizeActionResult) => void): void {
        var cur = this.curDialog();
        if (cur && this.message.text.indexOf('action?') !== 0) {
            var dialog = this.findDialog(cur.id);
            dialog.recognize({ message: this.message, dialogData: cur.state, activeDialog: true }, done);
        } else {
            done(null, { score: 0.0 });
        }
    }

    private recognizeCallstackActions(done: (err: Error, result: actions.IRecognizeActionResult) => void): void {
        var ss = this.sessionState;
        var i = ss.callstack.length - 1;
        var result: actions.IRecognizeActionResult = { score: 0.0 };
        async.whilst(() => {
            return (i >= 0 && result.score < 1.0);
        }, (cb) => {
            try {
                var index = i--;
                var cur = ss.callstack[index];
                var dialog = this.findDialog(cur.id);
                dialog.recognizeAction(this.message, (err, r) => {
                    if (!err && r && r.score > result.score) {
                        result = r;
                        result.dialogId = cur.id;
                        result.dialogIndex = index;
                    }
                    cb(err);
                });
            } catch (e) {
                cb(e);
            }
        }, (err) => {
            if (!err) {
                if (result.score < 1.0 && this.options.actions) {
                    this.options.actions.recognizeAction(this.message, (err, r) => {
                        if (!err && r && r.score > result.score) {
                            result = r;
                        }
                        done(err, result);
                    });
                } else {
                    done(null, result);
                }
            } else {
                done(err instanceof Error ? err : new Error(err.toString()), null);
            }
        });
    }
 
    private vgettext(messageid: string, args?: any[]): string {
        var tmpl: string;
        if (this.localizer && this.message) {
            tmpl = this.localizer.gettext(this.preferredLocale() || this.message.textLocale || '', messageid);
        } else {
            tmpl = messageid;
        }
        return args && args.length > 0 ? sprintf.vsprintf(tmpl, args) : tmpl;
    }

    /** Checks for any unsupported dialogs on the callstack. */
    private validateCallstack(): boolean {
        var ss = this.sessionState;
        for (var i = 0; i < ss.callstack.length; i++) {
            var id = ss.callstack[i].id;
            if (!this.findDialog(id)) {
                return false;
            }
        }
        return true;
    }

    private resolveDialogId(id: string) {
        if (id.indexOf(':') >= 0) {
            return id;
        }
        var cur = this.curDialog();
        var libName = cur && !this.inMiddleware ? cur.id.split(':')[0] : consts.Library.default;
        return libName + ':' + id;
    }

    private findDialog(id: string): dlg.Dialog {
        var parts = id.split(':');
        return this.library.findDialog(parts[0] || consts.Library.default, parts[1]);
    }

    private pushDialog(ds: IDialogState): IDialogState {
        var ss = this.sessionState;
        var cur = this.curDialog();
        if (cur) {
            cur.state = this.dialogData || {};
        }
        ss.callstack.push(ds);
        this.dialogData = ds.state || {};
        return ds;
    }

    private popDialog(): IDialogState {
        var ss = this.sessionState;
        if (ss.callstack.length > 0) {
            ss.callstack.pop();
        }
        var cur = this.curDialog();
        this.dialogData = cur ? cur.state : null;
        return cur;
    }

    private deleteDialogs(dialogId?: string|number): IDialogState {
        var ss = this.sessionState;
        var index = -1;
        if (typeof dialogId === 'string') {
            for (var i = ss.callstack.length - 1; i >= 0; i--) {
                if (ss.callstack[i].id == dialogId) {
                    index = i;
                    break;
                }
            }
        } else {
            index = <number>dialogId;
        }
        if (index < 0 && index < ss.callstack.length) {
            throw new Error('Unable to cancel dialog. Dialog[' + dialogId + '] not found.');
        }
        ss.callstack.splice(index);
        var cur = this.curDialog();
        this.dialogData = cur ? cur.state : null;
        return cur;
    }

    private curDialog(): IDialogState {
        var cur: IDialogState;
        var ss = this.sessionState;
        if (ss.callstack.length > 0) {
            cur = ss.callstack[ss.callstack.length - 1];
        }
        return cur;
    }

    //-----------------------------------------------------
    // DEPRECATED METHODS
    //-----------------------------------------------------
    
    public getMessageReceived(): any {
        console.warn("Session.getMessageReceived() is deprecated. Use Session.message.sourceEvent instead.");
        return this.message.sourceEvent;
    }
}
