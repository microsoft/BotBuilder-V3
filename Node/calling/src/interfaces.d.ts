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
interface ILocalizer {
    gettext(language: string, msgid: string): string;
    ngettext(language: string, msgid: string, msgid_plural: string, count: number): string;
}

interface ISession {
    sessionState: ISessionState;
    userData: any;
    dialogData: any;
    error(err: Error): ISession;
    gettext(msgid: string, ...args: any[]): string;
    ngettext(msgid: string, msgid_plural: string, count: number): string;
    send(message: string, ...args: any[]): ISession;
    send(msg: any): ISession;
    messageSent(): boolean;
    beginDialog<T>(id: string, args?: T): ISession;
    replaceDialog<T>(id: string, args?: T): ISession;
    endDialog(result?: any): ISession;
    reset(id: string): ISession;
    isReset(): boolean;
}

interface ISessionState {
    callstack: IDialogState[];
    lastAccess: number;
    version: number;
}

interface IDialogState {
    id: string;
    state: any;
}

interface IBeginDialogHandler {
    (session: ISession, args: any, next: (handled: boolean) => void): void; 
}

interface IDialogHandler<T> {
    (session: ISession, args?: T): void;
}

interface IIntent {
    intent: string;
    score: number;
}

interface IEntity {
    entity: string;
    type: string;
    startIndex?: number;
    endIndex?: number;
    score?: number;
}

interface IMessage {
    type: string;
    address: IConversationAddress;
    user?: IParticipant;
}

interface IIsMessage {
    toMessage(): IMessage;
}

interface IConversationAddress {
    id: string;
    participants: IParticipant[];
    isMultiParty: boolean;
    threadId?: string;
    subject?: string;
    correlationId?: string;
}

interface IConversation extends IMessage {
    callState: string;
    appId?: string;
    links?: any;
    presentedModalityTypes: string[];
}

interface IParticipant {
    identity: string;
    displayName?: string;
    languageId?: string;
    originator: boolean;
}


interface IConversationResult extends IMessage {
    callState: string;
    appId?: string;
    appState?: any;
    links?: any;
    operationOutcome: IActionOutcome; 
    recordedAudio?: Buffer;
}

interface IWorkflow extends IMessage {
    actions: IAction[];
    links?: any;
    appState?: string;
    notificationSubscriptions?: string[]; 
}

interface IActionOutcome {
    type: string;
    id: string;
    outcome: string;
    failureReason?: string;
}

interface IAction {
    action: string;
    operationId: string;
}

interface IIsAction {
    toAction(): IAction;
}

interface IAnswerAction extends IAction {
    acceptModalityTypes?: string[]; 
}

interface IRecordAction extends IAction {
    playPrompt?: IPlayPromptAction;
    maxDurationInSeconds?: number;
    initialSilenceTimeoutInSeconds?: number;
    maxSilenceTimeoutInSeconds?: number; 
    recordingFormat?: string; 
    playBeep?: boolean; 
    stopTones?: string[];
 }

 interface IRecordOutcome extends IActionOutcome {
    completionReason: string;
    lengthOfRecordingInSecs: number; 
 }

interface IPlayPromptAction extends IAction {
    prompts: IPrompt[];
}

interface IPrompt {
    value?: string;
    fileUri?: string;
    voice?: string;
    culture?: string;
    silenceLengthInMilliseconds?: number;
    emphasize?: boolean;
    sayAs?: string;
 }

 interface IIsPrompt {
     toPrompt(): IPrompt;
 }

interface IRecognizeAction extends IAction {
    playPrompt?: IPlayPromptAction;
    bargeInAllowed?: boolean;
    culture?: string; 
    initialSilenceTimeoutInSeconds?: number; 
    interdigitTimeoutInSeconds?: number; 
    choices?: IRecognitionChoice[];
    collectDigits?: ICollectDigits;
}

interface IRecognizeOutcome extends IActionOutcome {
    choiceOutcome?: IChoiceOutcome;
    collectDigitsOutcome?: ICollectDigitsOutcome;
}

interface IRecognitionChoice {
    name: string;
    speechVariation?: string[];
    dtmfVariation?: string;
}

interface ICollectDigits {
    maxNumberOfDtmfs?: number; 
    stopTones?: string[];
}

interface IChoiceOutcome {
    completionReason: string; 
    choiceName?: string;
}

interface ICollectDigitsOutcome {
    completionReason: string; 
    digits?: string;
}