//=============================================================================
//
// INTERFACES
//
//=============================================================================

/** A communication message recieved from a User or sent out of band from a Bot. */
export interface IMessage {
    /** What kind of message is this. */
    type?: string;

    /** Bot.Connector Id for the message (always assigned by transport.) */
    id?: string;

    /** Bot.Connector ConverationId id for the conversation (always assigned by transport.) */
    conversationId?: string;

    /** Timestamp of when the message was created. */
    created?: string;

    /** (if translated) The OriginalText of the message. */
    sourceText?: string;

    /** (if translated) The language of the OriginalText of the message. */
    sourceLanguage?: string;

    /** The language that the Text is expressed in. */
    language?: string;

    /** The text of the message (this will be target language depending on flags and destination.)*/
    text?: string;

    /** Array of attachments that can be anything. */
    attachments?: IAttachment[];

    /** ChannelIdentity that sent the message. */
    from?: IChannelAccount;

    /** ChannelIdentity the message is sent to. */
    to?: IChannelAccount;

    /** Account to send replies to (for example, a group account that the message was part of.) */
    replyTo?: IChannelAccount;

    /** The message Id that this message is a reply to. */
    replyToMessageId?: string;

    /** List of ChannelAccounts in the conversation (NOTE: this is not for delivery means but for information.) */
    participants?: IChannelAccount[];

    /** Total participants in the conversation.  2 means 1:1 message. */
    totalParticipants?: number;

    /** Array of mentions from the channel context. */
    mentions?: IMention[];

    /** Place in user readable format: For example: "Starbucks, 140th Ave NE, Bellevue, WA" */
    place?: string;

    /** Channel Message Id. */
    channelMessageId?: string;

    /** Channel Conversation Id. */
    channelConversationId?: string;

    /** Channel specific properties.  For example: Email channel may pass the Subject field as a property. */
    channelData?: any;

    /** Location information (see https://dev.onedrive.com/facets/location_facet.htm) */
    location?: ILocation;

    /** Hashtags for the message. */
    hashtags?: string[];

    /** Required to modify messages when manually reading from a store. */
    eTag?: string;
}

/** An attachment. */
export interface IAttachment {
    /** (REQUIRED) mimetype/Contenttype for the file, either ContentUrl or Content must be set depending on the mimetype. */
    contentType: string;

    /** Url to content. */
    contentUrl?: string;

    /** Content Payload (for example, lat/long for contentype="location". */
    content?: any;

    /** (OPTIONAL-CARD) FallbackText - used for downlevel clients, should be simple markup with links. */
    fallbackText?: string;

    /** (OPTIONAL-CARD) Title. */
    title?: string;

    /** (OPTIONAL-CARD) link to use for the title. */
    titleLink?: string;

    /** (OPTIONAL-CARD) The Text description the attachment. */
    text?: string;

    /** (OPTIONAL-CARD) Thumbnail associated with attachment. */
    thumbnailUrl?: string;
}

/** Information needed to route a message. */
export interface IChannelAccount {
    /** Display friendly name of the user. */
    name?: string;

    /** Channel Id that the channelAccount is to be communicated with (Example: GroupMe.) */
    channelId: string;

    /** Channel Address for the channelAccount (Example: @thermous.) */
    address: string;

    /** Id - global intercom id. */
    id?: string;

    /** Is this account id an bot? */
    isBot?: boolean;
}

/** Mention information. */
export interface IMention {
    /** The mentioned user. */
    mentioned?: IChannelAccount;

    /** Sub Text which represents the mention (can be null or empty.) */
    text?: string;
}

/** A GEO location. */
export interface ILocation {
    /** Altitude. */
    altitude?: number;

    /** Latitude for the user when the message was created. */
    latitude: number;

    /** Longitude for the user when the message was created. */
    longitude: number;
}

/** Address info passed to Bot.beginDialog() calls. Specifies the address of the user to start a conversation with. */
export interface IBeginDialogAddress {
    /** Address of user to begin dialog with. */
    to: IChannelAccount;

    /** Optional "from" address for the bot. Required if IConnectorSession.defaultFrom hasn't been specified. */
    from?: IChannelAccount;

    /** Optional language to use when messaging the user. */
    language?: string;

    /** Optional text to initialize the dialogs message with. Useful for scenarios where the dialog being called is expecting to be replying to something the user said. */
    text?: string;
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

/** 
 * Action object which exposes a partial set of session functionality and can be used to capture 
 * messages sent to a child dialog.
 */
interface ISessionAction {
    /** Data for the user that's persisted across all conversations with the bot. */
    userData: any;

    /** Data that's only visible to the current dialog. */
    dialogData: any;

    /** Does not capture anything and proceedes to the next parent dialog in the callstack. */
    next(): void;

    /**
     * Ends all of the dialogs children and returns control to the current dialog. This permanently 
     * captures back the users replies.
     * @param result Optional results to pass to dialogResumed().
     */
    endDialog<T>(result?: IDialogResult<T>): void;
    
    /**
     * Sends a message to the user. The message will be localized using the sessions 
     * configured ILocalizer and if arguments are passed in the message will be formatted using
     * sprintf-js. See https://github.com/alexei/sprintf.js for documentation. 
     * @param msg
     * * __msg:__ _{string}_ - Text of a message to send the user. The message will be localized using the sessions configured [localizer](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html#localizer). If arguments are passed in the message will be formatted using [sprintf-js](https://github.com/alexei/sprintf.js).
     * * __msg:__ _{IMessage}_ - Message to send the user.
     * @param args Optional arguments used to format the final output text when __msg__ is a _{string}_.
     */
    send(msg: string, ...args: any[]): void;
    send(msg: IMessage): void;
}

/** Persisted session state used to track a conversations dialog stack. */
export interface ISessionState {
    /** Dialog stack for the current session. */
    callstack: IDialogState[];

    /** Timestamp of when the session was last accessed. */
    lastAccess: number;
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

/** Options passed to  */
export interface IPromptOptions {
    /** Optional retry prompt to send if the users response isn't understood. Default is to just reprompt with "I Didn't understand." plus the original prompt. */
    retryPrompt?: string;

    /** Optional maximum number of times to reprompt the user. Default value is 2. */
    maxRetries?: number;

    /** Optional reference date when recognizing times. Date expressed in ticks using Date.getTime(). */
    refDate?: number;

    /** Optional type of list to render for PromptType.choice. Default value is ListStyle.list. */
    listStyle?: ListStyle;
}

/** Arguments passed to the built-in prompts beginDialog() call. */
export interface IPromptArgs extends IPromptOptions {
    /** Type of prompt invoked. */
    promptType: PromptType;

    /** Initial message to send to user. */
    prompt: string;

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

    /** Optional reference date when recognizing times. */
    refDate?: number;

    /**
     * Lets a prompt recognizer compare its confidence that it understood an utterance with the prompts parent. 
     * The callback will return true if the utterance was processed by the parent. This function lets a
     * parent of the prompt handle utterances like "what can I say?" or "nevermind". 
     * @param language The langauge of the utterance taken from IMessage.language.
     * @param utterance The users utterance taken from IMessage.text.
     * @param score The dialogs confidence level on a scale of 0 to 1.0 that it understood the users intent.
     * @param callback Function to invoke with the result of the comparison. If handled is true the dialog should not process the utterance.
     * @param callback.handled If true the utterance was handled by the parent and the recognizer should not continue. 
     */
    compareConfidence(language: string, utterance: string, score: number, callback: (handled: boolean) => void): void;
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

/** Arguments passed to intent handlers when invoked. */
export interface IIntentArgs {
    /** Array of intents that were recognized. */
    intents: IIntent[];

    /** Array of entities that were recognized. */
    entities: IEntity[];
}

/** Arguments passed to command handlers when invoked. */
export interface ICommandArgs {
    /** Compiled expression that was matched. */
    expression: RegExp;

    /** List of values that matched the expression. */
    matches: RegExpExecArray;
}

/** Additional data parameters supported by the BotConnectorBot. */
export interface IBotConnectorMessage extends IMessage {
    /** Private Bot opaque data associated with a user (across all channels and conversations.) */
    botUserData?: any;

    /** Private Bot opaque state data associated with a conversation. */
    botConversationData?: any;

    /** Private Bot opaque state data associated with a user in a conversation. */
    botPerUserInConversationData?: any;
}

/** Options passed to the constructor of a session. */
export interface ISessionOptions {
    /** Collection of dialogs to use for routing purposes. Typically this is just the bot. */
    dialogs: DialogCollection;

    /** Unique ID of the dialog to use when starting a new conversation with a user. */
    dialogId: string;

    /** Optional arguments to pass to the conversations initial dialog. */
    dialogArgs?: any;

    /** Optional localizer to use when localizing the bots responses. */
    localizer?: ILocalizer;
    
    /** Optional minimum delay between messages sent to the user from the bot.  */
    minSendDelay?: number;
}

/** Signature of error events fired from a session. */
export interface ISessionErrorEvent {
    /**
     * @param err The error that occured.
     */
    (err: Error): void;
}

/** Signature of message related events fired from a session. */
export interface ISessionMessageEvent {
    /**
     * @param message Relevant message for the event.
     */
    (message: IMessage): void;
}

/** Signature of error events fired from bots. */
export interface IBotErrorEvent {
    /**
     * @param err The error that occured.
     * @param message Optional message that was being processed. May be _null_.
     */
    (err: Error, message?: any): void;
}

/** Signature of message related events fired from bots. */
export interface IBotMessageEvent {
    /**
     * @param message Relevant message for the event.
     */
    (message: any): void;
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

/** Storage abstraction used to persist session state & user data. */
export interface IStorage {
    /**
      * Loads a value from storage.
      * @param id ID of the value being loaded.
      * @param callaback Function used to receive the loaded value.
      * @param callback.err Any error that occured.
      * @param callback.data Data retrieved from storage. May be _null_ or _undefined_ if missing.
      */
    get(id: string, callback: (err: Error, data: any) => void): void;

    /**
      * Saves a value to storage.
      * @param id ID of the value to save.
      * @param data Value to save.
      * @param callback Optional function to invoke with the success or failure of the save.
      * @param callback.err Any error that occured.
      */
    save(id: string, data: any, callback?: (err: Error) => void): void;
}

/** Options used to configure the BotConnectorBot. */
export interface IBotConnectorOptions {
    /** URL of API endpoint to connect to for outgoing messages. */
    endpoint?: string;

    /** Bots application ID. */
    appId?: string;

    /** Bots application secret. */
    appSecret?: string;

    /** Default "from" address used in calls to ConnectorSession.beginDialog(). */
    defaultFrom?: IChannelAccount;
    
    /** Optional localizer used to localize the bots responses to the user. */
    localizer?: ILocalizer;
    
    /** Optional minimum delay between messages sent to the user from the bot. Default value is 1000. */
    minSendDelay?: number;

    /** Dialog to launch when a user initiates a new conversation with a bot. Default value is '/'. */
    defaultDialogId?: string;

    /** Optional arguments to pass to the initial dialog for a conversation. */
    defaultDialogArgs?: any;

    /** Sets a welcome message to send anytime a bot is added to a group conversation like a slack channel. */
    groupWelcomeMessage?: string;

    /** Sets a welcome message to send anytime a user is added to a group conversation the bots a member of like a slack channel. */
    userWelcomeMessage?: string;

    /** Sets a goodbye message to send anytime a user asks to end a conversation. */
    goodbyeMessage?: string;
}

/** Options used to configure the SkypeBot. */
export interface ISkypeBotOptions {
    /** Storage system to use for persisting Session.userData values. By default the MemoryStorage is used. */
    userStore?: IStorage;

    /** Storage system to use for persisting Session.sessionState values. By default the MemoryStorage is used. */
    sessionStore?: IStorage;

    /** Maximum time (in milliseconds) since ISessionState.lastAccess before the current session state is discarded. Default is 4 hours. */
    maxSessionAge?: number;

    /** Optional localizer used to localize the bots responses to the user. */
    localizer?: ILocalizer;
    
    /** Optional minimum delay between messages sent to the user from the bot. Default value is 1000. */
    minSendDelay?: number;

    /** Dialog to launch when a user initiates a new conversation with a bot. Default value is '/'. */
    defaultDialogId?: string;

    /** Optional arguments to pass to the initial dialog for a conversation. */
    defaultDialogArgs?: any;

    /** Sets the message to send when a user adds the bot as a contact. */
    contactAddedmessage?: string;

    /** Sets the message to send when the bot is added to a group chat. */
    botAddedMessage?: string;

    /** Sets the message to send when a bot is removed from a group chat. */
    botRemovedMessage?: string;

    /** Sets the message to send when a user joins a group chat monitored by the bot. */
    memberAddedMessage?: string;

    /** Sets the message to send when a user leaves a group chat monitored by the bot. */
    memberRemovedMessage?: string;
}

/** Options used to configure the SlackBot. */
export interface ISlackBotOptions {
    /** Maximum time (in milliseconds) since ISessionState.lastAccess before the current session state is discarded. Default is 4 hours. */
    maxSessionAge?: number;

    /** Optional localizer used to localize the bots responses to the user. */
    localizer?: ILocalizer;
    
    /** Optional minimum delay between messages sent to the user from the bot. Default value is 1500. */
    minSendDelay?: number;

    /** Dialog to launch when a user initiates a new conversation with a bot. Default value is '/'. */
    defaultDialogId?: string;

    /** Optional arguments to pass to the initial dialog for a conversation. */
    defaultDialogArgs?: any;
    
    /** Maximum time (in milliseconds) that a bot continues to recieve ambient messages after its been @mentioned. Default 5 minutes.  */
    ambientMentionDuration?: number;
    
    /** Optional flag that if true will cause a 'typing' message to be sent when the bot recieves a message. */
    sendIsTyping?: boolean;
}

/** Address info passed to SlackBot.beginDialog() calls. Specifies the address of the user or channel to start a conversation with. */
export interface ISlackBeginDialogAddress {
    /** ID of the user to begin a conversation with. If this is specified channel should be blank. */
    user?: string;
    
    /** ID of the channel to begin a conversation with. If this is specified user should be blank. */
    channel?: string;

    /** Optional team ID. If specified the SlackSession.teamData will be loaded. */
    team?: string;

    /** Optional text to initialize the dialogs message with. Useful for scenarios where the dialog being called is expecting to be replying to something the user said. */
    text?: string;
}

/** Options used to configure the TextBot. */
export interface ITextBotOptions {
    /** Storage system to use for persisting Session.userData values. By default the MemoryStorage is used. */
    userStore?: IStorage;

    /** Storage system to use for persisting Session.sessionState values. By default the MemoryStorage is used. */
    sessionStore?: IStorage;

    /** Maximum time (in milliseconds) since ISessionState.lastAccess before the current session state is discarded. Default is 4 hours. */
    maxSessionAge?: number;

    /** Optional localizer used to localize the bots responses to the user. */
    localizer?: ILocalizer;
    
    /** Optional minimum delay between messages sent to the user from the bot. Default value is 1000. */
    minSendDelay?: number;

    /** Dialog to launch when a user initiates a new conversation with a bot. Default value is '/'. */
    defaultDialogId?: string;

    /** Optional arguments to pass to the initial dialog for a conversation. */
    defaultDialogArgs?: any;
}

/** Signature for function passed as a step to DialogAction.waterfall(). */
export interface IDialogWaterfallStep {
    <T>(session: Session, result?: IDialogResult<T>, skip?: (results?: IDialogResult<any>) => void): any;
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
    forward,

    /** A captured utterance that resulted in a new child dialog being pushed onto the stack is completing. */
    captureCompleted,

    /** The child was forcibly ended by a parent. */
    childEnded
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
    time
}

/** Type of list to render for PromptType.choice prompt. */
export enum ListStyle { 
    /** No list is rendered. This is used when the list is included as part of the prompt. */
    none, 
    
    /** Choices are rendered as an inline list of the form "1. red, 2. green, or 3. blue". */
    inline, 
    
    /** Choices are rendered as a numbered list. */
    list 
}

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
     * - __error:__ An error occured. [ISessionErrorEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.isessionerrorevent.html)
     * - __send:__ A message should be sent to the user. [ISessionMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.isessionmessageevent.html)
     * - __quit:__ The bot would like to end the conversation. _{Function}_
     * @param listener Function to invoke.
     */
    on(event: string, listener: Function): void;

    /** Sessions configuration options. */
    protected options: ISessionOptions;

    /** Provides derived classes with access to the sessions localizer. */
    protected localizer: ILocalizer;

    /** ID of the dialog to start for any new conversations. */
    protected dialogId: string;

    /** Optional arguments to pass to the dialog when starting a new conversation. */
    protected dialogArgs: any;

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
    ISessionState: ISessionState;

    /** The message recieved from the user. For bot originated messages this may only contain the "to" & "from" fields. */
    message: IMessage;

    /** Data for the user that's persisted across all conversations with the bot. */
    userData: any;

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
     * @param args Optional arguments used to format the final output string. 
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

    /**
     * Sends a message to the user. If [send()](#send) is called without any parameters any changes to
     * [dialogData](#dialogdata) or [userData](#userdata) will be saved but the user will not recieve any reply. 
     * @param msg 
     * * __msg:__ _{string}_ - Text of the message to send. The message will be localized using the sessions configured [localizer](#localizer). If arguments are passed in the message will be formatted using [sprintf-js](https://github.com/alexei/sprintf.js).
     * * __msg:__ _{IMessage}_ - Message to send. 
     * @param args Optional arguments used to format the final output text when __msg__ is a _{string}_.
     */
    send(msg: string, ...args: any[]): Session;
    send(msg: IMessage): Session;
    send(): Session;

    /** Returns a native message the bot received. This message is pulled from the [IMessage.channelData](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.imessage.html#channeldata) received. */
    getMessageReceived(): any;
    
    /**
     * Sends a message in the channels native format.
     * @param msg Message formated in the channels native format.
     */
    sendMessage(msg: any): Session;

    /**
     * Returns true if a message has been sent for this session.
     */
    messageSent(): boolean;

    /**
     * Passes control of the conversation to a new dialog. The current dialog will be suspended 
     * until the child dialog completes. Once the child ends the current dialog will receive a
     * call to [dialogResumed()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialog.html#dialogresumed) 
     * where it can inspect any results returned from the child. 
     * @param id Unique ID of the dialog to start.
     * @param args Optional arguments to pass to the dialogs [begin()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialog.html#begin) method.
     */
    beginDialog<T>(id: string, args?: T): Session;

    /**
     * Ends the current dialog and starts a new one its place. The parent dialog will not be 
     * resumed until the new dialog completes. 
     * @param id Unique ID of the dialog to start.
     * @param args Optional arguments to pass to the dialogs [begin()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialog.html#begin) method.
     */
    replaceDialog<T>(id: string, args?: T): Session;

    /**
     * Ends the current dialog and optionally sends a message to the user. It's 
     * typically more efficient to call [endDialog()](#enddialog) with a message then it is to call 
     * [send()](#send) seperately before ending the dialog. 
     * 
     * If a message is sent to the user it will be sent before the dialogs parent is resumed. The
     * parent will be resumed with an [IDialogResult.resumed](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#resumed) 
     * reason of [completed](http://docs.botframework.com/sdkreference/nodejs/enums/_botbuilder_d_.resumereason.html#completed).  
     * @param result 
     * * __result:__ _{string}_ - Text of a message to send the user. The message will be localized using the sessions configured [localizer](#localizer). If arguments are passed in the message will be formatted using [sprintf-js](https://github.com/alexei/sprintf.js).
     * * __result:__ _{IMessage}_ - Message to send the user.
     * * __result:__ _{IDialogResult<any>}_ - Optional results to pass to the parent. If [endDialog()](#enddialog)
     * is called without any arguments the parent will be resumed with an [IDialogResult.resumed](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#resumed)
     * reason of [completed](http://docs.botframework.com/sdkreference/nodejs/enums/_botbuilder_d_.resumereason.html#completed).  
     * @param args Optional arguments used to format the final output text when __result__ is a _{string}_.
     */
    endDialog(result: string, ...args: any[]): Session;
    endDialog(result: IMessage): Session;
    endDialog(result?: IDialogResult<any>): Session;

    /**
     * Lets a dialog compare its confidence that it understood an utterance with it's parent. The
     * callback will return true if the utterance was processed by the parent. This function lets a
     * parent of the dialog handle messages not understood by the dialog. 
     * @param language The langauge of the utterance taken from [IMessage.language](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.imessage.html#language).
     * @param utterance The users utterance taken from [IMessage.text](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.imessage.html#text).
     * @param score The dialogs confidence level on a scale of 0 to 1.0 that it understood the users intent.
     * @param callback Function to invoke with the result of the comparison. 
     * @param callback.handled If true the dialog should not process the utterance.
     */
    compareConfidence(language: string, utterance: string, score: number, callback: (handled: boolean) => void): void;

    /**
     * Clears the sessions callstack and restarts the conversation with the configured [dialogId](#dialogid).
     * @param dialogId Optional ID of the dialog to start.
     * @param dialogArgs Optional arguments to pass to the dialogs [begin()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialog.html#begin) method.
     */
    reset<T>(dialogId?: string, dialogArgs?: T): Session;

    /**
     * Returns true if the session has been reset.
     */
    isReset(): boolean;
}

/**
 * Message builder class that simplifies building reply messages with attachments.
 */
export class Message implements IMessage {
    /**
     * Sets the messages language.
     * @param language The language of the message.
     */
    setLanguage(language: string): Message;
    
    /**
     * Sets the localized text of the message.
     * @param session Session object used to localize the message text.
     * @param prompt Text or template string for the reply. If an array is passed the reply will be chosen at random. The reply will be localized using session.gettext().
     * @param args Optional arguments used to format the message text when Text is a template.  
     */
    setText(session: Session, prompt: string, ...args: any[]): Message;
    setText(session: Session, prompt: string[], ...args: any[]): Message;

    /**
     * Loads the plural form of a localized string for the messages language. The output string will be formatted to 
     * include the count by replacing %d in the string with the count.
     * @param session Session object used to localize the message text.
     * @param msg Singular form of the string to use as a key in the localized string table. Use %d to specify where the count should go.
     * @param msg_plural Plural form of the string to use as a key in the localized string table. Use %d to specify where the count should go.
     * @param count Count to use when determining whether the singular or plural form of the string should be used.
     */
    setNText(session: Session, msg: string, msg_plural: string, count: number): Message;

    /**
     * Combines an array of prompts into a single localized prompt and then optionally fills the
     * prompts template slots with the passed in arguments. 
     * @param session Session object used to localize the individual prompt parts.
     * @param prompts Array of prompt lists. Each entry in the array is another array of prompts 
     *                which will be chosen at random.  The combined output text will be space delimited.
     * @param args Optional arguments used to format the output text when the prompt is a template.  
     * @example
     * <pre><code>
     * var prompts = {
     *     hello: ["Hello", "Hi"],
     *     world: ["World", "Planet"]
     * };
     * var bot = new builder.BotConnectorBot();
     * bot.add('/', function (session) {
     *      var msg = new Message().composePrompt(session, [prompts.hello, prompts.world]);
     *      session.send(msg);
     * });
     * </code></pre>
     */
    composePrompt(session: Session, prompts: string[][], ...args: any[]): Message;
    
    /**
     * Adds an attachment to the message.
     * @param attachment The attachment to add.  
     */    
    addAttachment(attachment: IAttachment): Message;
    
    /**
     * Sets the channelData for the message. Typically used to attach a message in the channels native format.
     * @param data The channel data to assign.
     */
    setChannelData(data: any): Message;
    
    /**
     * Selects a prompt at random.
     * @param prompts Array of prompts to choose from.
     */
    static randomPrompt(prompts: string[]): string;
    
    /**
     * Combines an array of prompts into a single localized prompt and then optionally fills the
     * prompts template slots with the passed in arguments. 
     * @param session Session object used to localize the individual prompt parts.
     * @param prompts Array of prompt lists. Each entry in the array is another array of prompts 
     *                which will be chosen at random.  The combined output text will be space delimited.
     * @param args Optional array of arguments used to format the output text when the prompt is a template.  
     */
    static composePrompt(session: Session, prompts: string[][], args?: any[]): string;
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
     * @param args Optional arguments passed to the dialog by its parent.
     */
    begin<T>(session: Session, args?: T): void;

    /**
     * Called when a new reply message has been recieved from a user.
     *
     * Derived classes should implement this to process the message recieved from the user.
     * @param session Session object for the current conversation.
     */
    abstract replyReceived(session: Session): void;

    /**
     * A child dialog has ended and the current one is being resumed.
     * @param session Session object for the current conversation.
     * @param result Result returned by the child dialog.
     */
    dialogResumed<T>(session: Session, result: IDialogResult<T>): void;

    /**
     * Called when a child dialog is wanting to compare its confidence for an utterance with its parent.
     * This lets the parent determine if it can do a better job of responding to the utterance then
     * the child can. This is useful for handling things like "quit" or "what can I say?".  
     * @param action Methods to lookup dialog state data and control what happens as a result of the 
     * comparison. Dialogs should at least call action.next() to signal a non-match.
     * @param language The langauge of the utterance taken from IMessage.language.
     * @param utterance The users utterance taken from IMessage.text.
     * @param score The childs confidence level on a scale of 0 to 1.0 that it understood the users intent.
     */
    compareConfidence(action: ISessionAction, language: string, utterance: string, score: number): void;
}

/**
 * A collection of dialogs & middleware that's used for routing purposes. Bots typically derive from this class.
 */
export class DialogCollection {
    /**
     * Raises an event.
     * @param event Name of the event to raise.
     * @param args Optional arguments for the event.
     */
    emit(event: string, ...args: any[]): void;

    /**
     * Adds dialog(s) to a bot.
     * @param id 
     * * __id:__ _{string}_ - Unique ID of the dialog being added.
     * * __id:__ _{Object}_ - Map of [Dialog](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialog.html) objects to add to the collection. Each entry in the map should be keyed off the ID of the dialog being added. `{ [id: string]: Dialog; }` 
     * @param dialog
     * * __dialog:__ _{Dialog}_ - Dialog to add.
     * * __dialog:__ _{IDialogWaterfallStep[]}_ - Waterfall of steps to execute. See [DialogAction.waterfall()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialogaction.html#waterfall) for details.
     * * __dialog:__ _{Function}_ - Closure to base dialog on. The closure will be called anytime a message is recieved 
     * from the user or when the dialog is being resumed. You can check for [args.resumed](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#resumed) 
     * to tell that the dialog is being resumed.
     * > `(session: Session, args?: any): void`
     * > * __session:__ [Session](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html) - Session object for the current conversation.
     * > * __args:__ _{any}_ - Any arguments passed to the dialog when [beginDialog()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html#begindialog) is called.
     * > * __args:__ [IDialogResult](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html) - If the closure initiates a [beginDialog()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html#begindialog) call the results will be returned via a second call to the closure.
     */
    add(id: string, dialog: Dialog): DialogCollection;
    add(id: string, dialog: IDialogWaterfallStep[]): DialogCollection;
    add(id: string, dialog: (session: Session, args?: any) => void): DialogCollection;
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
     * @param args Optional arguments used to format the final output string. 
     */
    static send(msg: string, ...args: any[]): (session: Session) => void;

    /**
     * Returns a closure that will passes control of the conversation to a new dialog.  
     * @param id Unique ID of the dialog to start.
     * @param args Optional arguments to pass to the dialogs begin() method.
     */
    static beginDialog<T>(id: string, args?: T): (session: Session, args: T) => void; 

    /**
     * Returns a closure that will end the current dialog.
     * @param result Optional results to pass to the parent dialog.
     */
    static endDialog(result?: any): (session: Session) => void;

    /**
     * Returns a closure that will prompt the user for information in an async waterfall like 
     * sequence. When the closure is first invoked it will execute the first function in the
     * waterfall and the results of that prompt will be passed as input to the second function
     * and the result of the second passed to the third and so on.  
     *
     * Each step within the waterfall may optionally return a [ResumeReson](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#resumed) to influence the flow 
     * of the waterfall:
     * - [ResumeReason.forward](http://docs.botframework.com/sdkreference/nodejs/enums/_botbuilder_d_.resumereason.html#forward): skips the next function in the waterfall.
     * - [ResumeReason.back](http://docs.botframework.com/sdkreference/nodejs/enums/_botbuilder_d_.resumereason.html#back): returns to the previous function in the waterfall.
     * - [ResumeReason.canceled](http://docs.botframework.com/sdkreference/nodejs/enums/_botbuilder_d_.resumereason.html#canceled): ends the waterfall all together.
     * 
     * Calling other dialogs like built-in prompts can influence the flow as well. If a child dialog
     * returns either ResumeReason.forward or ResumeReason.back it will automatically be handled.
     * If ResumeReason.canceled is returnd it will be handed to the step for processing which can
     * then decide to cancel the action or not.
     * @param steps Steps of a waterfall.
     */
    static waterfall(steps: IDialogWaterfallStep[]): (session: Session, args: any) => void;

    /**
     * Returns a closure that wraps a built-in prompt with validation logic. The closure should be used
     * to define a new dialog for the prompt using bot.add('/myPrompt', builder.DialogAction.)
     * @param promptType Type of built-in prompt to validate.
     * @param validator Function used to validate the response. Should return true if the response is valid.
     * @param validator.response The users [IDialogResult.response](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#response) returned by the built-in prompt. 
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
     * bot.add('/meaningOfLife'. builder.DialogAction.validatedPrompt(builder.PromptType.text, function (response) {
     *     return response === '42';
     * }));
     * </code></pre>
     */    
    static validatedPrompt(promptType: PromptType, validator: (response: any) => boolean): (session: Session, args: any) => void;
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
     * @param prompt Message to send to the user.
     */
    static text(session: Session, prompt: string): void;

    /**
     * Prompts the user to enter a number.
     * @param session Session object for the current conversation.
     * @param prompt Initial message to send the user.
     * @param options Optional flags parameters to control the behaviour of the prompt.
     */
    static number(session: Session, prompt: string, options?: IPromptOptions): void;

    /**
     * Prompts the user to confirm an action with a yes/no response.
     * @param session Session object for the current conversation.
     * @param prompt Initial message to send the user.
     * @param options Optional flags parameters to control the behaviour of the prompt.
     */
    static confirm(session: Session, prompt: string, options?: IPromptOptions): void;

    /**
     * Prompts the user to choose from a list of options.
     * @param session Session object for the current conversation.
     * @param prompt Initial message to send the user.
     * @param choices 
     * * __choices:__ _{string}_ - List of choices as a pipe ('|') delimted string.
     * * __choices:__ _{Object}_ - List of choices expressed as an Object map. The objects field names will be used to build the list of values.
     * * __choices:__ _{string[]}_ - List of choices as an array of strings. 
     * @param options Optional flags parameters to control the behaviour of the prompt.
     */
    static choice(session: Session, prompt: string, choices: string, options?: IPromptOptions): void;
    static choice(session: Session, prompt: string, choices: Object, options?: IPromptOptions): void;
    static choice(session: Session, prompt: string, choices: string[], options?: IPromptOptions): void;

    /**
     * Prompts the user to enter a time.
     * @param session Session object for the current conversation.
     * @param prompt Initial message to send the user.
     * @param options Optional flags parameters to control the behaviour of the prompt.
     */
    static time(session: Session, prompt: string, options?: IPromptOptions): void;
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

/**
 * Base class for an intent based dialog where the incoming message is sent to an intent recognizer
 * to first identify any intents & entities. The top intent will be used to lookup a handler that 
 * will be used process the recieved message.
*/
export abstract class IntentDialog extends Dialog {
    /**
     * Processes messages received from the user. Called by the dialog system. 
     * @param session Session object for the current conversation.
     */
    replyReceived(session: Session): void;

    /**
     * Adds a IntentGroup to the dialog. Intent groups help organize larger dialogs with many
     * intents. They let you move the processing of related handlers to a seperate file.
     * @param group Group to add to dialog.
     */
    addGroup(group: IntentGroup): IntentDialog;

    /**
     * The handler will be called anytime the dialog is started for a session. Call next() to continue default processing.
     * @param handler Handler to invoke when the dialog is started.
     * @param handler.session Session object for the current conversation.
     * @param handler.args Any arguments passed to the dialog in the call to [beginDialog()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html#begindialog).
     * @param handler.next Callback used to continue the dialogs execution.
     */
    onBegin(handler: (session: Session, args: any, next: () => void) => void): IntentDialog;

    /**
     * Executes a block of code when the given intent is recognized. Use [DialogAction](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialogaction.html) 
     * methods to implement common actions.
     * @param intent Intent to trigger on.
     * @param handler 
     * * __handler:__ _{string}_ - The ID of a dialog to begin. 
     * * __handler:__ _{IDialogWaterfallStep[]}_ - An array of waterfall steps to execute. See [DialogAction.waterfall()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialogaction.html#waterfall) for details.
     * * __handler:__ _{Function}_ - Handler to invoke when the intent is recognized. The handler will also be invoked when a dialog started by the handler returns. Check for [args.resumed](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#resumed) to detect that the handler is being resumed.
     * > `(session: Session, args?: any): void`
     * > * __session:__ [Session](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html) - Session object for the current conversation.
     * > * __args:__ [IIntentArgs](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.iintentargs.html) - The full list of intents and entities that were recognized.
     * > * __args:__ [IDialogResult](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html) - If the handler initiates a [beginDialog()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html#begindialog) call the results will be returned via a second call to the handler.
     * @param dialogArgs Optional arguments to pass to the dialog when __handler__ is type _{string}_. They will be merged with the _{IIntentArgs}_ args passed to the handler.
     */
    on(intent: string, handler: string, dialogArgs?: any): IntentDialog;
    on(intent: string, handler: IDialogWaterfallStep[]): IntentDialog;
    on(intent: string, handler: (session: Session, args?: any) => void): IntentDialog;

    /**
     * Executes a block of code when there are no handlers registered for the intent that was 
     * recognized. Use [DialogAction](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialogaction.html) 
     * methods to implement common actions.
     * @param handler 
     * * __handler:__ _{string}_ - The ID of a dialog to begin. 
     * * __handler:__ _{IDialogWaterfallStep[]}_ - An array of waterfall steps to execute. See [DialogAction.waterfall()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialogaction.html#waterfall) for details.
     * * __handler:__ _{Function}_ - Handler to invoke. The handler will also be invoked when a dialog started by the handler returns. Check for [args.resumed](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#resumed) to detect that the handler is being resumed.
     * > `(session: Session, args?: any): void`
     * > * __session:__ [Session](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html) - Session object for the current conversation.
     * > * __args:__ [IIntentArgs](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.iintentargs.html) - The full list of intents and entities that were recognized.
     * > * __args:__ [IDialogResult](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html) - If the handler initiates a [beginDialog()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html#begindialog) call the results will be returned via a second call to the handler.
     * @param dialogArgs Optional arguments to pass to the dialog when __handler__ is type _{string}_. They will be merged with the _{IIntentArgs}_ args passed to the handler.
     */
    onDefault(handler: string, dialogArgs?: any): IntentDialog;
    onDefault(handler: IDialogWaterfallStep[]): IntentDialog;
    onDefault(handler: (session: Session, args?: any) => void): IntentDialog;

    /** Returns the minimum score needed for an intent to be triggered. */
    getThreshold(): number;

    /**
     * Sets the minimum score needed for an intent to be triggered. The default value is 0.1.
     * @param score Minimum score needed to trigger an intent.
     */
    setThreshold(score: number): IntentDialog;

    /**
     * Called to recognize the intents & entities for a received message.
     *
     * Derived classes should implement this method with the logic needed to perform the actual intent recognition.
     * @param session Session object for the current conversation.
     * @param callback Callback to invoke with the results of the intent recognition step.
     * @param callback.err Error that occured during the recognition step.
     * @param callback.intents List of intents that were recognized.
     * @param callback.entities List of entities that were recognized.
     */
    protected abstract recognizeIntents(session: Session, callback: (err: Error, intents?: IIntent[], entities?: IEntity[]) => void): void;
}

/**
 * Defines a related group of intent handlers. Primarily useful for dialogs with a large number of 
 * intents or for team development where you want seperate developers to more easily work on the same bot. 
 */
export class IntentGroup {
    /**
     * Creates a new IntentGroup. The group needs to be labled with a unique ID for
     * routing purposes.
     * @param id Unique ID of the intent group.
     */
    constructor(id: string);

    /**
     * Returns the groups ID.
     */
    getId(): string;

    /**
     * Executes a block of code when the given intent is recognized. Use [DialogAction](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialogaction.html) 
     * methods to implement common actions.
     * @param intent Intent to trigger on.
     * @param handler 
     * * __handler:__ _{string}_ - The ID of a dialog to begin. 
     * * __handler:__ _{IDialogWaterfallStep[]}_ - An array of waterfall steps to execute. See [DialogAction.waterfall()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialogaction.html#waterfall) for details.
     * * __handler:__ _{Function}_ - Handler to invoke when the intent is recognized. The handler will also be invoked when a dialog started by the handler returns. Check for [args.resumed](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#resumed) to detect that the handler is being resumed.
     * > `(session: Session, args?: any): void`
     * > * __session:__ [Session](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html) - Session object for the current conversation.
     * > * __args:__ [IIntentArgs](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.iintentargs.html) - The full list of intents and entities that were recognized.
     * > * __args:__ [IDialogResult](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html) - If the handler initiates a [beginDialog()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html#begindialog) call the results will be returned via a second call to the handler.
     * @param dialogArgs Optional arguments to pass to the dialog when __handler__ is type _{string}_. They will be merged with the _{IIntentArgs}_ args passed to the handler.
     */
    on(intent: string, handler: string, dialogArgs?: any): IntentDialog;
    on(intent: string, handler: IDialogWaterfallStep[]): IntentDialog;
    on(intent: string, handler: (session: Session, args?: any) => void): IntentDialog;
}

/**
 * Routes incoming messages to a LUIS app hosted on http://luis.ai for intent recognition.
 * Once a messages intent has been recognized it will rerouted to a registered intent handler, along
 * with any entities, for further processing. 
 */
export class LuisDialog extends IntentDialog {
    /**
     * Creates a new instance of a LUIS dialog.
     * @param serviceUri URI for LUIS App hosted on http://luis.ai.
     */
    constructor(serviceUri: string);

    /**
     * Performs the step of recognizing intents & entities when a message is recieved vy the dialog. Called by IntentDialog.
     * @param session Session object for the current conversation.
     * @param callback Callback to invoke with the results of the intent recognition step.
     * @param callback.err Error that occured during the recognition step.
     * @param callback.intents List of intents that were recognized.
     * @param callback.entities List of entities that were recognized.
     */
    protected recognizeIntents(session: Session, callback: (err: Error, intents?: IIntent[], entities?: IEntity[]) => void): void;
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
    static parseTime(value: string): Date;
    static parseTime(value: IEntity[]): Date;

    /**
     * Calculates a Date from a set of datetime entities.
     * @param entities List of entities to extract date from.
     * @returns The successfully calculated Date or _null_ if a date couldn't be determined. 
     */
    static resolveTime(entities: IEntity[]): Date;

    /**
     * Recognizes a time from a users utterance. The utterance is parsed using the [Chrono](http://wanasit.github.io/pages/chrono/) library.
     * @param utterance Text utterance to parse.
     * @param refDate Optional reference date used to calculate the final date.
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
    static parseNumber(value: string): number;
    static parseNumber(value: IEntity[]): number;

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
     * @param threshold Optional minimum score needed for a match to be considered. The default value is 0.6.
     */
    static findBestMatch(choices: string, utterance: string, threshold?: number): IFindMatchResult;
    static findBestMatch(choices: Object, utterance: string, threshold?: number): IFindMatchResult;
    static findBestMatch(choices: string[], utterance: string, threshold?: number): IFindMatchResult;

    /**
     * Finds all possible matches for a users utterance given a list of choices.
     * @param choices 
     * * __choices:__ _{string}_ - Pipe ('|') delimited list of values to compare against the users utterance. 
     * * __choices:__ _{Object}_ - Object used to generate the list of choices. The objects field names will be used to build the list of choices. 
     * * __choices:__ _{string[]}_ - Array of strings to compare against the users utterance. 
     * @param utterance Text utterance to parse.
     * @param threshold Optional minimum score needed for a match to be considered. The default value is 0.6.
     */
    static findAllMatches(choices: string, utterance: string, threshold?: number): IFindMatchResult[];
    static findAllMatches(choices: Object, utterance: string, threshold?: number): IFindMatchResult[];
    static findAllMatches(choices: string[], utterance: string, threshold?: number): IFindMatchResult[];

    /**
     * Converts a set of choices into an expanded array.
     * @param choices 
     * * __choices:__ _{string}_ - Pipe ('|') delimited list of values. 
     * * __choices:__ _{Object}_ - Object used to generate the list of choices. The objects field names will be used to build the list of choices. 
     * * __choices:__ _{string[]}_ - Array of strings. This will just be echoed back as the output. 
     */
    static expandChoices(choices: string): string[];
    static expandChoices(choices: Object): string[];
    static expandChoices(choices: string[]): string[];
}

/**
 * Enables the building of a /command style bots. Regular expressions are matched against a users
 * responses and used to trigger handlers when matched.
 */
export class CommandDialog extends Dialog {
    /**
     * Processes messages received from the user. Called by the dialog system. 
     * @param session Session object for the current conversation.
     */
    replyReceived(session: Session): void;

    /**
     * The handler will be called anytime the dialog is started for a session. Call next() to continue the dialogs default processing. 
     * @param handler Handler to invoke when the dialog is started.
     * @param handler.session Session object for the current conversation.
     * @param handler.args Any arguments passed to the dialog in the call to [beginDialog()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html#begindialog).
     * @param handler.next Callback used to continue the dialogs execution.
     */
    onBegin(handler: (session: Session, args: any, next: () => void) => void): CommandDialog;

    /**
     * Triggers the handler when the pattern(s) is matched. Use [DialogAction](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialogaction.html) 
     * methods to implement common actions.
     * @param pattern 
     * * __patern:__ _{string}_ - A regular expression to match against. Comparisons are case insensitive.
     * * __patern:__ _{string[]}_ - Array of regular expressions to match against. All comparisons are case insensitive.
     * @param handler 
     * * __handler:__ _{string}_ - The ID of a dialog to begin. 
     * * __handler:__ _{IDialogWaterfallStep[]}_ - An array of waterfall steps to execute. See [DialogAction.waterfall()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialogaction.html#waterfall) for details.
     * * __handler:__ _{Function}_ - Handler to invoke when the pattern is matched. The handler will also be invoked when a dialog started by the handler returns. Check for [args.resumed](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#resumed) to detect that the handler is being resumed.
     * > `(session: Session, args?: any): void`
     * > * __session:__ [Session](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html) - Session object for the current conversation.
     * > * __args:__ [ICommandArgs](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.icommandargs.html) - The compiled expression and any matches for the pattern that was matched.
     * > * __args:__ [IDialogResult](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html) - If the handler initiates a [beginDialog()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html#begindialog) call the results will be returned via a second call to the handler.
     * @param dialogArgs Optional arguments to pass to the dialog when __handler__ is type _{string}_. They will be merged with the _{ICommandArgs}_ args passed to the handler.
     */
    matches(pattern: string, handler: string, dialogArgs?: any): CommandDialog;
    matches(pattern: string[], handler: string, dialogArgs?: any): CommandDialog;
    matches(pattern: string, handler: (session: Session, args?: any) => void): CommandDialog;
    matches(pattern: string[], handler: (session: Session, args?: any) => void): CommandDialog;
    matches(pattern: string, handler: IDialogWaterfallStep[]): IntentDialog;
    matches(pattern: string[], handler: IDialogWaterfallStep[]): IntentDialog;

    /**
     * Triggers a handler when an unknown pattern is received. Use [DialogAction](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialogaction.html) 
     * methods to implement common actions.
     * @param handler 
     * * __handler:__ _{string}_ - The ID of a dialog to begin. 
     * * __handler:__ _{IDialogWaterfallStep[]}_ - An array of waterfall steps to execute. See [DialogAction.waterfall()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.dialogaction.html#waterfall) for details.
     * * __handler:__ _{Function}_ - Handler to invoke when the pattern is matched. The handler will also be invoked when a dialog started by the handler returns. Check for [args.resumed](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html#resumed) to detect that the handler is being resumed.
     * > `(session: Session, args: ICommandArgs|IDialogResult): void`
     * > * __session:__ [Session](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html) - Session object for the current conversation.
     * > * __args:__ [ICommandArgs](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.icommandargs.html) - The compiled expression and any matches for the pattern that was matched.
     * > * __args:__ [IDialogResult](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.idialogresult.html) - If the handler initiates a [beginDialog()](http://docs.botframework.com/sdkreference/nodejs/classes/_botbuilder_d_.session.html#begindialog) call the results will be returned via a second call to the handler.
     * @param dialogArgs Optional arguments to pass to the dialog when __handler__ is type _{string}_. They will be merged with the _{ICommandArgs}_ args passed to the handler.
     */
    onDefault(handler: string, dialogArgs?: any): CommandDialog;
    onDefault(handler: IDialogWaterfallStep[]): IntentDialog;
    onDefault(handler: (session: Session, args?: ICommandArgs) => void): CommandDialog;
}

/** Default in memory storage implementation for storing user & session state data. */
export class MemoryStorage implements IStorage {
    /**
      * Loads a value from storage.
      * @param id ID of the value being loaded.
      * @param callaback Function used to receive the loaded value.
      * @param callback.err Any error that occured.
      * @param callback.data Data retrieved from storage. May be _null_ or _undefined_ if missing.
      */
    get(id: string, callback: (err: Error, data: any) => void): void;

    /**
      * Saves a value to storage.
      * @param id ID of the value to save.
      * @param data Value to save.
      * @param callback Optional function to invoke with the success or failure of the save.
      * @param callback.err Any error that occured.
      */
    save(id: string, data: any, callback?: (err: Error) => void): void;

    /**
     * Deletes a value from storage.
     * @param id ID of the value to delete.
     */
    delete(id: string): void;
}

/**
 * Connects your bots dialogs to the Bot Framework.
 */
export class BotConnectorBot extends DialogCollection {
    /**
     * @param options Optional configuration settings for the bot.
     */
    constructor(options?: IBotConnectorOptions);

    /**
     * Registers an event listener to get notified of bot related events. 
     * @param event Name of event to listen for. The message to passed to events will be of type [IBotConnectorMessage](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotconnectormessage.html). Event types:
     * - __error:__ An error occured.  [IBotErrorEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.iboterrorevent.html)
     * - __reply:__ A reply to an existing message was sent. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __send:__ A new message was sent to start a new conversation. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __quit:__ The bot has elected to ended the current conversation. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __message:__ A user message was received. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __DeleteUserData:__ The user has requested to have their data deleted. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __BotAddedToConversation:__ The bot has been added to a conversation. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __BotRemovedFromConversation:__ The bot has been removed from a conversation. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __UserAddedToConversation:__ A user has joined a conversation monitored by the bot. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __UserRemovedFromConversation:__ A user has left a conversation monitored by the bot. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __EndOfConversation:__ The user has elected to end the current conversation. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * @param listener Function to invoke.
     */
    on(event: string, listener: Function): void;

    /**
     * Updates the bots configuration settings.
     * @param options Configuration options to set.
     */
    configure(options: IBotConnectorOptions): void;

    /**
     * Returns a piece of Express or Restify compliant middleware that will ensure only messages from the Bot Framework are processed.
     * _NOTE: Ignored for HTTP requests and also requires configuring of the bots appId and appSecret._
     * @param options Optional configuration options to pass in.
     * @example
     * <pre><code>
     * var bot = new builder.BotConnectorBot();
     * app.use(bot.verifyBotFramework({ appId: 'your appId', appSecret: 'your appSecret' }));
     * </code></pre>
     */
    verifyBotFramework(options?: IBotConnectorOptions): (req: any, res: any, next: any) => void;

    /**
     * Returns a piece of Express or Restify compliant middleware that will route incoming messages to the bot. 
     * _NOTE: The middleware should be mounted to a route that receives an HTTPS POST._
     * @param dialogId Optional ID of the bots dialog to begin for new conversations. If ommited the bots [defaultDialogId](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotconnectoroptions.html#defaultdialogid) will be used.
     * @param dialogArgs Optional arguments to pass to the dialog when a new conversation is started.
     * @example
     * <pre><code>
     * var bot = new builder.BotConnectorBot();
     * app.post('/api/messages', bot.listen());
     * </code></pre>
     */
    listen(dialogId?: string, dialogArgs?: any): (req: any, res: any) => void;

    /**
     * Starts a new conversation with a user.
     * @param address Address of the user to begin the conversation with.
     * @param dialogId Unique ID of the bots dialog to begin the conversation with.
     * @param dialogArgs Optional arguments to pass to the dialog.
     */
    beginDialog(address: IBeginDialogAddress, dialogId: string, dialogArgs?: any): void;
}

/**
 * Adds additional properties for working with Bot Framework bots.
 */
export class BotConnectorSession extends Session {
    /** Group data that's persisted across all members of a conversation. */
    conversationData: any;

    /** User data that's persisted on a per conversation basis. */
    perUserConversationData: any;
}

/**
 * Connects your bots dialogs to Skype.
 */
export class SkypeBot extends DialogCollection {
    /**
     * @param botService Skype BotService() instance.
     * @param options Optional configuration settings for the bot.
     */
    constructor(botService: any, options?: ISkypeBotOptions);

    /**
     * Registers an event listener to get notified of bot related events. 
     * @param event Name of event to listen for. The message to passed to events will be a skype message. Event types:
     * - __error:__ An error occured.  [IBotErrorEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.iboterrorevent.html)
     * - __reply:__ A reply to an existing message was sent. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __send:__ A new message was sent to start a new conversation. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __quit:__ The bot has elected to ended the current conversation. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __message:__ This event is emitted for every received message. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - personalMessage: This event is emitted for every 1:1 chat message. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - groupMessage: This event is emitted for every group chat message. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - threadBotAdded: This event is emitted when the bot is added to group chat. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - threadAddMember: This event is emitted when some users are added to group chat. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - threadBotRemoved: This event is emitted when the bot is removed from group chat. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - threadRemoveMember: This event is emitted when some users are removed from group chat. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - contactAdded: This event is emitted when users add the bot as a buddy. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - threadTopicUpdated: This event is emitted when the topic of a group chat is updated. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - threadHistoryDisclosedUpdate: This event is emitted when the "history disclosed" option of a group chat is changed. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * @param listener Function to invoke.
     */
    on(event: string, listener: Function): void;

    /**
     * Updates the bots configuration settings.
     * @param options Configuration options to set.
     */
    configure(options: ISkypeBotOptions): SkypeBot;
    
    /**
     * Starts a new conversation with a user.
     * @param address Address of the user to begin the conversation with.
     * @param dialogId Unique ID of the bots dialog to begin the conversation with.
     * @param dialogArgs Optional arguments to pass to the dialog.
     */
    beginDialog(address: IBeginDialogAddress, dialogId: string, dialogArgs?: any): SkypeBot;
}

/**
 * Adds additional properties and methods for working with Skype bots.
 */
export class SkypeSession extends Session {
    /**
     * Escapes &, <, and > characters in a text string. These characters are reserved in Slack for 
     * control codes so should always be escaped when returning user generated text.
     */
    escapeText(text: string): string;
    
    /**
     * Unescapes &amp;, &lt;, and &gt; characters in a text string. This restores a previously
     * escaped string.
     */
    unescapeText(text: string): string;
}

/**
 * Connects your bots dialogs to Slack via [BotKit](http://howdy.ai/botkit/).
 */
export class SlackBot extends DialogCollection {
    /**
     * Creates a new instance of the Slack bot using BotKit. 
     * @param controller Controller created from a call to Botkit.slackbot().
     * @param bot The bot created from a call to controller.spawn(). 
     * @param options Optional configuration settings for the bot.
     */
    constructor(controller: any, bot: any, options?: ISlackBotOptions);

    /**
     * Registers an event listener to get notified of bot related events. 
     * @param event Name of event to listen for. The message to passed to events will a slack message. Event types:
     * - __error:__ An error occured. [IBotErrorEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.iboterrorevent.html)
     * - __reply:__ A reply to an existing message was sent. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __send:__ A new message was sent to start a new conversation. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __quit:__ The bot has elected to ended the current conversation. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __typing:__ The bot is sending a 'typing' message to indicate its busy. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __message_received:__ The bot received a message. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __bot_channel_join:__ The bot has joined a channel. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __user_channel_join:__ A user has joined a channel. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __bot_group_join:__ The bot has joined a group. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __user_group_join:__ A user has joined a group. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html) 
     * @param listener Function to invoke.
     */
    on(event: string, listener: Function): void;

    /**
     * Updates the bots configuration settings.
     * @param options Configuration options to set.
     */
    configure(options: ISlackBotOptions): void;

    /**
     * Begins listening for incoming messages of the specified types.
     * @param types The type of events to listen for. Valid types:
     * - __ambient:__ Ambient messages are messages that the bot can hear in a channel, but that do not mention the bot in any way.
     * - __direct_mention:__ Direct mentions are messages that begin with the bot's name, as in "@bot hello".
     * - __mention:__ Mentions are messages that contain the bot's name, but not at the beginning, as in "hello @bot". 
     * - __direct_message:__ Direct messages are sent via private 1:1 direct message channels. 
     * @param dialogId Optional ID of the bots dialog to begin for new conversations. If ommited the bots [defaultDialogId](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.islackbotoptions.html#defaultdialogid) will be used.
     * @param dialogArgs Optional arguments to pass to the dialog when a new conversation is started.
     */
    listen(types: string[], dialogId?: string, dialogArgs?: any): SlackBot;

    /**
     * Begins listening for messages sent to the bot. The bot will recieve direct messages, 
     * direct mentions, and mentions. Once the bot has been mentioned it will continue to receive
     * ambient messages from the user that mentioned them for a short period of time. This time
     * can be configured using [ISlackBotOptions.ambientMentionDuration](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.islackbotoptions.html#ambientmentionduration).
     * @param dialogId Optional ID of the bots dialog to begin for new conversations. If ommited the bots [defaultDialogId](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.islackbotoptions.html#defaultdialogid) will be used.
     * @param dialogArgs Optional arguments to pass to the dialog when a new conversation is started.
     */
    listenForMentions(dialogId?: string, dialogArgs?: any): SlackBot;

    /**
     * Starts a new conversation with a user.
     * @param address Address of the user to begin the conversation with.
     * @param dialogId Unique ID of the bots dialog to begin the conversation with.
     * @param dialogArgs Optional arguments to pass to the dialog.
     */
    beginDialog(address: IBeginDialogAddress, dialogId: string, dialogArgs?: any): void;
}

/**
 * Adds additional properties and methods for working with Slack bots.
 */
export class SlackSession extends Session {
    /** Data that's persisted on a per team basis. */
    teamData: any;

    /** Data that's persisted on a per channel basis. */
    channelData: any;
    
    /**
     * Causes the bot to send a 'typing' message indicating its busy.
     */
    isTyping(): void;
    
    /**
     * Escapes &, <, and > characters in a text string. These characters are reserved in Slack for 
     * control codes so should always be escaped when returning user generated text.
     */
    escapeText(text: string): string;
    
    /**
     * Unescapes &amp;, &lt;, and &gt; characters in a text string. This restores a previously
     * escaped string.
     */
    unescapeText(text: string): string;
}

/**
 * Generic TextBot which lets you drive your bots dialogs from either the console or
 * pratically any other bot platform.
 *
 * There are primarily 2 ways of using the TextBot either purely event driven (preferred) or in
 * mixed mode where you pass a callback to the TextBot.processMessage() method and also listen
 * for events. In this second mode the first reply or error will be returned via the callback and
 * any additonal replies will be delivered as events. Should you decide to ignore the events just
 * be aware that any additional replies from the bot will be lost. 
 */
export class TextBot extends DialogCollection {
    /**
     * @param options Optional configuration settings for the bot.
     */
    constructor(options?: ITextBotOptions);

    /**
     * Registers an event listener to get notified of bot related events. 
     * @param event Name of event to listen for. The message to passed to events will be an [IMessage](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.imessage.html). Event types:
     * - __error:__ An error occured.  [IBotErrorEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.iboterrorevent.html)
     * - __reply:__ A reply to an existing message was sent. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __send:__ A new message was sent to start a new conversation. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __quit:__ The bot has elected to ended the current conversation. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * - __message:__ This event is emitted for every received message. [IBotMessageEvent](http://docs.botframework.com/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotmessageevent.html)
     * @param listener Function to invoke.
     */
    on(event: string, listener: Function): void;

    /**
     * Updates the bots configuration settings.
     * @param options Configuration options to set.
     */
    configure(options: ITextBotOptions): void;

    /**
     * Starts a new conversation with a user.
     * @param address Address of the user to begin the conversation with.
     * @param dialogId Unique ID of the bots dialog to begin the conversation with.
     * @param dialogArgs Optional arguments to pass to the dialog.
     */
    beginDialog(address: IBeginDialogAddress, dialogId: string, dialogArgs?: any): void;

    /**
     * Processes a message received from the user.
     * @param message Message to process.
     * @param callback Optional callback used to return bots initial reply or an error. If ommited all 
     * replies and errors will be returned as events.
     * @param callback.err If not _null_ then an error occured while processing the message.
     * @param callback.reply The bots initial reply for this message that should be sent to the user.
     */
    processMessage(message: IMessage, callback?: (err: Error, reply: IMessage) => void): void;

    /**
     * Begins monitoring console input from stdin. The bot can quit using a call to endDialog() to exit the
     * app.
     */
    listenStdin(): void;
}
