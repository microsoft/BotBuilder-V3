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

type TextType = string|string[];
type MessageType = IMessage|IIsMessage;
type TextOrMessageType = TextType|MessageType;
type CardActionType = ICardAction|IIsCardAction;
type CardImageType = ICardImage|IIsCardImage;
type AttachmentType = IAttachment|IIsAttachment;
type MatchType = RegExp|string|(RegExp|string)[];
type ValueListType = string|string[];
type SemanticActionStates = 'start' | 'continue' | 'done';

interface IEvent {
    type: string;
    address: IAddress;
    agent?: string;
    source?: string;
    sourceEvent?: any;
    user?: IIdentity;
}

interface IMessage extends IEvent {
    timestamp?: string;              // UTC Time when message was sent (set by service)
    localTimestamp?: string;         // Contains the local date and time of the message, expressed in ISO-8601 format. For example, 2016-09-23T13:07:49.4714686-07:00.
    localTimezone?: string;          // Contains the name of the local timezone of the message, expressed in IANA Time Zone database format. For example, America/Los_Angeles.
    summary?: string;                // Text to be displayed by as fall-back and as short description of the message content in e.g. list of recent conversations 
    text?: string;                   // Message text
    speak?: string;                  // Spoken message as Speech Synthesis Markup Language (SSML)
    textLocale?: string;             // Identified language of the message text.
    attachments?: IAttachment[];     // This is placeholder for structured objects attached to this message 
    suggestedActions: ISuggestedActions; // Quick reply actions that can be suggested as part of the message 
    entities?: any[];                // This property is intended to keep structured data objects intended for Client application e.g.: Contacts, Reservation, Booking, Tickets. Structure of these object objects should be known to Client application.
    textFormat?: string;             // Format of text fields [plain|markdown|xml] default:markdown
    attachmentLayout?: string;       // AttachmentLayout - hint for how to deal with multiple attachments Values: [list|carousel] default:list
    inputHint?: string;              // Hint for clients to indicate if the bot is waiting for input or not.
    value?: any;                     // Open-ended value.
    name?: string;                   // Name of the operation to invoke or the name of the event.
    relatesTo?: IAddress;            // Reference to another conversation or message.
    code?: string;                   // Code indicating why the conversation has ended.
    valueType?: string;              // The type of the activity's value object.
    label?: string;                  // A descriptive label for the activity.
    listenFor?: string[];            // List of phrases and references that speech and language priming systems should listen for.
    semanticAction?: ISemanticAction; // An optional programmatic action accompanying this request.
    textHighlights?: ITextHighlight[]; // The collection of text fragments to highlight when the activity contains a ReplyToId value.
    expriation?: string;              // The time at which the activity should be considered to be "expired" and should not be presented to the recipient.
    importance?: string;              // The importance of the activity.
    deliveryMode?: string;            // A delivery hint to signal to the recipient alternate delivery paths for the activity. The default delivery mode is "default".
    callerId?: string                // A string containing an IRI identifying the caller of a bot. This field is not intended to be transmitted over the wire, but is instead populated by bots and clients based on cryptographically verifiable data that asserts the identity of the callers (e.g. tokens).
}

interface IIsMessage {
    toMessage(): IMessage;
}

interface IMessageOptions {
    attachments?: AttachmentType[];
    attachmentLayout?: string;
    entities?: any[];
    textFormat?: string;
    inputHint?: string;
}

interface IIdentity {
    id: string;                     // Channel specific ID for this identity
    name?: string;                  // Friendly name for this identity
    isGroup?: boolean;              // If true the identity is a group. 
    conversationType?: string;      // Indicates the type of the conversation in channels that distinguish  
    role?: string;                  // Role of the entity behind the account (Possible values include: 'user', 'bot')
    aadObjectId?: string;           // This account's object ID within Azure Active Directory (AAD)
    tenantId?: string;              // This conversation's tenant ID, for conversation identities */
}

interface IConversationMembers {
    id: string;                     // Conversation ID
    members: IIdentity[];           // List of members in this conversation
}

interface IConversationsResult {
    continuationToken: string;                // Paging token
    conversations: IConversationMembers[];    // List of conversations
}

interface IPagedMembersResult {
    continuationToken: string;      // Paging token.
    members: IIdentity[];           // List of members in this conversation.
}

interface IBotStateData {
    conversationId?: string;        // ID of the conversation the data is for (if relevant.)
    userId?: string;                // ID of the user the data is for (if relevant.)
    data: string;                   // Exported data.
    lastModified: string;           // Timestamp of when the data was last modified.
}

interface IBotStateDataResult {
    continuationToken: string;      // Paging token.
    botStateData: IBotStateData[];  // Exported bot state records.
}

interface ITokenResponse {
    connectionName: string;         // The connection name.
    token: string;                  // The user token.
    expiration: string;             // Expiration for the token, in ISO 8601 format.
    channelId: string               // The channelId of the TokenResponse
}

interface IAddress {
    channelId: string;              // Unique identifier for channel
    user: IIdentity;                // User that sent or should receive the message
    bot?: IIdentity;                // Bot that either received or is sending the message
    conversation?: IIdentity;       // Represents the current conversation and tracks where replies should be routed to. 
}

interface IAttachment {
    contentType: string;            // MIME type string which describes type of attachment 
    content?: any;                  // (Optional) object structure of attachment 
    contentUrl?: string;            // (Optional) reference to location of attachment content
}

interface IIsAttachment {
    toAttachment(): IAttachment;
}

interface ISigninCard {
    text: string;                   // Title of the Card 
    buttons: ICardAction[];         // Sign in action 
}

interface IOAuthCard {
    text: string;
    connectionName: string;
    buttons: ICardAction[];
}

interface IKeyboard {
    buttons: ICardAction[];         // Set of actions applicable to the current card. 
}

interface IThumbnailCard extends IKeyboard {
    title: string;                  // Title of the Card 
    subtitle: string;               // Subtitle appears just below Title field, differs from Title in font styling only 
    text: string;                   // Text field appears just below subtitle, differs from Subtitle in font styling only 
    images: ICardImage[];           // Messaging supports all media formats: audio, video, images and thumbnails as well to optimize content download. 
    tap: ICardAction;               // This action will be activated when user taps on the section bubble. 
}

interface IMediaCard extends IKeyboard{
    title: string;                  // Title of the Card 
    subtitle: string;               // Subtitle appears just below Title field, differs from Title in font styling only 
    text: string;                   // Text field appears just below subtitle, differs from Subtitle in font styling only 
    image: ICardImage;              // Messaging supports all media formats: audio, video, images and thumbnails as well to optimize content download.
    media: ICardMediaUrl[];         // Media source for video, audio or animations
    autoloop: boolean;              // Should the media source reproduction run in a lool
    autostart: boolean;             // Should the media start automatically
    shareable: boolean;             // Should media be shareable
    value: any;                     // Supplementary parameter for this card.
    duration: string;               // Describes the length of the media content without requiring a receiver to open the content. Formatted as an ISO 8601 Duration field.
    aspect: string;                 // Hint of the aspect ratio of the video or animation. Allowed values are "16:9" and "4:3"
}

interface IVideoCard extends IMediaCard {
}

interface IAnimationCard extends IMediaCard {
}

interface IAudioCard extends IMediaCard {
}

interface IIsCardMedia{
    toMedia(): ICardMediaUrl;      //Returns the media to serialize
}

interface ICardMediaUrl {
    url: string,                    // Url to audio, video or animation media
    profile: string                 // Optional profile hint to the client to differentiate multiple MediaUrl objects from each other
}

interface IReceiptCard {
    title: string;                  // Title of the Card 
    items: IReceiptItem[];          // Array of receipt items.
    facts: IFact[];                 // Array of key-value pairs. 
    tap: ICardAction;                   // This action will be activated when user taps on the section bubble. 
    total: string;                  // Total amount of money paid (or should be paid) 
    tax: string;                    // Total amount of TAX paid (or should be paid) 
    vat: string;                    // Total amount of VAT paid (or should be paid) 
    buttons: ICardAction[];             // Set of actions applicable to the current card. 
}

interface IReceiptItem {
    title: string;                  // Title of the Card 
    subtitle: string;               // Subtitle appears just below Title field, differs from Title in font styling only 
    text: string;                   // Text field appears just below subtitle, differs from Subtitle in font styling only 
    image: ICardImage;
    price: string;                  // Amount with currency 
    quantity: string;               // Number of items of given kind 
    tap: ICardAction;               // This action will be activated when user taps on the Item bubble. 
}

interface IIsReceiptItem {
    toItem(): IReceiptItem;
}

interface ICardAction {
    type: string;                   // Defines the type of action implemented by this button.  
    title: string;                  // Text description which appear on the button. 
    value: string;                  // Parameter for Action. Content of this property depends on Action type. 
    image?: string;                 // (Optional) Picture which will appear on the button, next to text label. 
    text?: string;                  // (Optional) Text for this action.
    displayText?: string;           // (Optional) text to display in the chat feed if the button is clicked.
    channelData?: any;              // (Optional) Channel-specific data associated with this action.
}

interface IIsCardAction {
    toAction(): ICardAction;
}

interface ISuggestedActions {
    to?: string[]; // Optional recipients of the suggested actions. Not supported in all channels.
    actions: ICardAction[]; // Quick reply actions that can be suggested as part of the message 
}


interface IIsSuggestedActions {
    toSuggestedActions(): ISuggestedActions;
}

interface ICardImage {
    url: string;                    // Thumbnail image for major content property. 
    alt: string;                    // Image description intended for screen readers 
    tap: ICardAction;                   // Action assigned to specific Attachment. E.g. navigate to specific URL or play/open media content 
}

interface IIsCardImage {
    toImage(): ICardImage;
}

interface IFact {
    key: string;                    // Name of parameter 
    value: string;                  // Value of parameter 
}

interface IIsFact {
    toFact(): IFact;
}

interface IRating {
    score: number;                  // Score is a floating point number. 
    max: number;                    // Defines maximum score (e.g. 5, 10 or etc). This is mandatory property. 
    text: string;                   // Text to be displayed next to score. 
}

interface ILocationV2 {
    altitude?: number;
    latitude: number;
    longitude: number;
}

interface ILocalizer {
    load(locale: string, callback?: async.ErrorCallback<any>): void;     
    defaultLocale(locale?: string): string   
    gettext(locale: string, msgid: string, namespace?: string): string;
    trygettext(locale: string, msgid: string, namespace?: string): string;
    ngettext(locale: string, msgid: string, msgid_plural: string, count: number, namespace?: string): string;
}

interface IDefaultLocalizerSettings {
    botLocalePath?: string;
    defaultLocale?: string;
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

interface IIntent {
    intent: string;
    score: number;
}

interface IEntity<T> {
    entity: T;
    type: string;
    startIndex?: number;
    endIndex?: number;
    score?: number;
    //resolution?: IEntityResolution<T>;
}

interface IEntityResolution<T> {
    value?: string;
    values?: string[]|ILuisValues[];
}

interface ILuisValues {
    timex: string;
    type: string;
    value: string;
    Start: string;
    End: string;
}

interface ICompositeEntity<T> {
    parentType: string;
    value: string;
    children: ICompositeEntityChild<T>[]
}

interface ICompositeEntityChild<T> {
    type: string;
    value: string;
}

interface ITranscript {
    activities: IMessage[];
}

interface ISemanticAction {
    id: string;
    state?: SemanticActionStates;
    entities: any;
}

interface ITextHighlight {
    text: string;
    occurrence: number;
}

interface ISentiment {
    label: string;
    score: number;
} 