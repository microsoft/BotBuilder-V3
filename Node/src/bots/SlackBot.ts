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
    ambientMentionDuration?: number;
}

export interface ISlackBeginDialogAddress {
    team?: string;
    user?: string;
    channel?: string;
    text?: string;
}

export class SlackBot extends collection.DialogCollection {
    private options: ISlackBotOptions = {
        maxSessionAge: 14400000,        // <-- default max session age of 4 hours
        defaultDialogId: '/',
        ambientMentionDuration: 300000  // <-- default duration of 5 minutes
    };
    
    constructor(private controller: BotKitController, private bot: Bot, options?: ISlackBotOptions) {
        super();
        this.configure(options);
        ['message_received','bot_channel_join','user_channel_join','bot_group_join','user_group_join'].forEach((type) => {
            this.controller.on(type, (bot: Bot, msg: ISlackMessage) => {
               this.emit(type, bot, msg); 
            });
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
    
    public listen(types: string[], dialogId?: string, dialogArgs?: any): this {
        dialogId = dialogId || this.options.defaultDialogId;
        dialogArgs = dialogArgs || this.options.defaultDialogArgs;
        types.forEach((type) => {
            this.controller.on(type, (bot: Bot, msg: ISlackMessage) => {
                bot.identifyTeam((err, teamId) => {
                    msg.team = teamId;
                    this.dispatchMessage(bot, msg, dialogId, dialogArgs);
                });
            });
        });
        return this;
    }
    
    public listenForMentions(dialogId?: string, dialogArgs?: any): this {
        var sessions: { [key: string]: ISessionState; } = {};
        var dispatch = (bot: Bot, msg: ISlackMessage, ss?: ISessionState) => {
            bot.identifyTeam((err, teamId) => {
                msg.team = teamId;
                this.dispatchMessage(bot, msg, dialogId, dialogArgs, ss);
            });
        };
        
        dialogId = dialogId || this.options.defaultDialogId;
        dialogArgs = dialogArgs || this.options.defaultDialogArgs;
        this.controller.on('direct_message', (bot: Bot, msg: ISlackMessage) => {
            dispatch(bot, msg);
        });
        ['direct_mention','mention'].forEach((type) => {
            this.controller.on(type, (bot: Bot, msg: ISlackMessage) => {
                // Create a new session
                var key = msg.channel + ':' + msg.user;
                var ss = sessions[key] = { callstack: <any>[], lastAccess: new Date().getTime() };
                dispatch(bot, msg, ss);
            });
        });
        this.controller.on('ambient', (bot: Bot, msg: ISlackMessage) => {
            // Conditionally dispatch the message
            var key = msg.channel + ':' + msg.user;
            if (sessions.hasOwnProperty(key)) {
                // Validate session
                var ss = sessions[key];
                if (ss.callstack && ss.callstack.length > 0 && (new Date().getTime() - ss.lastAccess) <= this.options.ambientMentionDuration) {
                    dispatch(bot, msg, ss);
                } else {
                    delete sessions[key];
                }
            }
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

    private dispatchMessage(bot: Bot, msg: ISlackMessage, dialogId: string, dialogArgs: any, smartState?: ISessionState) {
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
            if (channelData && !smartState) {
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
                if (smartState) {
                    sessionState = smartState;
                } else if (data.channelData && data.channelData.hasOwnProperty(consts.Data.SessionState)) {
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
            text = text.replace(/&/g, '&amp;');
            text = text.replace(/</g, '&lt;');
            text = text.replace(/>/g, '&gt;');
        }
        return text;
    }

    public unescapeText(text: string): string {
        if (text) {
            text = text.replace(/&amp;/g, '&');
            text = text.replace(/&lt;/g, '<');
            text = text.replace(/&gt;/g, '>');
        }
        return text;
    }
}
