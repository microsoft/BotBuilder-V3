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
import ses = require('../Session');
import consts = require('../consts');
import action = require('./DialogAction');
import prompts = require('./Prompts');

enum FieldType { text, number, confirm, choice, time, dialog }

interface IField {
    type: FieldType;
    name: string;
}

export interface IFieldConext {
    userData: any;
    form: any;
    field: string;
}

export interface IFieldPromptHandler {
    (context: IFieldConext, next: (skip: boolean) => void): void;
}

export interface IFieldOptions extends prompts.IPromptOptions {
    onPrompt?: IFieldPromptHandler;
}

export class Fields {
    static text(field: string, prompt: string|string[], options: IFieldOptions = {}): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            // Save results and see if we should continue.
            var r = saveResults(session, results); 
            if (r.resumed == dialog.ResumeReason.completed) {
                try {
                    // Check to see if we should skip the prompt
                    onPrompt(session, field, 'string', options, (skip) => {
                        if (!skip) {
                            session.dialogData[consts.Data.Field] = { type: FieldType.text, name: field };
                            prompts.Prompts.text(session, prompt);
                        } else {
                            next();
                        }
                    });
                } catch (e) {
                    next({ error: e instanceof Error ? e : new Error(e.toString()), resumed: dialog.ResumeReason.notCompleted });
                }
            } else {
                next(r);
            }         
        };
    }

    static number(field: string, prompt: string|string[], options: IFieldOptions = {}): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            // Save results and see if we should continue.
            var r = saveResults(session, results); 
            if (r.resumed == dialog.ResumeReason.completed) {
                try {
                    // Check to see if we should skip the prompt
                    onPrompt(session, field, 'number', options, (skip) => {
                        if (!skip) {
                            session.dialogData[consts.Data.Field] = { type: FieldType.number, name: field };
                            prompts.Prompts.number(session, prompt, options);
                        } else {
                            next();
                        }
                    });
                } catch (e) {
                    next({ error: e instanceof Error ? e : new Error(e.toString()), resumed: dialog.ResumeReason.notCompleted });
                }
            } else {
                next(r);
            }         
        };
    }

    static confirm(field: string, prompt: string|string[], options: IFieldOptions = {}): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            // Save results and see if we should continue.
            var r = saveResults(session, results); 
            if (r.resumed == dialog.ResumeReason.completed) {
                try {
                    // Check to see if we should skip the prompt
                    onPrompt(session, field, 'boolean', options, (skip) => {
                        if (!skip) {
                            session.dialogData[consts.Data.Field] = { type: FieldType.confirm, name: field };
                            prompts.Prompts.confirm(session, prompt, options);
                        } else {
                            next();
                        }
                    });
                } catch (e) {
                    next({ error: e instanceof Error ? e : new Error(e.toString()), resumed: dialog.ResumeReason.notCompleted });
                }
            } else {
                next(r);
            }         
        };
    }
    
    static choice(field: string, prompt: string|string[], choices: string|Object|string[], options: IFieldOptions = {}): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            // Save results and see if we should continue.
            var r = saveResults(session, results); 
            if (r.resumed == dialog.ResumeReason.completed) {
                try {
                    // Check to see if we should skip the prompt
                    onPrompt(session, field, 'text', options, (skip) => {
                        if (!skip) {
                            session.dialogData[consts.Data.Field] = { type: FieldType.choice, name: field };
                            prompts.Prompts.choice(session, prompt, choices, options);
                        } else {
                            next();
                        }
                    });
                } catch (e) {
                    next({ error: e instanceof Error ? e : new Error(e.toString()), resumed: dialog.ResumeReason.notCompleted });
                }
            } else {
                next(r);
            }         
        };
    }

    static time(field: string, prompt: string|string[], options: IFieldOptions = {}): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            // Save results and see if we should continue.
            var r = saveResults(session, results); 
            if (r.resumed == dialog.ResumeReason.completed) {
                try {
                    // Check to see if we should skip the prompt
                    onPrompt(session, field, 'number', options, (skip) => {
                        if (!skip) {
                            session.dialogData[consts.Data.Field] = { type: FieldType.time, name: field };
                            prompts.Prompts.time(session, prompt, options);
                        } else {
                            next();
                        }
                    });
                } catch (e) {
                    next({ error: e instanceof Error ? e : new Error(e.toString()), resumed: dialog.ResumeReason.notCompleted });
                }
            } else {
                next(r);
            }         
        };
    }
    
    static dialog(field: string, dialogId: string, dialogArgs?: any, options: IFieldOptions = {}): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            // Save results and see if we should continue.
            var r = saveResults(session, results); 
            if (r.resumed == dialog.ResumeReason.completed) {
                try {
                    // Check to see if we should skip the prompt
                    onPrompt(session, field, null, options, (skip) => {
                        if (!skip) {
                            session.dialogData[consts.Data.Field] = { type: FieldType.dialog, name: field };
                            session.beginDialog(dialogId, dialogArgs || session.dialogData[consts.Data.Form][field]);
                        } else {
                            next();
                        }
                    });
                } catch (e) {
                    next({ error: e instanceof Error ? e : new Error(e.toString()), resumed: dialog.ResumeReason.notCompleted });
                }
            } else {
                next(r);
            }         
        };
    }
    
    static endForm(): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            // Save results and see if we should continue.
            var r = saveResults(session, results); 
            if (r.resumed == dialog.ResumeReason.completed) {
                // Pass form to next waterfall step
                var form = session.dialogData[consts.Data.Form];
                delete session.dialogData[consts.Data.Form];
                next({ resumed: dialog.ResumeReason.completed, response: form });
            } else {
                next(r);
            }
        };
    }

    static returnForm(): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            // Save results and see if we should continue.
            var r = saveResults(session, results); 
            if (r.resumed == dialog.ResumeReason.completed) {
                // Return form to parent
                var form = session.dialogData[consts.Data.Form];
                delete session.dialogData[consts.Data.Form];
                session.endDialog({ resumed: dialog.ResumeReason.completed, response: form });
            } else {
                session.endDialog(r);
            }
        };
    }
    
    static onPromptUseDefault(): IFieldPromptHandler {
        return function (context, next) {
            var type = typeof context.form[context.field];
            if (type === 'undefined' || type === 'null') {
                type = typeof context.userData[context.field];
                if (type === 'undefined' || type === 'null') {
                    next(false);
                } else {
                    context.form[context.field] = context.userData[context.field];
                    next(true);
                }
            } else {
                next(true);
            }
        };
    }
}

function saveResults(session: ses.Session, results: dialog.IDialogResult<any>): dialog.IDialogResult<any> {
    var r: dialog.IDialogResult<any>;
    if (results && results.hasOwnProperty('resumed')) {
        if (session.dialogData.hasOwnProperty(consts.Data.Form) && session.dialogData.hasOwnProperty(consts.Data.Field)) {
            // Save field we're waiting on.
            var field: IField = session.dialogData[consts.Data.Field];
            delete session.dialogData[consts.Data.Field];
            if (results.response) {
                switch(field.type) {
                    case FieldType.choice:
                        session.dialogData[consts.Data.Form][field.name] = results.response.entity;
                        break;
                    case FieldType.time:
                        if (results.response.resolution && results.response.resolution.start) {
                            session.dialogData[consts.Data.Form][field.name] = results.response.resolution.start.getTime();
                        }
                        break;
                    case FieldType.confirm:
                    case FieldType.number:
                    case FieldType.text:
                    case FieldType.dialog:
                    default:
                        session.dialogData[consts.Data.Form][field.name] = results.response;
                        break;
                }
            } else {
                r = results.response;
            }
        } else if (typeof results.response === 'object') {
            // Save passed in form
            session.dialogData[consts.Data.Form] = results.response;
        }
        if (!r) {
            r = { resumed: dialog.ResumeReason.completed };
        }
    } else {
        session.dialogData[consts.Data.Form] = results || {};
        r = { resumed: dialog.ResumeReason.completed };
    }
    return r;
}

function onPrompt(session: ses.Session, field: string, type: string, options: IFieldOptions, cb: (skip: boolean) => void): void {
    var form = session.dialogData[consts.Data.Form];
    var context: IFieldConext = { userData: session.userData, form: form, field: field };    
    if (options.onPrompt) {
        options.onPrompt(context, cb);
    } else if (form && form.hasOwnProperty(field)) {
        var fieldType = typeof form[field];
        switch (fieldType) {
            case 'null':
            case 'undefined':
                cb(false);
                break;
            default:
                cb(type == null || fieldType == type);
                break;
        }
    } else {
        cb(false);
    }
}