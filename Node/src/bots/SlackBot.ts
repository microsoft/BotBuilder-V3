import collection = require('../dialogs/DialogCollection');
import session = require('../Session');
import storage = require('../storage/Storage');
import consts = require('../consts');
import utils = require('../utils');

interface ISlackMessage {
    team: string;
    event: string;
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
    storage: {
        channels: IBotKitStore,
        teams: IBotKitStore,
        users: IBotKitStore
    };
}

interface IBotKitStore {
    get(id: string, cb: (err: Error, item: IBotKitStoreItem) => void): void;
    save(item: IBotKitStoreItem, cb: (err: Error) => void): void;
}

interface IBotKitStoreItem {
    id: string;
}

interface IStoredData {
    teamData: IBotKitStoreItem;
    channelData: IBotKitStoreItem;
    userData: IBotKitStoreItem;
}

declare class Bot {
    reply(message: ISlackMessage, text: string): void;
    say(message: ISlackMessage, cb: (err: Error) => void): void;
    identifyTeam(cb: (err: Error, teamId: string) => void): void;
}

export interface ISlackBotOptions {
    maxSessionAge?: number;
    localizer?: ILocalizer;
    defaultDialogId?: string;
    defaultDialogArgs?: any;
}

export interface ISlackBeginDialogAddress {
    team?: string;
    user?: string;
    channel?: string;
    text?: string;
}

export class SlackBot extends collection.DialogCollection {
    protected options: ISlackBotOptions = {
        maxSessionAge: 14400000,    // <-- default max session age of 4 hours
        defaultDialogId: '/'
    };

    constructor(protected controller: BotKitController, protected bot: Bot, options?: ISlackBotOptions) {
        super();
        this.configure(options);
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
    
    public listen(types: string[], dialogId?: string, dialogArgs?: any): this {
        types.forEach((type) => {
            this.controller.on(type, (bot: Bot, msg: ISlackMessage) => {
                bot.identifyTeam((err, teamId) => {
                    msg.team = teamId;
                    this.emit(type, msg);
                    this.dispatchMessage(bot, msg, dialogId || this.options.defaultDialogId, dialogArgs || this.options.defaultDialogArgs);
                });
            });
        });
        return this;
    }

    public beginDialog(address: ISlackBeginDialogAddress, dialogId: string, dialogArgs?: any): this {
        // Validate args
        if (!address.user && !address.channel) {
            throw new Error('Invalid address passed to SlackBot.beginDialog().');
        }
        if (!this.hasDialog(dialogId)) {
            throw new Error('Invalid dialog passed to SlackBot.beginDialog().');
        }
        
        // Dispatch message
        this.dispatchMessage(null, <ISlackMessage>address, dialogId, dialogArgs);
        return this;
    }

    private dispatchMessage(bot: Bot, msg: ISlackMessage, dialogId: string, dialogArgs: any) {
        var onError = (err: Error) => {
            this.emit('error', err, msg);
        };

        // Initialize session
        var ses = new SlackSession({
            localizer: this.options.localizer,
            dialogs: this,
            dialogId: this.options.defaultDialogId,
            dialogArgs: this.options.defaultDialogArgs
        });
        ses.on('send', (reply: IMessage) => {
            // Clone data fields & store session state
            var teamData = ses.teamData && ses.teamData.id ? utils.clone(ses.teamData) : null;
            var channelData = ses.channelData && ses.channelData.id ? utils.clone(ses.channelData) : null;
            var userData = ses.userData && ses.userData.id ? utils.clone(ses.userData) : null;
            if (channelData) {
                channelData[consts.Data.SessionState] = ses.sessionState;
            }
            
            // Save data
            this.saveData(teamData, channelData, userData, () => {
                // If we have no message text then we're just saving state.
                if (reply && (reply.text || reply.channelData)) {
                    var slackReply = this.toSlackMessage(reply);
                    if (bot) {
                        // Check for a different TO address
                        if (slackReply.user && slackReply.user != msg.user) {
                            this.emit('send', slackReply);
                            bot.say(slackReply, onError);
                        } else {
                            this.emit('reply', slackReply);
                            bot.reply(msg, slackReply.text);
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
            this.emit('error', err, msg);
        });
        ses.on('quit', () => {
            this.emit('quit', msg);
        });

        // Load data from storage
        var sessionState: ISessionState;
        var message = this.fromSlackMessage(msg);
        this.getData(msg, (err, data) => {
            if (!err) {
                // Init data
                if (!data.teamData && msg.team) {
                    data.teamData = { id: msg.team };
                }
                if (!data.channelData && msg.channel) {
                    data.channelData = { id: msg.channel };
                }
                if (!data.userData && msg.user) {
                    data.userData = { id: msg.user };
                }
                
                // Unpack session state
                if (data.channelData && data.channelData.hasOwnProperty(consts.Data.SessionState)) {
                    sessionState = (<any>data.channelData)[consts.Data.SessionState];
                    delete (<any>data.channelData)[consts.Data.SessionState];
                }
               
                // Dispatch message
                ses.teamData = data.teamData;
                ses.channelData = data.channelData;
                ses.userData = data.userData;
                ses.dispatch(sessionState, message);
            } else {
                this.emit('error', err, msg);
            }
        });
    }

    private getData(msg: ISlackMessage, callback: (err: Error, data: IStoredData) => void) {
        var ops = 3;
        var data = <IStoredData>{};
        function load(store: IBotKitStore, id: string, field: string) {
            (<any>data)[field] = null;
            if (id && id.length > 0) {
                store.get(id, (err, item) => {
                    if (callback) {
                        if (!err) {
                            (<any>data)[field] = item;
                            if (--ops == 0) {
                                callback(null, data);
                            }
                        } else {
                            callback(err, null);
                            callback = null;
                        }
                    }
                });
            } else if (callback && --ops == 0) {
                callback(null, data);
            }
        }
        load(this.controller.storage.teams, msg.team, 'teamData');
        load(this.controller.storage.channels, msg.channel, 'channelData');
        load(this.controller.storage.users, msg.user, 'userData');
    }

    private saveData(teamData: IBotKitStoreItem, channelData: IBotKitStoreItem, userData: IBotKitStoreItem, callback: (err: Error) => void) {
        var ops = 3;
        function save(store: IBotKitStore, data: IBotKitStoreItem) {
            if (data) {
                store.save(data, (err) => {
                    if (callback) {
                        if (!err && --ops == 0) {
                            callback(null);
                        } else {
                            callback(err);
                            callback = null;
                        }
                    }
                });
            } else if (callback && --ops == 0) {
                callback(null);
            }
        }
        save(this.controller.storage.teams, teamData);
        save(this.controller.storage.channels, channelData);
        save(this.controller.storage.users, userData);
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
        return msg.channelData || {
            event:'direct_message',
            type: msg.type,
            ts: msg.id,
            text: msg.text,
            user: msg.to ? msg.to.address : (msg.from ? msg.from.address : null),
            channel: msg.channelConversationId
        };
    }
}

export class SlackSession extends session.Session {
    public teamData: any;
    public channelData: any;
    
    public escapeText(text: string): string {
        if (text) {
            text = text.replace('&', '&amp;');
            text = text.replace('<', '&lt;');
            text = text.replace('>', '&gt;');
        }
        return text;
    }
    
    public unescapeText(text: string): string {
        if (text) {
            text = text.replace('&amp;', '&');
            text = text.replace('&lt;', '<');
            text = text.replace('&gt;', '>');
        }
        return text;
    }
    
    
}
