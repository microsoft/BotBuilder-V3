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
import consts = require('../consts');
import utils = require('../utils');
import request = require('request');
import storage = require('../storage/Storage');
import bcStorage = require('../storage/BotConnectorStorage');
import uuid = require('node-uuid');

export interface IBotConnectorOptions {
    endpoint?: string;
    appId?: string;
    appSecret?: string;
    defaultFrom?: IChannelAccount;
    userStore?: storage.IStorage;
    conversationStore?: storage.IStorage;
    perUserInConversationStore?: storage.IStorage;
    localizer?: ILocalizer;
    minSendDelay?: number;
    defaultDialogId?: string;
    defaultDialogArgs?: any;
    groupWelcomeMessage?: string;
    userWelcomeMessage?: string;
    goodbyeMessage?: string;
}

/** Express or Restify Request object. */
interface IRequest {
    body: any;
    headers: {
        [name: string]: string;
    };
    on(event: string, ...args: any[]): void;
}

/** Express or Restify Response object. */
interface IResponse {
    send(status: number, body?: any): void;
    send(body: any): void;
}

/** Express or Restify Middleware Function. */
interface IMiddleware {
    (req: IRequest, res: IResponse, next?: Function): void;
}

interface IStoredData {
    userData: any;
    conversationData: any;
    perUserConversationData: any;
}

interface IDispatchOptions {
    dialogId?: string;
    dialogArgs?: any;
    replyToDialogId?: string;
}

export class BotConnectorBot extends collection.DialogCollection {
    private options: IBotConnectorOptions = {
        endpoint: process.env['endpoint'] || 'https://api.botframework.com',
        appId: process.env['appId'] || '',
        appSecret: process.env['appSecret'] || '',
        defaultDialogId: '/',
        minSendDelay: 1000
    }

    constructor(options?: IBotConnectorOptions) {
        super();
        this.configure(options);
    }

    public configure(options: IBotConnectorOptions) {
        if (options) {
            for (var key in options) {
                if (options.hasOwnProperty(key)) {
                    (<any>this.options)[key] = (<any>options)[key];
                }
            }
        }
    }

    public verifyBotFramework(options?: IBotConnectorOptions): IMiddleware {
        this.configure(options);
        return (req: IRequest, res: IResponse, next: Function) => {
            // Check authorization
            var authorized: boolean;
            var isSecure = req.headers['x-forwarded-proto'] === 'https' || req.headers['x-arr-ssl'];
            if (isSecure && this.options.appId && this.options.appSecret) {
                if (req.headers.hasOwnProperty('authorization')) {
                    var tmp = req.headers['authorization'].split(' ');
                    var buf = new Buffer(tmp[1], 'base64');
                    var cred = buf.toString().split(':');
                    if (cred[0] == this.options.appId && cred[1] == this.options.appSecret) {
                        authorized = true;
                    } else {
                        authorized = false;
                    }
                } else {
                    authorized = false;
                }
            } else {
                authorized = true;
            }
            if (authorized) {
                next();
            } else {
                res.send(403);
            }
        };
    }

    public listen(dialogId?: string, dialogArgs?: any): IMiddleware {
        return (req: IRequest, res: IResponse) => {
            if (req.body) {
                this.dispatchMessage(null, req.body, { dialogId: dialogId, dialogArgs: dialogArgs }, res);
            } else {
                var requestData = '';
                req.on('data', (chunk: string) => {
                    requestData += chunk
                });
                req.on('end', () => {
                    try {
                        var msg = JSON.parse(requestData);
                        this.dispatchMessage(null, msg, { dialogId: dialogId, dialogArgs: dialogArgs }, res);
                    } catch (e) {
                        this.emit('error', new Error('Invalid Bot Framework Message'));
                        res.send(400);
                    }
                });
            }
        };
    }

    public beginDialog(address: IBeginDialogAddress, dialogId: string, dialogArgs?: any): void {
        // Fixup address fields
        var message: IBotConnectorMessage = address;
        message.type = 'Message';
        if (!message.from) {
            message.from = this.options.defaultFrom;
        }

        // Validate args
        if (!message.to || !message.from) {
            throw new Error('Invalid address passed to BotConnectorBot.beginDialog().');
        }
        if (!this.hasDialog(dialogId)) {
            throw new Error('Invalid dialog passed to BotConnectorBot.beginDialog().');
        }

        // Dispatch message
        this.dispatchMessage(message.to.id, message, { dialogId: dialogId, dialogArgs: dialogArgs });
    }
    
    /** IN DEVELOPMENT
    public continueDialog(message: IBotConnectorMessage, replyToDialogId?: string): void {
        // Validate args
        message.type = 'Message';
        if (!message.from || !message.conversationId) {
            throw new Error('Invalid message passed to BotConnectorBot.continueDialog().');
        }
        
        // Calculate storage paths
        var userId = message.from.id;
        var botPath = '/' + this.options.appId;
        var userPath = botPath + '/users/' + userId;
        var convoPath = botPath + '/conversations/' + message.conversationId;
        var perUserConvoPath = botPath + '/conversations/' + message.conversationId + '/users/' + userId;

        // Load botData fields from connector
        // - We'll optimize for the use of custom stores. If custom stores are being
        //   we need to at least retrieve the botConversationData field which contains
        //   the sessionId.
        var connector = new bcStorage.BotConnectorStorage(<any>this.options);
        var ops = 3;
        function load(id: string, field: string) {
            connector.get(id, (err, item) => {
                if (!err) {
                    (<any>message)[field] = item;
                    if (--ops == 0) {
                        this.dispatchMessage(null, message, { replyToDialogId: replyToDialogId });
                    }
                } else {
                    this.emit('error', err, message);
                }
            });
        }
        if (!this.options.userStore) {
            load(userPath, 'botUserData');
        } else {
            message.botUserData = {};
            ops--;
        }
        if (!this.options.perUserInConversationStore) {
            load(perUserConvoPath, 'botPerUserInConversationData');
        } else {
            message.botPerUserInConversationData = {};
            ops--;
        }
        load(convoPath, 'botConversationData');
    }
    */
    
    private dispatchMessage(userId: string, message: IBotConnectorMessage, options: IDispatchOptions, res?: IResponse) {
        try {
            // Validate message
            if (!message || !message.type) {
                this.emit('error', new Error('Invalid Bot Framework Message'));
                return  res ? res.send(400) : null;
            }
            if (!userId) {
                if (message.from && message.from.id) {
                    userId = message.from.id;
                } else {
                    this.emit('error', new Error('Invalid Bot Framework Message'));
                    return  res ? res.send(400) : null;
                }
            }
            
            // Generate a session ID
            // - We're storing this at the conversation level because we're using it in
            //   place of the Bot Connectors conversationId for storage purposes. We just
            //   call it a session ID to avoid confusion.
            var sessionId: string;
            if (message.botConversationData && message.botConversationData[consts.Data.SessionId]) {
                sessionId = message.botConversationData[consts.Data.SessionId];
            } else {
                sessionId = uuid.v1();
                message.botConversationData = message.botConversationData || {};
                message.botConversationData[consts.Data.SessionId] = sessionId;
            }

            // Dispatch messages
            this.emit(message.type, message);
            if (message.type == 'Message') {
                // Initialize session
                var ses = new BotConnectorSession({
                    localizer: this.options.localizer,
                    minSendDelay: this.options.minSendDelay,
                    dialogs: this,
                    dialogId: options.dialogId || this.options.defaultDialogId,
                    dialogArgs: options.dialogArgs || this.options.defaultDialogArgs
                });
                ses.on('send', (reply: IBotConnectorMessage) => {
                    // Compose reply
                    reply = reply || {};
                    reply.botConversationData = message.botConversationData;    // <-- Ensures we save the session ID
                    if (reply.text && !reply.language && message.language) {
                        reply.language = message.language;
                    }

                    // Save data
                    var data: IStoredData = {
                        userData: ses.userData,
                        conversationData: ses.conversationData,
                        perUserConversationData: ses.perUserInConversationData
                    }
                    data.perUserConversationData[consts.Data.SessionState] = ses.sessionState;
                    this.saveData(userId, sessionId, data, reply, (err) =>{
                        // Check for emulator
                        var settings = ses.message.to.channelId == 'emulator' ? { endpoint: 'http://localhost:9000' } : this.options;
                        
                        // Send message
                        if (res) {
                            this.emit('reply', reply);
                            res.send(200, reply);
                            res = null;
                        } else if (ses.message.conversationId) {
                            // Post an additional reply
                            reply.from = ses.message.to;
                            reply.to = ses.message.replyTo ? ses.message.replyTo : ses.message.from;
                            reply.replyToMessageId = ses.message.id;
                            reply.conversationId = ses.message.conversationId;
                            reply.channelConversationId = ses.message.channelConversationId;
                            reply.channelMessageId = ses.message.channelMessageId;
                            reply.participants = ses.message.participants;
                            reply.totalParticipants = ses.message.totalParticipants;
                            this.emit('reply', reply);
                            post(settings, '/bot/v1.0/messages', reply, (err) => {
                                if (err) {
                                    this.emit('error', err);
                                }
                            });
                        } else {
                            // Start a new conversation
                            reply.from = ses.message.from;
                            reply.to = ses.message.to;
                            this.emit('send', reply);
                            post(settings, '/bot/v1.0/messages', reply, (err) => {
                                if (err) {
                                    this.emit('error', err);
                                }
                            });
                        }
                    });
                });
                ses.on('error', (err: Error) => {
                    this.emit('error', err, ses.message);
                    if (res) {
                        res.send(500);
                    }
                });
                ses.on('quit', () => {
                    this.emit('quit', ses.message);
                });

                // Load data from storage
                this.getData(userId, sessionId, message, (err, data) => {
                    if (!err) {
                        // Initialize session data
                        var sessionState: ISessionState;
                        ses.userData = data.userData || {};
                        ses.conversationData = data.conversationData || {};
                        ses.perUserInConversationData = data.perUserConversationData || {};
                        if (ses.perUserInConversationData.hasOwnProperty(consts.Data.SessionState)) {
                            sessionState = ses.perUserInConversationData[consts.Data.SessionState];
                            delete ses.perUserInConversationData[consts.Data.SessionState];    
                        }
                        
                        // Dispatch message
                        if (options.replyToDialogId) {
                            // Enforce that the required dialog is active
                            if (sessionState && sessionState.callstack[sessionState.callstack.length - 1].id == options.replyToDialogId) {
                                ses.dispatch(sessionState, message);
                            }                            
                        } else {
                            ses.dispatch(sessionState, message);
                        }
                    } else {
                        this.emit('error', err, message);
                    }
                });
            } else if (res) {
                var msg: string;
                switch (message.type) {
                    case "botAddedToConversation":
                        msg = this.options.groupWelcomeMessage;
                        break;
                    case "userAddedToConversation":
                        msg = this.options.userWelcomeMessage;
                        break;
                    case "endOfConversation":
                        msg = this.options.goodbyeMessage;
                        break;
                }
                res.send(msg ? { type: message.type, text: msg } : {});
            }
        } catch (e) {
            this.emit('error', e instanceof Error ? e : new Error(e.toString()));
            res.send(500);
        }
    }
    
    private getData(userId: string, sessionId: string, msg: IBotConnectorMessage, callback?: (err: Error, data: IStoredData) => void): void {
        // Calculate storage paths
        var botPath = '/' + this.options.appId;
        var userPath = botPath + '/users/' + userId;
        var convoPath = botPath + '/conversations/' + sessionId;
        var perUserConvoPath = botPath + '/conversations/' + sessionId + '/users/' + userId;
        
        // Load data
        var ops = 3;
        var data = <IStoredData>{};
        function load(id: string, field: string, store: storage.IStorage, botData: any) {
            (<any>data)[field] = botData;
            if (store) {
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
        load(userPath, 'userData', this.options.userStore, msg.botUserData);
        load(convoPath, 'conversationData', this.options.conversationStore, msg.botConversationData);
        load(perUserConvoPath, 'perUserConversationData', this.options.perUserInConversationStore, msg.botPerUserInConversationData);
    }
    
    private saveData(userId: string, sessionId: string, data: IStoredData, msg: IBotConnectorMessage, callback: (err: Error) => void): void {
        // Calculate storage paths
        var botPath = '/' + this.options.appId;
        var userPath = botPath + '/users/' + userId;
        var convoPath = botPath + '/conversations/' + sessionId;
        var perUserConvoPath = botPath + '/conversations/' + sessionId + '/users/' + userId;
        
        // Save data
        var ops = 3;
        function save(id: string, field: string, store: storage.IStorage, botData: any) {
            if (store) {
                store.save(id, botData, (err) => {
                    if (callback) {
                        if (!err && --ops == 0) {
                            callback(null);
                        } else {
                            callback(err);
                            callback = null;
                        }
                    }
                });
            } else {
                (<any>msg)[field] = botData;
                if (callback && --ops == 0) {
                    callback(null);
                }
            }
        }
        save(userPath, 'botUserData', this.options.userStore, data.userData);
        save(convoPath, 'botConversationData', this.options.conversationStore, data.conversationData);
        save(perUserConvoPath, 'botPerUserInConversationData', this.options.perUserInConversationStore, data.perUserConversationData);
    }
}

export class BotConnectorSession extends session.Session {
    public conversationData: any;
    public perUserInConversationData: any;
}

function post(settings: IBotConnectorOptions, path: string, body: any, callback?: (error: any) => void): void {
    var options: request.Options = {
        method: 'POST',
        url: settings.endpoint + path,
        body: body,
        json: true
    };
    if (settings.appId && settings.appSecret) {
        options.auth = {
            username: settings.appId,
            password: settings.appSecret
        };
        options.headers = {
            'Ocp-Apim-Subscription-Key': settings.appSecret
        };
    }
    request(options, callback);
}