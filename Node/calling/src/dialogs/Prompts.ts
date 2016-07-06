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
import ses = require('../CallSession');
import consts = require('../consts');
import dl = require('../bots/Library');
import recognize = require('../workflow/RecognizeAction');
import record = require('../workflow/RecordAction');
import prompt = require('../workflow/PlayPromptAction');
import utils = require('../utils');

export enum PromptType { action, confirm, choice, digits, record }

export interface IPromptArgs {
    promptType: PromptType;
    action: IAction;
    maxRetries: number;
}

export interface IPromptOptions {
    maxRetries?: number;
}

export interface IRecognizerPromptOptions extends IPromptOptions {
    bargeInAllowed?: boolean;
    culture?: string; 
    initialSilenceTimeoutInSeconds?: number; 
    interdigitTimeoutInSeconds?: number; 
}

export interface IRecordPromptOptions extends IPromptOptions {
    maxDurationInSeconds?: number;
    initialSilenceTimeoutInSeconds?: number;
    maxSilenceTimeoutInSeconds?: number; 
    recordingFormat?: string; 
    playBeep?: boolean; 
    stopTones?: string[];
}

export interface IConfirmPromptOptions extends IRecognizerPromptOptions {
    yesChoice?: IRecognitionChoice;
    noChoice?: IRecognitionChoice;
    cancelChoice?: IRecognitionChoice;
}

export interface IDigitsPromptOptions extends IRecognizerPromptOptions {
    stopTones?: string[];
}

export interface IPromptResult<T> extends dlg.IDialogResult<T> {
    promptType?: PromptType;
}

export interface IPromptsSettings {
    recognizeSilencePrompt?: string|string[]|IAction|IIsAction;
    invalidDtmfPrompt?: string|string[]|IAction|IIsAction;
    invalidRecognizePrompt?: string|string[]|IAction|IIsAction;
    recordSilencePrompt?: string|string[]|IAction|IIsAction;
    maxRecordingPrompt?: string|string[]|IAction|IIsAction;
    invalidRecordingPrompt?: string|string[]|IAction|IIsAction;
}

enum PromptResponseState { completed, retry, canceled, terminated, failed }

export class Prompts extends dlg.Dialog {
    private static settings: IPromptsSettings = {
        recognizeSilencePrompt: "I couldn't hear anything.",
        invalidDtmfPrompt: "That's an invalid option.", 
        invalidRecognizePrompt: "I'm sorry. I didn't understand.",
        recordSilencePrompt: "I couldn't hear anything.",
        maxRecordingPrompt: "I'm sorry. Your message was too long.",
        invalidRecordingPrompt: "I'm sorry. There was a problem with your recording."
    };
    
    public begin(session: ses.CallSession, args: IPromptArgs): void {
        utils.copyTo(args || {}, session.dialogData);
        session.send(args.action);
        session.sendBatch();
    }

    public replyReceived(session: ses.CallSession): void {
        var args: IPromptArgs = session.dialogData;
        var results = <IConversationResult>session.message;
        if (results.operationOutcome) {
            // Parse response
            var state = PromptResponseState.completed;
            var retryPrompt: any;
            var response: any;
            switch (args.promptType) {
                case PromptType.action:
                    response = results.operationOutcome;
                    break;
                case PromptType.choice:
                    var recognizeOutcome = <IRecognizeOutcome>results.operationOutcome;
                    var choiceOutcome = <IChoiceOutcome>(recognizeOutcome.choiceOutcome || {});
                    switch (choiceOutcome.completionReason) {
                        case recognize.RecognitionCompletionReason.dtmfOptionMatched:
                        case recognize.RecognitionCompletionReason.speechOptionMatched:
                            response = { entity: choiceOutcome.choiceName, score: 1.0 };
                            break;
                        case recognize.RecognitionCompletionReason.callTerminated:
                            state = PromptResponseState.terminated;
                            break;
                        case recognize.RecognitionCompletionReason.temporarySystemFailure:
                            state = PromptResponseState.failed;
                            break;
                        case recognize.RecognitionCompletionReason.incorrectDtmf:
                            state = PromptResponseState.retry;
                            retryPrompt = Prompts.settings.invalidDtmfPrompt;
                            break;
                        case recognize.RecognitionCompletionReason.initialSilenceTimeout:
                            state = PromptResponseState.retry;
                            retryPrompt = Prompts.settings.recognizeSilencePrompt;
                            break;
                        default:
                            state = PromptResponseState.retry;
                            retryPrompt = Prompts.settings.invalidRecognizePrompt;
                            break;
                    }
                    break;
                case PromptType.confirm:
                    var recognizeOutcome = <IRecognizeOutcome>results.operationOutcome;
                    var choiceOutcome = <IChoiceOutcome>(recognizeOutcome.choiceOutcome || {});
                    switch (choiceOutcome.completionReason) {
                        case recognize.RecognitionCompletionReason.dtmfOptionMatched:
                        case recognize.RecognitionCompletionReason.speechOptionMatched:
                            switch (choiceOutcome.choiceName) {
                                case 'yes':
                                    response = true;
                                    break;
                                case 'no':
                                default:
                                    response = false;
                                    break;
                                case 'cancel':
                                    state = PromptResponseState.canceled;
                                    break;
                            }
                            break;
                        case recognize.RecognitionCompletionReason.callTerminated:
                            state = PromptResponseState.terminated;
                            break;
                        case recognize.RecognitionCompletionReason.temporarySystemFailure:
                            state = PromptResponseState.failed;
                            break;
                        case recognize.RecognitionCompletionReason.incorrectDtmf:
                            state = PromptResponseState.retry;
                            retryPrompt = Prompts.settings.invalidDtmfPrompt;
                            break;
                        case recognize.RecognitionCompletionReason.initialSilenceTimeout:
                            state = PromptResponseState.retry;
                            retryPrompt = Prompts.settings.recognizeSilencePrompt;
                            break;
                        default:
                            state = PromptResponseState.retry;
                            retryPrompt = Prompts.settings.invalidRecognizePrompt;
                            break;
                    }
                    break;
                case PromptType.digits:
                    var recognizeOutcome = <IRecognizeOutcome>results.operationOutcome;
                    var digitsOutcome = <ICollectDigitsOutcome>(recognizeOutcome.collectDigitsOutcome || {});
                    switch (digitsOutcome.completionReason) {
                        case recognize.DigitCollectionCompletionReason.completedStopToneDetected:
                            response = digitsOutcome.digits;
                            break;
                        case recognize.DigitCollectionCompletionReason.interdigitTimeout:
                            var stopTones = (<IRecognizeAction>args.action).collectDigits.stopTones;
                            if (stopTones && stopTones.length > 0) {
                                state = PromptResponseState.retry;
                                retryPrompt = Prompts.settings.invalidRecognizePrompt;
                            } else {
                                response = digitsOutcome.digits;
                            }
                            break;
                        case recognize.DigitCollectionCompletionReason.callTerminated:
                            state = PromptResponseState.terminated;
                            break;
                        case recognize.DigitCollectionCompletionReason.temporarySystemFailure:
                            state = PromptResponseState.failed;
                            break;
                        case recognize.DigitCollectionCompletionReason.initialSilenceTimeout:
                            state = PromptResponseState.retry;
                            retryPrompt = Prompts.settings.recognizeSilencePrompt;
                            break;
                        default:
                            state = PromptResponseState.retry;
                            retryPrompt = Prompts.settings.invalidRecognizePrompt;
                            break;
                    }
                    break;
                case PromptType.record:
                    var recordOutcome = <IRecordOutcome>results.operationOutcome;
                    switch (recordOutcome.completionReason) {
                        case record.RecordingCompletionReason.completedSilenceDetected:
                        case record.RecordingCompletionReason.completedStopToneDetected:
                            response = <record.IRecording>{
                                recordedAudio: results.recordedAudio,
                                lengthOfRecordingInSecs: recordOutcome.lengthOfRecordingInSecs
                            };
                            break;
                        case record.RecordingCompletionReason.callTerminated:
                            state = PromptResponseState.terminated;
                            break;
                        case record.RecordingCompletionReason.temporarySystemFailure:
                            state = PromptResponseState.failed;
                            break;
                        case record.RecordingCompletionReason.initialSilenceTimeout:
                            state = PromptResponseState.retry;
                            retryPrompt = Prompts.settings.recordSilencePrompt;
                            break;
                        case record.RecordingCompletionReason.maxRecordingTimeout:
                            state = PromptResponseState.retry;
                            retryPrompt = Prompts.settings.maxRecordingPrompt;
                            break;
                        default:
                            state = PromptResponseState.retry;
                            retryPrompt = Prompts.settings.invalidRecordingPrompt;
                            break;
                    }
                    break;
            }

            // Route response
            switch(state) {
                case PromptResponseState.canceled:
                    session.endDialogWithResult({ resumed: dlg.ResumeReason.canceled });
                    break;
                case PromptResponseState.completed:
                    session.endDialogWithResult({ resumed: dlg.ResumeReason.completed, response: response  });
                    break;
                case PromptResponseState.failed:
                    session.endDialogWithResult(<any>{ resumed: dlg.ResumeReason.notCompleted, error: new Error('prompt error: service encountered a temporary failure'), promptType: args.promptType });
                    break;
                case PromptResponseState.retry:
                    if (args.maxRetries > 0) {
                        args.maxRetries--;
                        session.send(retryPrompt);
                        session.send(args.action);
                        session.sendBatch();
                    } else {
                        session.endDialogWithResult({ resumed: dlg.ResumeReason.notCompleted });
                    }
                    break;
                case PromptResponseState.terminated:
                    session.endConversation();
                    break;
            }
        } else {
            var msg = results.operationOutcome ? results.operationOutcome.failureReason : 'Message missing operationOutcome.';
            session.endDialogWithResult(<any>{ resumed: dlg.ResumeReason.notCompleted, error: new Error('prompt error: ' + msg), promptType: args.promptType });
        }
    }

    static configure(settings: IPromptsSettings): void {
        utils.copyTo(settings, Prompts.settings);
    }

    static action(session: ses.CallSession, action: IAction|IIsAction): void {
        beginPrompt(session, {
            promptType: PromptType.action,
            action: (<IIsAction>action).toAction ? (<IIsAction>action).toAction() : <IAction>action,
            maxRetries: 0
        });
    }

    static confirm(session: ses.CallSession, playPrompt: string|string[]|IAction|IIsAction, options: IConfirmPromptOptions = {}): void {
        // Initialize choices
        var yesChoice = <IRecognitionChoice>(options.yesChoice || { speechVariation: speechArray(session, 'yes|yep|sure|ok|true'), dtmfVariation: '1' });
        yesChoice.name = 'yes';
        var noChoice = <IRecognitionChoice>(options.noChoice || { speechVariation: speechArray(session, 'no|nope|not|false'), dtmfVariation: '2' });
        noChoice.name = 'no';
        var choices = <IRecognitionChoice[]>[yesChoice, noChoice];
        if (options.cancelChoice) {
            options.cancelChoice.name = 'cancel';
            choices.push(options.cancelChoice);
        }

        // Start prompt
        var action = createRecognizeAction(session, playPrompt, options).choices(choices);
        beginPrompt(session, {
            promptType: PromptType.confirm,
            action: action.toAction(),
            maxRetries: options.maxRetries
        });
    }

    static choice(session: ses.CallSession, playPrompt: string|string[]|IAction|IIsAction, choices: IRecognitionChoice[], options: IRecognizerPromptOptions = {}): void {
        var action = createRecognizeAction(session, playPrompt, options).choices(choices);
        beginPrompt(session, {
            promptType: PromptType.choice,
            action: action.toAction(),
            maxRetries: options.maxRetries
        });
    }

    static digits(session: ses.CallSession, playPrompt: string|string[]|IAction|IIsAction, maxDigits: number, options: IDigitsPromptOptions = {}): void {
        var collectDigits = <ICollectDigits>{ maxNumberOfDtmfs: maxDigits };
        if (options.stopTones) {
            collectDigits.stopTones = options.stopTones;
        }
        var action = createRecognizeAction(session, playPrompt, options).collectDigits(collectDigits);
        beginPrompt(session, {
            promptType: PromptType.digits,
            action: action.toAction(),
            maxRetries: options.maxRetries
        });
    }

    static record(session: ses.CallSession, playPrompt: string|string[]|IAction|IIsAction, options: IRecordPromptOptions = {}): void {
        var action = new record.RecordAction(session).playPrompt(createPrompt(session, playPrompt));
        utils.copyFieldsTo(options, action, 'maxDurationInSeconds|initialSilenceTimeoutInSeconds|maxSilenceTimeoutInSeconds|recordingFormat|playBeep|stopTones');        
        beginPrompt(session, {
            promptType: PromptType.record,
            action: action.toAction(),
            maxRetries: options.maxRetries
        });
    }
}
dl.systemLib.dialog(consts.DialogId.Prompts, new Prompts());

function beginPrompt(session: ses.CallSession, args: IPromptArgs) {
    if (typeof args.maxRetries !== 'number') {
        args.maxRetries = 2;
    }
    session.beginDialog(consts.DialogId.Prompts, args);
}

function createRecognizeAction(session: ses.CallSession, playPrompt: string|string[]|IAction|IIsAction, options: IRecognizerPromptOptions): recognize.RecognizeAction {
    var action = new recognize.RecognizeAction(session).playPrompt(createPrompt(session, playPrompt));
    utils.copyFieldsTo(options, action, 'bargeInAllowed|culture|initialSilenceTimeoutInSeconds|interdigitTimeoutInSeconds');
    return action;
}

function createPrompt(session: ses.CallSession, playPrompt: string|string[]|IAction|IIsAction): IAction {
    if (typeof playPrompt === 'string' || Array.isArray(playPrompt)) {
        return prompt.PlayPromptAction.text(session, <any>playPrompt).toAction();
    } else if ((<IIsAction>playPrompt).toAction) {
        return (<IIsAction>playPrompt).toAction();
    }
    return <IAction>playPrompt;
}

function speechArray(session: ses.CallSession, choices: string): string[] {
    var output = <string[]>[];
    choices.split('|').forEach((choice) => {
        output.push(session.gettext(choice));
    });
    return output;
}