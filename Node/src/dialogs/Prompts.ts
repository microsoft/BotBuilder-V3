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
import session = require('../Session');
import consts = require('../consts');
import entities = require('./EntityRecognizer');

export enum PromptType { text, number, confirm, choice, time }

export enum ListStyle { none, inline, list }

export interface IPromptOptions {
    retryPrompt?: string;
    maxRetries?: number;
    refDate?: number;
    listStyle?: ListStyle;
}

export interface IPromptArgs extends IPromptOptions {
    promptType: PromptType;
    prompt: string;
    enumValues?: string[];
}

export interface IPromptResult<T> extends dialog.IDialogResult<T> {
    promptType?: PromptType;
}

export interface IPromptRecognizerResult<T> extends IPromptResult<T> {
    handled?: boolean;
}

export interface IPromptRecognizer {
    recognize<T>(args: IPromptRecognizerArgs, callback: (result: IPromptRecognizerResult<T>) => void, session?: ISession): void;
}

export interface IPromptRecognizerArgs {
    promptType: PromptType;
    language: string;
    utterance: string;
    enumValues?: string[];
    refDate?: number;
    compareConfidence(language: string, utterance: string, score: number, callback: (handled: boolean) => void): void;
}

export interface IPromptsOptions {
    recognizer?: IPromptRecognizer
}

export interface IChronoDuration extends IEntity {
    resolution: {
        start: Date;
        end?: Date;
        ref?: Date;
    };
}

export class SimplePromptRecognizer implements IPromptRecognizer {
    private cancelExp = /^(cancel|nevermind|never mind|back|stop|forget it)/i;

    public recognize(args: IPromptRecognizerArgs, callback: (result: IPromptRecognizerResult<any>) => void, session?: ISession): void {
        this.checkCanceled(args, () => {
            try {
                // Recognize value
                var score = 0.0;
                var response: any;
                var text = args.utterance.trim();
                switch (args.promptType) {
                    case PromptType.text:
                        // This is an open ended question so it's a little tricky to know what to pass as a confidence
                        // score. Currently we're saying that we have 0.1 confidence that we understand the users intent
                        // which will give all of the prompts parents a chance to capture the utterance. If no one 
                        // captures the utterance we'll return the full text of the utterance as the result.
                        score = 0.1;
                        response = text;
                        break;
                    case PromptType.number:
                        var n = entities.EntityRecognizer.parseNumber(text);
                        if (!isNaN(n)) {
                            var score = n.toString().length / text.length;
                            response = n;
                        }
                        break;
                    case PromptType.confirm:
                        var b = entities.EntityRecognizer.parseBoolean(text);
                        if (typeof b == 'boolean') {
                            score = 1.0;
                            response = b;
                        }
                        break;
                    case PromptType.time:
                        var entity = entities.EntityRecognizer.recognizeTime(text, args.refDate ? new Date(args.refDate) : null);
                        if (entity) {
                            score = entity.entity.length / text.length;
                            response = entity;
                        } 
                        break;
                    case PromptType.choice:
                        var best = entities.EntityRecognizer.findBestMatch(args.enumValues, text);
                        if (!best) {
                            var n = entities.EntityRecognizer.parseNumber(text);
                            if (!isNaN(n) && n > 0 && n <= args.enumValues.length) {
                                best = { index: n, entity: args.enumValues[n - 1], score: 1.0 };
                            }
                        }
                        if (best) {
                            score = best.score;
                            response = best;
                        }
                        break;
                    default:
                }

                // Return results
                args.compareConfidence(args.language, text, score, (handled) => {
                    if (!handled && score > 0) {
                        callback({ resumed: dialog.ResumeReason.completed, promptType: args.promptType, response: response });
                    } else {
                        callback({ resumed: dialog.ResumeReason.notCompleted, promptType: args.promptType, handled: handled });
                    }
                });
            } catch (err) {
                callback({ resumed: dialog.ResumeReason.notCompleted, promptType: args.promptType, error: err instanceof Error ? err : new Error(err.toString()) });
            }
        }, callback);
    }

    protected checkCanceled(args: IPromptRecognizerArgs, onContinue: Function, callback: (result: IPromptRecognizerResult<IEntity>) => void) {
        if (!this.cancelExp.test(args.utterance.trim())) {
            onContinue();
        } else {
            callback({ resumed: dialog.ResumeReason.canceled, promptType: args.promptType });
        }
    }
} 

export class Prompts extends dialog.Dialog {
    private static options: IPromptsOptions = {
        recognizer: new SimplePromptRecognizer()
    };

    public begin(session: ISession, args: IPromptArgs): void {
        args = <any>args || {};
        args.maxRetries = args.maxRetries || 1; 
        for (var key in args) {
            if (args.hasOwnProperty(key)) {
                session.dialogData[key] = (<any>args)[key];
            }
        }
        session.send(args.prompt);
    }

    public replyReceived(session: ISession): void {
        var args: IPromptArgs = session.dialogData;
        Prompts.options.recognizer.recognize(
            {
                promptType: args.promptType,
                utterance: session.message.text,
                language: session.message.language,
                enumValues: args.enumValues,
                refDate: args.refDate,
                compareConfidence: function (language, utterance, score, callback) {
                    session.compareConfidence(language, utterance, score, callback);
                }
            }, (result) => {
                if (!result.handled) {
                    if (result.error || result.resumed == dialog.ResumeReason.completed ||
                        result.resumed == dialog.ResumeReason.canceled || args.maxRetries == 0) {
                        result.promptType = args.promptType;
                        session.endDialog(result);
                    } else {
                        args.maxRetries--;
                        session.send(args.retryPrompt || "I didn't understand. " + args.prompt);
                    }
                }
            });
    }

    static configure(options: IPromptsOptions): void {
        if (options) {
            for (var key in options) {
                if (options.hasOwnProperty(key)) {
                    (<any>Prompts.options)[key] = (<any>options)[key];
                }
            }
        }
    }

    static text(ses: session.Session, prompt: string): void {
        beginPrompt(ses, {
            promptType: PromptType.text,
            prompt: prompt
        });
    }

    static number(ses: session.Session, prompt: string, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.number;
        args.prompt = prompt;
        beginPrompt(ses, args);
    }

    static confirm(ses: session.Session, prompt: string, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.confirm;
        args.prompt = prompt;
        beginPrompt(ses, args);
    }

    static choice(ses: session.Session, prompt: string, choices: string, options?: IPromptOptions): void;
    static choice(ses: session.Session, prompt: string, choices: Object, options?: IPromptOptions): void;
    static choice(ses: session.Session, prompt: string, choices: string[], options?: IPromptOptions): void;
    static choice(ses: session.Session, prompt: string, choices: any, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.choice;
        args.prompt = prompt;
        args.enumValues = entities.EntityRecognizer.expandChoices(choices);
        args.listStyle = args.listStyle || ListStyle.list;
        
        // Format list
        var connector = '', list: string;
        switch (args.listStyle) {
            case ListStyle.list:
                list = '\n   ';
                args.enumValues.forEach((value, index) => {
                    list += connector + (index + 1) + '. ' + value;
                    connector = '\n   ';
                });
                args.prompt += list;
                break;
            case ListStyle.inline:
                list = ' ';
                args.enumValues.forEach((value, index) => {
                    list += connector + (index + 1) + '. ' + value;
                    if (index == args.enumValues.length - 2) {
                        connector = index == 0 ? ' or ' : ', or ';
                    } else {
                        connector = ', ';
                    } 
                });
                args.prompt += list;
                break;
        }
        beginPrompt(ses, args);
    }

    static time(ses: session.Session, prompt: string, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.time;
        args.prompt = prompt;
        beginPrompt(ses, args);
    }
}

function beginPrompt(ses: session.Session, args: IPromptArgs) {
    if (!ses.dialogs.hasDialog(consts.DialogId.Prompts)) {
        ses.dialogs.add(consts.DialogId.Prompts, new Prompts());
    }
    ses.beginDialog(consts.DialogId.Prompts, args);
}
