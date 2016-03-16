import collection = require('./DialogCollection');
import session = require('./Session');
import storage = require('./Storage');
import botkit = require('skype-botkit');

export interface ISkypeBotOptions {
    userStore?: storage.IStorage;
    sessionStore?: storage.IStorage;
    maxSessionAge?: number;
    localizer?: ILocalizer;
    defaultDialogId?: string;
    defaultDialogArgs?: any;
    contactAddedmessage?: string;
    botAddedMessage?: string;
    botRemovedMessage?: string;
    memberAddedMessage?: string;
    memberRemovedMessage?: string;
}

export class SkypeBot extends collection.DialogCollection {
    private options: ISkypeBotOptions = {
        maxSessionAge: 14400000,    // <-- default max session age of 4 hours
        defaultDialogId: '/'
    };

    constructor(protected botService: botkit.BotService, options?: ISkypeBotOptions) {
        super();
        this.configure(options);
        var events = 'message|personalMessage|groupMessage|attachment|threadBotAdded|threadAddMember|threadBotRemoved|threadRemoveMember|contactAdded|threadTopicUpdated|threadHistoryDisclosedUpdate'.split('|');
        events.forEach((value) => {
            botService.on(value, (bot, data) => {
                this.emit(value, bot, data);
            });
        });
    }

    public configure(options: ISkypeBotOptions) {
        if (options) {
            for (var key in options) {
                if (options.hasOwnProperty(key)) {
                    this.options[key] = options[key];
                }
            }
        }
    }

    public beginDialog(address: IBeginDialogAddress, dialogId: string, dialogArgs?: any): void {
        // Validate args
        if (!address.to) {
            throw new Error('Invalid address passed to SkypeBot.beginDialog().');
        }
        if (!this.hasDialog(dialogId)) {
            throw new Error('Invalid dialog passed to SkypeBot.beginDialog().');
        }

        // Dispatch message
        this.dispatchMessage(null, this.toSkypeMessage(address), dialogId, dialogArgs);
    }

    private handleEvent(event: string, bot: botkit.Bot, data: any) {
        var onError = (err) => {
            this.emit('error', err, data);
        };

        switch (event) {
            case 'personalMessage':
                this.dispatchMessage(bot, data, this.options.defaultDialogId, this.options.defaultDialogArgs);
                break;
            case 'threadBotAdded':
                if (this.options.botAddedMessage) {
                    bot.reply(this.options.botAddedMessage, onError);
                }
                break;
            case 'threadAddMember':
                if (this.options.memberAddedMessage) {
                    bot.reply(this.options.memberAddedMessage, onError);
                }
                break;
            case 'threadBotRemoved':
                if (this.options.botRemovedMessage) {
                    bot.reply(this.options.botRemovedMessage, onError);
                }
                break;
            case 'threadRemoveMember':
                if (this.options.memberRemovedMessage) {
                    bot.reply(this.options.memberRemovedMessage, onError);
                }
                break;
            case 'contactAdded':
                if (this.options.contactAddedmessage) {
                    bot.reply(this.options.contactAddedmessage, onError);
                }
                break;
        }
    }

    private dispatchMessage(bot: botkit.Bot, data: botkit.IMessage, dialogId: string, dialogArgs: any) {
        var onError = (err) => {
            this.emit('error', err, data);
        };
 
        // Initialize session
        var ses = new session.Session({
            localizer: this.options.localizer,
            dialogs: this,
            dialogId: dialogId,
            dialogArgs: dialogArgs
        });
        ses.on('send', (reply: IMessage) => {
            this.saveData(msg.from.address, ses.userData, ses.sessionState, () => {
                // If we have no message text then we're just saving state.
                if (reply && reply.text) {
                    // Do we have a bot?
                    var skypeReply = this.toSkypeMessage(reply);
                    if (bot) {
                        // Check for a different TO address
                        if (skypeReply.to && skypeReply.to != data.from) {
                            this.emit('send', skypeReply);
                            bot.send(skypeReply.to, skypeReply.content, onError);
                        } else {
                            this.emit('reply', skypeReply);
                            bot.reply(skypeReply.content, onError);
                        }
                    } else {
                        skypeReply.to = ses.message.to.address;
                        this.emit('send', skypeReply);
                        this.botService.send(skypeReply.to, skypeReply.content, onError);
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

        // Load data and dispatch message
        var msg = this.fromSkypeMessage(data);
        this.getData(msg.from.address, (userData, sessionState) => {
            ses.userData = userData || {};
            ses.dispatch(sessionState, msg);
        });
    }

    private getData(userId: string, callback: (userData: any, sessionState: ISessionState) => void) {
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
                    callback(userData, sessionState);
                }
            } else {
                this.emit('error', err);
            }
        });
        this.options.sessionStore.get(userId, (err, data: ISessionState) => {
            if (!err) {
                if (data && (new Date().getTime() - data.lastAccess) < this.options.maxSessionAge) {
                    sessionState = data;
                } 
                if (--ops == 0) {
                    callback(userData, sessionState);
                }
            } else {
                this.emit('error', err);
            }
        });
    }

    private saveData(userId: string, userData: any, sessionState: ISessionState, callback: Function) {
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

    private fromSkypeMessage(msg: botkit.IMessage): IMessage {
        return {
            type: msg.type,
            id: msg.messageId.toString(),
            from: {
                channelId: 'skype',
                address: msg.from
            },
            to: {
                channelId: 'skype',
                address: msg.to
            },
            text: msg.content,
            channelData: msg
        };
    }

    private toSkypeMessage(msg: IMessage): botkit.IMessage {
        return {
            type: msg.type,
            from: msg.from ? msg.from.address : '',
            to: msg.to ? msg.to.address : '',
            content: msg.text,
            messageId: msg.id ? Number(msg.id) : Number.NaN,
            contentType: "RichText",
            eventTime: msg.channelData ? msg.channelData.eventTime : new Date().getTime()
        };
    }
} 
