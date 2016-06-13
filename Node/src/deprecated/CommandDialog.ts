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

import ses = require('../Session');
import dlg = require('../dialogs/Dialog');
import intent = require('../dialogs/IntentDialog');
import luis = require('../dialogs/LuisRecognizer');
import actions = require('../dialogs/DialogAction');

export class CommandDialog extends dlg.Dialog {
    private dialog: intent.IntentDialog;

    constructor(serviceUri: string) {
        super();
        console.warn('CommandDialog class is deprecated. Use IntentDialog class instead.')
        this.dialog = new intent.IntentDialog();
    }

    public begin<T>(session: ses.Session, args?: T): void {
        this.dialog.begin(session, args);
    }

    public replyReceived(session: ses.Session, recognizeResult?: dlg.IRecognizeResult): void {
        this.dialog.replyReceived(session, recognizeResult);
    }

    public dialogResumed<T>(session: ses.Session, result: dlg.IDialogResult<T>): void {
        this.dialog.dialogResumed(session, result);
    }

    public recognize(context: dlg.IRecognizeContext, cb: (err: Error, result: dlg.IRecognizeResult) => void): void {
        this.dialog.recognize(context, cb);
    }

    public onBegin(handler: IBeginDialogHandler): this {
        this.dialog.onBegin(handler);
        return this;
    }

    public matches(patterns: string | string[], dialogId: string | actions.IDialogWaterfallStep[] | actions.IDialogWaterfallStep, dialogArgs?: any): this {
        var list = <string[]>(!Array.isArray(patterns) ? [patterns] : patterns);
        list.forEach((p: string) => {
            this.dialog.matches(new RegExp(p, 'i'), dialogId, dialogArgs);
        });
        return this;
    } 

    public onDefault(dialogId: string | actions.IDialogWaterfallStep[] | actions.IDialogWaterfallStep, dialogArgs?: any): this {
        this.dialog.onDefault(dialogId, dialogArgs);
        return this;
    }
}
