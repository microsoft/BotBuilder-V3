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


declare module skypeSdk {
    export function messagingHandler(botService: BotService): (req: any, res: any) => void;
    function ensureHttps(redirect: any, errorStatus: any): (req: any, res: any, next: Function) => void;
    function verifySkypeCert(options: any): (req: any, res: any, next: Function) => void; 

    export interface IBotServiceConfiguration {
        messaging?: {
            username: string;
            appId: string;
            appSecret: string;
        };
        requestTimeout?: number;
        calling?: {
            callbackUri: string;
        };
    }

    export interface IMessage {
        type: string;
        from: string;
        to: string;
        content: any;
        messageId: number;
        contentType: string;
        eventTime: number;
    }

    export interface IAttachment {
        type: string;
        from: string;
        to: string;
        id: string;
        attachmentName: string;
        attachmentType: string;
        views: IAttachmentViewInfo[];
        eventTime: string;
    }

    export interface IAttachmentViewInfo {
    }

    export interface IContactNotification {
        type: string;
        from: string;
        to: string;
        fromUserDisplayName: string;
        action: string;
    }

    export interface IHistoryDisclosed {
        type: string;
        from: string;
        to: string;
        historyDisclosed: boolean;
        eventTime: number;
    }

    export interface ITopicUpdated {
        type: string;
        from: string;
        to: string;
        topic: string;
        eventTime: number;
    }

    export interface IUserAdded {
        type: string;
        from: string;
        to: string;
        targets: string[];
        eventTime: number;
    }

    export interface IUserRemoved {
        type: string;
        from: string;
        to: string;
        targets: string[];
        eventTime: number;
    }

    export interface ICommandCallback {
        (bot: Bot, request: IMessage): void;
    }

    export interface ISendMessageCallback {
    }

    export interface ICallNotificationHandler {
        conversationResult: IConversationResult;
        workflow: IWorkflow;
        callback: IFinishEventHandling;
    }

    export interface IConversationResult {
    }

    export interface IWorkflow {
    }

    export interface IFinishEventHandling {
        error?: Error;
        workflow?: IWorkflow;
    }

    export interface IIncomingCallHandler {
        conversation: IConversation;
        workflow: IWorkflow;
        callback: IFinishEventHandling;     
    }

    export interface IConversation {
    }

    export interface IProcessCallCallback {
        error?: Error;
        responseMessage?: string;
    }

    export class BotService {
        constructor(configuration: IBotServiceConfiguration);
        on(event: string, listener: Function): void;
        onPersonalCommand(regex: RegExp, callback: ICommandCallback): void;
        onGroupCommand(regex: RegExp, callback: ICommandCallback): void;
        send(to: string, content: string, callback: ISendMessageCallback): void;
        onAnswerCompleted(handler: ICallNotificationHandler): void;
        onIncomingCall(handler: IIncomingCallHandler): void;
        onCallStateChange(handler: Function): void;
        onHangupCompleted(handler: Function): void;
        onPlayPromptCompleted(handler: Function): void;
        onRecognizeCompleted(handler: Function): void;
        onRecordCompleted(handler: Function): void;
        onRejectCompleted(handler: Function): void;
        onWorkflowValidationCompleted(handler: Function): void;
        processCall(content: any, callback: IProcessCallCallback): void;
        processCallback(content: any, additionalData: any, callback: Function): void;
    }

    export class Bot {
        reply(content: string, callback: ISendMessageCallback): void;
        send(to: string, content: string, callback: ISendMessageCallback): void;
    }
}