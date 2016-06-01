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
import entities = require('./EntityRecognizer');
import mb = require('../Message');
import Channel = require('../Channel');
import dc = require('./DialogCollection');

export enum PromptType { text, number, confirm, choice, time, attachment }

export enum ListStyle { none, inline, list, button, auto }

export interface IPromptOptions {
    retryPrompt?: string | string[] | IMessage;
    maxRetries?: number;
    refDate?: number;
    listStyle?: ListStyle;
}

export interface IPromptArgs extends IPromptOptions {
    promptType: PromptType;
    prompt: string | string[] | IMessage;
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
    attachments: IAttachment[];
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
    private cancelExp = /^(cancel|nevermind|never mind|stop|forget it|quit)/i;

    public recognize(args: IPromptRecognizerArgs, callback: (result: IPromptRecognizerResult<any>) => void, session?: ISession): void {
        this.checkCanceled(args, () => {
            try {
                // Recognize value
                var score = 0.0;
                var response: any;
                var text = args.utterance.trim();
                switch (args.promptType) {
                    default:
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
                        if (typeof b !== 'boolean') {
                            var n = entities.EntityRecognizer.parseNumber(text);
                            if (!isNaN(n) && n > 0 && n <= 2) {
                                b = (n === 1);
                            }
                            
                        }
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
                                best = { index: n - 1, entity: args.enumValues[n - 1], score: 1.0 };
                            }
                        }
                        if (best) {
                            score = best.score;
                            response = best;
                        }
                        break;
                    case PromptType.attachment:
                        if (args.attachments && args.attachments.length > 0) {
                            score = 1.0;
                            response = args.attachments;
                        }
                        break;
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
    private static defaultRetryPrompt = {
        text: "I didn't understand. Please try again.",
        number: "I didn't recognize that as a number. Please enter a number.",
        confirm: "I didn't understand. Please answer 'yes' or 'no'.",
        choice: "I didn't understand. Please choose an option from the list.", 
        time: "I didn't recognize the time you entered. Please try again.", 
        attachment: "I didn't receive a file. Please try again."  
    };

    public begin(session: ses.Session, args: IPromptArgs): void {
        args = <any>args || {};
        args.maxRetries = args.maxRetries || 1; 
        for (var key in args) {
            if (args.hasOwnProperty(key)) {
                session.dialogData[key] = (<any>args)[key];
            }
        }
        this.sendPrompt(session, args);
    }

    public replyReceived(session: ses.Session): void {
        var args: IPromptArgs = session.dialogData;
        Prompts.options.recognizer.recognize(
            {
                promptType: args.promptType,
                utterance: session.message.text,
                language: session.message.language,
                attachments: session.message.attachments,
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
                        this.sendPrompt(session, args, true);
                    }
                }
            });
    }
    
    private sendPrompt(session: ses.Session, args: IPromptArgs, retry = false): void {
        if (retry && typeof args.retryPrompt === 'object' && !Array.isArray(args.retryPrompt)) {
            // Send native IMessage
            session.send(args.retryPrompt);            
        } else if (typeof args.prompt === 'object' && !Array.isArray(args.prompt)) {
            // Send native IMessage
            session.send(args.prompt);            
        } else {
            // Calculate list style.
            var style = ListStyle.none;
            if (args.promptType == PromptType.choice || args.promptType == PromptType.confirm) {
                style = args.listStyle;
                if (style == ListStyle.auto) {
                    if (Channel.preferButtons(session, args.enumValues.length, retry)) {
                        style = ListStyle.button;
                    } else if (!retry) {
                        style = args.enumValues.length < 3 ? ListStyle.inline : ListStyle.list;
                    } else {
                        style = ListStyle.none;
                    }
                }
            }
            
            // Get message message
            var prompt: string;
            if (retry) {
                if (args.retryPrompt) {
                    prompt = mb.Message.randomPrompt(<any>args.retryPrompt);
                } else {
                    var type = PromptType[args.promptType];
                    prompt = mb.Message.randomPrompt((<any>Prompts.defaultRetryPrompt)[type]);
                }
            } else {
                prompt = mb.Message.randomPrompt(<any>args.prompt);
            }
            
            // Append list
            var connector = '';
            var list: string;
            var msg = new mb.Message();
            switch (style) {
                case ListStyle.button:
                    var a: IAttachment = { actions: [] };
                    for (var i = 0; i < session.dialogData.enumValues.length; i++) {
                        var action = session.dialogData.enumValues[i];
                        a.actions.push({ title: action, message: action });
                    }
                    msg.setText(session, prompt)
                       .addAttachment(a);
                    break;
                case ListStyle.inline:
                    list = ' (';
                    args.enumValues.forEach((value, index) => {
                        list += connector + (index + 1) + '. ' + value;
                        if (index == args.enumValues.length - 2) {
                            connector = index == 0 ? ' or ' : ', or ';
                        } else {
                            connector = ', ';
                        } 
                    });
                    list += ')';
                    msg.setText(session, prompt + '%s', list);
                    break;
                case ListStyle.list:
                    list = '\n   ';
                    args.enumValues.forEach((value, index) => {
                        list += connector + (index + 1) + '. ' + value;
                        connector = '\n   ';
                    });
                    msg.setText(session, prompt + '%s', list);
                    break;
                default:
                    msg.setText(session, prompt);
                    break;
            }
            
            // Send message
            session.send(msg);
        }
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

    static text(session: ses.Session, prompt: string|string[]|IMessage): void {
        beginPrompt(session, {
            promptType: PromptType.text,
            prompt: prompt
        });
    }

    static number(session: ses.Session, prompt: string|string[]|IMessage, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.number;
        args.prompt = prompt;
        beginPrompt(session, args);
    }

    static confirm(session: ses.Session, prompt: string|string[]|IMessage, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.confirm;
        args.prompt = prompt;
        args.enumValues = ['yes','no'];
        args.listStyle = args.hasOwnProperty('listStyle') ? args.listStyle : ListStyle.auto;
        beginPrompt(session, args);
    }

    static choice(session: ses.Session, prompt: string|string[]|IMessage, choices: string|Object|string[], options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.choice;
        args.prompt = prompt;
        args.listStyle = args.hasOwnProperty('listStyle') ? args.listStyle : ListStyle.auto;
        args.enumValues = entities.EntityRecognizer.expandChoices(choices);
        beginPrompt(session, args);
    }

    static time(session: ses.Session, prompt: string|string[]|IMessage, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.time;
        args.prompt = prompt;
        beginPrompt(session, args);
    }
    
    static attachment(session: ses.Session, prompt: string|string[]|IMessage, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.attachment;
        args.prompt = prompt;
        beginPrompt(session, args);
    }
}
dc.systemDialogs[consts.DialogId.Prompts] = new Prompts();

function beginPrompt(session: ses.Session, args: IPromptArgs) {
    session.beginDialog(consts.DialogId.Prompts, args);
}
