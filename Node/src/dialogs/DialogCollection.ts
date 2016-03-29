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

import dialog = require('./Dialog');
import actions = require('./DialogAction');
import simpleDialog = require('./SimpleDialog');
import events = require('events');

interface IDialogMap {
    [id: string]: dialog.IDialog;
}

export class DialogCollection extends events.EventEmitter {
    private middleware: { (session: ISession, next: Function): void; }[] = []; 
    private dialogs: IDialogMap = {};

    constructor() {
        super();
    }

    public add(dialogs: { [id: string]: dialog.IDialog; }): DialogCollection;
    public add(id: string, fn: IDialogHandler<any>): DialogCollection;
    public add(id: string, waterfall: actions.IDialogWaterfallStep[]): DialogCollection;
    public add(id: string, dialog: dialog.IDialog): DialogCollection;
    public add(id: any, dialog?: any): DialogCollection {
        // Fixup params
        var dialogs: { [id: string]: dialog.IDialog; };
        if (typeof id == 'string') {
            if (Array.isArray(dialog)) {
                dialog = new simpleDialog.SimpleDialog(actions.DialogAction.waterfall(dialog));
            } else if (typeof dialog == 'function') {
                dialog = new simpleDialog.SimpleDialog(dialog);
            }
            dialogs = { [id]: dialog };
        } else {
            dialogs = id;
        }

        // Add dialogs
        for (var key in dialogs) {
            if (!this.dialogs.hasOwnProperty(key)) {
                this.dialogs[key] = dialogs[key];
            } else {
                throw new Error('Dialog[' + key + '] already exists.');
            }
        }
        return this;
    }

    public getDialog(id: string): dialog.IDialog {
        return this.dialogs[id];
    }

    public getMiddleware(): { (session: ISession, next: Function): void; }[] {
        return this.middleware;
    }

    public hasDialog(id: string): boolean {
        return this.dialogs.hasOwnProperty(id);
    }

    public use(fn: (session: ISession, next: Function) => void): DialogCollection {
        this.middleware.push(fn);
        return this;
    }
}