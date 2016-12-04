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

import { IRecognizeResult } from './IntentRecognizerSet';
import { Dialog, IRecognizeDialogContext, IDialogResult, ResumeReason } from './Dialog';
import { Session } from '../Session';
import { EntityRecognizer } from './EntityRecognizer';
import { Message } from '../Message';
import { systemLib, IRouteResult } from '../bots/Library';
import { Keyboard } from '../cards/Keyboard';
import { CardAction } from '../cards/CardAction';
import * as Channel from '../Channel';
import * as consts from '../consts';
import * as logger from '../logger';

export enum PromptType { text, number, confirm, choice, time, attachment }

export enum ListStyle { none, inline, list, button, auto }

export interface IPromptOptions {
    retryPrompt?: string|string[]|IMessage|IIsMessage;
    maxRetries?: number;
    refDate?: number;
    listStyle?: ListStyle;
    promptAfterAction?: boolean;
    localizationNamespace?: string;
}

export interface IPromptArgs extends IPromptOptions {
    promptType: PromptType;
    prompt: string|string[]|IMessage|IIsMessage;
    enumValues?: string[];
    retryCnt?: number;
}

export interface IPromptResult<T> extends IDialogResult<T> {
    score: number;
    promptType?: PromptType;
}

export interface IPromptRecognizer {
    recognize<T>(args: IPromptRecognizerArgs, callback: (result: IPromptResult<T>) => void, session?: Session): void;
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

export interface IDisambiguateChoices {
    [label: string]: IRouteResult;
}

export class SimplePromptRecognizer implements IPromptRecognizer {
    public recognize(args: IPromptRecognizerArgs, callback: (result: IPromptResult<any>) => void, session?: Session): void {
        function findChoice(args: IPromptRecognizerArgs, text: string) {
            var best = EntityRecognizer.findBestMatch(args.enumValues, text);
            if (!best) {
                var n = EntityRecognizer.parseNumber(text);
                if (!isNaN(n) && n > 0 && n <= args.enumValues.length) {
                    best = { index: n - 1, entity: args.enumValues[n - 1], score: 1.0 };
                }
            }
            return best;
        }

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
                var n = EntityRecognizer.parseNumber(text);
                if (!isNaN(n)) {
                    var score = n.toString().length / text.length;
                    response = n;
                }
                break;
            case PromptType.confirm:
                var b = EntityRecognizer.parseBoolean(text);
                if (typeof b !== 'boolean') {
                    var best = findChoice(args, text);
                    if (best) {
                        b = (best.index === 0); // enumValues == ['yes', 'no']
                    }
                }
                if (typeof b == 'boolean') {
                    score = 1.0;
                    response = b;
                }
                break;
            case PromptType.time:
                var entity = EntityRecognizer.recognizeTime(text, args.refDate ? new Date(args.refDate) : null);
                if (entity) {
                    score = entity.entity.length / text.length;
                    response = entity;
                } 
                break;
            case PromptType.choice:
                var best = findChoice(args, text);
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
            callback({ score: score, resumed: ResumeReason.completed, promptType: args.promptType, response: response });
        } else {
            callback({ score: score, resumed: ResumeReason.notCompleted, promptType: args.promptType });
        }
    }
} 

export class Prompts extends Dialog {
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

    public begin(session: Session, args: IPromptArgs): void {
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

    public replyReceived(session: Session, result?: IPromptResult<any>): void {
        var args: IPromptArgs = session.dialogData;
        if (result.error || result.resumed == ResumeReason.completed) {
            result.promptType = args.promptType;
            session.endDialogWithResult(result);
        } else if (typeof args.maxRetries === 'number' && args.retryCnt >= args.maxRetries) {
            result.promptType = args.promptType;
            result.resumed = ResumeReason.notCompleted;
            session.endDialogWithResult(result);
        } else {
            args.retryCnt++;
            this.sendPrompt(session, args, true);
        }
    }
    public dialogResumed<T>(session: Session, result: IDialogResult<any>): void {
        // Comming back from an action so re-prompt the user.
        var args: IPromptArgs = session.dialogData;
        if (args.promptAfterAction) {
            this.sendPrompt(session, args);
        }
    }

    public recognize(context: IRecognizeDialogContext, cb: (err: Error, result: IRecognizeResult) => void): void {
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

    private sendPrompt(session: Session, args: IPromptArgs, retry = false): void {
        logger.debug("prompts::sendPrompt called");

        // Find message to deliver
        var msg: IMessage|IIsMessage;
        if (retry && typeof args.retryPrompt === 'object' && !Array.isArray(args.retryPrompt)) {
            msg = args.retryPrompt;
        } else if (typeof args.prompt === 'object' && !Array.isArray(args.prompt)) {
            msg = args.prompt;
        } else {
            msg = this.createPrompt(session, args, retry);
        }

        // Send message
        session.send(msg);

        // Commit batch
        session.sendBatch();
    }

    private createPrompt(session: Session, args: IPromptArgs, retry: boolean): IMessage|IIsMessage {
        var msg = new Message(session);
        var locale = session.preferredLocale();
        var localizationNamespace = args.localizationNamespace;

        // Calculate list style.
        var style = ListStyle.none;
        if (args.promptType == PromptType.choice || args.promptType == PromptType.confirm) {
            style = args.listStyle;
            if (style == ListStyle.auto) {
                if (Channel.supportsKeyboards(session, args.enumValues.length)) {
                    style = ListStyle.button;
                } else if (!retry && args.promptType == PromptType.choice) {
                    style = args.enumValues.length < 3 ? ListStyle.inline : ListStyle.list;
                } else {
                    style = ListStyle.none;
                }
            }
        }
        
        // Get localized text of the prompt
        var prompt: string;
        if (retry) {
            if (args.retryPrompt) {
                prompt = Message.randomPrompt(<any>args.retryPrompt);
            } else {
                // Use default system retry prompt
                var type = PromptType[args.promptType];
                prompt = (<any>Prompts.defaultRetryPrompt)[type];
                localizationNamespace = consts.Library.system;
            }
        } else {
            prompt = Message.randomPrompt(<any>args.prompt);
        }
        var text = session.localizer.gettext(locale, prompt, localizationNamespace);
                    
        // Populate message
        var connector = '';
        var list: string;
        switch (style) {
            case ListStyle.button:
                var buttons: CardAction[] = [];
                for (var i = 0; i < session.dialogData.enumValues.length; i++) {
                    var option = session.dialogData.enumValues[i];
                    buttons.push(CardAction.imBack(session, option, option));
                }
                msg.text(text)
                    .attachments([new Keyboard(session).buttons(buttons)]);
                break;
            case ListStyle.inline:
                list = ' (';
                args.enumValues.forEach((v, index) => {
                    var value = v.toString();
                    list += connector + (index + 1) + '. ' + session.localizer.gettext(locale, value, consts.Library.system);
                    if (index == args.enumValues.length - 2) {
                        connector = index == 0 ? session.localizer.gettext(locale, "list_or", consts.Library.system) : session.localizer.gettext(locale, "list_or_more", consts.Library.system);
                    } else {
                        connector = ', ';
                    } 
                });
                list += ')';
                msg.text(text + '%s', list);
                break;
            case ListStyle.list:
                list = '\n   ';
                args.enumValues.forEach((v, index) => {
                    var value = v.toString();
                    list += connector + (index + 1) + '. ' + session.localizer.gettext(locale, value, args.localizationNamespace);
                    connector = '\n   ';
                });
                msg.text(text + '%s', list);
                break;
            default:
                msg.text(text);
                break;
        }
        return msg;
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

    static text(session: Session, prompt: string|string[]|IMessage|IIsMessage, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.text;
        args.prompt = prompt;
        beginPrompt(session, args);
    }

    static number(session: Session, prompt: string|string[]|IMessage|IIsMessage, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.number;
        args.prompt = prompt;
        beginPrompt(session, args);
    }

    static confirm(session: Session, prompt: string|string[]|IMessage|IIsMessage, options?: IPromptOptions): void {
        var locale:string = session.preferredLocale();
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.confirm;
        args.prompt = prompt;
        args.enumValues = [
            session.localizer.gettext(locale, 'confirm_yes', consts.Library.system),
            session.localizer.gettext(locale, 'confirm_no', consts.Library.system)
        ];
        args.listStyle = args.hasOwnProperty('listStyle') ? args.listStyle : ListStyle.auto;
        beginPrompt(session, args);
    }

    static choice(session: Session, prompt: string|string[]|IMessage|IIsMessage, choices: string|Object|string[], options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.choice;
        args.prompt = prompt;
        args.listStyle = args.hasOwnProperty('listStyle') ? args.listStyle : ListStyle.auto;
        var c = EntityRecognizer.expandChoices(choices);
        if (c.length == 0) {
            console.error("0 length choice for prompt:", prompt);
            throw "0 length choice list supplied";
        }
        args.enumValues = c;
        beginPrompt(session, args);
    }

    static time(session: Session, prompt: string|string[]|IMessage|IIsMessage, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.time;
        args.prompt = prompt;
        beginPrompt(session, args);
    }
    
    static attachment(session: Session, prompt: string|string[]|IMessage|IIsMessage, options?: IPromptOptions): void {
        var args: IPromptArgs = <any>options || {};
        args.promptType = PromptType.attachment;
        args.prompt = prompt;
        beginPrompt(session, args);
    }

    static disambiguate(session: Session, prompt: string|string[]|IMessage|IIsMessage, choices: IDisambiguateChoices, options?: IPromptOptions): void {
        session.beginDialog(consts.DialogId.Disambiguate, {
            prompt: prompt,
            choices: choices,
            options: options
        });
    }
}
systemLib.dialog(consts.DialogId.Prompts, new Prompts());

function beginPrompt(session: Session, args: IPromptArgs) {
    // Calculate localization namespace
    if (!args.localizationNamespace) {
        // Get the namespace of the active dialog. Otherwise use root libraries namespace.
        var cur = Session.activeDialogStackEntry(session.dialogStack());
        args.localizationNamespace = cur ? cur.id.split(':')[0] : session.library.name;
    }

    // Fixup prompts
    if (typeof args.prompt == 'object' && (<IIsMessage>args.prompt).toMessage) {
        args.prompt = (<IIsMessage>args.prompt).toMessage();
    }
    if (typeof args.retryPrompt == 'object' && (<IIsMessage>args.retryPrompt).toMessage) {
        args.retryPrompt = (<IIsMessage>args.retryPrompt).toMessage();
    }
    session.beginDialog(consts.DialogId.Prompts, args);
}

/**
 * Internal dialog that prompts a user to confirm a cancelAction().
 * dialogArgs: { 
 *      localizationNamespace: string;
 *      confirmPrompt: string; 
 *      message?: string;
 *      dialogIndex?: number;
 *      endConversation?: boolean;
 * }
 */
systemLib.dialog(consts.DialogId.ConfirmCancel, [
    function (session, args) {
        session.dialogData.localizationNamespace = args.localizationNamespace;
        session.dialogData.dialogIndex = args.dialogIndex;
        session.dialogData.message = args.message;
        session.dialogData.endConversation = args.endConversation;
        Prompts.confirm(session, args.confirmPrompt, { localizationNamespace: args.localizationNamespace });
    },
    function (session, results) {
        if (results.response) {
            // Send optional message
            var args = session.dialogData;
            if (args.message) {
                session.sendLocalized(args.localizationNamespace, args.message);
            }

            // End conversation or cancel dialog
            if (args.endConversation) {
                session.endConversation();
            } else {
                session.cancelDialog(args.dialogIndex);
            }
        } else {
            session.endDialogWithResult({ resumed: ResumeReason.reprompt });
        }
    }
]);

/**
 * Internal dialog that prompts a user to confirm a that a root dialog should be
 * interrupted with a new dialog.
 * dialogArgs: { 
 *      localizationNamespace: string;
 *      confirmPrompt: string; 
 *      dialogId: string;
 *      dialogArgs?: any;
 * }
 */
systemLib.dialog(consts.DialogId.ConfirmInterruption, [
    function (session, args) {
        session.dialogData.dialogId = args.dialogId;
        session.dialogData.dialogArgs = args.dialogArgs;
        Prompts.confirm(session, args.confirmPrompt, { localizationNamespace: args.localizationNamespace });
    },
    function (session, results) {
        if (results.response) {
            var args = session.dialogData;
            session.clearDialogStack();
            session.beginDialog(args.dialogId, args.dialogArgs);
        } else {
            session.endDialogWithResult({ resumed: ResumeReason.reprompt });
        }
    }
]);

/**
 * Begins a new dialog as an interruption. If the stack has a depth of 1 that means
 * only the interruption exists so it will be replaced with the new dialog. Otherwise,
 * the interruption will stay on the stack and ensure that ResumeReason.reprompt is
 * returned.  This is to fix an issue with waterfalls that they can advance when we
 * don't want them too.
 * dialogArgs: { 
 *      dialogId: string; 
 *      dialogArgs?: any;
 *      isRootDialog?: boolean;
 * }
 */
systemLib.dialog(consts.DialogId.Interruption, [
    function (session, args) {
        if (session.sessionState.callstack.length > 1) {
            session.beginDialog(args.dialogId, args.dialogArgs);
        } else {
            session.replaceDialog(args.dialogId, args.dialogArgs);
        }
    },
    function (session, results) {
        session.endDialogWithResult({ resumed: ResumeReason.reprompt });
    }
]);


/**
 * Prompts the user to disambiguate between multiple routes that were troggered.
 * dialogArgs: { 
 *      prompt: string|string[]|IMessage|IIsMessage;
 *      choices: IDisambiguateChoices;
 *      options?: IPromptsOptions;
 * }
 */
systemLib.dialog(consts.DialogId.Disambiguate, [
    function (session, args) {
        // Prompt user
        session.dialogData.choices = args.choices;
        Prompts.choice(session, args.prompt, args.choices, args.options);
    },
    function (session, results) {
        var route = session.dialogData.choices[results.response.entity];
        if (route) {
            // Pop ourselves off the stack
            var stack = session.dialogStack();
            stack.pop();
            session.dialogStack(stack);

            // Route to action
            session.library.library(route.libraryName).selectRoute(session, route);
        } else {
            // Return with reprompt
            session.endDialogWithResult({ resumed: ResumeReason.reprompt });
        }
    }
]);
