//=============================================================================
//
// INTERFACES
//
//=============================================================================

/** 
 * A chat message sent between a User and a Bot. Messages from the bot to the user come in two flavors: 
 * 
 * * __reactive messages__ are messages sent from the Bot to the User as a reply to an incoming message from the user. 
 * * __proactive messages__ are messages sent from the Bot to the User in response to some external event like an alarm triggering.
 * 
 * In the reactive case the you should copy the [address](#address) field from the incoming message to the outgoing message (if you use the [Message]( /en-us/sdkreference/nodejs/classes/_botbuilder_d_.message.html) builder class and initialize it with the 
 * [session](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.session.html) this will happen automatically) and then set the [text](#text) or [attachments](#attachments).  For proactive messages you’ll need save the [address](#address) from the incoming message to 
 * an external storage somewhere. You can then later pass this in to [UniversalBot.beginDialog()](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.universalbot.html#begindialog) or copy it to an outgoing message passed to 
 * [UniversalBot.send()](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.universalbot.html#send). 
 *
 * Composing a message to the user using the incoming address object will by default send a reply to the user in the context of the current conversation. Some channels allow for the starting of new conversations with the user. To start a new proactive conversation with the user simply delete 
 * the [conversation](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.iaddress.html#conversation) field from the address object before composing the outgoing message.
 */
interface IMessage {
    /** Address routing information for the message. Save this field to external storage somewhere to later compose a proactive message to the user. */
    address: IAddress; 
    
    /** Timestamp of message given by chat service for incoming messages. */
    timestamp: string;

    /** Message in original/native format of the channel for incoming messages. For outgoing messages can be used to pass channel specific message data like channel specific attachments. */  
    channelData: any;  
    
    /** Defines type of notification. */
    type: string;

    /** Text to be displayed by as fall-back and as short description of the message content in e.g. list of recent conversations. */  
    summary: string; 

    /** Message text. */
    text: string;

    /** Identified language of the message if known. */   
    local: string;

    /** For incoming messages contains attachments like images sent from the user. For outgoing messages contains objects like cards or images to send to the user.   */
    attachments: IAttachment[]; 

    /** Structured objects passed to the bot or user. */
    entities: any[];

    /** Format of text fields. The default value is 'markdown'. */
    textFormat: string;

    /** Hint for how clients should layout multiple attachments. The default value is 'list'. */ 
    attachmentLayout: string; 

    /** 
     * For incoming messages this is the user that sent the message. By default this is a copy of [address.user](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.iaddress.html#user) but you can configure your bot with a 
     * [lookupUser](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.iuniversalbotsettings.html#lookupuser) function that lets map the incoming user to an internal user id.
     */
    user: IIdentity;
}

/** Implemented by classes that can be converted into a message. */
interface IIsMessage {
    /** Returns the JSON object for the message. */
    toMessage(): IMessage;
}

/** Represents a user, bot, or conversation. */
interface IIdentity {
    /** Channel specific ID for this identity. */
    id: string;

    /** Friendly name for this identity. */ 
    name?: string;

    /** If true the identity is a group. Typically only found on conversation identities. */ 
    isGroup?: boolean;   
}

/** 
 * Address routing information for a [message](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.imessage.html#address). 
 * Addresses are bidirectional meaning they can be used to address both incoming and outgoing messages. They're also connector specific meaning that
 * [connectors](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.iconnector.html) are free to add their own fields.
 */
interface IAddress {
    /** Unique identifier for channel. */
    channelId: string;

    /** User that sent or should receive the message. */
    user: IIdentity;

    /** Bot that either received or is sending the message. */ 
    bot: IIdentity;

    /** 
     * Represents the current conversation and tracks where replies should be routed to. 
     * Can be deleted to start a new conversation with a [user](#user) on channels that support new conversations.
     */ 
    conversation?: IIdentity;  
}

/**  
 * Many messaging channels provide the ability to attach richer objects. Bot Builder lets you express these attachments in a cross channel way and [connectors](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.iconnector.html) will do their best to render the 
 * attachments using the channels native constructs. If you desire more control over the channels rendering of a message you can use [IMessage.channelData](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.imessage.html#channeldata) to provide attachments using 
 * the channels native schema. The types of attachments that can be sent varies by channel but these are the basic types:
 * 
 * * __Media and Files:__  Basic files can be sent by setting [contentType](#contenttype) to the MIME type of the file and then passing a link to the file in [contentUrl](#contenturl).
 * * __Cards and Keyboards:__  A rich set of visual cards and custom keyboards can by setting [contentType](#contenttype) to the cards type and then passing the JSON for the card in [content](#content). If you use one of the rich card builder classes like
 * [HeroCard](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.herocard.html) the attachment will automatically filled in for you.    
 */
interface IAttachment {
    /** MIME type string which describes type of attachment. */
    contentType: string;

    /** (Optional) object structure of attachment. */  
    content?: any;

    /** (Optional) reference to location of attachment content. */  
    contentUrl?: string; 
}

/** Implemented by classes that can be converted into an attachment. */
interface IIsAttachment {
    /** Returns the JSON object for the attachment. */
    toAttachment(): IAttachment;
}

/** Displays a signin card and button to the user. Some channels may choose to render this as a text prompt and link to click. */
interface ISigninCard {
    /** Title of the Card. */
    title: string;

    /** Sign in action. */  
    buttons: ICardAction[];  
}

/** 
 * Displays a card to the user using either a smaller thumbnail layout or larger hero layout (the attachments [contentType](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.iattachment.html#contenttype) determines which). 
 * All of the cards fields are optional so this card can be used to specify things like a keyboard on certain channels. Some channels may choose to render a lower fidelity version of the card or use an alternate representation. 
 */
interface IThumbnailCard {
    /** Title of the Card. */
    title?: string;

    /** Subtitle appears just below Title field, differs from Title in font styling only. */  
    subtitle?: string;

    /** Text field appears just below subtitle, differs from Subtitle in font styling only. */
    text?: string;

    /** Messaging supports all media formats: audio, video, images and thumbnails as well to optimize content download. */  
    images?: ICardImage[];

    /** This action will be activated when user taps on the card. Not all channels support tap actions and some channels may choose to render the tap action as the titles link. */  
    tap?: ICardAction;

    /** Set of actions applicable to the current card. Not all channels support buttons or cards with buttons. Some channels may choose to render the buttons using a custom keyboard. */  
    buttons?: ICardAction[];  
}

/** Displays a rich receipt to a user for something they've either bought or are planning to buy. */
interface IReceiptCard {
    /** Title of the Card. */
    title: string;

    /** Array of receipt items. */  
    items: IReceiptItem[];

    /** Array of additional facts to display to user (shipping charges and such.) Not all facts will be displayed on all channels. */ 
    facts: IFact[];

    /** This action will be activated when user taps on the card. Not all channels support tap actions. */  
    tap: ICardAction;

    /** Total amount of money paid (or should be paid.) */  
    total: string;

    /** Total amount of TAX paid (or should be paid.) */
    tax: string;

    /** Total amount of VAT paid (or should be paid.) */  
    vat: string;

    /** Set of actions applicable to the current card. Not all channels support buttons and the number of allowed buttons varies by channel. */  
    buttons: ICardAction[];  
}

/** An individual item within a [receipt](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.ireceiptcard.html). */
interface IReceiptItem {
    /** Title of the item. */
    title: string;
    
    /** Subtitle appears just below Title field, differs from Title in font styling only. On some channels may be combined with the [title](#title) or [text](#text). */
    subtitle: string;

    /** Text field appears just below subtitle, differs from Subtitle in font styling only. */  
    text: string;

    /** Image to display on the card. Some channels may either send the image as a seperate message or simply include a link to the image. */  
    image: ICardImage;
    
    /** Amount with currency. */
    price: string;

    /** Number of items of given kind. */  
    quantity: string;

    /** This action will be activated when user taps on the Item bubble. Not all channels support tap actions. */  
    tap: ICardAction;  
}

/** Implemented by classes that can be converted into a receipt item. */
interface IIsReceiptItem {
    /** Returns the JSON object for the receipt item. */
    toItem(): IReceiptItem;
}

/** The action that should be performed when a card, button, or image is tapped.  */
interface ICardAction {
    /** Defines the type of action implemented by this button. Not all action types are supported by all channels. */
    type: string;

    /** Text description for button actions. */   
    title?: string;

    /** Parameter for Action. Content of this property depends on Action type. */  
    value: string;

    /** (Optional) Picture to display for button actions. Not all channels support button images. */  
    image?: string;  
}

/** Implemented by classes that can be converted into a card action. */
interface IIsCardAction {
    /** Returns the JSON object for the card attachment. */
    toAction(): ICardAction;
}

/** An image on a card. */
interface ICardImage {
    /** Thumbnail image for major content property. */
    url: string;

    /** Image description intended for screen readers. Not all channels will support alt text. */  
    alt: string;

    /** Action assigned to specific Attachment. E.g. navigate to specific URL or play/open media content. Not all channels will support tap actions. */  
    tap: ICardAction;  
}

/** Implemented by classes that can be converted into a card image. */
interface IIsCardImage {
    /** Returns the JSON object for the card image. */
    toImage(): ICardImage;
}

/** A fact displayed on a card like a [receipt](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.ireceiptcard.html). */
interface IFact {
    /** Display name of the fact. */
    key: string;

    /** Display value of the fact. */  
    value: string;  
}

/** Implemented by classes that can be converted into a fact. */
interface IIsFact {
    /** Returns the JSON object for the fact. */
    toFact(): IFact;
}

/** Plugin for localizing messages sent to the user by a bot. */
export interface ILocalizer {
    /**
     * Loads a localized string for the specified language.
     * @param language Desired language of the string to return.
     * @param msgid String to use as a key in the localized string table. Typically this will just be the english version of the string.
     */
    gettext(language: string, msgid: string): string;

    /**
     * Loads the plural form of a localized string for the specified language.
     * @param language Desired language of the string to return.
     * @param msgid Singular form of the string to use as a key in the localized string table.
     * @param msgid_plural Plural form of the string to use as a key in the localized string table.
     * @param count Count to use when determining whether the singular or plural form of the string should be used.
     */
    ngettext(language: string, msgid: string, msgid_plural: string, count: number): string;
}

/** Persisted session state used to track a conversations dialog stack. */
export interface ISessionState {
    /** Dialog stack for the current session. */
    callstack: IDialogState[];

    /** Timestamp of when the session was last accessed. */
    lastAccess: number;

    /** Version number of the current callstack. */
    version: number;
}

/** An entry on the sessions dialog stack. */
export interface IDialogState {
    /** ID of the dialog. */
    id: string;

    /** Persisted state for the dialog. */
    state: any;
}

/** 
  * Results returned by a child dialog to its parent via a call to session.endDialog(). 
  */
export interface IDialogResult<T> {
    /** The reason why the current dialog is being resumed. */
    resumed: ResumeReason;

    /** ID of the child dialog thats ending. */
    childId?: string;

    /** If an error occured the child dialog can return the error to the parent. */
    error?: Error;

    /** The users response. */
    response?: T;
}

/** Context of the recieved message passed to the Dialog.recognize() method. */
export interface IRecognizeContext {
    /** Message that was received. */
    message: IMessage;

    /** If true the Dialog is the active dialog on the callstack. */
    activeDialog: boolean;
}

/** Results from a call to a recognize() function. The implementation is free to add any additional properties to the result. */
export interface IRecognizeResult {
    /** Confidence that the users utterance was understood on a scale from 0.0 - 1.0.  */
    score: number;
}

/** Options passed to built-in prompts. */
export interface IPromptOptions {
    /** 
     * (Optional) retry prompt to send if the users response isn't understood. Default is to just 
     * reprompt with the configured [defaultRetryPrompt](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.ipromptsoptions.html#defaultretryprompt) 
     * plus the original prompt. 
     * 
     * Note that if the original prompt is an _IMessage_ the retry prompt will be sent as a seperate 
     * message followed by the original message. If the retryPrompt is also an _IMessage_ it will 
     * instead be sent in place of the original message. 
     * * _{string}_ - Initial message to send the user.
     * * _{string[]}_ - Array of possible messages to send user. One will be chosen at random. 
     * * _{IMessage}_ - Initial message to send the user. Message can contain attachments. 
     * * _{IIsMessage}_ - Instance of the [Message](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.message.html) builder class. 
     */
    retryPrompt?: string|string[]|IMessage|IIsMessage;

    /** (Optional) maximum number of times to reprompt the user. Default value is 2. */
    maxRetries?: number;

    /** (Optional) reference date when recognizing times. Date expressed in ticks using Date.getTime(). */
    refDate?: number;

    /** (Optional) type of list to render for PromptType.choice. Default value is ListStyle.auto. */
    listStyle?: ListStyle;
}

/** Arguments passed to the built-in prompts beginDialog() call. */
export interface IPromptArgs extends IPromptOptions {
    /** Type of prompt invoked. */
    promptType: PromptType;

    /** 
     * Initial message to send to user. 
     * * _{string}_ - Initial message to send the user.
     * * _{string[]}_ - Array of possible messages to send user. One will be chosen at random. 
     * * _{IMessage}_ - Initial message to send the user. Message can contain attachments. 
     * * _{IIsMessage}_ - Instance of the [Message](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.message.html) builder class. 
     */
    prompt: string|string[]|IMessage|IIsMessage;

    /** Enum values for a choice prompt. */
    enumsValues?: string[];
}

/** Dialog result returned by a system prompt. */
export interface IPromptResult<T> extends IDialogResult<T> {
    /** Type of prompt completing. */
    promptType?: PromptType;
}

/** Result returned from an IPromptRecognizer. */
export interface IPromptRecognizerResult<T> extends IPromptResult<T> {
    /** Returned from a prompt recognizer to indicate that a parent dialog handled (or captured) the utterance. */
    handled?: boolean;
}

/** Strongly typed Text Prompt Result. */
export interface IPromptTextResult extends IPromptResult<string> { }

/** Strongly typed Number Prompt Result. */
export interface IPromptNumberResult extends IPromptResult<number> { }

/** Strongly typed Confirm Prompt Result. */
export interface IPromptConfirmResult extends IPromptResult<boolean> { } 

/** Strongly typed Choice Prompt Result. */
export interface IPromptChoiceResult extends IPromptResult<IFindMatchResult> { }

/** Strongly typed Time Prompt Result. */
export interface IPromptTimeResult extends IPromptResult<IEntity> { }

/** Strongly typed Attachment Prompt Result. */
export interface IPromptAttachmentResult extends IPromptResult<IAttachment[]> { }

/** Plugin for recognizing prompt responses recieved by a user. */
export interface IPromptRecognizer {
    /**
      * Attempts to match a users reponse to a given prompt.
      * @param args Arguments passed to the recognizer including that language, text, and prompt choices.
      * @param callback Function to invoke with the result of the recognition attempt.
      * @param callback.result Returns the result of the recognition attempt.
      */
    recognize<T>(args: IPromptRecognizerArgs, callback: (result: IPromptRecognizerResult<T>) => void): void;
}

/** Arguments passed to the IPromptRecognizer.recognize() method.*/
export interface IPromptRecognizerArgs {
    /** Type of prompt being responded to. */
    promptType: PromptType;

    /** Text of the users response to the prompt. */
    text: string;

    /** Language of the text if known. */
    language?: string;

    /** For choice prompts the list of possible choices. */
    enumValues?: string[];

    /** (Optional) reference date when recognizing times. */
    refDate?: number;
}

/** Global configuration options for the Prompts dialog. */
export interface IPromptsOptions {
    /** Replaces the default recognizer (SimplePromptRecognizer) used to recognize prompt replies. */
    recognizer?: IPromptRecognizer
}

/** A recognized intent. */
export interface IIntent {
    /** Intent that was recognized. */
    intent: string;

    /** Confidence on a scale from 0.0 - 1.0 that the proper intent was recognized. */
    score: number;
}

/** A recognized entity. */
export interface IEntity {
    /** Type of entity that was recognized. */
    type: string;

    /** Value of the recognized entity. */
    entity: string;

    /** Start position of entity within text utterance. */
    startIndex?: number;

    /** End position of entity within text utterance. */
    endIndex?: number;

    /** Confidence on a scale from 0.0 - 1.0 that the proper entity was recognized. */
    score?: number;
}

/** Options used to configure an [IntentDialog](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.intentdialog.html). */
export interface IIntentDialogOptions {
    /** Minimum score needed to trigger the recognition of an intent. The default value is 0.1. */
    intentThreshold?: number;

    /** The order in which the configured [recognizers](#recognizers) should be evaluated. The default order is parallel. */
    recognizeOrder?: RecognizeOrder;

    /** (Optional) list of intent recognizers to run the users utterance through. */
    recognizers?: IIntentRecognizer[];

    /** Maximum number of recognizers to evaluate at one time when [recognizerOrder](#recognizerorder) is parallel. */
    processLimit?: number;
} 

/** Interface implemented by intent recognizers like the LuisRecognizer class. */
export interface IIntentRecognizer {
    /** Attempts to match a users text utterance to an intent. */
    recognize(context: IRecognizeContext, cb: (err: Error, result: IIntentRecognizerResult) => void): void;
}

/** Results returned by an intent recognizer. */
export interface IIntentRecognizerResult extends IRecognizeResult {
    /** Top intent that was matched. */
    intent: string;

    /** A regular expression that was matched. */
    expression?: RegExp;

    /** The text that was matched by [expression](#expression). */
    matched?: string;

    /** Full list of intents that were matched. */
    intents?: IIntent[];

    /** List of entities recognized. */
    entities?: IEntity[]; 
}

/** Options passed to the constructor of a session. */
export interface ISessionOptions {
    /** Function to invoke when the sessions state is saved. */
    onSave: (done: (err: Error) => void) => void;

    /** Function to invoke when a batch of messages are sent. */
    onSend: (messages: IMessage[], done: (err: Error) => void) => void;

    /** Collection of dialogs to use for routing purposes. Typically this is just the bot. */
    dialogs: DialogCollection;

    /** Unique ID of the dialog to use when starting a new conversation with a user. */
    dialogId: string;

    /** (Optional) arguments to pass to the conversations initial dialog. */
    dialogArgs?: any;

    /** (Optional) localizer to use when localizing the bots responses. */
    localizer?: ILocalizer;
    
    /** (Optional) time to allow between each message sent as a batch. The default value is 150ms.  */
    autoBatchDelay?: number;
}

/** result returnd from a call to EntityRecognizer.findBestMatch() or EntityRecognizer.findAllMatches(). */
export interface IFindMatchResult {
    /** Index of the matched value. */
    index: number;

    /** Value that was matched.  */
    entity: string;

    /** Confidence score on a scale from 0.0 - 1.0 that an value matched the users utterance. */
    score: number;
}

/** Context object passed to IBotStorage calls. */
export interface IBotStorageContext {
    /** (Optional) ID of the user being persisted. If missing __userData__ won't be persisted.  */
    userId?: string;

    /** (Optional) ID of the conversation being persisted. If missing __conversationData__ and __privateConversationData__ won't be persisted. */
    conversationId?: string;

    /** (Optional) Address of the message received by the bot. */
    address?: IAddress;

    /** If true IBotStorage should persist __userData__. */
    persistUserData: boolean;

    /** If true IBotStorage should persist __conversationData__.  */
    persistConversationData: boolean;
}

/** Data values persisted to IBotStorage. */
export interface IBotStorageData {
    /** The bots data about a user. This data is global across all of the users conversations. */
    userData?: any;

    /** The bots shared data for a conversation. This data is visible to every user within the conversation.  */
    conversationData?: any;

    /** 
     * The bots private data for a conversation.  This data is only visible to the given user within the conversation. 
     * The session stores its session state using privateConversationData so it should always be persisted. 
     */
    privateConversationData?: any;
}

/** Replacable storage system used by UniversalBot. */
export interface IBotStorage {
    /** Reads in data from storage. */
    getData(context: IBotStorageContext, callback: (err: Error, data: IBotStorageData) => void): void;
    
    /** Writes out data to storage. */
    saveData(context: IBotStorageContext, data: IBotStorageData, callback?: (err: Error) => void): void;
}

/** Options used to initialize a ChatConnector instance. */
export interface IChatConnectorSettings {
    /** The bots App ID assigned in the Bot Framework portal. */
    appId?: string;

    /** The bots App Password assigned in the Bot Framework Portal. */
    appPassword?: string;
}

/** Options used to initialize a UniversalBot instance. */
export interface IUniversalBotSettings {
    /** (Optional) dialog to launch when a user initiates a new conversation with a bot. Default value is '/'. */
    defaultDialogId?: string;
    
    /** (Optional) arguments to pass to the initial dialog for a conversation. */
    defaultDialogArgs?: any;

    /** (Optional) localizer used to localize the bots responses to the user. */
    localizer?: ILocalizer;

    /** (Optional) function used to map the user ID for an incoming message to another user ID.  This can be used to implement user account linking. */
    lookupUser?: (address: IAddress, done: (err: Error, user: IIdentity) => void) => void;
    
    /** (Optional) maximum number of async options to conduct in parallel. */
    processLimit?: number;

    /** (Optional) time to allow between each message sent as a batch. The default value is 150ms.  */
    autoBatchDelay?: number;

    /** (Optional) storage system to use for storing user & conversation data. */
    storage?: IBotStorage;

    /** (optional) if true userData will be persisted. The default value is true. */
    persistUserData?: boolean;

    /** (Optional) if true shared conversationData will be persisted. The default value is false. */
    persistConversationData?: boolean;
}

/** Implemented by connector plugins for the UniversalBot. */
export interface IConnector {
    /** Called by the UniversalBot at registration time to register a handler for receiving incoming messages from a user. */
    onMessage(handler: (messages: IMessage[], cb?: (err: Error) => void) => void): void;

    /** Called by the UniversalBot to deliver outgoing messages to a user. */
    send(messages: IMessage[], cb: (err: Error) => void): void;

    /** Called when a UniversalBot wants to start a new proactive conversation with a user. The connector should return a properly formated __address__ object with a populated __conversation__ field. */
    startConversation(address: IAddress, cb: (err: Error, address?: IAddress) => void): void;
}

/** 
 * Map of middleware hooks that can be registered in a call to __UniversalBot.use()__. */
export interface IMiddlewareMap {
    /** Called in series when an incoming message is received. */
    receive?: (message: IMessage, next: Function) => void;

    /** Called in parallel when an incoming message has been received. Executed immediately after [receive](#receive) middleware. */
    analyze?: (message: IMessage, done: (analysis: any) => void) => void;

    /** Called in series once an incoming message has been bound to a session. Executed after [analyze](#analyze) middleware.  */
    dialog?: (session: Session, next: Function) => void;

    /** Called in series before an outgoing message is sent. */
    send?: (message: IMessage, next: Function) => void;
}

/** 
 * Signature for functions passed as steps to [DialogAction.waterfall()](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.dialogaction.html#waterfall). 
 * 
 * Waterfalls let you prompt a user for information using a sequence of questions. Each step of the
 * waterfall can either execute one of the built-in [Prompts](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.prompts.html),
 * start a new dialog by calling [session.beginDialog()](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.session.html#begindialog),
 * advance to the next step of the waterfall manually using `skip()`, or terminate the waterfall.
 * 
 * When either a dialog or built-in prompt is called from a waterfall step, the results from that 
 * dialog or prompt will be passed via the `results` parameter to the next step of the waterfall. 
 * Users can say things like "nevermind" to cancel the built-in prompts so you should guard against
 * that by at least checking for [results.response](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#response) 
 * before proceeding. A more detailed explination of why the waterfall is being continued can be 
 * determined by looking at the [code](/en-us/sdkreference/nodejs/enums/_botbuilder_d_.resumereason.html) 
 * returned for [results.resumed](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#resumed).
 * 
 * You can manually advance to the next step of the waterfall using the `skip()` function passed
 * in. Calling `skip({ response: "some text" })` with an [IDialogResult](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html)
 * lets you more accurately mimic the results from a built-in prompt and can simplify your overall
 * waterfall logic.
 * 
 * You can terminate a waterfall early by either falling through every step of the waterfall using
 * calls to `skip()` or simply not starting another prompt or dialog.
 * 
 * __note:__ Waterfalls have a hidden last step which will automatically end the current dialog if 
 * if you call a prompt or dialog from the last step. This is useful where you have a deep stack of
 * dialogs and want a call to [session.endDialog()](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.session.html#enddialog)
 * from the last child on the stack to end the entire stack. The close of the last child will trigger
 * all of its parents to move to this hidden step which will cascade the close all the way up the stack.
 * This is typically a desired behaviour but if you want to avoid it or stop it somewhere in the 
 * middle you'll need to add a step to the end of your waterfall that either does nothing or calls 
 * something liek [session.send()](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.session.html#send)
 * which isn't going to advance the waterfall forward.   
 * @example
 * <pre><code>
 * var bot = new builder.BotConnectorBot();
 * bot.add('/', [
 *     function (session) {
 *         builder.Prompts.text(session, "Hi! What's your name?");
 *     },
 *     function (session, results) {
 *         if (results && results.response) {
 *             // User answered question.
 *             session.send("Hello %s.", results.response);
 *         } else {
 *             // User said nevermind.
 *             session.send("OK. Goodbye.");
 *         }
 *     }
 * ]);
 * </code></pre>
 */
export interface IDialogWaterfallStep {
    /**
     * @param session Session object for the current conversation.
     * @param result 
     * * __result:__ _{any}_ - For the first step of the waterfall this will be `null` or the value of any arguments passed to the handler.
     * * __result:__ _{IDialogResult}_ - For subsequent waterfall steps this will be the result of the prompt or dialog called in the previous step.
     * @param skip Fuction used to manually skip to the next step of the waterfall.  
     * @param skip.results (Optional) results to pass to the next waterfall step. This lets you more accurately mimic the results returned from a prompt or dialog.
     */
    (session: Session, result?: any | IDialogResult<any>, skip?: (results?: IDialogResult<any>) => void): any;
}

/** A per/local mapping of LUIS service url's to use for a LuisRecognizer.  */
export interface ILuisModelMap {
    [local: string]: string;
}

/** A per/channel mapping of custom channel data to send. */
export interface IChannelDataMap {
    [channelId: string]: any;
}

/** Options passed to Middleware.dialogVersion(). */
export interface IDialogVersionOptions {
    /** Current major.minor version for the bots dialogs. Major version increments result in existing conversations between the bot and user being restarted. */
    version: number;

    /** Optional message to send the user when their conversation is ended due to a version number change. */
    message?: string|string[]|IMessage|IIsMessage;

    /** Optional regular expression to listen for to manually detect a request to reset the users session state. */
    resetCommand?: RegExp;
}

//=============================================================================
//
// ENUMS
//
//=============================================================================

/** Reason codes for why a dialog was resumed. */
export enum ResumeReason {
    /** The user completed the child dialog and a result was returned. */
    completed,

    /** The user did not complete the child dialog for some reason. They may have exceeded maxRetries or canceled. */
    notCompleted,

    /** The user requested to cancel the current operation. */
    canceled,

    /** The user requested to return to the previous step in a dialog flow. */
    back,

    /** The user requested to skip the current step of a dialog flow. */
    forward
}

/** Order in which an [IntentDialogs](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.intentdialog.html) recognizers should be evaluated. */
export enum RecognizeOrder { 
    /** All recognizers will be evaluated in parallel. */
    parallel,

    /** Recognizers will be evaluated in series. Any recognizer that returns a score of 1.0 will prevent the evaluation of the remaining recognizers. */
    series 
}

/**
  * Type of prompt invoked.
  */
export enum PromptType {
    /** The user is prompted for a string of text. */
    text,

    /** The user is prompted to enter a number. */
    number,

    /** The user is prompted to confirm an action with a yes/no response. */
    confirm,

    /** The user is prompted to select from a list of choices. */
    choice,

    /** The user is prompted to enter a time. */
    time,

    /** The user is prompted to upload an attachment. */
    attachment
}

/** Type of list to render for PromptType.choice prompt. */
export enum ListStyle { 
    /** No list is rendered. This is used when the list is included as part of the prompt. */
    none, 
    
    /** Choices are rendered as an inline list of the form "1. red, 2. green, or 3. blue". */
    inline, 
    
    /** Choices are rendered as a numbered list. */
    list,
    
    /** Choices are rendered as buttons for channels that support buttons. For other channels they will be rendered as text. */
    button,
    
    /** The style is selected automatically based on the channel and number of options. */
    auto
}

/** Identifies the type of text being sent in a message.  */
export var TextFormat: {
    /** Text fields should be treated as plain text. */
    plain: string;

    /** Text fields may contain markdown formatting information. */
    markdown: string;

    /** Text fields may contain xml formatting information. */
    xml: string;
};

/** Identities how the client should render attachments for a message. */
export var AttachmentLayout: {
    /** Attachments should be rendred as a list. */    
    list: string;

    /** Attachments should be rendered as a carousel. */
    carousel: string;
};


//=============================================================================
//
// CLASSES
//
//=============================================================================

/**
 * Manages the bots conversation with a user.
 */
export class Session {
    /**
     * Registers an event listener.
     * @param event Name of the event. Event types:
     * - __error:__ An error occured. [ISessionErrorEvent](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.isessionerrorevent.html)
     * @param listener Function to invoke.
     */
    on(event: string, listener: Function): void;

    /** Sessions configuration options. */
    protected options: ISessionOptions;

    /**
     * Creates an instance of the session.
     * @param options Sessions configuration options.
     */
    constructor(options: ISessionOptions);

    /**
     * Dispatches a message for processing. The session will call any installed middleware before
     * the message to the active dialog for processing. 
     * @param sessionState The current session state. If _null_ a new conversation will be started beginning with the configured [dialogId](#dialogid).  
     * @param message The message to dispatch.
     */
    dispatch(sessionState: ISessionState, message: IMessage): Session;

    /** The sessions collection of available dialogs & middleware for message routing purposes. */
    dialogs: DialogCollection;

    /** Sessions current state information. */
    sessionState: ISessionState;

    /** The message recieved from the user. For bot originated messages this may only contain the "to" & "from" fields. */
    message: IMessage;

    /** Data for the user that's persisted across all conversations with the bot. */
    userData: any;
    
    /** Shared conversation data that's visible to all members of the conversation. */
    conversationData: any;
    
    /** Private conversation data that's only visible to the user. */
    privateConversationData: any;

    /** Data that's only visible to the current dialog. */
    dialogData: any;

    /**
     * Signals that an error occured. The bot will signal the error via an on('error', err) event.
     * @param err Error that occured.
     */
    error(err: Error): Session;

    /**
     * Loads a localized string for the messages language. If arguments are passed the localized string
     * will be treated as a template and formatted using [sprintf-js](https://github.com/alexei/sprintf.js) (see their docs for details.) 
     * @param msgid String to use as a key in the localized string table. Typically this will just be the english version of the string.
     * @param args (Optional) arguments used to format the final output string. 
     */
    gettext(msgid: string, ...args: any[]): string;

    /**
     * Loads the plural form of a localized string for the messages language. The output string will be formatted to 
     * include the count by replacing %d in the string with the count.
     * @param msgid Singular form of the string to use as a key in the localized string table. Use %d to specify where the count should go.
     * @param msgid_plural Plural form of the string to use as a key in the localized string table. Use %d to specify where the count should go.
     * @param count Count to use when determining whether the singular or plural form of the string should be used.
     */
    ngettext(msgid: string, msgid_plural: string, count: number): string;

    /** Triggers saving of changes made to [dialogData](#dialogdata), [userData](#userdata), [conversationdata](#conversationdata), or [privateConversationData'(#privateconversationdata). */
    save(): Session;

    /**
     * Sends a message to the user. If [send()](#send) is called without any parameters any changes to
     * [dialogData](#dialogdata) or [userData](#userdata) will be saved but the user will not recieve any reply. 
     * @param message 
     * * __message:__ _{string}_ - Text of the message to send. The message will be localized using the sessions configured localizer. If arguments are passed in the message will be formatted using [sprintf-js](https://github.com/alexei/sprintf.js).
     * * __message:__ _{string[]}_ - The sent message will be chosen at random from the array.
     * * __message:__ _{IMessage|IIsMessage}_ - Message to send. 
     * @param args (Optional) arguments used to format the final output text when __message__ is a _{string|string[]}_.
     */
    send(message: string|string[]|IMessage|IIsMessage, ...args: any[]): Session;

    /**
     * Returns true if a message has been sent for this session.
     */
    messageSent(): boolean;

    /**
     * Passes control of the conversation to a new dialog. The current dialog will be suspended 
     * until the child dialog completes. Once the child ends the current dialog will receive a
     * call to [dialogResumed()](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.dialog.html#dialogresumed) 
     * where it can inspect any results returned from the child. 
     * @param id Unique ID of the dialog to start.
     * @param args (Optional) arguments to pass to the dialogs [begin()](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.dialog.html#begin) method.
     */
    beginDialog<T>(id: string, args?: T): Session;

    /**
     * Ends the current dialog and starts a new one its place. The parent dialog will not be 
     * resumed until the new dialog completes. 
     * @param id Unique ID of the dialog to start.
     * @param args (Optional) arguments to pass to the dialogs [begin()](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.dialog.html#begin) method.
     */
    replaceDialog<T>(id: string, args?: T): Session;

    /** 
     * Ends the current conversation and optionally sends a message to the user. 
     * @param message (Optional)
     * * __message:__ _{string}_ - Text of the message to send. The message will be localized using the sessions configured localizer. If arguments are passed in the message will be formatted using [sprintf-js](https://github.com/alexei/sprintf.js).
     * * __message:__ _{string[]}_ - The sent message will be chosen at random from the array.
     * * __message:__ _{IMessage|IIsMessage}_ - Message to send. 
     * @param args (Optional) arguments used to format the final output text when __message__ is a _{string|string[]}_.
     */
    endConversation(message?: string|string[]|IMessage|IIsMessage, ...args: any[]): Session;


    /**
     * Ends the current dialog and optionally sends a message to the user. The parent will be resumed with an [IDialogResult.resumed](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#resumed) 
     * reason of [completed](/en-us/sdkreference/nodejs/enums/_botbuilder_d_.resumereason.html#completed).  
     * @param message (Optional)
     * * __message:__ _{string}_ - Text of the message to send. The message will be localized using the sessions configured localizer. If arguments are passed in the message will be formatted using [sprintf-js](https://github.com/alexei/sprintf.js).
     * * __message:__ _{string[]}_ - The sent message will be chosen at random from the array.
     * * __message:__ _{IMessage|IIsMessage}_ - Message to send. 
     * @param args (Optional) arguments used to format the final output text when __message__ is a _{string|string[]}_.
     */
    endDialog(message?: string|string[]|IMessage|IIsMessage, ...args: any[]): Session;

    /**
     * Ends the current dialog and optionally returns a result to the dialogs parent. 
     */
    endDialogWithResult(result?: IDialogResult<any>): Session;

    /**
     * Clears the sessions callstack and restarts the conversation with the configured dialogId.
     * @param dialogId (Optional) ID of the dialog to start.
     * @param dialogArgs (Optional) arguments to pass to the dialogs [begin()](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.dialog.html#begin) method.
     */
    reset(dialogId?: string, dialogArgs?: any): Session;

    /**
     * Returns true if the session has been reset.
     */
    isReset(): boolean;
}
    
/**
 * Message builder class that simplifies building complex messages with attachments.
 */
export class Message implements IIsMessage {

    /** 
     * Creates a new Message builder. 
     * @param session (Optional) will be used to populate the messages address and localize any text. 
     */
    constructor(session?: Session);
    
    /** Language of the message. */   
    local(local: string): Message;

    /** Format of text fields. */
    textFormat(style: string): Message;
    
    /** Sets the message text. */
    text(text: string|string[], ...args: any[]): Message;
    
    /** Conditionally set this message text given a specified count. */
    ntext(msg: string|string[], msg_plural: string|string[], count: number): Message;
    
    /** Composes a complex and randomized reply to the user.  */
    compose(prompts: string[][], ...args: any[]): Message;

    /** Text to be displayed by as fall-back and as short description of the message content in e.g. list of recent conversations. */  
    summary(text: string|string[], ...args: any[]): Message;

    /** Hint for how clients should layout multiple attachments. The default value is 'list'. */ 
    attachmentLayout(style: string): Message;
    
    /** Cards or images to send to the user.   */
    attachments(list: IAttachment[]|IIsAttachment[]): Message;
       
    /**
     * Adds an attachment to the message. See [IAttachment](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.iattachment.html) for examples.
     * @param attachment The attachment to add.   
     */    
    addAttachment(attachment: IAttachment|IIsAttachment): Message;
    
    /** Structured objects passed to the bot or user. */
    entities(list: Object[]): Message;
    
    /** Adds an entity to the message. */
    addEntity(obj: Object): Message;
    
    /** Address routing information for the message. Save this field to external storage somewhere to later compose a proactive message to the user. */
    address(adr: IAddress): Message;
    
    /** Timestamp of the message. If called will default the timestamp to (now). */
    timestamp(time?: string): Message;

    /** Message in original/native format of the channel for incoming messages. For outgoing messages can be used to pass channel specific message data like channel specific attachments. */  
    channelData(map: IChannelDataMap): Message;

    /** Returns the JSON for the message. */    
    toMessage(): IMessage;

    /** __DEPRECATED__ use [local()](#local) instead. */
    setLanguage(language: string): Message;
    
    /** __DEPRECATED__ use [text()](#text) instead. */ 
    setText(session: Session, prompt: string|string[], ...args: any[]): Message;

    /** __DEPRECATED__ use [ntext()](#ntext) instead. */ 
    setNText(session: Session, msg: string, msg_plural: string, count: number): Message;

    /** __DEPRECATED__ use [compose()](#compose) instead. */ 
    composePrompt(session: Session, prompts: string[][], ...args: any[]): Message;
    
    /** __DEPRECATED__ use [channelData()](#channeldata) instead. */ 
    setChannelData(data: any): Message;
    
    /**
     * Selects a prompt at random.
     * @param prompts Array of prompts to choose from. When prompts is type _string_ the prompt will simply be returned unmodified.
     */
    static randomPrompt(prompts: string|string[]): string;
    
    /**
     * Combines an array of prompts into a single localized prompt and then optionally fills the
     * prompts template slots with the passed in arguments. 
     * @param session Session object used to localize the individual prompt parts.
     * @param prompts Array of prompt lists. Each entry in the array is another array of prompts 
     *                which will be chosen at random.  The combined output text will be space delimited.
     * @param args (Optional) array of arguments used to format the output text when the prompt is a template.  
     */
    static composePrompt(session: Session, prompts: string[][], args?: any[]): string;
}

/** Builder class to simplify adding actions to a card. */
export class CardAction implements IIsCardAction {

    /** 
     * Creates a new CardAction. 
     * @param session (Optional) will be used to localize any text. 
     */
    constructor(session?: Session);
    
    /** Type of card action. */
    type(t: string): CardAction;
    
    /** Title of the action. For buttons this will be the label of the button.  For tap actions this may be used for accesibility purposes or shown on hover. */
    title(text: string|string[], ...args: any[]): CardAction;

    /** The actions value. */    
    value(v: string): CardAction;
    
    /** For buttons an image to include next to the buttons label. Not supported by all channels. */
    image(url: string): CardAction;
    
    /** Returns the JSON for the action. */    
    toAction(): ICardAction;

    /** Places a call to a phone number. The should include country code in +44/+1 format for Skype calls. */
    static call(session: Session, number: string, title?: string|string[]): CardAction;
    
    /** Opens the specified URL. */
    static openUrl(session: Session, url: string, title?: string|string[]): CardAction;
    
    /** Sends a message to the bot for processing in a way that's visible to all members of the conversation. For some channels this may get mapped to a [postBack](#postback). */
    static imBack(session: Session, msg: string, title?: string|string[]): CardAction;
    
    /** Sends a message to the bot for processing in a way that's hidden from all members of the conversation. For some channels this may get mapped to a [imBack](#imback). */
    static postBack(session: Session, msg: string, title?: string|string[]): CardAction;
    
    /** Plays the specified audio file to the user. Not currently supported for Skype. */
    static playAudio(session: Session, url: string, title?: string|string[]): CardAction;
    
    /** Plays the specified video to the user. Not currently supported for Skype. */
    static playVideo(session: Session, url: string, title?: string|string[]): CardAction;
    
    /** Opens the specified image in a native image viewer. For Skype only valid as a tap action on a CardImage. */
    static showImage(session: Session, url: string, title?: string|string[]): CardAction;
    
    /** Downloads the specified file to the users device. Not currently supported for Skype. */
    static downloadFile(session: Session, url: string, title?: string|string[]): CardAction;
}

/** Builder class to simplify adding images to a card. */
export class CardImage implements IIsCardImage {

    /** 
     * Creates a new CardImage. 
     * @param session (Optional) will be used to localize any text. 
     */
    constructor(session?: Session);
    
    /** URL of the image to display. */
    url(u: string): CardImage;
    
    /** Alternate text of the image to use for accessibility pourposes. */
    alt(text: string|string[], ...args: any[]): CardImage;
    
    /** Action to take when the image is tapped. */
    tap(action: ICardAction|IIsCardAction): CardImage;
    
    /** Returns the JSON for the image. */
    toImage(): ICardImage;

    /** Creates a new CardImage for a given url. */
    static create(session: Session, url: string): CardImage;
}

/** Card builder class that simplifies building thumbnail cards. */
export class ThumbnailCard implements IIsAttachment {

    /** 
     * Creates a new ThumbnailCard. 
     * @param session (Optional) will be used to localize any text. 
     */
    constructor(session?: Session);
    
    /** Title of the Card. */
    title(text: string|string[], ...args: any[]): ThumbnailCard;

    /** Subtitle appears just below Title field, differs from Title in font styling only. */  
    subtitle(text: string|string[], ...args: any[]): ThumbnailCard;
    
    /** Text field appears just below subtitle, differs from Subtitle in font styling only. */
    text(text: string|string[], ...args: any[]): ThumbnailCard;
    
    /** Messaging supports all media formats: audio, video, images and thumbnails as well to optimize content download. */  
    images(list: ICardImage[]|IIsCardImage[]): ThumbnailCard;

    /** Set of actions applicable to the current card. Not all channels support buttons or cards with buttons. Some channels may choose to render the buttons using a custom keyboard. */  
    buttons(list: ICardAction[]|IIsCardAction[]): ThumbnailCard;
    
    /** This action will be activated when user taps on the card. Not all channels support tap actions and some channels may choose to render the tap action as the titles link. */  
    tap(action: ICardAction|IIsCardAction): ThumbnailCard;

    /** Returns the JSON for the card, */
    toAttachment(): IAttachment;
}

/** Card builder class that simplifies building hero cards. Hero cards contain the same information as a thumbnail card, just with a larger more pronounced layout for the cards imagess. */
export class HeroCard extends ThumbnailCard {

    /** 
     * Creates a new HeroCard. 
     * @param session (Optional) will be used to localize any text. 
     */
    constructor(session?: Session);
}

/** Card builder class that simplifies building signin cards. */
export class SigninCard implements IIsAttachment {
    
    /** 
     * Creates a new SigninCard. 
     * @param session (Optional) will be used to localize any text. 
     */
    constructor(session?: Session);
    
    /** Title of the Card. */
    text(prompts: string|string[], ...args: any[]): SigninCard;
    
    /** Signin button label and link. */  
    button(title: string|string[], url: string): SigninCard;
    
    /** Returns the JSON for the card, */
    toAttachment(): IAttachment;
}

/** Card builder class that simplifies building receipt cards. */
export class ReceiptCard implements IIsAttachment {

    /** 
     * Creates a new ReceiptCard. 
     * @param session (Optional) will be used to localize any text. 
     */
    constructor(session?: Session);
    
    /** Title of the Card. */
    title(text: string|string[], ...args: any[]): ReceiptCard;
    
    /** Array of receipt items. */  
    items(list: IReceiptItem[]|IIsReceiptItem[]): ReceiptCard;

    /** Array of additional facts to display to user (shipping charges and such.) Not all facts will be displayed on all channels. */ 
    facts(list: IFact[]|IIsFact[]): ReceiptCard;

    /** This action will be activated when user taps on the card. Not all channels support tap actions. */  
    tap(action: ICardAction|IIsCardAction): ReceiptCard;
    
    /** Total amount of money paid (or should be paid.) */  
    total(v: string): ReceiptCard;

    /** Total amount of TAX paid (or should be paid.) */
    tax(v: string): ReceiptCard;

    /** Total amount of VAT paid (or should be paid.) */  
    vat(v: string): ReceiptCard;

    /** Set of actions applicable to the current card. Not all channels support buttons and the number of allowed buttons varies by channel. */  
    buttons(list: ICardAction[]|IIsCardAction[]): ReceiptCard;

    /** Returns the JSON for the card. */
    toAttachment(): IAttachment;
}

/** Builder class to simplify adding items to a receipt card. */
export class ReceiptItem implements IIsReceiptItem {

    /** 
     * Creates a new ReceiptItem. 
     * @param session (Optional) will be used to localize any text. 
     */
    constructor(session?: Session);
    
    /** Title of the item. */
    title(text: string|string[], ...args: any[]): ReceiptItem;

    /** Subtitle appears just below Title field, differs from Title in font styling only. On some channels may be combined with the [title](#title) or [text](#text). */
    subtitle(text: string|string[], ...args: any[]): ReceiptItem;
    
    /** Text field appears just below subtitle, differs from Subtitle in font styling only. */  
    text(text: string|string[], ...args: any[]): ReceiptItem;
    
    /** Image to display on the card. Some channels may either send the image as a seperate message or simply include a link to the image. */  
    image(img: ICardImage|IIsCardImage): ReceiptItem;

    /** Amount with currency. */
    price(v: string): ReceiptItem;
    
    /** Number of items of given kind. */  
    quantity(v: string): ReceiptItem;
    
    /** This action will be activated when user taps on the Item bubble. Not all channels support tap actions. */  
    tap(action: ICardAction|IIsCardAction): ReceiptItem;
    
    /** Returns the JSON for the item. */
    toItem(): IReceiptItem;

    /** Creates a new ReceiptItem. */
    static create(session: Session, price: string, title?: string|string[]): ReceiptItem;
}

/** Builder class to simplify creating a list of facts for a card like a receipt. */
export class Fact implements IIsFact {
    
    /** 
     * Creates a new Fact. 
     * @param session (Optional) will be used to localize any text. 
     */
    constructor(session?: Session);
    
    /** Display name of the fact. */
    key(text: string|string[], ...args: any[]): Fact;
    
    /** Display value of the fact. */  
    value(v: string): Fact;

    /** Returns the JSON for the fact. */   
    toFact(): IFact;

    /** Creates a new Fact. */
    static create(session: Session, value: string, key?: string|string[]): Fact;
}

/**
 * Base class for all dialogs. Dialogs are the core component of the BotBuilder 
 * framework. Bots use Dialogs to manage arbitrarily complex conversations with
 * a user. 
 */
export abstract class Dialog {
    /**
     * Called when a new dialog session is being started.
     * @param session Session object for the current conversation.
     * @param args (Optional) arguments passed to the dialog by its parent.
     */
    begin<T>(session: Session, args?: T): void;

    /**
     * Called when a new reply message has been recieved from a user.
     *
     * Derived classes should implement this to process the message recieved from the user.
     * @param session Session object for the current conversation.
     * @param (Optional) recognition results returned from a prior call to the dialogs [recognize()](#recognize) method.
     */
    abstract replyReceived(session: Session, recognizeResult?: IRecognizeResult): void;

    /**
     * A child dialog has ended and the current one is being resumed.
     * @param session Session object for the current conversation.
     * @param result Result returned by the child dialog.
     */
    dialogResumed<T>(session: Session, result: IDialogResult<T>): void;

    recognize(context: IRecognizeContext, cb: (err: Error, result: IRecognizeResult) => void): void;
}

/**
 * A collection of dialogs & middleware that's used for routing purposes. Bots typically derive from this class.
 */
export class DialogCollection {
    /**
     * Raises an event.
     * @param event Name of the event to raise.
     * @param args (Optional) arguments for the event.
     */
    emit(event: string, ...args: any[]): void;

    /**
     * Adds dialog(s) to a bot.
     * @param id 
     * * __id:__ _{string}_ - Unique ID of the dialog being added.
     * * __id:__ _{Object}_ - Map of [Dialog](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.dialog.html) objects to add to the collection. Each entry in the map should be keyed off the ID of the dialog being added. `{ [id: string]: Dialog; }` 
     * @param dialog
     * * __dialog:__ _{Dialog}_ - Dialog to add.
     * * __dialog:__ _{IDialogWaterfallStep[]}_ - Waterfall of steps to execute. See [IDialogWaterfallStep](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogwaterfallstep.html) for details.
     * * __dialog:__ _{IDialogWaterfallStep}_ - Single step waterfall. Calling a built-in prompt or starting a new dialog will result in the current dialog ending upon completion of the child prompt/dialog. 
     */
    add(id: string, dialog: Dialog|IDialogWaterfallStep[]|IDialogWaterfallStep): DialogCollection;
    add(id: { [id: string]: Dialog; }): DialogCollection;

    /**
     * Returns a dialog given its ID.
     * @param id ID of the dialog to lookup. 
     */
    getDialog(id: string): Dialog;

    /**
     * Returns an array of middleware to invoke.
     * @returns Array of middleware functions.
     */
    getMiddleware(): { (session: Session, next: Function): void; }[];

    /**
     * Returns true if a dialog with a given ID exists within the collection.
     * @param id ID of the dialog to lookup. 
     */
    hasDialog(id: string): boolean;

    /**
     * Registers a piece of middleware that will be called for every message receieved.
     * @param middleware Function to execute anytime a message is received.
     * @param middleware.session Session object for the current conversation.
     * @param middleware.next Function to invoke to call the next piece of middleware and continue processing of the message. Middleware can intercept a message by not calling next().
     */
    use(middleware: (session: Session, next: Function) => void): void;
}

/** Dialog actions offer shortcuts to implementing common actions. */
export class DialogAction {
    /**
     * Returns a closure that will send a simple text message to the user. 
     * @param msg Text of the message to send. The message will be localized using the sessions configured [localizer](#localizer). If arguments are passed in the message will be formatted using [sprintf-js](https://github.com/alexei/sprintf.js) (see the docs for details.)
     * @param args (Optional) arguments used to format the final output string. 
     */
    static send(msg: string, ...args: any[]): IDialogWaterfallStep;

    /**
     * Returns a closure that will passes control of the conversation to a new dialog.  
     * @param id Unique ID of the dialog to start.
     * @param args (Optional) arguments to pass to the dialogs begin() method.
     */
    static beginDialog<T>(id: string, args?: T): IDialogWaterfallStep; 

    /**
     * Returns a closure that will end the current dialog.
     * @param result (Optional) results to pass to the parent dialog.
     */
    static endDialog(result?: any): IDialogWaterfallStep;

    /**
     * Returns a closure that wraps a built-in prompt with validation logic. The closure should be used
     * to define a new dialog for the prompt using bot.add('/myPrompt', builder.DialogAction.)
     * @param promptType Type of built-in prompt to validate.
     * @param validator Function used to validate the response. Should return true if the response is valid.
     * @param validator.response The users [IDialogResult.response](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#response) returned by the built-in prompt. 
     * @example
     * <pre><code>
     * var bot = new builder.BotConnectorBot();
     * bot.add('/', [
     *     function (session) {
     *         session.beginDialog('/meaningOfLife', { prompt: "What's the meaning of life?" });
     *     },
     *     function (session, results) {
     *         if (results.response) {
     *             session.send("That's correct! The meaning of life is 42.");
     *         } else {
     *             session.send("Sorry you couldn't figure it out. Everyone knows that the meaning of life is 42.");
     *         }
     *     }
     * ]);
     * bot.add('/meaningOfLife', builder.DialogAction.validatedPrompt(builder.PromptType.text, function (response) {
     *     return response === '42';
     * }));
     * </code></pre>
     */    
    static validatedPrompt(promptType: PromptType, validator: (response: any) => boolean): Dialog;
}

/**
 * Built in built-in prompts that can be called from any dialog. 
 */
export class Prompts extends Dialog {
    /**
     * Processes messages received from the user. Called by the dialog system. 
     * @param session Session object for the current conversation.
     */
    replyReceived(session: Session): void;

    /**
     * Updates global options for the Prompts dialog. 
     * @param options Options to set.
     */
    static configure(options: IPromptsOptions): void;

    /**
     * Captures from the user a raw string of text. 
     * @param session Session object for the current conversation.
     * @param prompt 
     * * __prompt:__ _{string}_ - Initial message to send the user.
     * * __prompt:__ _{string[]}_ - Array of possible messages to send user. One will be chosen at random. 
     * * __prompt:__ _{IMessage|IIsMessage}_ - Initial message to send the user. Message can contain attachments. 
     */
    static text(session: Session, prompt: string|string[]|IMessage|IIsMessage): void;

    /**
     * Prompts the user to enter a number.
     * @param session Session object for the current conversation.
     * @param prompt 
     * * __prompt:__ _{string}_ - Initial message to send the user.
     * * __prompt:__ _{string[]}_ - Array of possible messages to send user. One will be chosen at random. 
     * * __prompt:__ _{IMessage|IIsMessage}_ - Initial message to send the user. Message can contain attachments. 
     * @param options (Optional) parameters to control the behaviour of the prompt.
     */
    static number(session: Session, prompt: string|string[]|IMessage|IIsMessage, options?: IPromptOptions): void;

    /**
     * Prompts the user to confirm an action with a yes/no response.
     * @param session Session object for the current conversation.
     * @param prompt 
     * * __prompt:__ _{string}_ - Initial message to send the user.
     * * __prompt:__ _{string[]}_ - Array of possible messages to send user. One will be chosen at random. 
     * * __prompt:__ _{IMessage|IIsMessage}_ - Initial message to send the user. Message can contain attachments. 
     * @param options (Optional) parameters to control the behaviour of the prompt.
     */
    static confirm(session: Session, prompt: string|string[]|IMessage|IIsMessage, options?: IPromptOptions): void;

    /**
     * Prompts the user to choose from a list of options.
     * @param session Session object for the current conversation.
     * @param prompt 
     * * __prompt:__ _{string}_ - Initial message to send the user.
     * * __prompt:__ _{string[]}_ - Array of possible messages to send user. One will be chosen at random. 
     * * __prompt:__ _{IMessage|IIsMessage}_ - Initial message to send the user. Message can contain attachments. Any [listStyle](/en-us/sdkreference/nodejs/interfaces/_botbuilder_d_.ipromptoptions.html#liststyle) options will be ignored.
     * @param choices 
     * * __choices:__ _{string}_ - List of choices as a pipe ('|') delimted string.
     * * __choices:__ _{Object}_ - List of choices expressed as an Object map. The objects field names will be used to build the list of values.
     * * __choices:__ _{string[]}_ - List of choices as an array of strings. 
     * @param options (Optional) parameters to control the behaviour of the prompt.
     */
    static choice(session: Session, prompt: string|string[]|IMessage|IIsMessage, choices: string|Object|string[], options?: IPromptOptions): void;

    /**
     * Prompts the user to enter a time.
     * @param session Session object for the current conversation.
     * @param prompt 
     * * __prompt:__ _{string}_ - Initial message to send the user.
     * * __prompt:__ _{string[]}_ - Array of possible messages to send user. One will be chosen at random. 
     * * __prompt:__ _{IMessage|IIsMessage}_ - Initial message to send the user. Message can contain attachments. 
     * @param options (Optional) parameters to control the behaviour of the prompt.
     */
    static time(session: Session, prompt: string|string[]|IMessage|IIsMessage, options?: IPromptOptions): void;

    /**
     * Prompts the user to upload a file attachment.
     * @param session Session object for the current conversation.
     * @param prompt 
     * * __prompt:__ _{string}_ - Initial message to send the user.
     * * __prompt:__ _{string[]}_ - Array of possible messages to send user. One will be chosen at random. 
     * * __prompt:__ _{IMessage|IIsMessage}_ - Initial message to send the user. Message can contain attachments. 
     * @param options (Optional) parameters to control the behaviour of the prompt.
     */
    static attachment(session: Session, prompt: string|string[]|IMessage|IIsMessage, options?: IPromptOptions): void;
}

/**
 * Implements a simple pattern based recognizer for parsing the built-in prompts. Derived classes can 
 * inherit from SimplePromptRecognizer and override the recognize() method to change the recognition
 * of one or more prompt types. 
 */
export class SimplePromptRecognizer implements IPromptRecognizer {
    /**
      * Attempts to match a users reponse to a given prompt.
      * @param args Arguments passed to the recognizer including that language, text, and prompt choices.
      * @param callback Function to invoke with the result of the recognition attempt.
      */
    recognize(args: IPromptRecognizerArgs, callback: (result: IPromptResult<any>) => void): void;
}

/** Identifies a users intent and optionally extracts entities from a users utterance. */
export class IntentDialog extends Dialog {
    constructor(options?: IIntentDialogOptions);

    replyReceived(session: Session, recognizeResult?: IRecognizeResult): void;

    onBegin(handler: (session: Session, args: any, next: () => void) => void): IntentDialog;

    matches(intent: string|RegExp, dialogId: string|IDialogWaterfallStep[]|IDialogWaterfallStep, dialogArgs?: any): IntentDialog;

    onDefault(dialogId: string|IDialogWaterfallStep[]|IDialogWaterfallStep, dialogArgs?: any): IntentDialog;
}

/**
 * Routes incoming messages to a LUIS app hosted on http://luis.ai for intent recognition.
 * Once a messages intent has been recognized it will rerouted to a registered intent handler, along
 * with any entities, for further processing. 
 */
export class LuisRecognizer implements IIntentRecognizer {
    /**
     * Constructs a new instance of a LUIS recognizer.
     * @param models Either an individual LUIS model used for all utterances or a map of per/local models conditionally used depending on the local of the utterance. 
     */
    constructor(models: string|ILuisModelMap);

    /** Called by the IntentDialog to perform the actual recognition. */
    public recognize(context: IRecognizeContext, cb: (err: Error, result: IIntentRecognizerResult) => void): void;

    /**
     * Calls LUIS to recognizing intents & entities in a users utterance.
     * @param utterance The text to pass to LUIS for recognition.
     * @param serviceUri URI for LUIS App hosted on http://luis.ai.
     * @param callback Callback to invoke with the results of the intent recognition step.
     * @param callback.err Error that occured during the recognition step.
     * @param callback.intents List of intents that were recognized.
     * @param callback.entities List of entities that were recognized.
     */
    static recognize(utterance: string, modelUrl: string, callback: (err: Error, intents?: IIntent[], entities?: IEntity[]) => void): void;
}

/**
 * Utility class used to parse & resolve common entities like datetimes received from LUIS.
 */
export class EntityRecognizer {
    /**
     * Searches for the first occurance of a specific entity type within a set.
     * @param entities Set of entities to search over.
     * @param type Type of entity to find.
     */
    static findEntity(entities: IEntity[], type: string): IEntity;
    
    /**
     * Finds all occurrences of a specific entity type within a set.
     * @param entities Set of entities to search over.
     * @param type Type of entity to find.
     */
    static findAllEntities(entities: IEntity[], type: string): IEntity[];

    /**
     * Parses a date from either a users text utterance or a set of entities.
     * @param value 
     * * __value:__ _{string}_ - Text utterance to parse. The utterance is parsed using the [Chrono](http://wanasit.github.io/pages/chrono/) library.
     * * __value:__ _{IEntity[]}_ - Set of entities to resolve.
     * @returns A valid Date object if the user spoke a time otherwise _null_.
     */   
    static parseTime(value: string | IEntity[]): Date;

    /**
     * Calculates a Date from a set of datetime entities.
     * @param entities List of entities to extract date from.
     * @returns The successfully calculated Date or _null_ if a date couldn't be determined. 
     */
    static resolveTime(entities: IEntity[]): Date;

    /**
     * Recognizes a time from a users utterance. The utterance is parsed using the [Chrono](http://wanasit.github.io/pages/chrono/) library.
     * @param utterance Text utterance to parse.
     * @param refDate (Optional) reference date used to calculate the final date.
     * @returns An entity containing the resolved date if successful or _null_ if a date couldn't be determined. 
     */
    static recognizeTime(utterance: string, refDate?: Date): IEntity;

    /**
     * Parses a number from either a users text utterance or a set of entities.
     * @param value
     * * __value:__ _{string}_ - Text utterance to parse. 
     * * __value:__ _{IEntity[]}_ - Set of entities to resolve.
     * @returns A valid number otherwise _Number.NaN_. 
     */
    static parseNumber(value: string | IEntity[]): number;

    /**
     * Parses a boolean from a users utterance.
     * @param value Text utterance to parse.
     * @returns A valid boolean otherwise _undefined_. 
     */
    static parseBoolean(value: string): boolean;
    
    /**
     * Finds the best match for a users utterance given a list of choices.
     * @param choices 
     * * __choices:__ _{string}_ - Pipe ('|') delimited list of values to compare against the users utterance. 
     * * __choices:__ _{Object}_ - Object used to generate the list of choices. The objects field names will be used to build the list of choices. 
     * * __choices:__ _{string[]}_ - Array of strings to compare against the users utterance. 
     * @param utterance Text utterance to parse.
     * @param threshold (Optional) minimum score needed for a match to be considered. The default value is 0.6.
     */
    static findBestMatch(choices: string | Object | string[], utterance: string, threshold?: number): IFindMatchResult;

    /**
     * Finds all possible matches for a users utterance given a list of choices.
     * @param choices 
     * * __choices:__ _{string}_ - Pipe ('|') delimited list of values to compare against the users utterance. 
     * * __choices:__ _{Object}_ - Object used to generate the list of choices. The objects field names will be used to build the list of choices. 
     * * __choices:__ _{string[]}_ - Array of strings to compare against the users utterance. 
     * @param utterance Text utterance to parse.
     * @param threshold (Optional) minimum score needed for a match to be considered. The default value is 0.6.
     */
    static findAllMatches(choices: string | Object | string[], utterance: string, threshold?: number): IFindMatchResult[];

    /**
     * Converts a set of choices into an expanded array.
     * @param choices 
     * * __choices:__ _{string}_ - Pipe ('|') delimited list of values. 
     * * __choices:__ _{Object}_ - Object used to generate the list of choices. The objects field names will be used to build the list of choices. 
     * * __choices:__ _{string[]}_ - Array of strings. This will just be echoed back as the output. 
     */
    static expandChoices(choices: string | Object | string[]): string[];
}

/**
 * Allows for the creation of custom dialogs that are based on a simple closure. This is useful for 
 * cases where you want a dynamic conversation flow or you have a situation that just doesn’t map 
 * very well to using a waterfall.  The things to keep in mind:
 * * Your dialogs closure is can get called in two different contexts that you potentially need to
 *   test for. It will get called as expected when the user send your dialog a message but if you 
 *   call another prompt or dialog from your closure it will get called a second time with the 
 *   results from the prompt/dialog. You can typically test for this second case by checking for the 
 *   existant of an `args.resumed` property. It's important to avoid getting yourself into an 
 *   infinite loop which can be easy to do.
 * * Unlike a waterfall your dialog will not automatically end. It will remain the active dialog 
 *   until you call [session.endDialog()](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.session.html#enddialog). 
 */
export class SimpleDialog extends Dialog {
    /**
     * Creates a new custom dialog based on a simple closure.
     * @param handler The function closure for your dialog. 
     * @param handler.session Session object for the current conversation.
     * @param handler.args 
     * * __args:__ _{any}_ - For the first call to the handler this will be either `null` or the value of any arguments passed to [Session.beginDialog()](/en-us/sdkreference/nodejs/classes/_botbuilder_d_.session.html#begindialog).
     * * __args:__ _{IDialogResult}_ - If the handler takes an action that results in a new dialog being started those results will be returned via subsequent calls to the handler.
     */
    constructor(handler: (session: Session, args?: any | IDialogResult<any>) => void);
    
    /**
     * Processes messages received from the user. Called by the dialog system. 
     * @param session Session object for the current conversation.
     */
    replyReceived(session: Session): void;
}

/** Default in memory storage implementation for storing user & session state data. */
export class MemoryBotStorage implements IBotStorage {
    /** Returns data from memmory for the given context. */
    getData(context: IBotStorageContext, callback: (err: Error, data: IBotStorageData) => void): void;
    
    /** Saves data to memory for the given context. */
    saveData(context: IBotStorageContext, data: IBotStorageData, callback?: (err: Error) => void): void;
    
    /** Deletes in-memory data for the given context. */
    deleteData(context: IBotStorageContext): void;
}

/** Manages your bots conversations with users across multiple channels. */
export class UniversalBot  {
    
    /** 
     * Creates a new instance of the UniversalBot.
     * @param connector (Optional) the default connector to use for requests. If there's not a more specific connector registered for a channel then this connector will be used./**
     * @param settings (Optional) settings to configure the bot with.
     */
    constructor(connector?: IConnector, settings?: IUniversalBotSettings);

    /** Sets a setting on the bot. Valid names are properties on IUniversalBotSettings. */
    set(name: string, value: any): UniversalBot;
    
    /** Returns the current value of a setting. Valid names are properties on IUniversalBotSettings. */
    get(name: string): any;

    /** Returns or registers a connector for a specific channel. Use a channelId of '*' to get the default connector. */    
    connector(channelId: string, connector?: IConnector): IConnector;

    /** Returns or registers a dialog for a given id. */    
    dialog(id: string, dialog?: Dialog|IDialogWaterfallStep[]|IDialogWaterfallStep): Dialog;
    
    /** Registers a piece of middleware with the bot. */
    use(middleware: IMiddlewareMap): UniversalBot;
    
    /** Called when a new message is recieved. This can be called manually to mimic the bot receiving a message from the user.  */
    receive(messages: IMessage|IMessage[], done?: (err: Error) => void): void;
 
    /** Proactively starts a new dialog with the user. Any current conversation between the bot and user will be replaced with a new dialog stack. */
    beginDialog(message: IMessage|IIsMessage, dialogId: string, dialogArgs?: any, done?: (err: Error) => void): void;

    /** Sends a message to the user without disrupting the current conversations dialog stack. */
    send(messages: IIsMessage|IMessage|IMessage[], done?: (err: Error) => void): void;

    /** Returns information about when the last turn between the user and a bot occured. */
    isInConversation(address: IAddress, cb: (err: Error, lastAccess: Date) => void): void;
}

/** Connects a UniversalBot to multiple channels via the Bot Framework. */
export class ChatConnector implements IConnector, IBotStorage {

    /** 
     * Creates a new instnace of the ChatConnector.
     * @param settings (Optional) config params that let you specify the bots App ID & Password you were assigned in the Bot Frameworks developer portal. 
     */
    constructor(settings?: IChatConnectorSettings);

    /** Registers an Express or Restify style hook to listen for new messages. */
    listen(): (req: any, res: any) => void;

    /** Express or Resitify style middleware that verifies recieved messages are from the Bot Framework. */
    verifyBotFramework(): (req: any, res: any, next: any) => void;

    /** Called by the UniversalBot at registration time to register a handler for receiving incoming messages from a user. */
    onMessage(handler: (messages: IMessage[], cb?: (err: Error) => void) => void): void;
    
    /** Called by the UniversalBot to deliver outgoing messages to a user. */
    send(messages: IMessage[], done: (err: Error) => void): void;

    /** Called when a UniversalBot wants to start a new proactive conversation with a user. The connector should return a properly formated __address__ object with a populated __conversation__ field. */
    startConversation(address: IAddress, done: (err: Error, address?: IAddress) => void): void;

    /** Reads in data from the Bot Frameworks state service. */
    getData(context: IBotStorageContext, callback: (err: Error, data: IBotStorageData) => void): void;

    /** Writes out data to the Bot Frameworks state service. */
    saveData(context: IBotStorageContext, data: IBotStorageData, callback?: (err: Error) => void): void;
}

/** Connects a UniversalBot to the command line via a console window. */
export class ConsoleConnector implements IConnector {
    /** Starts the connector listening to stdIn. */
    listen(): ConsoleConnector;

    /** Sends a message through the connector. */
    processMessage(line: string): ConsoleConnector;
    
    /** Called by the UniversalBot at registration time to register a handler for receiving incoming messages from a user. */
    onMessage(handler: (messages: IMessage[], cb?: (err: Error) => void) => void): void;
    
    /** Called by the UniversalBot to deliver outgoing messages to a user. */
    send(messages: IMessage[], cb: (err: Error, conversationId?: string) => void): void;

    /** Called when a UniversalBot wants to start a new proactive conversation with a user. The connector should return a properly formated __address__ object with a populated __conversation__ field. */
    startConversation(address: IAddress, cb: (err: Error, address?: IAddress) => void): void;
}

export class Middleware {
    /** Installs a piece of middleware that manages the versioning of a bots dialogs. */
    static dialogVersion(options: IDialogVersionOptions): IMiddlewareMap;
}

/** __DEPRECATED__ use an IntentDialog with a LuisRecognizer instead. */
export class LuisDialog extends Dialog {
    replyReceived(session: Session, recognizeResult?: IRecognizeResult): void;
}

/** __DEPRECATED__ use an IntentDialog instead. */
export class CommandDialog extends Dialog {
    replyReceived(session: Session, recognizeResult?: IRecognizeResult): void;
}

/** __DEPRECATED__ use UniversalBot and a ChatConnector instead. */
export class BotConnectorBot extends DialogCollection {
}

/** __DEPRECATED__ use UniversalBot and a ConsoleConnector instead. */
export class TextBot extends DialogCollection {
}