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
import consts = require('../consts');

export interface IDialogWaterfallStep {
    (session: ISession, result?: any, skip?: (results?: dialog.IDialogResult<any>) => void): void;
}

export class DialogAction {
    static send(msg: string, ...args: any[]): IDialogHandler<any> {
        args.splice(0, 0, msg);
        return function sendAction(s: ISession) {
            // Send a message to the user.
            session.Session.prototype.send.apply(s, args);
        };
    }

    static beginDialog<T>(id: string, args?: T): IDialogHandler<T> {
        return function beginDialogAction(s: ISession, a: any) {
            // Handle calls where we're being resumed.
            if (a && a.hasOwnProperty('resumed')) {
                // We have to implement logic to ensure our callstack gets persisted.
                var r = <dialog.IDialogResult<any>>a;
                if (r.error) {
                    s.error(r.error);
                } else {
                    s.send();
                }
            } else  {
                // Merge args
                if (args) {
                    a = a || {};
                    for (var key in args) {
                        if (args.hasOwnProperty(key)) {
                            a[key] = (<any>args)[key];
                        }
                    }
                }

                // Begin a new dialog
                s.beginDialog(id, a);
            }
        };
    }

    static endDialog(result?: any): IDialogHandler<any> {
        return function endDialogAction(s: ISession) {
            // End dialog
            s.endDialog(result);
        };
    }

    static waterfall(steps: IDialogWaterfallStep[]): IDialogHandler<any> {
        return function waterfallAction(s: ISession, r: dialog.IDialogResult<any>) {
            var skip = (result?: dialog.IDialogResult<any>) => {
                result = result || <any>{};
                if (!result.resumed) {
                    result.resumed = dialog.ResumeReason.forward;
                }
                waterfallAction(s, result);
            };

            try {
                // Check for continuation of waterfall
                if (r && r.hasOwnProperty('resumed')) {
                    // Adjust step based on users utterance
                    var step = s.dialogData[consts.Data.WaterfallStep];
                    switch (r.resumed) {
                        case dialog.ResumeReason.back:
                            step -= 1;
                            break;
                        default:
                            step++;
                    }

                    // Handle result
                    if (step >= 0 && step < steps.length) {
                        s.dialogData[consts.Data.WaterfallStep] = step;
                        steps[step](s, r, skip);
                    } else {
                        delete s.dialogData[consts.Data.WaterfallStep];
                        s.send();
                    }
                } else if (steps && steps.length > 0) {
                    // Start waterfall
                    s.dialogData[consts.Data.WaterfallStep] = 0;
                    steps[0](s, r, skip);
                } else {
                    delete s.dialogData[consts.Data.WaterfallStep];
                    s.send();
                }
            } catch (e) {
                delete s.dialogData[consts.Data.WaterfallStep];
                s.endDialog({ resumed: dialog.ResumeReason.notCompleted, error: e instanceof Error ? e : new Error(e.toString()) });
            }
        }; 
    }
}