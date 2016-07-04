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
import mb = require('../Message'); 
import sd = require('./SimpleDialog');
import dl = require('../bots/Library');
import utils = require('../utils');
import er = require('./EntityRecognizer');

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
    confirmPrompt?: string|string[];
    optionalPrompt?: string|string[];
}

interface IFieldArgs extends IFieldOptions {
    field: string;
    fieldType: FieldType;
    value: any;
    prompt?: string|string[];
    enumValues?: string[];
    dialogId?: string;
    dialogArgs?: any;
    returnResults?: boolean;
}

export class Fields {
    static text(field: string, prompt: string|string[], options: IFieldOptions = {}): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            var args: IFieldArgs = utils.clone(options);
            args.field = field;
            args.fieldType = FieldType.text;
            args.prompt = prompt;
            processField(session, results, next, args);
        };
    }

    static number(field: string, prompt: string|string[], options: IFieldOptions = {}): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            var args: IFieldArgs = utils.clone(options);
            args.field = field;
            args.fieldType = FieldType.number;
            args.prompt = prompt;
            processField(session, results, next, args);
        };
    }

    static confirm(field: string, prompt: string|string[], options: IFieldOptions = {}): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            var args: IFieldArgs = utils.clone(options);
            args.field = field;
            args.fieldType = FieldType.confirm;
            args.prompt = prompt;
            processField(session, results, next, args);
        };
    }
    
    static choice(field: string, prompt: string|string[], choices: string|Object|string[], options: IFieldOptions = {}): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            var args: IFieldArgs = utils.clone(options);
            args.field = field;
            args.fieldType = FieldType.choice;
            args.prompt = prompt;
            args.enumValues = er.EntityRecognizer.expandChoices(choices);
            args.listStyle = args.hasOwnProperty('listStyle') ? args.listStyle : prompts.ListStyle.auto;
            processField(session, results, next, args);
        };
    }

    static time(field: string, prompt: string|string[], options: IFieldOptions = {}): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            var args: IFieldArgs = utils.clone(options);
            args.field = field;
            args.fieldType = FieldType.time;
            args.prompt = prompt;
            processField(session, results, next, args);
        };
    }
    
    static dialog(field: string, dialogId: string, dialogArgs?: any, options: IFieldOptions = {}): action.IDialogWaterfallStep {
        return function (session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void): void {
            var args: IFieldArgs = utils.clone(options);
            args.field = field;
            args.fieldType = FieldType.time;
            args.dialogId = dialogId;
            args.dialogArgs = dialogArgs;
            processField(session, results, next, args);
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
                session.endDialogWithResult({ resumed: dialog.ResumeReason.completed, response: form });
            } else {
                session.endDialogWithResult(r);
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

// Add field dialog
dl.systemLib.dialog(consts.DialogId.Field, new sd.SimpleDialog((session, args) => {
    var fieldArgs: IFieldArgs = session.dialogData;
    function callPrompt() {
        fieldArgs.returnResults = true;
        if (fieldArgs.fieldType == FieldType.dialog) {
            session.beginDialog(fieldArgs.dialogId, fieldArgs.dialogArgs);
        } else {
            session.beginDialog(consts.DialogId.Prompts, fieldArgs);
        } 
    }
    
    if (args.hasOwnProperty('resumed')) {
        if (fieldArgs.returnResults || args.resumed !== dialog.ResumeReason.completed) {
            session.endDialog(args);
        } else {
            // Check confirmation
            if (fieldArgs.confirmPrompt && !args.response) {
                callPrompt();
            } else if (fieldArgs.optionalPrompt && args.response) {
                callPrompt();
            } else {
                session.endDialogWithResult({ response: fieldArgs.value, resumed: dialog.ResumeReason.completed });
            }
        }
        
    } else {
        // First call so save args to dialogData
        for (var key in args) {
            if (args.hasOwnProperty(key) && typeof args[key] !== 'function') {
                (<any>fieldArgs)[key] = args[key];
            }
        }
        
        // Check for confirm prompt
        if (fieldArgs.confirmPrompt || fieldArgs.optionalPrompt) {
            prompts.Prompts.confirm(session, fieldArgs.confirmPrompt || fieldArgs.optionalPrompt);
        } else {
            callPrompt();
        }
    }
}));

function processField(session: ses.Session, results: dialog.IDialogResult<any>, next: (results?: dialog.IDialogResult<any>) => void, args: IFieldArgs) {
    // Save results and see if we should continue.
    var r = saveResults(session, results); 
    if (r.resumed == dialog.ResumeReason.completed) {
        try {
            // Determine data type
            var dataType: string;
            switch (args.fieldType) {
                case FieldType.choice:
                case FieldType.text:
                    dataType = 'text';
                    break;
                case FieldType.confirm:
                    dataType = 'boolean';
                    break;
                case FieldType.number:
                case FieldType.time:
                    dataType = 'number';
                default:
                    dataType = null;
                    break;
            }
            
            // Check to see if we should skip the prompt
            onPrompt(session, args.field, dataType, args, (skip) => {
                // Check to see if field has a value and override skip
                args.value = session.dialogData[consts.Data.Form][args.field];
                var valueType = typeof args.value;
                var hasValue = (valueType !== 'null' && valueType !== 'undefined');
                if (args.confirmPrompt) {
                    if (hasValue) {
                        skip = false;
                        args.confirmPrompt = expandTemplate(session, args.field, <any>args.confirmPrompt);
                    } else {
                        delete args.confirmPrompt;
                    }
                } 
                if (args.optionalPrompt) {
                    if (!hasValue) {
                        skip = false;
                        args.optionalPrompt = expandTemplate(session, args.field, <any>args.optionalPrompt);
                    } else {
                        delete args.optionalPrompt;
                    }
                }
                
                if (!skip) {
                    // Expand prompts
                    if (args.prompt) {
                        args.prompt = expandTemplate(session, args.field, args.prompt);
                    }
                    if (args.retryPrompt && typeof args.retryPrompt !== 'object') {
                        args.retryPrompt = expandTemplate(session, args.field, <any>args.retryPrompt);
                    }
                    
                    // Begin field prompt(s)
                    session.dialogData[consts.Data.Field] = { type: args.fieldType, name: args.field };
                    session.beginDialog(consts.DialogId.Field, args);
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
}

function saveResults(session: ses.Session, results: dialog.IDialogResult<any>): dialog.IDialogResult<any> {
    var r: dialog.IDialogResult<any>;
    if (results && results.hasOwnProperty('resumed')) {
        if (session.dialogData.hasOwnProperty(consts.Data.Form) && session.dialogData.hasOwnProperty(consts.Data.Field)) {
            // Save field we're waiting on.
            var field: IField = session.dialogData[consts.Data.Field];
            delete session.dialogData[consts.Data.Field];
            if (results.resumed == dialog.ResumeReason.completed) {
                var dataType = typeof results.response;
                if (dataType == 'object') {
                    switch (field.type) {
                        case FieldType.choice:
                            session.dialogData[consts.Data.Form][field.name] = results.response.entity;
                            break;
                        case FieldType.time:
                            if (results.response.resolution && results.response.resolution.start) {
                                session.dialogData[consts.Data.Form][field.name] = results.response.resolution.start.getTime();
                            }
                            break;
                        default:
                            session.dialogData[consts.Data.Form][field.name] = results.response;
                            break;
                    }
                } else {
                    session.dialogData[consts.Data.Form][field.name] = results.response;
                }
            } else {
                r = results;
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
        var dataType = typeof form[field];
        switch (dataType) {
            case 'null':
            case 'undefined':
                cb(false);
                break;
            default:
                cb(type == null || dataType == type);
                break;
        }
    } else {
        cb(false);
    }
}

function expandTemplate(session: ses.Session, field: string, prompt: string|string[]): string {
    var form = session.dialogData[consts.Data.Form];
    var value = form.hasOwnProperty(field) ? form[field] : '';
    var args = { userData: session.userData, form: form, value: value };
    return session.gettext(mb.Message.randomPrompt(prompt), args);
}