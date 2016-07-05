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

import consts = require('./consts');
import utils = require('./utils');
import ses = require('./CallSession');
import dialog = require('./dialogs/Dialog');
import actions = require('./dialogs/DialogAction');
import prompts = require('./dialogs/Prompts');
import simple = require('./dialogs/SimpleDialog');
import ucb = require('./bots/UniversalCallBot');
import dl = require('./bots/Library');
import calling = require('./bots/CallConnector');
import bs = require('./storage/BotStorage');
import answer = require('./workflow/AnswerAction');
import hangup = require('./workflow/HangupAction');
import playPrompt = require('./workflow/PlayPromptAction');
import prompt = require('./workflow/Prompt');
import recognize = require('./workflow/RecognizeAction');
import record = require('./workflow/RecordAction');
import reject = require('./workflow/RejectAction');

declare var exports: any;

exports.CallSession = ses.CallSession;
exports.CallState = ses.CallState;
exports.ModalityType = ses.ModalityType;
exports.NotificationType = ses.NotificationType;
exports.OperationOutcome = ses.OperationOutcome;
exports.AnswerAction = answer.AnswerAction;
exports.HangupAction = hangup.HangupAction;
exports.PlayPromptAction = playPrompt.PlayPromptAction;
exports.Prompt = prompt.Prompt;
exports.SayAs = prompt.SayAs;
exports.VoiceGender = prompt.VoiceGender;
exports.RecognizeAction = recognize.RecognizeAction;
exports.RecognitionCompletionReason = recognize.RecognitionCompletionReason;
exports.DigitCollectionCompletionReason = recognize.DigitCollectionCompletionReason;
exports.RecordAction = record.RecordAction;
exports.RecordingCompletionReason = record.RecordingCompletionReason;
exports.RecordingFormat = record.RecordingFormat;
exports.RejectAction = reject.RejectAction;
exports.Dialog = dialog.Dialog;
exports.ResumeReason = dialog.ResumeReason;
exports.DialogAction = actions.DialogAction;
exports.PromptType = prompts.PromptType;
exports.Prompts = prompts.Prompts;
exports.SimpleDialog = simple.SimpleDialog;
exports.UniversalCallBot = ucb.UniversalCallBot;
exports.Library = dl.Library;
exports.CallConnector = calling.CallConnector;
exports.MemoryBotStorage = bs.MemoryBotStorage;

