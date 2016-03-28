
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