
interface IMessage {
    type?: string;
    id?: string;
    conversationId?: string;
    created?: string;
    sourceText?: string;
    sourceLanguage?: string;
    language?: string;
    text?: string;
    attachments?: IAttachment[];
    from?: IChannelAccount;
    to?: IChannelAccount;
    replyTo?: IChannelAccount;
    replyToMessageId?: string;
    participants?: IChannelAccount[];
    totalParticipants?: number;
    mentions?: IMention[];
    place?: string;
    channelMessageId?: string;
    channelConversationId?: string;
    channelData?: any;
    location?: ILocation;
    hashtags?: string[];
    eTag?: string;
}

interface IAttachment {
    contentType: string;
    contentUrl?: string;
    content?: any;
    fallbackText?: string;
    title?: string;
    titleLink?: string;
    text?: string;
    thumbnailUrl?: string;
}

interface IChannelAccount {
    name?: string;
    channelId: string;
    address: string;
    id?: string;
    isBot?: boolean;
}

interface IMention {
    mentioned?: IChannelAccount;
    text?: string;
}

interface ILocation {
    altitude?: number;
    latitude: number;
    longitude: number;
}

interface IBeginDialogAddress {
    to: IChannelAccount;
    from?: IChannelAccount;
    language?: string;
    text?: string;
}

interface ILocalizer {
    gettext(language: string, msgid: string): string;
    ngettext(language: string, msgid: string, msgid_plural: string, count: number): string;
}

interface ISession {
    sessionState: ISessionState;
    message: IMessage;
    userData: any;
    dialogData: any;
    error(err: Error): ISession;
    gettext(msgid: string, ...args: any[]): string;
    ngettext(msgid: string, msgid_plural: string, count: number): string;
    send(): ISession;
    send(msg: string, ...args: any[]): ISession;
    send(msg: IMessage): ISession;
    getMessageReceived(): any;
    sendMessage(msg: any): ISession;
    messageSent(): boolean;
    beginDialog<T>(id: string, args?: T): ISession;
    replaceDialog<T>(id: string, args?: T): ISession;
    endDialog(result?: any): ISession;
    compareConfidence(language: string, utterance: string, score: number, callback: (handled: boolean) => void): void;
    reset(id: string): ISession;
    isReset(): boolean;
}

interface ISessionAction {
    userData: any;
    dialogData: any;
    next(): void;
    endDialog(result?: any): void;
    send(msg: string, ...args: any[]): void;
    send(msg: IMessage): void;
}

interface ISessionState {
    callstack: IDialogState[];
    lastAccess: number;
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
