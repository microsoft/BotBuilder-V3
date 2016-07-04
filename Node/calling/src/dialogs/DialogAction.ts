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

import ses = require('../CallSession');
import consts = require('../consts');
import utils = require('../utils');
import dlg = require('./Dialog');
import prompts = require('./Prompts');
import simple = require('./SimpleDialog');

export interface IDialogWaterfallStep {
    (session: ses.CallSession, result?: any, skip?: (results?: dlg.IDialogResult<any>) => void): void;
}

export class DialogAction {
    static send(msg: string|string[], ...args: any[]): IDialogHandler<any> {
        args.splice(0, 0, msg);
        return function sendAction(s: ses.CallSession) {
            // Send a message to the user.
            ses.CallSession.prototype.send.apply(s, args);
        };
    }

    static beginDialog<T>(id: string, args?: T): IDialogHandler<T> {
        return function beginDialogAction(s: ses.CallSession, a: any) {
            // Handle calls where we're being resumed.
            if (a && a.hasOwnProperty('resumed')) {
                var r = <dlg.IDialogResult<any>>a;
                if (r.error) {
                    s.error(r.error);
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
        return function endDialogAction(s: ses.CallSession) {
            // End dialog
            s.endDialog(result);
        };
    }
}

export function waterfall(steps: IDialogWaterfallStep[]): IDialogHandler<any> {
    return function waterfallAction(s: ses.CallSession, r: dlg.IDialogResult<any>) {
        var skip = (result?: dlg.IDialogResult<any>) => {
            result = result || <any>{};
            if (!result.resumed) {
                result.resumed = dlg.ResumeReason.forward;
            }
            waterfallAction(s, result);
        };

        // Check for a playPromptOutcome
        // - There's an issue with callign bots that sending a playPrompt action causes
        //   a recursive callback into the waterfall step. We want to catch that and treat
        //   it like any other dialog completing to get the waterfall to advance.
        if (!r && s.dialogData.hasOwnProperty(consts.Data.WaterfallStep)) {
            r = { resumed: dlg.ResumeReason.completed };
        }

        // Check for continuation of waterfall
        if (r && r.hasOwnProperty('resumed')) {
            // Adjust step based on users utterance
            var step = s.dialogData[consts.Data.WaterfallStep];
            switch (r.resumed) {
                case dlg.ResumeReason.back:
                    step -= 1;
                    break;
                default:
                    step++;
            }

            // Handle result
            if (step >= 0 && step < steps.length) {
                // Execute next step of the waterfall
                try {
                    s.dialogData[consts.Data.WaterfallStep] = step;
                    steps[step](s, r, skip);
                } catch (e) {
                    s.error(e);
                }
            } else {
                // End the current dialog and return results to parent
                s.endDialogWithResult(r);
            }
        } else if (steps && steps.length > 0) {
            // Start waterfall
            try {
                s.dialogData[consts.Data.WaterfallStep] = 0;
                steps[0](s, r, skip);
            } catch (e) {
                s.error(e);
            }
        } else {
            // Empty waterfall so end dialog with not completed
            s.endDialogWithResult({ resumed: dlg.ResumeReason.notCompleted });
        }
    }; 
}
