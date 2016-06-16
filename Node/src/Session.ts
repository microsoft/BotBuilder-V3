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

import collection = require('./dialogs/DialogCollection');
import dialog = require('./dialogs/Dialog');
import consts = require('./consts');
import sprintf = require('sprintf-js');
import events = require('events');
import utils = require('./utils');
import msg = require('./Message');

export interface ISessionOptions {
    onSave: (done: (err: Error) => void) => void;
    onSend: (messages: IMessage[], done: (err: Error) => void) => void;
    dialogs: collection.DialogCollection;
    dialogId: string;
    dialogArgs?: any;
    localizer?: ILocalizer;
    autoBatchDelay?: number;
}

export class Session extends events.EventEmitter implements ISession {
    private msgSent = false;
    private _isReset = false;
    private lastSendTime = new Date().getTime();
    private batch: IMessage[] = [];
    private batchTimer: NodeJS.Timer;
    private batchStarted = false;
    private sendingBatch = false;

    constructor(protected options: ISessionOptions) {
        super();
        this.dialogs = options.dialogs;
        if (typeof this.options.autoBatchDelay !== 'number') {
            this.options.autoBatchDelay = 150;  // 150ms delay
        }
    }

    public dispatch(sessionState: ISessionState, message: IMessage): ISession {
        var index = 0;
        var handlers: { (session: Session, next: Function): void; }[];
        var session = this;
        var next = () => {
            var handler = index < handlers.length ? handlers[index] : null;
            if (handler) {
                index++;
                handler(session, next);
            } else {
                this.routeMessage();
            }
        };

        // Dispatch message
        this.sessionState = sessionState || { callstack: [], lastAccess: 0 };
        this.sessionState.lastAccess = new Date().getTime();
        this.message = <IMessage>(message || { text: '' });
        if (!this.message.type) {
            this.message.type = 'Message';
        }
        handlers = this.dialogs.getMiddleware();
        next();
        return this;
    }

    public dialogs: collection.DialogCollection;
    public sessionState: ISessionState;
    public message: IMessage;
    public userData: any;
    public conversationData: any;
    public privateConversationData: any;
    public dialogData: any;

    public error(err: Error): ISession {
        err = err instanceof Error ? err : new Error(err.toString());
        console.error('ERROR: Session Error: ' + err.message);
        this.emit('error', err);
        return this;
    }

    public gettext(messageid: string, ...args: any[]): string {
        return this.vgettext(messageid, args);
    }

    public ngettext(messageid: string, messageid_plural: string, count: number): string {
        var tmpl: string;
        if (this.options.localizer && this.message) {
            tmpl = this.options.localizer.ngettext(this.message.local || '', messageid, messageid_plural, count);
        } else if (count == 1) {
            tmpl = messageid;
        } else {
            tmpl = messageid_plural;
        }
        return sprintf.sprintf(tmpl, count);
    }
    
    public save(): this {
        this.startBatch();
        return this;
    }

    public send(message?: string|string[]|IMessage|IIsMessage, ...args: any[]): this {
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
        }
        this.startBatch();
        return this;
    }
    
    public sendMessage(message: IMessage|IIsMessage): this {
        this.msgSent = true;
        if (message) {
            var m = (<IIsMessage>message).toMessage ? (<IIsMessage>message).toMessage() : <IMessage>message;
            this.prepareMessage(m);
            this.batch.push(m);
        }    
        this.startBatch();
        return this;
    }

    public messageSent(): boolean {
        return this.msgSent;
    }

    public beginDialog<T>(id: string, args?: T): ISession {
        // Find dialog
        var dlg = this.dialogs.getDialog(id);
        if (!dlg) {
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
        dlg.begin(this, args);
        return this;
    }

    public replaceDialog<T>(id: string, args?: T): ISession {
        // Find dialog
        var dlg = this.dialogs.getDialog(id);
        if (!dlg) {
            throw new Error('Dialog[' + id + '] not found.');
        }
        
        // Update the stack and start dialog
        this.popDialog();
        this.pushDialog({ id: id, state: {} });
        this.startBatch();
        dlg.begin(this, args);
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
                
        // Clear stack and save.
        var ss = this.sessionState;
        ss.callstack = [];
        this.startBatch();
        return this;
    }

    public endDialog(message?: string|string[]|IMessage|IIsMessage, ...args: any[]): ISession {
        // Check for result being passed
        if (typeof message === 'object' && (message.hasOwnProperty('response') || message.hasOwnProperty('resumed') || message.hasOwnProperty('error'))) {
            console.warn('Returning results via Session.endDialog() is deprecated. Use Session.endDialogWithResult() instead.')            
            return this.endDialogWithResult(<any>message);
        }

        // Validate callstack
        // - Protect against too many calls to endDialog()
        var cur = this.curDialog();
        if (!cur) {
            console.error('ERROR: Too many calls to session.endDialog().')
            return this;
        }
        
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
        var childId = cur.id;
        cur = this.popDialog();
        this.startBatch();
        if (cur) {
            var dlg = this.dialogs.getDialog(cur.id);
            if (dlg) {
                dlg.dialogResumed(this, { resumed: dialog.ResumeReason.completed, response: true, childId: childId });
            } else {
                // Bad dialog on the stack so just end it.
                // - Because of the stack validation we should never actually get here.
                this.endDialogWithResult({ resumed: dialog.ResumeReason.notCompleted, error: new Error("ERROR: Can't resume missing parent dialog '" + cur.id + "'.") });
            }
        }
        return this;
    }

    public endDialogWithResult(result?: dialog.IDialogResult<any>): ISession {
        // Validate callstack
        // - Protect against too many calls to endDialogWithResult()
        var cur = this.curDialog();
        if (!cur) {
            console.error('ERROR: Too many calls to session.endDialog().')
            return this;
        }
        
        // Validate result
        result = result || <any>{};
        if (!result.hasOwnProperty('resumed')) {
            result.resumed = dialog.ResumeReason.completed;
        }
        result.childId = cur.id;
                
        // Pop dialog off the stack and resume parent dialog.
        cur = this.popDialog();
        this.startBatch();
        if (cur) {
            var dlg = this.dialogs.getDialog(cur.id);
            if (dlg) {
                dlg.dialogResumed(this, result);
            } else {
                // Bad dialog on the stack so just end it.
                // - Because of the stack validation we should never actually get here.
                this.endDialogWithResult({ resumed: dialog.ResumeReason.notCompleted, error: new Error("ERROR: Can't resume missing parent dialog '" + cur.id + "'.") });
            }
        }
        return this;
    }

    public reset(dialogId?: string, dialogArgs?: any): ISession {
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

    private sendBatch(): void {
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
            if (!err && batch.length) {
                this.options.onSend(batch, (err) => {
                    this.sendingBatch = false;
                    if (this.batchStarted) {
                        this.startBatch();
                    }
                });
            } else {
                this.sendingBatch = false;
                if (this.batchStarted) {
                    this.startBatch();
                }
            }
        });
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
        if (!msg.local && this.message.local) {
            msg.local = this.message.local;
        }
    }

    private routeMessage(): void {
        try {
            // Route message to dialog.
            var cur = this.curDialog();
            if (!cur) {
                this.beginDialog(this.options.dialogId, this.options.dialogArgs);
            } else if (this.validateCallstack()) {
                var dialog = this.dialogs.getDialog(cur.id);
                this.dialogData = cur.state;
                dialog.replyReceived(this);
            } else {
                console.warn('Callstack is invalid, resetting session.');
                this.reset(this.options.dialogId, this.options.dialogArgs);
            }
        } catch (e) {
            this.error(e);
        }
    }

    private vgettext(messageid: string, args?: any[]): string {
        var tmpl: string;
        if (this.options.localizer && this.message) {
            tmpl = this.options.localizer.gettext(this.message.local || '', messageid);
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
            if (!this.dialogs.hasDialog(id)) {
                return false;
            }
        }
        return true;
    }

    private pushDialog(dlg: IDialogState): IDialogState {
        var ss = this.sessionState;
        var cur = this.curDialog();
        if (cur) {
            cur.state = this.dialogData || {};
        }
        ss.callstack.push(dlg);
        this.dialogData = dlg.state || {};
        return dlg;
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
        console.warn("Session.getMessageReceived() is deprecated. Use Session.message.channelData instead.");
        return this.message.channelData;
    }
}
