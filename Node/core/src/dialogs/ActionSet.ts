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
import consts = require('../consts');
import utils = require('../utils');

export interface IDialogActionHandler {
    (session: ses.Session, args: IRecognizeActionResult): void;
}

export interface IDialogActionOptions {
    matches?: RegExp;
    intentThreshold?: number;
    dialogArgs?: any;
}

export interface IRecognizeActionResult {
    score: number;
    action?: string;
    expression?: RegExp;
    matched?: string[]; 
    data?: string;
    dialogId?: string;
    dialogIndex?: number;
}

export class ActionSet {
    private actions: { [name: string]: IDialogActionHandlerEntry; } = {};

    public recognizeAction(message: IMessage, cb: (err: Error, result: IRecognizeActionResult) => void): void {
        var result: IRecognizeActionResult = { score: 0.0 };
        if (message && message.text) {
            if (message.text.indexOf('action?') == 0) {
                var parts = message.text.split('?')[1].split('=');
                if (this.actions.hasOwnProperty(parts[0])) {
                    result.score = 1.0;
                    result.action = parts[0];
                    if (parts.length > 1) {
                        result.data = parts[1];
                    }
                }
            } else {
                for (var name in this.actions) {
                    var entry = this.actions[name];
                    if (message.text && entry.options.matches) {
                        var exp = entry.options.matches;
                        var matches = exp.exec(message.text);
                        if (matches && matches.length) {
                            var matched = matches[0];
                            var score = matched.length / message.text.length;
                            if (score > result.score && score >= (entry.options.intentThreshold || 0.1)) {
                                result.score = score;
                                result.action = name;
                                result.expression = exp;
                                result.matched = matches;
                                if (score == 1.0) {
                                    break;
                                }
                            }
                        }
                    }
                }
            }        
        }
        cb(null, result);
    }

    public invokeAction(session: ses.Session, recognizeResult: IRecognizeActionResult): void {
        this.actions[recognizeResult.action].handler(session, recognizeResult);
    }

    public cancelAction(name: string, msg?: string|string[]|IMessage|IIsMessage, options?: IDialogActionOptions): this {
        return this.action(name, (session, args) => {
            if (args && typeof args.dialogIndex === 'number') {
                if (msg) {
                    session.send(msg)
                }
                session.cancelDialog(args.dialogIndex);
            }
        }, options);
    }

    public reloadAction(name: string, msg?: string|string[]|IMessage|IIsMessage, options: IDialogActionOptions = {}): this {
        return this.action(name, (session, args) => {
            if (msg) {
                session.send(msg)
            }
            session.cancelDialog(args.dialogIndex, args.dialogId, options.dialogArgs);
        }, options);
    }

    public beginDialogAction(name: string, id: string, options: IDialogActionOptions = {}): this {
        return this.action(name, (session, args) => {
            if (options.dialogArgs) {
                utils.copyTo(options.dialogArgs, args);
            }
            if (id.indexOf(':') < 0) {
                var lib = args.dialogId ? args.dialogId.split(':')[0] : consts.Library.default;
                id = lib + ':' + id;
            }
            session.beginDialog(id, args);
        }, options);
    }

    public endConversationAction(name: string, msg?: string|string[]|IMessage|IIsMessage, options?: IDialogActionOptions): this {
        return this.action(name, (session, args) => {
            session.endConversation(msg);
        }, options);
    }

    private action(name: string, handler: IDialogActionHandler, options: IDialogActionOptions = {}): this {
        // Ensure unique
        if (this.actions.hasOwnProperty(name)) {
            throw new Error("DialogAction[" + name + "] already exists.")
        }
        this.actions[name] = { handler: handler, options: options };
        return this;
    }
}

interface IDialogActionHandlerEntry {
    handler: IDialogActionHandler;
    options: IDialogActionOptions;
}