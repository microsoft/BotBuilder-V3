//=============================================================================
//
// INTERFACES
//
//=============================================================================

/**
 * An event recieved from or being sent to a source.
 */
interface IEvent {
    /** Defines type of event. Should be 'message' for an IEvent. */
    type: string;

    /** SDK thats processing the event. Will always be 'botbuilder'. */
    agent: string;

    /** The original source of the event (i.e. 'facebook', 'skype', 'slack', etc.) */
    source: string;

    /** The original event in the sources native schema. For outgoing messages can be used to pass source specific event data like custom attachments. */
    sourceEvent: any;

    /** Address routing information for the event. Save this field to external storage somewhere to later compose a proactive message to the user. */
    address: IAddress; 

    /** 
     * For incoming event this is the user that sent the event. By default this is a copy of [address.user](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.iaddress.html#user) but you can configure your bot with a 
     * [lookupUser](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.iuniversalcallbotsettings.html#lookupuser) function that lets map the incoming user to an internal user id.
     */
    user: IIdentity;
}

/** Implemented by classes that can be converted into an event. */
interface IIsEvent {
    /** Returns the JSON object for the event. */
    toEvent(): IEvent;
}

/** Represents a user, bot, or conversation. */
interface IIdentity {
    /** Channel specific ID for this identity. */
    id: string;

    /** Friendly name for this identity. */ 
    name?: string;

    /** If true the identity is a group. Typically only found on conversation identities and represents a group call. */ 
    isGroup?: boolean;   

    /** Users local if known. */
    locale?: string;

    /** If true the user started the call. */
    originator?: boolean; 
}

/** 
 * Address routing information for a [message](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.imessage.html#address). 
 * Addresses are bidirectional meaning they can be used to address both incoming and outgoing messages. They're also connector specific meaning that
 * [connectors](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.iconnector.html) are free to add their own fields.
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

/** Chat connector specific address. */
interface ICallConnectorAddress extends IAddress {
    /** List of participants on the call. */
    participants: IIdentity[];

    /** ID for the chat thread provided the call is a group call. */
    threadId?: string;

    /** The subject of the call. */
    subject?: string;
    
    /** ID used to debug issues when contacting support. */
    correlationId?: string;

    /** Specifies the URL to post messages back. */ 
    serviceUrl?: string; 

    /** Specifies that auth is required when posting the message back . */ 
    useAuth?: boolean; 
}

/** 
 * IConversation is a JSON body of a first request for new Skype voice call made by Skype Bot Platform 
 * for Calling to a bot. IConversation JSON body is posted on initial HTTPs endpoint registered by a bot 
 * developer in the Bot Framework Portal. IConversation request contains information about caller and target 
 * of the call and some additional information about initial state of a call. 
 */
interface IConversation extends IEvent {
    /** Indicates the current state of the call. */
    callState: string;

    /** Dictionary containing list of HTTPs links. */
    links?: any;

    /** Flag indicates which modalities were presented by Skype user for a call. */
    presentedModalityTypes: string[];
}

/**
 * IConversationResult is a JSON body of any subsequent request following the initial IConversation 
 * notification that is send to a bot from Skype Bot Platform for Calling. IConversationResult is 
 * posted to a callback link provided by previous Workflow response. IConversationResult represents 
 * the result of a last successful action from previous Workflow response.
 */
interface IConversationResult extends IEvent {
    /** Indicates the current state of the call. */
    callState: string;

    /** Dictionary containing list of HTTPs links. */
    links?: any;

    /** Outcome of last executed workflow action. */
    operationOutcome: IActionOutcome;

    /** Buffer of recorded data for a RecordAction. */
    recordedAudio?: any;
}

/**
 * IWorkflow is a JSON body send by the bot in response to IConversation or IConversationResult 
 * request from Skype Bot Platform for Calling. IWorkflow contains list of one or more actions 
 * that bots instructs Skype Bot Platform for Calling on execute on its behalf as well as 
 * callback HTTPs address if bot wants to be notified about result of last executed action 
 * outcome.
 */
interface IWorkflow extends IEvent {
    /** A list of one or more actions that a bot wants to execute on call. */
    actions: IAction[];

    /** A callback link that will be used once the workflow is executed to reply with outcome of workflow. */
    links?: any;

    /** This field indicates that application wants to receive notification updates. Call state change notification is added to this list by default and cannot be unsubscribed. */
    notificationSubscriptions?: string[]; 
}

/** Base class for all actions. */
interface IAction {
    /** Type of action. */
    action: string;

    /** Used to correlate outcomes to actions in ConversationResult. */
    operationId: string;
}

/** Base class for all action outcome. */
interface IActionOutcome {
    /** Type of outcome. */
    type: string;

    /** Id of the operation. */
    id: string;

    /** Indicates the success or failure of the action. */
    outcome: string;

    /** The reason for teh failure. */
    failureReason?: string;
}

/** Implemented by classes that can be converted to actions. */
interface IIsAction {
    /** Returns the JSON value of the action. */
    toAction(): IAction;
}

/**
 * Answer action allows a bot to accept a Skype call. Answer action should be a first action in 
 * response to Conversation notification. 
 */
interface IAnswerAction extends IAction {
    /** The modality types the application will accept. If none are specified it assumes audio only. */
    acceptModalityTypes?: string[]; 
}

/** Result of Answer action. */
interface IAnswerOutcome extends IActionOutcome {
}

/** 
 * Reject allows to decline to answer the call. Reject action could be used as first action of 
 * first workflow instead of Answer. 
 */
interface IRejectAction extends IAction {
}

/** 
 * Result of Reject action. Reject can be used instead of Answer action if bot decides that 
 * the bot does not want to answer the call.
 */
interface IRejectOutcome extends IActionOutcome {
}

/** Record action is interactive action where Skype user audio is recorded. */
interface IRecordAction extends IAction {
    /** A prompt to be played before the recording. */
    playPrompt?: IPlayPromptAction;
    
    /** Maximum duration of recording. The default value is 180 seconds. */
    maxDurationInSeconds?: number;

    /** Maximum initial silence allowed before the recording is stopped. The default value is 5 seconds. */
    initialSilenceTimeoutInSeconds?: number;

    /** Maximum silence allowed after the speech is detected. The default value is 5 seconds. */
    maxSilenceTimeoutInSeconds?: number; 

    /** The format expected for the recording. The RecordingFormat enum describes the supported values. The default value is “wma”. */
    recordingFormat?: string; 

    /** Indicates whether to play beep sound before starting a recording action. */
    playBeep?: boolean; 

    /** Stop digits that user can press on dial pad to stop the recording. */
    stopTones?: string[];
}

/** 
 * Record outcome returns the result of record audio action. RecordOutcome could be returned as 
 * multipart content where first part of multipart contain contains the result of action while 
 * second part contains binary stream representing recorded audio. The audo stream will be 
 * available via the IConversationResult.recordedAudio property.
 */
interface IRecordOutcome extends IActionOutcome {
    /** The RecordingCompletionReason enum value for the completion's reason. */
    completionReason: string;

    /** Length of recorded audio in seconds. */
    lengthOfRecordingInSecs: number; 
}

/** PlayPrompt allows to play either Text-To-Speech audio or a media file. */
interface IPlayPromptAction extends IAction {
    /** List of prompts to play out with each single prompt object. */
    prompts: IPrompt[];
}

/** Prompt played as part of the PlayPrompt action. */
interface IPrompt {
    /** Text-To-Speech text to be played to Skype user. Either [value](#value) or [fileUri](#fileuri) must be specified. */
    value?: string;

    /** HTTP of played media file. Supported formats are WMA or WAV. The file is limited to 512kb in size and cached by Skype Bot Platform for Calling. Either [value](#value) or [fileUri](#fileuri) must be specified. */
    fileUri?: string;

    /** VoiceGender enum value. The default value is “female”. */
    voice?: string;

    /** The Language enum value to use for Text-To-Speech. Only applicable if [value](#value) is text. The default value is “en-US”. Note, currently en-US is the only supported language. */
    culture?: string;

    /** Any silence played out before [value](#value) is played. If [value](#value) is null, this field must be a valid > 0 value. */
    silenceLengthInMilliseconds?: number;

    /** Indicates whether to emphasize when tts'ing out. It's applicable only if [value](#value) is text. The default value is false. */
    emphasize?: boolean;

    /** The SayAs enum value indicates whether to customize pronunciation during tts. It's applicable only if [value](#value) is text. */
    sayAs?: string;
}

/** Implemented by classes that can be converted to prompts. */
interface IIsPrompt {
    /** Returns the JSON value of the prompt. */
    toPrompt(): IPrompt;
}

/** Play prompt outcome returns the result of playing a prompt. */
interface IPlayPromptOutcome extends IActionOutcome {
}

/** 
 * Recognize action allows to either capture the speech recognition output or collect digits 
 * from Skype user dial pad. 
 */
interface IRecognizeAction extends IAction {
    /** A prompt to be played before the recognition starts. */
    playPrompt?: IPlayPromptAction;

    /** Indicates if Skype user is allowed to enter choice before the prompt finishes. The default value is true. */
    bargeInAllowed?: boolean;

    /** Culture is an enum indicating what culture the speech recognizer should use. The default value is “en-US”. Currently the only culture supported is en-US. */
    culture?: string;

    /** Maximum initial silence allowed before failing the operation from the time we start the recording. The default value is 5 seconds. */ 
    initialSilenceTimeoutInSeconds?: number; 

    /** Maximum allowed time between dial pad digits. The default value is 1 second. */
    interdigitTimeoutInSeconds?: number; 

    /** List of RecognitionOption objects dictating the recognizable choices. Choices can be speech or dial pad digit based. Either [collectDigits](#collectDigits) or [choices](#choices) must be specified, but not both. */
    choices?: IRecognitionChoice[];

    /** CollectDigits will result in collecting digits from Skype user dial pad as part of recognize. Either [collectDigits](#collectDigits) or [choices](#choices) must be specified, but not both. */
    collectDigits?: ICollectDigits;
}

/** Recognize outcome is a result of recognize action. It contains either recognized digits or recognized speech. */
interface IRecognizeOutcome extends IActionOutcome {
    /** The value indicating captured speech recognition choice. */
    choiceOutcome?: IChoiceOutcome;
    
    /** The value indicating capturing collected dial pad digit. */
    collectDigitsOutcome?: ICollectDigitsOutcome;
}

/** 
 * Hang up allows for bot to end ongoing call. Hang up is the last action in workflow. Note, the 
 * different between Hangup and Reject. Reject action allows the bot to end the call instead of 
 * answering the call while Hangup terminates ongoing call.
 */
interface IHangupAction extends IAction {
}

/** Returns the result of hangup. */
interface IHangupOutcome extends IActionOutcome {
}

/** The RecordingCompletionReason enum describes the reasons for a recording operation's completion. */
export var RecordingCompletionReason: {
    /** If the maximum initial silence tolerated had been reached. It results in a failed recording attempt. */
    initialSilenceTimeout: string;

    /** If the maximum recording duration for recording was reached. It results in a failed recognition attempt. */
    maxRecordingTimeout: string;

    /** Silence after a burst of talking was detected, ending the call. It results in a successful recording attempt. */
    completedSilenceDetected: string;

    /** The customer completed recording by punching in a stop tone. It results in a successful recording attempt. */
    completedStopToneDetected: string;

    /** The underlying call was terminated. If there were any bytes recorded, it results in a successful recording attempt. */
    callTerminated: string;

    /** System failure. */
    temporarySystemFailure: string;
};

/** Specifies the speech & DTMF options for a choice based recognition. For example, "Say 'Sales' or press 1 for the sales department." */
interface IRecognitionChoice {
    /** The choice's name. Once a choice matches, this name is conveyed back to the bot in the outcome. */
    name: string;

    /** Speech variations that form the choice's grammar. For example, Name : "Yes", SpeechVariation : ["Yes", "yeah", "ya", "yo"] */
    speechVariation?: string[];

    /** DTMF variations for the choice. For example, Name : "Yes" , DtmfVariation : {'1'} */
    dtmfVariation?: string;
}

/** Specifies the options for digit collection. For example, "Enter your 5-digit zip code followed by the pound sign." */
interface ICollectDigits {
    /** Maximum number of digits expected. */
    maxNumberOfDtmfs?: number; 

    /** Stop tones that will end the dial pad digit collection. */
    stopTones?: string[];
}

/** Returned when a choice recognition is selected. */
interface IChoiceOutcome {
    /** RecognizeCompletionReason enum indicates the recognition operation's completion reason of the recognition operation. */
    completionReason: string; 

    /** Choice that was recognized if any. */
    choiceName?: string;
}

/** The RecognitionCompletionReason enum describes the reasons for completing the speech or digit recognition. */
export var RecognitionCompletionReason: {
    /** Indicates the maximum initial silence tolerated was reached. It results in a failed recognition attempt. */
    initialSilenceTimeout: string;

    /** The recognition completed because the customer pressed in a digit not among the possible choices. For speech recognition based menus, this completion reason is never possible. It results in a failed recognition attempt. */
    incorrectDtmf: string;

    /** The maximum time between a customer punching in successive digits has elapsed. For speech menus, the completion reason is never possible. It results in a failed recognition attempt. */
    interdigitTimeout: string;

    /** The recognition successfully matched a grammar option. */
    speechOptionMatched: string;

    /** The recognition successfully matched a digit option. */
    dtmfOptionMatched: string;

    /** The underlying call was terminated. It results in a failed recognition attempt. */
    callTerminated: string;

    /** System failure. */
    temporarySystemFailure: string;
};

/** Returned when digit collection is selected.  */
interface ICollectDigitsOutcome {
    /** DigitCollectionCompletionReason enum. Indicates the completion reason of the collectdigits operation. */
    completionReason: string;

    /** Recognized digits. */ 
    digits?: string;
}

/** The DigitCollectionCompletionReason enum describes the completion reasons of the digit collection operation. */
export var DigitCollectionCompletionReason: {
    /** Indicates the maximum initial silence tolerated was reached. It results in a failed recognition attempt. */
    initialSilenceTimeout: string;

    /** The maximum time between a customer pressing in successive digits has elapsed. It results in a successful digit collection together with the digits collected until timeout. */
    interdigitTimeout: string;

    /** The customer completed recording by pressing a stop tone. It results in a successful recording attempt. The stoptones detected are excluded and not returned in collection of captured digits. */
    completedStopToneDetected: string;

    /** The underlying call was terminated. It results in a failed recognition attempt. */
    callTerminated: string;

    /** System failure. */
    temporarySystemFailure: string;
};

/** The VoiceGender enum describes the list of voice genders for text to speech. */
export var VoiceGender: {
    /** Indicates male voice. */
    male: string;

    /** Indicates female voice. */
    female: string;
};

/** The RecordingFormat enum describes the list of encoding formats used for recording. */
export var RecordingFormat: {
    /** Indicates Windows media audio format. */
    wma: string;

    /** Indicates waveform audio file format. */
    wav: string;

    /** Indicates mp3 audio file format. */
    mp3: string;
};

/** Recording returned from the built-in record prompt.  */
export interface IRecording {
    /** Buffer of recorded data for a RecordAction. */
    recordedAudio: any;

    /** Length of recorded audio in seconds. */
    lengthOfRecordingInSecs: number;
}

/** The CallState enum describes call's various possible states. */
export var CallState: {
    /** Indicates the call's initial state. */
    idle: string;

    /** Indicates the call has just been received. */
    incoming: string;

    /** Indicates the call establishment is in progress after initiating or accepting the call. */
    establishing: string;

    /** Indicates the call is established. */
    established: string;

    /** Indicates the call is on hold. */
    hold: string;

    /** Indicates the call is no longer on hold. */
    unhold: string;

    /** Indicates the call initiated a transfer. */
    transferring: string;

    /** Indicates the call initiated a redirection. */
    redirecting: string;

    /** Indicates the call is terminating. */
    terminating: string;

    /** Indicates the call is terminated. */
    terminated: string;
};

/** The ModalityType enum describes the various supported call modality types. */
export var ModalityType: {
    /** Indicates the call has audio modality. */
    audio: string;

    /** Indicates the call has video modality. */
    video: string;

    /** Indicates the call has video-based screen sharing modality. */
    videoBasedScreenSharing: string;
};

/** The Outcomes enum describes possible result value. */
export var OperationOutcome: {
    /** Indicates success. */
    success: string;

    /** Indicates failure. */
    failure: string;
}

/** The SayAs enum describes the list of supported pronunciation attributes when using text to speech. */
export var SayAs: {
    /** Say as year, month and day. */
    yearMonthDay: string;

    /** Say as month, day and year. */
    monthDayYear: string;

    /** Say as day, month and year. */
    dayMonthYear: string;

    /** Say as year and month. */
    yearMonth: string;

    /** Say as month and year. */
    monthYear: string;

    /** Say as month and day. */
    monthDay: string;

    /** Say as day and month. */
    dayMonth: string;

    /** Say as day. */
    day: string;

    /** Say as month. */
    month: string;

    /** Say as year. */
    year: string;

    /** Say as cardinal. */
    cardinal: string;

    /** Say as ordinal. */
    ordinal: string;

    /** Say as letters. */
    letters: string;

    /** Say as 12 hour time. */
    time12: string;

    /** Say as 24 hour time. */
    time24: string;

    /** Say as telephone. */
    telephone: string;

    /** Say as name. */
    name: string;

    /** Say as phonetic name. */
    phoneticName: string;
};

/** Plugin for localizing messages sent to the user by a bot. */
interface ILocalizer {
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
interface ISessionState {
    /** Dialog stack for the current session. */
    callstack: IDialogState[];

    /** Timestamp of when the session was last accessed. */
    lastAccess: number;

    /** Version number of the current callstack. */
    version: number;
}

/** An entry on the sessions dialog stack. */
interface IDialogState {
    /** ID of the dialog. */
    id: string;

    /** Persisted state for the dialog. */
    state: any;
}

/** 
  * Results returned by a child dialog to its parent via a call to session.endDialog(). 
  */
interface IDialogResult<T> {
    /** The reason why the current dialog is being resumed. */
    resumed: ResumeReason;

    /** ID of the child dialog thats ending. */
    childId?: string;

    /** If an error occured the child dialog can return the error to the parent. */
    error?: Error;

    /** The users response. */
    response?: T;
}

/** Options passed to built-in prompts. */
interface IPromptOptions {
    /** (Optional) maximum number of times to reprompt the user. Default value is 2. */
    maxRetries?: number;
}

/** Options passed to recognizer based prompts. */
export interface IRecognizerPromptOptions extends IPromptOptions {
    /** Indicates if Skype user is allowed to enter choice before the prompt finishes. The default value is true.   */
    bargeInAllowed?: boolean;

    /** Culture is an enum indicating what culture the speech recognizer should use. The default value is “en-US”. Currently the only culture supported is en-US. */
    culture?: string; 

    /** Maximum initial silence allowed before failing the operation from the time we start the recording. The default value is 5 seconds. */
    initialSilenceTimeoutInSeconds?: number; 

    /** Maximum allowed time between dial pad digits. The default value is 1 second. */
    interdigitTimeoutInSeconds?: number; 
}

/** Options passed to a 'record' prompt. */
export interface IRecordPromptOptions extends IPromptOptions {
    /** Maximum duration of recording. The default value is 180 seconds. */
    maxDurationInSeconds?: number;

    /** Maximum initial silence allowed before the recording is stopped. The default value is 5 seconds. */
    initialSilenceTimeoutInSeconds?: number;

    /** Maximum silence allowed after the speech is detected. The default value is 5 seconds. */
    maxSilenceTimeoutInSeconds?: number; 

    /** The format expected for the recording. The RecordingFormat enum describes the supported values. The default value is “wma”. */
    recordingFormat?: string; 

    /** Indicates whether to play beep sound before starting a recording action. */
    playBeep?: boolean; 

    /** Stop digits that user can press on dial pad to stop the recording. */
    stopTones?: string[];
}

/** Options passed to a 'confirm' prompt. */
export interface IConfirmPromptOptions extends IRecognizerPromptOptions {
    /** Overrides the default options for the 'yes' choice. */
    yesChoice?: IRecognitionChoice;

    /** Overrides the default options for the 'no' choice. */
    noChoice?: IRecognitionChoice;

    /** Enables a third cancel choice. */
    cancelChoice?: IRecognitionChoice;
}

/** Options passed to a 'digits' prompt. */
export interface IDigitsPromptOptions extends IRecognizerPromptOptions {
    /** (Optional) stop tones used to terminate the digit collection. */
    stopTones?: string[];
}

/** Dialog result returned by a system prompt. */
interface IPromptResult<T> extends IDialogResult<T> {
    /** Type of prompt completing. */
    promptType?: PromptType;
}

/** Strongly typed Action Prompt Result. */
interface IPromptActionResult extends IPromptResult<IActionOutcome> { }

/** Strongly typed Confirm Prompt Result. */
interface IPromptConfirmResult extends IPromptResult<boolean> { } 

/** Strongly typed Choice Prompt Result. */
interface IPromptChoiceResult extends IPromptResult<IFindMatchResult> { }

/** Strongly typed Digits Prompt Result. */
interface IPromptDigitsResult extends IPromptResult<string> { }

/** Strongly typed Record Prompt Result. */
interface IPromptRecordResult extends IPromptResult<IRecording> { }

/** Global configuration options for the Prompts dialog. */
interface IPromptsSettings {
    /** PlayPrompt to send when a recognizer prompt detects too much silence. */
    recognizeSilencePrompt?: string|string[]|IAction|IIsAction;

    /** PlayPrompt to send when a recognizer prompt detects an invalid DTMF. */
    invalidDtmfPrompt?: string|string[]|IAction|IIsAction;

    /** PlayPrompt to send when a recognizer prompt can't recognize the users sppech utterance. */
    invalidRecognizePrompt?: string|string[]|IAction|IIsAction;
    
    /** PlayPrompt to send when a record prompt detects too much silence. */
    recordSilencePrompt?: string|string[]|IAction|IIsAction;

    /** PlayPrompt to send when a user leaves a message thats too long. */
    maxRecordingPrompt?: string|string[]|IAction|IIsAction;

    /** PlayPrompt to send when a recording is invalid. */
    invalidRecordingPrompt?: string|string[]|IAction|IIsAction;
}

/** Options passed to the constructor of a session. */
interface ICallSessionOptions {
    /** Function to invoke when the sessions state is saved. */
    onSave: (done: (err: Error) => void) => void;

    /** Function to invoke when a batch of messages are sent. */
    onSend: (messages: IEvent[], done: (err: Error) => void) => void;

    /** The bots root library of dialogs. */
    library: Library;

    /** Array of session middleware to execute prior to each request. */
    middleware: ICallSessionMiddleware[];

    /** Unique ID of the dialog to use when starting a new conversation with a user. */
    dialogId: string;

    /** (Optional) arguments to pass to the conversations initial dialog. */
    dialogArgs?: any;

    /** (Optional) localizer to use when localizing the bots responses. */
    localizer?: ILocalizer;
    
    /** (Optional) time to allow between each message sent as a batch. The default value is 150ms.  */
    autoBatchDelay?: number;

    /** Default error message to send users when a dialog error occurs. */
    dialogErrorMessage?: string|string[]|IAction|IIsAction;

    /** Default prompt settings to use. */
    promptDefaults: IPrompt;

    /** Default recognizer settings to use. */
    recognizeDefaults: IRecognizeAction;

    /** Default recording settings to use. */
    recordDefaults: IRecordAction;
}

/** result returnd from a call to EntityRecognizer.findBestMatch() or EntityRecognizer.findAllMatches(). */
interface IFindMatchResult {
    /** Value that was matched.  */
    entity: string;

    /** Confidence score on a scale from 0.0 - 1.0 that an value matched the users utterance. */
    score: number;
}

/** Context object passed to IBotStorage calls. */
interface IBotStorageContext {
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
interface IBotStorageData {
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

/** Replacable storage system used by UniversalCallBot. */
interface IBotStorage {
    /** Reads in data from storage. */
    getData(context: IBotStorageContext, callback: (err: Error, data: IBotStorageData) => void): void;
    
    /** Writes out data to storage. */
    saveData(context: IBotStorageContext, data: IBotStorageData, callback?: (err: Error) => void): void;
}

/** Options used to initialize a ChatConnector instance. */
interface ICallConnectorSettings {
    /** The URI that should be used to receive workflow callbacks. This should typically be the endpoint calling entered into the developer portal. */
    callbackUri: string;
    
    /** The bots App ID assigned in the Bot Framework portal. */
    appId?: string;

    /** The bots App Password assigned in the Bot Framework Portal. */
    appPassword?: string;
}

/** Options used to initialize a UniversalCallBot instance. */
interface IUniversalCallBotSettings {
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

    /** (Optional) message to send the user should an unexpected error occur during a conversation. A default message is provided. */
    dialogErrorMessage?: string|string[]|IAction|IIsAction;

    /** Default prompt settings to use. */
    promptDefaults: IPrompt;

    /** Default recognizer settings to use. */
    recognizeDefaults: IRecognizeAction;

    /** Default recording settings to use. */
    recordDefaults: IRecordAction;
}

/** Implemented by connector plugins for the UniversalCallBot. */
interface ICallConnector {
    /** Called by the UniversalCallBot at creation time to register a handler for receiving incoming call events from the service. */
    onEvent(handler: (event: IEvent, cb?: (err: Error) => void) => void): void;

    /** Called by the UniversalCallBot to deliver workflow events to the service. */
    send(event: IEvent, cb: (err: Error) => void): void;
}

/** Function signature for a piece of middleware that hooks the 'recieve' or 'send' events. */
interface IEventMiddleware {
    (event: IEvent, next: Function): void;
}

/** Function signature for a piece of middleware that hooks the 'botbuilder' event. */
interface ICallSessionMiddleware {
    (session: CallSession, next: Function): void;
}

/** 
 * Map of middleware hooks that can be registered in a call to __UniversalCallBot.use()__. 
 */
interface IMiddlewareMap {
    /** Called in series when an incoming event is received. */
    receive?: IEventMiddleware|IEventMiddleware[];

    /** Called in series before an outgoing event is sent. */
    send?: IEventMiddleware|IEventMiddleware[];

    /** Called in series once an incoming message has been bound to a session. Executed after [analyze](#analyze) middleware.  */
    botbuilder?: ICallSessionMiddleware|ICallSessionMiddleware[];
}

/** 
 * Signature for functions passed as steps to [DialogAction.waterfall()](en-us/node/builder/calling-reference/classes/_botbuilder_d_.dialogaction.html#waterfall). 
 * 
 * Waterfalls let you prompt a user for information using a sequence of questions. Each step of the
 * waterfall can either execute one of the built-in [Prompts](en-us/node/builder/calling-reference/classes/_botbuilder_d_.prompts.html),
 * start a new dialog by calling [session.beginDialog()](en-us/node/builder/calling-reference/classes/_botbuilder_d_.session.html#begindialog),
 * advance to the next step of the waterfall manually using `skip()`, or terminate the waterfall.
 * 
 * When either a dialog or built-in prompt is called from a waterfall step, the results from that 
 * dialog or prompt will be passed via the `results` parameter to the next step of the waterfall. 
 * Users can say things like "nevermind" to cancel the built-in prompts so you should guard against
 * that by at least checking for [results.response](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.idialogresult.html#response) 
 * before proceeding. A more detailed explination of why the waterfall is being continued can be 
 * determined by looking at the [code](en-us/node/builder/calling-reference/enums/_botbuilder_d_.resumereason.html) 
 * returned for [results.resumed](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.idialogresult.html#resumed).
 * 
 * You can manually advance to the next step of the waterfall using the `skip()` function passed
 * in. Calling `skip({ response: "some text" })` with an [IDialogResult](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.idialogresult.html)
 * lets you more accurately mimic the results from a built-in prompt and can simplify your overall
 * waterfall logic.
 * 
 * You can terminate a waterfall early by either falling through every step of the waterfall using
 * calls to `skip()` or simply not starting another prompt or dialog.
 * 
 * __note:__ Waterfalls have a hidden last step which will automatically end the current dialog if 
 * if you call a prompt or dialog from the last step. This is useful where you have a deep stack of
 * dialogs and want a call to [session.endDialog()](en-us/node/builder/calling-reference/classes/_botbuilder_d_.session.html#enddialog)
 * from the last child on the stack to end the entire stack. The close of the last child will trigger
 * all of its parents to move to this hidden step which will cascade the close all the way up the stack.
 * This is typically a desired behaviour but if you want to avoid it or stop it somewhere in the 
 * middle you'll need to add a step to the end of your waterfall that either does nothing or calls 
 * something liek [session.send()](en-us/node/builder/calling-reference/classes/_botbuilder_d_.session.html#send)
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
interface IDialogWaterfallStep {
    /**
     * @param session Session object for the current conversation.
     * @param result 
     * * __result:__ _{any}_ - For the first step of the waterfall this will be `null` or the value of any arguments passed to the handler.
     * * __result:__ _{IDialogResult}_ - For subsequent waterfall steps this will be the result of the prompt or dialog called in the previous step.
     * @param skip Fuction used to manually skip to the next step of the waterfall.  
     * @param skip.results (Optional) results to pass to the next waterfall step. This lets you more accurately mimic the results returned from a prompt or dialog.
     */
    (session: CallSession, result?: any | IDialogResult<any>, skip?: (results?: IDialogResult<any>) => void): any;
}

/** Function signature for an error event handler. */
interface IErrorEvent {
    (err: Error): void;
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

/**
  * Type of prompt invoked.
  */
export enum PromptType {
    /** Send a workflow action as a prompt. Allows you to handle the raw outcome for the action. */
    action, 

    /** The user is prompted to confirm an action with a yes/no response. */
    confirm, 
    
    /** The user is prompted to select from a list of choices. */
    choice, 
    
    /** The user is prompted to enter a sequence of digits. */
    digits, 

    /** The user is prompted to record a message. */    
    record
}


//=============================================================================
//
// CLASSES
//
//=============================================================================

/**
 * Manages the bots conversation with a user.
 */
export class CallSession {
    /**
     * Registers an event listener.
     * @param event Name of the event. Event types:
     * - __error:__ An error occured. [IErrorEvent](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.ierrorevent.html)
     * @param listener Function to invoke.
     */
    on(event: string, listener: Function): void;

    /**
     * Creates an instance of the session.
     * @param options Sessions configuration options.
     */
    constructor(options: ICallSessionOptions);

    /**
     * Dispatches a message for processing. The session will call any installed middleware before
     * the message to the active dialog for processing. 
     * @param sessionState The current session state. If _null_ a new conversation will be started beginning with the configured [dialogId](#dialogid).  
     * @param message The message to dispatch.
     */
    dispatch(sessionState: ISessionState, message: IEvent): CallSession;

    /** The bots root library of dialogs. */
    library: Library;

    /** Sessions current state information. */
    sessionState: ISessionState;

    /** The message recieved from the user. For bot originated messages this may only contain the "to" & "from" fields. */
    message: IEvent;

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
    error(err: Error): CallSession;

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
    save(): CallSession;

    /** Manually answers the call. The call will be automatically answered when the bot takes an action. */
    answer(): CallSession;

    /** Rejects an incoming call. */
    reject(): CallSession;

    /** Manually ends an established call. The call will be automatically ended when the bot stops prompting the user for input. */
    hangup(): CallSession;

    /**
     * Sends a PlayPrompt action to the user.  
     * @param action 
     * * __action:__ _{string}_ - Text of the message to send. The message will be localized using the sessions configured localizer. If arguments are passed in the message will be formatted using [sprintf-js](https://github.com/alexei/sprintf.js).
     * * __action:__ _{string[]}_ - The sent message will be chosen at random from the array.
     * * __action:__ _{IAction|IIsAction}_ - Action to send. 
     * @param args (Optional) arguments used to format the final output text when __action__ is a _{string|string[]}_.
     */
    send(action: string|string[]|IAction|IIsAction, ...args: any[]): CallSession;

    /**
     * Returns true if a message has been sent for this session.
     */
    messageSent(): boolean;

    /**
     * Passes control of the conversation to a new dialog. The current dialog will be suspended 
     * until the child dialog completes. Once the child ends the current dialog will receive a
     * call to [dialogResumed()](en-us/node/builder/calling-reference/classes/_botbuilder_d_.dialog.html#dialogresumed) 
     * where it can inspect any results returned from the child. 
     * @param id Unique ID of the dialog to start.
     * @param args (Optional) arguments to pass to the dialogs [begin()](en-us/node/builder/calling-reference/classes/_botbuilder_d_.dialog.html#begin) method.
     */
    beginDialog<T>(id: string, args?: T): CallSession;

    /**
     * Ends the current dialog and starts a new one its place. The parent dialog will not be 
     * resumed until the new dialog completes. 
     * @param id Unique ID of the dialog to start.
     * @param args (Optional) arguments to pass to the dialogs [begin()](en-us/node/builder/calling-reference/classes/_botbuilder_d_.dialog.html#begin) method.
     */
    replaceDialog<T>(id: string, args?: T): CallSession;

    /** 
     * Ends the current conversation and optionally sends a message to the user. The call will be automatically hungup or rejected. 
     * @param action (Optional)
     * * __action:__ _{string}_ - Text of the message to send. The message will be localized using the sessions configured localizer. If arguments are passed in the message will be formatted using [sprintf-js](https://github.com/alexei/sprintf.js).
     * * __action:__ _{string[]}_ - The sent message will be chosen at random from the array.
     * * __action:__ _{IAction|IIsAction}_ - Action to send. 
     * @param args (Optional) arguments used to format the final output text when __message__ is a _{string|string[]}_.
     */
    endConversation(action?: string|string[]|IAction|IIsAction, ...args: any[]): CallSession;

    /**
     * Ends the current dialog and optionally sends a message to the user. The parent will be resumed with an [IDialogResult.resumed](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.idialogresult.html#resumed) 
     * reason of [completed](en-us/node/builder/calling-reference/enums/_botbuilder_d_.resumereason.html#completed).  
     * @param action (Optional)
     * * __action:__ _{string}_ - Text of the message to send. The message will be localized using the sessions configured localizer. If arguments are passed in the message will be formatted using [sprintf-js](https://github.com/alexei/sprintf.js).
     * * __action:__ _{string[]}_ - The sent message will be chosen at random from the array.
     * * __action:__ _{IAction|IIsAction}_ - Action to send. 
     * @param args (Optional) arguments used to format the final output text when __message__ is a _{string|string[]}_.
     */
    endDialog(action?: string|string[]|IAction|IIsAction, ...args: any[]): CallSession;

    /**
     * Ends the current dialog and optionally returns a result to the dialogs parent. 
     */
    endDialogWithResult(result?: IDialogResult<any>): CallSession;

    /**
     * Clears the sessions callstack and restarts the conversation with the configured dialogId.
     * @param dialogId (Optional) ID of the dialog to start.
     * @param dialogArgs (Optional) arguments to pass to the dialogs [begin()](en-us/node/builder/calling-reference/classes/_botbuilder_d_.dialog.html#begin) method.
     */
    reset(dialogId?: string, dialogArgs?: any): CallSession;

    /** Returns true if the session has been reset. */
    isReset(): boolean;

    /** Immediately ends the current batch and delivers any queued up messages. */
    sendBatch(): void;
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
    begin<T>(session: CallSession, args?: T): void;

    /**
     * Called when a new reply message has been recieved from a user.
     *
     * Derived classes should implement this to process the message recieved from the user.
     * @param session Session object for the current conversation.
     */
    abstract replyReceived(session: CallSession): void;

    /**
     * A child dialog has ended and the current one is being resumed.
     * @param session Session object for the current conversation.
     * @param result Result returned by the child dialog.
     */
    dialogResumed<T>(session: CallSession, result: IDialogResult<T>): void;
}

/**
 * A library of related dialogs used for routing purposes. Libraries can be chained together to enable
 * the development of complex bots. The [UniversalCallBot](en-us/node/builder/calling-reference/classes/_botbuilder_d_.UniversalCallBot.html)
 * class is itself a Library that forms the root of this chain. 
 * 
 * Libraries of reusable parts can be developed by creating a new Library instance and adding dialogs 
 * just as you would to a bot. Your library should have a unique name that corresponds to either your 
 * libraries website or NPM module name.  Bots can then reuse your library by simply adding your parts
 * Library instance to their bot using [UniversalCallBot.library()](en-us/node/builder/calling-reference/classes/_botbuilder_d_.UniversalCallBot.html#library).
 * If your library itself depends on other libraries you should add them to your library as a dependency 
 * using [Library.library()](#library). You can easily manage multiple versions of your library by 
 * adding a version number to your library name.
 * 
 * To invoke dialogs within your library bots will need to call [session.beginDialog()](en-us/node/builder/calling-reference/classes/_botbuilder_d_.session.html#begindialog)
 * with a fully qualified dialog id in the form of '<libName>:<dialogId>'. You'll typically hide 
 * this from the devloper by exposing a function from their module that starts the dialog for them.
 * So calling something like `myLib.someDialog(session, { arg: '' });` would end up calling
 * `session.beginDialog('myLib:someDialog', args);` under the covers.
 * 
 * Its worth noting that dialogs are always invoked within the current dialog so once your within
 * a dialog from your library you don't need to prefix every beginDialog() call your with your 
 * libraries name. Its only when crossing from one library context to another that you need to 
 * include the library name prefix.  
 */
export class Library {
    /** Unique name of the library. */
    name: string;

    /** Creates a new instance of the library. */
    constructor(name: string);

    /**
     * Registers or returns a dialog from the library.
     * @param id Unique ID of the dialog being regsitered or retrieved.
     * @param dialog (Optional) dialog or waterfall to register.
     * * __dialog:__ _{Dialog}_ - Dialog to add.
     * * __dialog:__ _{IDialogWaterfallStep[]}_ - Waterfall of steps to execute. See [IDialogWaterfallStep](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.idialogwaterfallstep.html) for details.
     * * __dialog:__ _{IDialogWaterfallStep}_ - Single step waterfall. Calling a built-in prompt or starting a new dialog will result in the current dialog ending upon completion of the child prompt/dialog. 
     */
    dialog(id: string, dialog?: Dialog|IDialogWaterfallStep[]|IDialogWaterfallStep): Dialog;

    /**  
     * Registers or returns a library dependency.
     * @param lib 
     * * __lib:__ _{Library}_ - Library to register as a dependency.
     * * __lib:__ _{string}_ - Unique name of the library to lookup. All dependencies will be searched as well.
     */
    library(lib: Library|string): Library;

    /**
     * Searches the library and all of its dependencies for a specific dialog. Returns the dialog 
     * if found, otherwise null.
     * @param libName Name of the library containing the dialog.
     * @param dialogId Unique ID of the dialog within the library.
     */
    findDialog(libName: string, dialogId: string): Dialog;
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
}

/**
 * Built in built-in prompts that can be called from any dialog. 
 */
export class Prompts extends Dialog {
    /**
     * Processes messages received from the user. Called by the dialog system. 
     * @param session Session object for the current conversation.
     */
    replyReceived(session: CallSession): void;

    /**
     * Updates global options for the Prompts dialog. 
     * @param settings Options to set.
     */
    static configure(settings: IPromptsSettings): void;

    /**
     * Sends a wrokflow action as a prompt to the user. Lets you process the raw outcome
     * @param session Session object for the current conversation.
     * @param action The workflow action to send.
     */
    static action(session: CallSession, action: IAction|IIsAction): void;

    /**
     * Prompts the user to confirm an action with a yes/no response.
     * @param session Session object for the current conversation.
     * @param playPrompt 
     * * __playPrompt:__ _{string}_ - Initial message to send the user.
     * * __playPrompt:__ _{string[]}_ - Array of possible messages to send user. One will be chosen at random. 
     * * __playPrompt:__ _{IAction|IIsAction}_ - Initial PlayPrompt action to send the user. 
     * @param options (Optional) parameters to control the behaviour of the prompt.
     */
    static confirm(session: CallSession, playPrompt: string|string[]|IAction|IIsAction, options?: IConfirmPromptOptions): void;    

    /**
     * Prompts the user to choose from a list of options.
     * @param session Session object for the current conversation.
     * @param playPrompt 
     * * __playPrompt:__ _{string}_ - Initial message to send the user.
     * * __playPrompt:__ _{string[]}_ - Array of possible messages to send user. One will be chosen at random. 
     * * __playPrompt:__ _{IAction|IIsAction}_ - Initial PlayPrompt action to send the user. 
     * @param choices List of choices to prompt user with.
     * @param options (Optional) parameters to control the behaviour of the prompt.
     */
    static choice(session: CallSession, playPrompt: string|string[]|IAction|IIsAction, choices: IRecognitionChoice[], options?: IRecognizerPromptOptions): void;

    /**
     * Prompts the user to input a sequence of digits.
     * @param session Session object for the current conversation.
     * @param playPrompt 
     * * __playPrompt:__ _{string}_ - Initial message to send the user.
     * * __playPrompt:__ _{string[]}_ - Array of possible messages to send user. One will be chosen at random. 
     * * __playPrompt:__ _{IAction|IIsAction}_ - Initial PlayPrompt action to send the user. 
     * @param maxDigits Maximum number of digits allowed.
     * @param options (Optional) parameters to control the behaviour of the prompt.
     */
    static digits(session: CallSession, playPrompt: string|string[]|IAction|IIsAction, maxDigits: number, options?: IDigitsPromptOptions): void;

    /**
     * Prompts the user to record a message.
     * @param session Session object for the current conversation.
     * @param playPrompt 
     * * __playPrompt:__ _{string}_ - Initial message to send the user.
     * * __playPrompt:__ _{string[]}_ - Array of possible messages to send user. One will be chosen at random. 
     * * __playPrompt:__ _{IAction|IIsAction}_ - Initial PlayPrompt action to send the user. 
     * @param options (Optional) parameters to control the behaviour of the prompt.
     */
    static record(session: CallSession, playPrompt: string|string[]|IAction|IIsAction, options?: IRecordPromptOptions): void;
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
 *   until you call [session.endDialog()](en-us/node/builder/calling-reference/classes/_botbuilder_d_.session.html#enddialog). 
 */
export class SimpleDialog extends Dialog {
    /**
     * Creates a new custom dialog based on a simple closure.
     * @param handler The function closure for your dialog. 
     * @param handler.session Session object for the current conversation.
     * @param handler.args 
     * * __args:__ _{any}_ - For the first call to the handler this will be either `null` or the value of any arguments passed to [Session.beginDialog()](en-us/node/builder/calling-reference/classes/_botbuilder_d_.session.html#begindialog).
     * * __args:__ _{IDialogResult}_ - If the handler takes an action that results in a new dialog being started those results will be returned via subsequent calls to the handler.
     */
    constructor(handler: (session: CallSession, args?: any | IDialogResult<any>) => void);
    
    /**
     * Processes messages received from the user. Called by the dialog system. 
     * @param session Session object for the current conversation.
     */
    replyReceived(session: CallSession): void;
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
export class UniversalCallBot  {
    
    /** 
     * Creates a new instance of the UniversalCallBot.
     * @param connector (Optional) the default connector to use for requests. If there's not a more specific connector registered for a channel then this connector will be used./**
     * @param settings (Optional) settings to configure the bot with.
     */
    constructor(connector: ICallConnector, settings?: IUniversalCallBotSettings);

    /**
     * Registers an event listener.
     * @param event Name of the event. Event types:
     * - __error:__ An error occured. [IErrorEvent](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.ierrorevent.html)
     * @param listener Function to invoke.
     */
    on(event: string, listener: Function): void;

    /** 
     * Sets a setting on the bot. 
     * @param name Name of the property to set. Valid names are properties on [IUniversalCallBotSettings](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.iUniversalCallBotsettings.html).
     * @param value The value to assign to the setting.
     */
    set(name: string, value: any): UniversalCallBot;

    /** 
     * Returns the current value of a setting.
     * @param name Name of the property to return. Valid names are properties on [IUniversalCallBotSettings](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.iUniversalCallBotsettings.html).
     */
    get(name: string): any;

    /**
     * Registers or returns a dialog for the bot.
     * @param id Unique ID of the dialog being regsitered or retrieved.
     * @param dialog (Optional) dialog or waterfall to register.
     * * __dialog:__ _{Dialog}_ - Dialog to add.
     * * __dialog:__ _{IDialogWaterfallStep[]}_ - Waterfall of steps to execute. See [IDialogWaterfallStep](en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.idialogwaterfallstep.html) for details.
     * * __dialog:__ _{IDialogWaterfallStep}_ - Single step waterfall. Calling a built-in prompt or starting a new dialog will result in the current dialog ending upon completion of the child prompt/dialog. 
     */
    dialog(id: string, dialog?: Dialog|IDialogWaterfallStep[]|IDialogWaterfallStep): Dialog;
    
    /**  
     * Registers or returns a library dependency.
     * @param lib 
     * * __lib:__ _{Library}_ - Library to register as a dependency.
     * * __lib:__ _{string}_ - Unique name of the library to lookup. All dependencies will be searched as well.
     */
    library(lib: Library|string): Library;
    
    /** 
     * Installs middleware for the bot. Middleware lets you intercept incoming and outgoing events/messages. 
     * @param args One or more sets of middleware hooks to install.
     */
    use(...args: IMiddlewareMap[]): UniversalCallBot;
}

/** Connect a UniversalCallBot to the Skype calling service. */
export class CallConnector implements ICallConnector, IBotStorage {

    /** 
     * Creates a new instnace of the ChatConnector.
     * @param settings (Optional) config params that let you specify the bots App ID & Password you were assigned in the Bot Frameworks developer portal. 
     */
    constructor(settings?: ICallConnectorSettings);

    /** Registers an Express or Restify style hook to listen for new messages. */
    listen(): (req: any, res: any) => void;

    /** Express or Resitify style middleware that verifies recieved messages are from the Bot Framework. */
    verifyBotFramework(): (req: any, res: any, next: any) => void;

    /** Called by the UniversalCallBot at registration time to register a handler for receiving incoming events from the calling service. */
    onEvent(handler: (event: IEvent, cb?: (err: Error) => void) => void): void;
    
    /** Called by the UniversalCallBot to deliver workflow actions to the service. */
    send(event: IEvent, done: (err: Error) => void): void;

    /** Reads in data from the Bot Frameworks state service. */
    getData(context: IBotStorageContext, callback: (err: Error, data: IBotStorageData) => void): void;

    /** Writes out data to the Bot Frameworks state service. */
    saveData(context: IBotStorageContext, data: IBotStorageData, callback?: (err: Error) => void): void;
}

/** Action builder class designed to simplify building [answer actions](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.iansweraction). */
export class AnswerAction implements IIsAction {
    /** Creates a new instance of the action builder. */
    constructor(session?: CallSession);
    
    /** The modality types the application will accept. If none are specified it assumes audio only. */
    acceptModalityTypes(types: string[]): AnswerAction;

    /** Returns the JSON object for the action. */
    toAction(): IAction;
}

/** Action builder class designed to simplify building [hangup actions](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.ihangupaction). */
export class HangupAction implements IIsAction {
    /** Creates a new instance of the action builder. */
    constructor(session?: CallSession);

    /** Returns the JSON object for the action. */
    toAction(): IAction;
}

/** Action builder class designed to simplify building [playPrompt actions](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.iplaypromptaction). */
export class PlayPromptAction implements IIsAction {
    /** Creates a new instance of the action builder. */
    constructor(session?: CallSession);
    
    /** List of prompts to play out with each single prompt object. */
    prompts(list: IPrompt[]|IIsPrompt[]): PlayPromptAction;

    /** Returns the JSON object for the action. */
    toAction(): IAction;

    /** Creates a text prompt that will be spoken to the user using TTS. */
    static text(session: CallSession, text: string|string[], ...args: any[]): PlayPromptAction;

    /** Creates a file prompt that will be played to the user. */
    static file(session: CallSession, uri: string): PlayPromptAction;
    
    /** Creates a prompt that plays silence to the user. */
    static silence(session: CallSession, time: number): PlayPromptAction;
}

/** Prompt builder class that simplifies building prompts for playPrompt action.  */
export class Prompt implements IIsPrompt {
    /** Creates a new instance of the prompt builder. */
    constructor(session?: CallSession);
    
    /** Text-To-Speech text to be played to Skype user. Either [value](#value) or [fileUri](#fileuri) must be specified. */
    value(text: string|string[], ...args: any[]): Prompt;

    /** HTTP of played media file. Supported formats are WMA or WAV. The file is limited to 512kb in size and cached by Skype Bot Platform for Calling. Either [value](#value) or [fileUri](#fileuri) must be specified. */
    fileUri(uri: string): Prompt;

    /** VoiceGender enum value. The default value is “female”. */
    voice(gender: string): Prompt;

    /** The Language enum value to use for Text-To-Speech. Only applicable if [value](#value) is text. The default value is “en-US”. Note, currently en-US is the only supported language. */
    culture(locale: string): Prompt;

    /** Any silence played out before [value](#value) is played. If [value](#value) is null, this field must be a valid > 0 value. */
    silenceLengthInMilliseconds(time: number): Prompt;

    /** Indicates whether to emphasize when tts'ing out. It's applicable only if [value](#value) is text. The default value is false. */
    emphasize(flag: boolean): Prompt;

    /** The SayAs enum value indicates whether to customize pronunciation during tts. It's applicable only if [value](#value) is text. */
    sayAs(type: string): Prompt;

    /** Returns the JSON object for the prompt. */
    toPrompt(): IPrompt;

    /** Creates a text prompt that will be spoken to the user using TTS. */
    static text(session: CallSession, text: string|string[], ...args: any[]): Prompt;

    /** Creates a file prompt that will be played to the user. */
    static file(session: CallSession, uri: string): Prompt;

    /** Creates a prompt that plays silence to the user. */
    static silence(session: CallSession, time: number): Prompt;
}

/** Action builder class designed to simplify building [recognize actions](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.irecognizeaction). */
export class RecognizeAction implements IIsAction {
    /** Creates a new instance of the action builder. */
    constructor(session?: CallSession);

    /** A prompt to be played before the recognition starts. */
    playPrompt(action: IAction|IIsAction): RecognizeAction;

    /** Indicates if Skype user is allowed to enter choice before the prompt finishes. The default value is true. */
    bargeInAllowed(flag: boolean): RecognizeAction;

    /** Culture is an enum indicating what culture the speech recognizer should use. The default value is “en-US”. Currently the only culture supported is en-US. */
    culture(locale: string): RecognizeAction;
    
    /** Maximum initial silence allowed before failing the operation from the time we start the recording. The default value is 5 seconds. */ 
    initialSilenceTimeoutInSeconds(time: number): RecognizeAction;
    
    /** Maximum allowed time between dial pad digits. The default value is 1 second. */
    interdigitTimeoutInSeconds(time: number): RecognizeAction;
    
    /** List of RecognitionOption objects dictating the recognizable choices. Choices can be speech or dial pad digit based. Either [collectDigits](#collectDigits) or [choices](#choices) must be specified, but not both. */
    choices(list: IRecognitionChoice[]): RecognizeAction;
    
    /** CollectDigits will result in collecting digits from Skype user dial pad as part of recognize. Either [collectDigits](#collectDigits) or [choices](#choices) must be specified, but not both. */
    collectDigits(options: ICollectDigits): RecognizeAction;
    
    /** Returns the JSON object for the action. */
    toAction(): IAction;
}

/** Action builder class designed to simplify building [record actions](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.irecordaction). */
export class RecordAction implements IIsAction {
    /** Creates a new instance of the action builder. */
    constructor(session?: CallSession);

    /** A prompt to be played before the recording. */
    playPrompt(action: IAction|IIsAction): RecordAction;

    /** Maximum duration of recording. The default value is 180 seconds. */
    maxDurationInSeconds(time: number): RecordAction;

    /** Maximum initial silence allowed before the recording is stopped. The default value is 5 seconds. */
    initialSilenceTimeoutInSeconds(time: number): RecordAction;

    /** Maximum silence allowed after the speech is detected. The default value is 5 seconds. */
    maxSilenceTimeoutInSeconds(time: number): RecordAction;

    /** The format expected for the recording. The RecordingFormat enum describes the supported values. The default value is “wma”. */
    recordingFormat(fmt: string): RecordAction;

    /** Indicates whether to play beep sound before starting a recording action. */
    playBeep(flag: boolean): RecordAction;

    /** Stop digits that user can press on dial pad to stop the recording. */
    stopTones(dtmf: string[]): RecordAction;
    
    /** Returns the JSON object for the action. */
    toAction(): IAction;
}

/** Action builder class designed to simplify building [reject actions](/en-us/node/builder/calling-reference/interfaces/_botbuilder_d_.irejectaction). */
export class RejectAction implements IIsAction {
    /** Creates a new instance of the action builder. */
    constructor(session?: CallSession);

    /** Returns the JSON object for the action. */
    toAction(): IAction;
}

