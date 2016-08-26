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
import dlg = require('./Dialog');
import prompts = require('./Prompts');
import simple = require('./SimpleDialog');
import logger = require('../logger');

export interface IDialogWaterfallStep {
    (session: ses.Session, result?: any, skip?: (results?: dlg.IDialogResult<any>) => void): void;
}

export class DialogAction {
    static send(msg: string|string[]|IMessage|IIsMessage, ...args: any[]): IDialogHandler<any> {
        args.splice(0, 0, msg);
        return function sendAction(s: ses.Session) {
            // Send a message to the user.
            ses.Session.prototype.send.apply(s, args);
        };
    }

    static beginDialog<T>(id: string, args?: T): IDialogHandler<T> {
        return function beginDialogAction(s: ses.Session, a: any) {
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
        return function endDialogAction(s: ses.Session) {
            // End dialog
            s.endDialog(result);
        };
    }
    
    static validatedPrompt(promptType: prompts.PromptType, validator: (response: any) => boolean): dlg.Dialog {
        return new simple.SimpleDialog((s: ses.Session, r: dlg.IDialogResult<any>) => {
            r = r || <any>{};

            // Validate response
            var valid = false;
            if (r.response) {
                try {
                    valid = validator(r.response);
                } catch (e) {
                    s.error(e);
                }
            } 
            
            // Check for the user canceling the prompt
            var canceled = false;
            switch (r.resumed) {
                case dlg.ResumeReason.canceled:
                case dlg.ResumeReason.forward:
                case dlg.ResumeReason.back:
                    canceled = true;
                    break;
            }

            // Return results or prompt the user     
            if (valid || canceled) {
                // The user either entered a properly formatted response or they canceled by saying "nevermind"  
                s.endDialogWithResult(r);
            } else if (!s.dialogData.hasOwnProperty('prompt')) {
                // First call to the prompt so save args passed to the prompt
                s.dialogData = utils.clone(r);
                s.dialogData.promptType = promptType;
                if (!s.dialogData.hasOwnProperty('maxRetries')) {
                    s.dialogData.maxRetries = 2;
                }
                
                // Prompt user
                var a: prompts.IPromptArgs = utils.clone(s.dialogData);
                a.maxRetries = 0;
                s.beginDialog(consts.DialogId.Prompts, a);
            } else if (s.dialogData.maxRetries > 0) {
                // Reprompt the user
                s.dialogData.maxRetries--;
                var a: prompts.IPromptArgs = utils.clone(s.dialogData);
                a.maxRetries = 0;
                a.prompt = s.dialogData.retryPrompt || "I didn't understand. " + s.dialogData.prompt;
                s.beginDialog(consts.DialogId.Prompts, a);
            } else {
                // User failed to enter a valid response
                s.endDialogWithResult({ resumed: dlg.ResumeReason.notCompleted });
            }
        }); 
    }
}

export function waterfall(steps: IDialogWaterfallStep[]): IDialogHandler<any> {
    return function waterfallAction(s: ses.Session, r: dlg.IDialogResult<any>) {
        var skip = (result?: dlg.IDialogResult<any>) => {
            result = result || <any>{};
            if (result.resumed == null) {
                result.resumed = dlg.ResumeReason.forward;
            }
            waterfallAction(s, result);
        };

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
                    logger.info(s, 'waterfall() step %d of %d', step + 1, steps.length);
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
                logger.info(s, 'waterfall() step %d of %d', 1, steps.length);
                s.dialogData[consts.Data.WaterfallStep] = 0;
                steps[0](s, r, skip);
            } catch (e) {
                s.error(e);
            }
        } else {
            // Empty waterfall so end dialog with not completed
            logger.warn(s, 'waterfall() empty waterfall detected');
            s.endDialogWithResult({ resumed: dlg.ResumeReason.notCompleted });
        }
    }; 
}
