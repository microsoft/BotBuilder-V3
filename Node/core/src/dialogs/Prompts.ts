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

import dlg = require('./Dialog');
import ses = require('../Session');
import consts = require('../consts');
import entities = require('./EntityRecognizer');
import mb = require('../Message');
import Channel = require('../Channel');
import dl = require('../bots/Library');
import kb = require('../cards/Keyboard');
import ca = require('../cards/CardAction');
import logger = require('../logger');

export enum PromptType { text, number, confirm, choice, time, attachment }

export enum ListStyle { none, inline, list, button, auto }

export interface IPromptOptions {
    retryPrompt?: string|string[]|IMessage|IIsMessage;
    maxRetries?: number;
    refDate?: number;
    listStyle?: ListStyle;
    promptAfterAction?: boolean;
}

export interface IPromptArgs extends IPromptOptions {
    promptType: PromptType;
    prompt: string|string[]|IMessage|IIsMessage;
    enumValues?: string[];
    retryCnt?: number;
    localizationNamespace?: string;
}

export interface IPromptResult<T> extends dlg.IDialogResult<T> {
    score: number;
    promptType?: PromptType;
}

export interface IPromptRecognizer {
    recognize<T>(args: IPromptRecognizerArgs, callback: (result: IPromptResult<T>) => void, session?: ISession): void;
}

export interface IPromptRecognizerArgs {
    promptType: PromptType;
    locale: string;
    utterance: string;
    attachments: IAttachment[];
    enumValues?: string[];
    refDate?: number;
}

export interface IPromptsOptions {
    recognizer?: IPromptRecognizer;
    promptAfterAction?: boolean;
}

export interface IChronoDuration extends IEntity {
    resolution: {
        start: Date;
        end?: Date;
        ref?: Date;
    };
}

export class SimplePromptRecognizer implements IPromptRecognizer {
    public recognize(args: IPromptRecognizerArgs, callback: (result: IPromptResult<any>) => void, session?: ISession): void {
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
                score = 0.5;
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
        if (score > 0) {
            callback({ score: score, resumed: dlg.ResumeReason.completed, promptType: args.promptType, response: response });
        } else {
            callback({ score: score, resumed: dlg.ResumeReason.notCompleted, promptType: args.promptType });
        }
    }
} 

export class Prompts extends dlg.Dialog {
    private static options: IPromptsOptions = {
        recognizer: new SimplePromptRecognizer(),
        promptAfterAction: true
    };
    
    private static defaultRetryPrompt = {
        text: "default_text",
        number: "default_number",
        confirm: "default_confirm",
        choice: "default_choice", 
        time: "default_time", 
        attachment: "default_file"  
    };

    public begin(session: ses.Session, args: IPromptArgs): void {
        args = <any>args || {};
        args.promptAfterAction = args.hasOwnProperty('promptAfterAction') ? args.promptAfterAction : Prompts.options.promptAfterAction;
        args.retryCnt = 0;
        for (var key in args) {
            if (args.hasOwnProperty(key)) {
                session.dialogData[key] = (<any>args)[key];
            }
        }
        this.sendPrompt(session, args);
    }

    public replyReceived(session: ses.Session, result?: IPromptResult<any>): void {
        var args: IPromptArgs = session.dialogData;
        if (result.error || result.resumed == dlg.ResumeReason.completed) {
            result.promptType = args.promptType;
            session.endDialogWithResult(result);
        } else if (typeof args.maxRetries === 'number' && args.retryCnt >= args.maxRetries) {
            result.promptType = args.promptType;
            result.resumed = dlg.ResumeReason.notCompleted;
            session.endDialogWithResult(result);
        } else {
            args.retryCnt++;
            this.sendPrompt(session, args, true);
        }
    }
    public dialogResumed<T>(session: ses.Session, result: dlg.IDialogResult<any>): void {
        // Comming back from an action so re-prompt the user.
        var args: IPromptArgs = session.dialogData;
        if (args.promptAfterAction) {
            this.sendPrompt(session, args);
        }
    }

    public recognize(context: dlg.IRecognizeContext, cb: (err: Error, result: dlg.IRecognizeResult) => void): void {
        var args: IPromptArgs = context.dialogData;
        Prompts.options.recognizer.recognize({
            promptType: args.promptType,
            utterance: context.message.text,
            locale: context.message.textLocale,
            attachments: context.message.attachments,
            enumValues: args.enumValues,
            refDate: args.refDate
        }, (result) => {
            if (result.error) {
                cb(result.error, null);
            } else {
                cb(null, result);
            }
        });
    }

    
    private sendPrompt(session: ses.Session, args: IPromptArgs, retry = false): void {
        logger.debug("prompts::sendPrompt called");                                               

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
                    if (Channel.supportsKeyboards(session, args.enumValues.length)) {
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
                    args.localizationNamespace = consts.Library.system;
                    logger.debug("prompts::sendPrompt setting ns to %s", args.localizationNamespace);                                                                   
                }
            } else {
                prompt = mb.Message.randomPrompt(<any>args.prompt);
            }
                                                                               
            var locale:string = session.preferredLocale();
            logger.debug("prompts::preferred locale %s", locale);    
            if (!locale && session.localizer) {
                locale = session.localizer.defaultLocale();
                logger.debug("prompts::sendPrompt using default locale %s", locale);                                                                                   
            }
            prompt = session.localizer.gettext(locale, prompt, args.localizationNamespace);
            logger.debug("prompts::sendPrompt localized prompt %s", prompt);                                                                   
            
                        
            // Append list
            var connector = '';
            var list: string;
            var msg = new mb.Message();
            switch (style) {
                case ListStyle.button:
                    var buttons: ca.CardAction[] = [];
                    for (var i = 0; i < session.dialogData.enumValues.length; i++) {
                        var option = session.dialogData.enumValues[i];
                        buttons.push(ca.CardAction.imBack(session, option, option));
                    }
                    msg.text(prompt)
                       .attachments([new kb.Keyboard(session).buttons(buttons)]);
                    break;
                case ListStyle.inline:
                    list = ' (';
                    args.enumValues.forEach((value, index) => {
                        list += connector + (index + 1) + '. ' + session.localizer.gettext(locale, value, consts.Library.system);
                        if (index == args.enumValues.length - 2) {
                            connector = index == 0 ? session.localizer.gettext(locale, "list_or", consts.Library.system) : session.localizer.gettext(locale, "list_or_more", consts.Library.system);
                        } else {
                            connector = ', ';
                        } 
                    });
                    list += ')';
                    msg.text(prompt + '%s', list);
                    break;
                case ListStyle.list:
                    list = '\n   ';
                    args.enumValues.forEach((value, index) => {
                        list += connector + (index + 1) + '. ' + session.localizer.gettext(locale, value, args.localizationNamespace);
                        connector = '\n   ';
                    });
                    msg.text(prompt + '%s', list);
                    break;
                default:
                    msg.text(prompt);
                    break;
            }
            
            // Send message
            session.send(msg);
        }
        session.sendBatch();
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

    static text(session: ses.Session, prompt: string|string[]|IMessage|IIsMessage): void {
        beginPrompt(session, {
            promptType: PromptType.text,
            prompt: prompt
        });
    }

    static number(session: ses.Session, prompt: string|string[]|IMessage|IIsMessage, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.number;
        args.prompt = prompt;
        beginPrompt(session, args);
    }

    static confirm(session: ses.Session, prompt: string|string[]|IMessage|IIsMessage, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.confirm;
        args.prompt = prompt;
        args.enumValues = ['confirm_yes','confirm_no'];
        args.listStyle = args.hasOwnProperty('listStyle') ? args.listStyle : ListStyle.auto;
        beginPrompt(session, args);
    }

    static choice(session: ses.Session, prompt: string|string[]|IMessage|IIsMessage, choices: string|Object|string[], options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.choice;
        args.prompt = prompt;
        args.listStyle = args.hasOwnProperty('listStyle') ? args.listStyle : ListStyle.auto;
        args.enumValues = entities.EntityRecognizer.expandChoices(choices);
        beginPrompt(session, args);
    }

    static time(session: ses.Session, prompt: string|string[]|IMessage|IIsMessage, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.time;
        args.prompt = prompt;
        beginPrompt(session, args);
    }
    
    static attachment(session: ses.Session, prompt: string|string[]|IMessage|IIsMessage, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.attachment;
        args.prompt = prompt;
        beginPrompt(session, args);
    }
}
dl.systemLib.dialog(consts.DialogId.Prompts, new Prompts());

function beginPrompt(session: ses.Session, args: IPromptArgs) {
    // Fixup prompts
    if (typeof args.prompt == 'object' && (<IIsMessage>args.prompt).toMessage) {
        args.prompt = (<IIsMessage>args.prompt).toMessage();
    }
    if (typeof args.retryPrompt == 'object' && (<IIsMessage>args.retryPrompt).toMessage) {
        args.retryPrompt = (<IIsMessage>args.retryPrompt).toMessage();
    }
    session.beginDialog(consts.DialogId.Prompts, args);
}
