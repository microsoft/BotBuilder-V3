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
import consts = require('./Consts');
import sprintf = require('sprintf-js');
import events = require('events');

export interface ISessionArgs {
    dialogs: collection.DialogCollection;
    dialogId: string;
    dialogArgs?: any;
    localizer?: ILocalizer;
}

export class Session extends events.EventEmitter implements ISession {
    private msgSent = false;
    private _isReset = false;

    constructor(protected args: ISessionArgs) {
        super();
        this.dialogs = args.dialogs;
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
        this.message = message || { text: '' };
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
    public dialogData: any;

    public error(err: Error): ISession {
        err = err instanceof Error ? err : new Error(err.toString());
        console.error('Session Error: ' + err.message);
        this.emit('error', err);
        return this;
    }

    public gettext(msgid: string, ...args: any[]): string {
        return this.vgettext(msgid, args);
    }

    public ngettext(msgid: string, msgid_plural: string, count: number): string {
        var tmpl: string;
        if (this.args.localizer && this.message) {
            tmpl = this.args.localizer.ngettext(this.message.language || '', msgid, msgid_plural, count);
        } else if (count == 1) {
            tmpl = msgid;
        } else {
            tmpl = msgid_plural;
        }
        return sprintf.sprintf(tmpl, count);
    }

    public send(): ISession;
    public send(msg: string, ...args: any[]): ISession;
    public send(msg: IMessage): ISession;
    public send(msg?: any, ...args: any[]): ISession {
        // Update dialog state
        // - Deals with a situation where the user assigns a whole new object to dialogState.
        var ss = this.sessionState;
        if (ss.callstack.length > 0) {
            ss.callstack[ss.callstack.length - 1].state = this.dialogData || {};
        }

        // Compose message
        this.msgSent = true;
        var message: IMessage = typeof msg == 'string' ? this.createMessage(msg, args) : msg;
        this.emit('send', message);
        return this;
    }
    
    public getMessageReceived(): any {
        return this.message.channelData;
    }
    
    public sendMessage(msg: any): ISession {
        return this.send({ channelData: msg });
    }

    public messageSent(): boolean {
        return this.msgSent;
    }

    public beginDialog<T>(id: string, args?: T): ISession {
        var dialog = this.dialogs.getDialog(id);
        if (!dialog) {
            throw new Error('Dialog[' + id + '] not found.');
        }
        var ss = this.sessionState;
        var cur: IDialogState = { id: id, state: {} };
        ss.callstack.push(cur);
        this.dialogData = cur.state;
        dialog.begin(this, args);
        return this;
    }

    public replaceDialog<T>(id: string, args?: T): ISession {
        var dialog = this.dialogs.getDialog(id);
        if (!dialog) {
            throw new Error('Dialog[' + id + '] not found.');
        }
        var ss = this.sessionState;
        var cur: IDialogState = { id: id, state: {} };
        ss.callstack.pop();
        ss.callstack.push(cur);
        this.dialogData = cur.state;
        dialog.begin(this, args);
        return this;
    }

    public endDialog(result?: any): ISession {
        var ss = this.sessionState;
        var r: dialog.IDialogResult<any> = result || {};
        if (!r.hasOwnProperty('resumed')) {
            r.resumed = dialog.ResumeReason.completed;
        }
        r.childId = ss.callstack[ss.callstack.length - 1].id;
        ss.callstack.pop();
        if (ss.callstack.length > 0) {
            var cur = ss.callstack[ss.callstack.length - 1];
            var d = this.dialogs.getDialog(cur.id);
            this.dialogData = cur.state;
            d.dialogResumed(this, r);
        } else {
            this.send();
            this.emit('quit');
        }
        return this;
    }

    public compareConfidence(language: string, utterance: string, score: number, callback: (handled: boolean) => void): void {
        var comparer = new SessionConfidenceComparor(this, language, utterance, score, callback);
        comparer.next();
    }

    public reset(dialogId: string, dialogArgs?: any): ISession {
        this._isReset = true;
        this.sessionState.callstack = [];
        this.beginDialog(dialogId, dialogArgs);
        return this;
    }

    public isReset(): boolean {
        return this._isReset;
    }

    public createMessage(text: string, args?: any[]): IMessage {
        var message: IMessage = {
            text: this.vgettext(text, args)
        };
        if (this.message.language) {
            message.language = this.message.language
        }
        return message;
    }

    private routeMessage(): void {
        try {
            // Route message to dialog.
            var ss = this.sessionState;
            if (ss.callstack.length == 0) {
                this.beginDialog(this.args.dialogId, this.args.dialogArgs);
            } else if (this.validateCallstack()) {
                var cur = ss.callstack[ss.callstack.length - 1];
                var dialog = this.dialogs.getDialog(cur.id);
                this.dialogData = cur.state;
                dialog.replyReceived(this);
            } else {
                console.error('Callstack is invalid, resetting session.');
                this.reset(this.args.dialogId, this.args.dialogArgs);
            }
        } catch (e) {
            this.error(e);
        }
    }

    private vgettext(msgid: string, args?: any[]): string {
        var tmpl: string;
        if (this.args.localizer && this.message) {
            tmpl = this.args.localizer.gettext(this.message.language || '', msgid);
        } else {
            tmpl = msgid;
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
}

class SessionConfidenceComparor implements ISessionAction {
    private index: number;

    constructor(
        private session: Session,
        private language: string,
        private utterance: string,
        private score: number,
        private callback: (handled: boolean) => void) {
        this.index = session.sessionState.callstack.length - 1;
        this.userData = session.userData;
    }

    public userData: any;
    public dialogData: any;

    public next(): void {
        this.index--;
        if (this.index >= 0) {
            this.getDialog().compareConfidence(this, this.language, this.utterance, this.score);
        } else {
            this.callback(false);
        }
    }

    public endDialog<T>(result?: dialog.IDialogResult<T>): void {
        // End dialog up to current point in the stack.
        this.session.sessionState.callstack.splice(this.index + 1);
        this.getDialog().dialogResumed(this.session, result || { resumed: dialog.ResumeReason.childEnded });
        this.callback(true);
    }

    public send(msg: string, ...args: any[]): void;
    public send(msg: IMessage): void;
    public send(msg: any, ...args: any[]): void {
        // Send a message to the user.
        args.splice(0, 0, [msg]);
        Session.prototype.send.apply(this.session, args);
        this.callback(true);
    }

    private getDialog(): dialog.IDialog {
        var cur = this.session.sessionState.callstack[this.index];
        this.dialogData = cur.state;
        return this.session.dialogs.getDialog(cur.id);
    }
}