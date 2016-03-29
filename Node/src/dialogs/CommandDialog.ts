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

import session = require('../Session');
import dialog = require('./Dialog');
import actions = require('./DialogAction');
import consts = require('../consts');
import util = require('util');

export interface ICommandArgs {
    expression: RegExp;
    matches: RegExpExecArray;
}

interface ICommandDialogEntry {
    expressions?: RegExp[];
    fn: IDialogHandler<ICommandArgs>;
}

export class CommandDialog extends dialog.Dialog {
    private beginDialog: (session: ISession, args: any, next: (handled: boolean) => void) => void;
    private commands: ICommandDialogEntry[] = [];
    private default: ICommandDialogEntry;

    public begin<T>(session: ISession, args: T): void {
        if (this.beginDialog) {
            session.dialogData[consts.Data.Handler] = -1;
            this.beginDialog(session, args, () => {
                super.begin(session, args);
            });
        } else {
            super.begin(session, args);
        }
    }

    public replyReceived(session: ISession): void {
        var score = 0.0;
        var expression: RegExp;
        var matches: RegExpExecArray;
        var text = session.message.text;
        var matched: ICommandDialogEntry;
        for (var i = 0; i < this.commands.length; i++) {
            var cmd = this.commands[i];
            for (var j = 0; j < cmd.expressions.length; j++) {
                expression = cmd.expressions[j];
                if (expression.test(text)) {
                    matched = cmd;
                    session.dialogData[consts.Data.Handler] = i;
                    matches = expression.exec(text);
                    if (matches) {
                        var length = 0;
                        matches.forEach((value) => {
                            length += value.length;
                        });
                        score = length / text.length;
                    }
                    break;
                }
            }
            if (matched) break;
        }
        if (!matched && this.default) {
            expression = null;
            matched = this.default;
            session.dialogData[consts.Data.Handler] = this.commands.length;
        }
        if (matched) {
            session.compareConfidence(session.message.language, text, score, (handled) => {
                if (!handled) {
                    matched.fn(session, { expression: expression, matches: matches });
                }
            });
        } else {
            session.send();
        }
    }

    public dialogResumed<T>(session: ISession, result: dialog.IDialogResult<T>): void {
        var cur: ICommandDialogEntry;
        var handler = session.dialogData[consts.Data.Handler];
        if (handler >= 0 && handler < this.commands.length) {
            cur = this.commands[handler];
        } else if (handler > this.commands.length && this.default) {
            cur = this.default;
        }
        if (cur) {
            cur.fn(session, <any>result);
        } else {
            super.dialogResumed(session, result);
        }
    }

    public onBegin(handler: IBeginDialogHandler): this {
        this.beginDialog = handler;
        return this;
    }

    public matches(pattern: string, fn: IDialogHandler<ICommandArgs>): this; 
    public matches(patterns: string[], fn: IDialogHandler<ICommandArgs>): this; 
    public matches(pattern: string, waterfall: actions.IDialogWaterfallStep[]): this;
    public matches(patterns: string[], waterfall: actions.IDialogWaterfallStep[]): this;
    public matches(pattern: string, dialogId: string, dialogArgs?: any): this;
    public matches(patterns: string[], dialogId: string, dialogArgs?: any): this; 
    public matches(patterns: any, dialogId: any, dialogArgs?: any): this {
        // Fix args
        var fn: IDialogHandler<ICommandArgs>;
        var patterns = !util.isArray(patterns) ? [patterns] : patterns;
        if (Array.isArray(dialogId)) {
            fn = actions.DialogAction.waterfall(dialogId);
        } else if (typeof dialogId == 'string') {
            fn = actions.DialogAction.beginDialog(dialogId, dialogArgs);
        } else {
            fn = dialogId;
        }

        // Save compiled expressions
        var expressions: RegExp[] = [];
        for (var i = 0; i < (<string[]>patterns).length; i++) {
            expressions.push(new RegExp((<string[]>patterns)[i], 'i'));
        }
        this.commands.push({ expressions: expressions, fn: fn });
        return this;
    } 

    public onDefault(fn: IDialogHandler<ICommandArgs>): this;
    public onDefault(waterfall: actions.IDialogWaterfallStep[]): this;
    public onDefault(dialogId: string, dialogArgs?: any): this;
    public onDefault(dialogId: any, dialogArgs?: any): this {
        var fn: IDialogHandler<ICommandArgs>;
        if (Array.isArray(dialogId)) {
            fn = actions.DialogAction.waterfall(dialogId);
        } else if (typeof dialogId == 'string') {
            fn = actions.DialogAction.beginDialog(dialogId, dialogArgs);
        } else {
            fn = dialogId;
        }
        this.default = { fn: fn };
        return this;
    }
}