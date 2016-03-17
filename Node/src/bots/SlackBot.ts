import collection = require('../dialogs/DialogCollection');
import session = require('../Session');
import storage = require('../storage/Storage');

interface ISlackMessage {
    type: string;
    subtype?: string;
    channel: string;
    user: string;
    text: string;
    attachments?: ISlackAttachment[];
    ts: string;
}

interface ISlackAttachment {
}

declare class BotKitController  {
    on(event: string, listener: Function): void;
} 

declare class Bot {
    reply(message: ISlackMessage, text: string): void;
    say(message: ISlackMessage, cb: (err: Error) => void): void;
}

export interface ISlackBotOptions {
    userStore?: storage.IStorage;
    sessionStore?: storage.IStorage;
    maxSessionAge?: number;
    localizer?: ILocalizer;
    defaultDialogId?: string;
    defaultDialogArgs?: any;
}

export class SlackBot extends collection.DialogCollection {
    protected options: ISlackBotOptions = {
        maxSessionAge: 14400000,    // <-- default max session age of 4 hours
        defaultDialogId: '/'
    };

    constructor(protected controller: BotKitController, protected bot?: Bot, options?: ISlackBotOptions) {
        super();
        this.configure(options);
        controller.on('direct_message', (bot: Bot, msg: ISlackMessage) => {
            this.emit('message', msg);
            this.dispatchMessage(bot, msg, this.options.defaultDialogId, this.options.defaultDialogArgs);
        });
    }

    public configure(options: ISlackBotOptions): this {
        if (options) {
            for (var key in options) {
                if (options.hasOwnProperty(key)) {
                    (<any>this.options)[key] = (<any>options)[key];
                }
            }
        }
        return this;
    }

    public beginDialog(address: IBeginDialogAddress, dialogId: string, dialogArgs?: any): void {
        // Validate args
        if (!this.bot) {
            throw new Error('Spawned BotKit Bot not passed to constructor.');
        }
        if (!address.to) {
            throw new Error('Invalid address passed to SlackBot.beginDialog().');
        }
        if (!this.hasDialog(dialogId)) {
            throw new Error('Invalid dialog passed to SlackBot.beginDialog().');
        }

        // Dispatch message
        this.dispatchMessage(null, this.toSlackMessage(address), dialogId, dialogArgs);
    }

    private dispatchMessage(bot: Bot, data: ISlackMessage, dialogId: string, dialogArgs: any) {
        var onError = (err: Error) => {
            this.emit('error', err, data);
        };

        // Initialize session
        var ses = new session.Session({
            localizer: this.options.localizer,
            dialogs: this,
            dialogId: this.options.defaultDialogId,
            dialogArgs: this.options.defaultDialogArgs
        });
        ses.on('send', (reply: IMessage) => {
            this.saveData(message.from.address, ses.userData, ses.sessionState, () => {
                // If we have no message text then we're just saving state.
                if (reply && reply.text) {
                    var slackReply = this.toSlackMessage(reply);
                    if (bot) {
                        // Check for a different TO address
                        if (slackReply.user && slackReply.user != data.user) {
                            this.emit('send', slackReply);
                            bot.say(slackReply, onError);
                        } else {
                            this.emit('reply', slackReply);
                            bot.reply(data, slackReply.text);
                        }
                    } else {
                        slackReply.user = ses.message.to.address;
                        this.emit('send', slackReply);
                        this.bot.say(slackReply, onError);
                    }
                }
            });
        });
        ses.on('error', (err: Error) => {
            this.emit('error', err, data);
        });
        ses.on('quit', () => {
            this.emit('quit', data);
        });

        // Dispatch message
        var message = this.fromSlackMessage(data);
        this.getData(message.from.address, (err, userData, sessionState) => {
            ses.userData = userData || {};
            ses.dispatch(sessionState, message);
        });
    }

    private getData(userId: string, callback: (err: Error, userData: any, sessionState: ISessionState) => void) {
        // Ensure stores specified
        if (!this.options.userStore) {
            this.options.userStore = new storage.MemoryStorage();
        }
        if (!this.options.sessionStore) {
            this.options.sessionStore = new storage.MemoryStorage();
        }

        // Load data
        var ops = 2;
        var userData: any, sessionState: ISessionState;
        this.options.userStore.get(userId, (err, data) => {
            if (!err) {
                userData = data;
                if (--ops == 0) {
                    callback(null, userData, sessionState);
                }
            } else {
                callback(err, null, null);
            }
        });
        this.options.sessionStore.get(userId, (err: Error, data: ISessionState) => {
            if (!err) {
                if (data && (new Date().getTime() - data.lastAccess) < this.options.maxSessionAge) {
                    sessionState = data;
                }
                if (--ops == 0) {
                    callback(null, userData, sessionState);
                }
            } else {
                callback(err, null, null);
            }
        });
    }

    private saveData(userId: string, userData: any, sessionState: ISessionState, callback: (err: Error) => void) {
        var ops = 2;
        function onComplete(err: Error) {
            if (!err) {
                if (--ops == 0) {
                    callback(null);
                }
            } else {
                callback(err);
            }
        }
        this.options.userStore.save(userId, userData, onComplete);
        this.options.sessionStore.save(userId, sessionState, onComplete);
    }

    private fromSlackMessage(msg: ISlackMessage): IMessage {
        return {
            type: msg.type,
            id: msg.ts,
            text: msg.text,
            from: {
                channelId: 'slack',
                address: msg.user
            },
            channelConversationId: msg.channel,
            channelData: msg
        };
    }

    private toSlackMessage(msg: IMessage): ISlackMessage {
        return {
            type: msg.type,
            ts: msg.id,
            text: msg.text,
            user: msg.to ? msg.to.address : msg.from.address,
            channel: msg.channelConversationId
        };
    }
}