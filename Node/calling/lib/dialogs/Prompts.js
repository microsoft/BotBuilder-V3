"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var dlg = require('./Dialog');
var consts = require('../consts');
var dl = require('../bots/Library');
var recognize = require('../workflow/RecognizeAction');
var record = require('../workflow/RecordAction');
var prompt = require('../workflow/PlayPromptAction');
var utils = require('../utils');
(function (PromptType) {
    PromptType[PromptType["action"] = 0] = "action";
    PromptType[PromptType["confirm"] = 1] = "confirm";
    PromptType[PromptType["choice"] = 2] = "choice";
    PromptType[PromptType["digits"] = 3] = "digits";
    PromptType[PromptType["record"] = 4] = "record";
})(exports.PromptType || (exports.PromptType = {}));
var PromptType = exports.PromptType;
var PromptResponseState;
(function (PromptResponseState) {
    PromptResponseState[PromptResponseState["completed"] = 0] = "completed";
    PromptResponseState[PromptResponseState["retry"] = 1] = "retry";
    PromptResponseState[PromptResponseState["canceled"] = 2] = "canceled";
    PromptResponseState[PromptResponseState["terminated"] = 3] = "terminated";
    PromptResponseState[PromptResponseState["failed"] = 4] = "failed";
})(PromptResponseState || (PromptResponseState = {}));
var Prompts = (function (_super) {
    __extends(Prompts, _super);
    function Prompts() {
        _super.apply(this, arguments);
    }
    Prompts.prototype.begin = function (session, args) {
        utils.copyTo(args || {}, session.dialogData);
        session.send(args.action);
        session.sendBatch();
    };
    Prompts.prototype.replyReceived = function (session) {
        var args = session.dialogData;
        var results = session.message;
        if (results.operationOutcome) {
            var state = PromptResponseState.completed;
            var retryPrompt;
            var response;
            switch (args.promptType) {
                case PromptType.action:
                    response = results.operationOutcome;
                    break;
                case PromptType.choice:
                    var recognizeOutcome = results.operationOutcome;
                    var choiceOutcome = (recognizeOutcome.choiceOutcome || {});
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
                    var recognizeOutcome = results.operationOutcome;
                    var choiceOutcome = (recognizeOutcome.choiceOutcome || {});
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
                    var recognizeOutcome = results.operationOutcome;
                    var digitsOutcome = (recognizeOutcome.collectDigitsOutcome || {});
                    switch (digitsOutcome.completionReason) {
                        case recognize.DigitCollectionCompletionReason.completedStopToneDetected:
                            response = digitsOutcome.digits;
                            break;
                        case recognize.DigitCollectionCompletionReason.interdigitTimeout:
                            var stopTones = args.action.collectDigits.stopTones;
                            if (stopTones && stopTones.length > 0) {
                                state = PromptResponseState.retry;
                                retryPrompt = Prompts.settings.invalidRecognizePrompt;
                            }
                            else {
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
                    var recordOutcome = results.operationOutcome;
                    switch (recordOutcome.completionReason) {
                        case record.RecordingCompletionReason.completedSilenceDetected:
                        case record.RecordingCompletionReason.completedStopToneDetected:
                            response = {
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
            switch (state) {
                case PromptResponseState.canceled:
                    session.endDialogWithResult({ resumed: dlg.ResumeReason.canceled });
                    break;
                case PromptResponseState.completed:
                    session.endDialogWithResult({ resumed: dlg.ResumeReason.completed, response: response });
                    break;
                case PromptResponseState.failed:
                    session.endDialogWithResult({ resumed: dlg.ResumeReason.notCompleted, error: new Error('prompt error: service encountered a temporary failure'), promptType: args.promptType });
                    break;
                case PromptResponseState.retry:
                    if (args.maxRetries > 0) {
                        args.maxRetries--;
                        session.send(retryPrompt);
                        session.send(args.action);
                        session.sendBatch();
                    }
                    else {
                        session.endDialogWithResult({ resumed: dlg.ResumeReason.notCompleted });
                    }
                    break;
                case PromptResponseState.terminated:
                    session.endConversation();
                    break;
            }
        }
        else {
            var msg = results.operationOutcome ? results.operationOutcome.failureReason : 'Message missing operationOutcome.';
            session.endDialogWithResult({ resumed: dlg.ResumeReason.notCompleted, error: new Error('prompt error: ' + msg), promptType: args.promptType });
        }
    };
    Prompts.configure = function (settings) {
        utils.copyTo(settings, Prompts.settings);
    };
    Prompts.action = function (session, action) {
        beginPrompt(session, {
            promptType: PromptType.action,
            action: action.toAction ? action.toAction() : action,
            maxRetries: 0
        });
    };
    Prompts.confirm = function (session, playPrompt, options) {
        if (options === void 0) { options = {}; }
        var yesChoice = (options.yesChoice || { speechVariation: speechArray(session, 'yes|yep|sure|ok|true'), dtmfVariation: '1' });
        yesChoice.name = 'yes';
        var noChoice = (options.noChoice || { speechVariation: speechArray(session, 'no|nope|not|false'), dtmfVariation: '2' });
        noChoice.name = 'no';
        var choices = [yesChoice, noChoice];
        if (options.cancelChoice) {
            options.cancelChoice.name = 'cancel';
            choices.push(options.cancelChoice);
        }
        var action = createRecognizeAction(session, playPrompt, options).choices(choices);
        beginPrompt(session, {
            promptType: PromptType.confirm,
            action: action.toAction(),
            maxRetries: options.maxRetries
        });
    };
    Prompts.choice = function (session, playPrompt, choices, options) {
        if (options === void 0) { options = {}; }
        var action = createRecognizeAction(session, playPrompt, options).choices(choices);
        beginPrompt(session, {
            promptType: PromptType.choice,
            action: action.toAction(),
            maxRetries: options.maxRetries
        });
    };
    Prompts.digits = function (session, playPrompt, maxDigits, options) {
        if (options === void 0) { options = {}; }
        var collectDigits = { maxNumberOfDtmfs: maxDigits };
        if (options.stopTones) {
            collectDigits.stopTones = options.stopTones;
        }
        var action = createRecognizeAction(session, playPrompt, options).collectDigits(collectDigits);
        beginPrompt(session, {
            promptType: PromptType.digits,
            action: action.toAction(),
            maxRetries: options.maxRetries
        });
    };
    Prompts.record = function (session, playPrompt, options) {
        if (options === void 0) { options = {}; }
        var action = new record.RecordAction(session).playPrompt(createPrompt(session, playPrompt));
        utils.copyFieldsTo(options, action, 'maxDurationInSeconds|initialSilenceTimeoutInSeconds|maxSilenceTimeoutInSeconds|recordingFormat|playBeep|stopTones');
        beginPrompt(session, {
            promptType: PromptType.record,
            action: action.toAction(),
            maxRetries: options.maxRetries
        });
    };
    Prompts.settings = {
        recognizeSilencePrompt: "I couldn't hear anything.",
        invalidDtmfPrompt: "That's an invalid option.",
        invalidRecognizePrompt: "I'm sorry. I didn't understand.",
        recordSilencePrompt: "I couldn't hear anything.",
        maxRecordingPrompt: "I'm sorry. Your message was too long.",
        invalidRecordingPrompt: "I'm sorry. There was a problem with your recording."
    };
    return Prompts;
}(dlg.Dialog));
exports.Prompts = Prompts;
dl.systemLib.dialog(consts.DialogId.Prompts, new Prompts());
function beginPrompt(session, args) {
    if (typeof args.maxRetries !== 'number') {
        args.maxRetries = 2;
    }
    session.beginDialog(consts.DialogId.Prompts, args);
}
function createRecognizeAction(session, playPrompt, options) {
    var action = new recognize.RecognizeAction(session).playPrompt(createPrompt(session, playPrompt));
    utils.copyFieldsTo(options, action, 'bargeInAllowed|culture|initialSilenceTimeoutInSeconds|interdigitTimeoutInSeconds');
    return action;
}
function createPrompt(session, playPrompt) {
    if (typeof playPrompt === 'string' || Array.isArray(playPrompt)) {
        return prompt.PlayPromptAction.text(session, playPrompt).toAction();
    }
    else if (playPrompt.toAction) {
        return playPrompt.toAction();
    }
    return playPrompt;
}
function speechArray(session, choices) {
    var output = [];
    choices.split('|').forEach(function (choice) {
        output.push(session.gettext(choice));
    });
    return output;
}
