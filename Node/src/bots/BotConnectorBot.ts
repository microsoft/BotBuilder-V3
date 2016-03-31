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

export interface IBotConnectorOptions {
    endpoint?: string;
    appId?: string;
    appSecret?: string;
    subscriptionKey?: string;
    defaultFrom?: IChannelAccount;
    localizer?: ILocalizer;
    defaultDialogId?: string;
    defaultDialogArgs?: any;
    groupWelcomeMessage?: string;
    userWelcomeMessage?: string;
    goodbyeMessage?: string;
}

export interface IBotConnectorMessage extends IMessage {
    botUserData?: any;
    botConversationData?: any;
    botPerUserInConversationData?: any;
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

export class BotConnectorBot extends collection.DialogCollection {
    private options: IBotConnectorOptions = {
        endpoint: process.env['endpoint'] || 'https://api.botframework.com',
        appId: process.env['appId'] || '',
        appSecret: process.env['appSecret'] || '',
        subscriptionKey: process.env['subscriptionKey'] || '',
        defaultDialogId: '/'
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
            if (this.options.appId && this.options.appSecret) {
                if (req.headers && req.headers.hasOwnProperty('authorization')) {
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

    public listen(options?: IBotConnectorOptions): IMiddleware {
        this.configure(options);
        return (req: IRequest, res: IResponse) => {
            if (req.body) {
                this.processMessage(req.body, this.options.defaultDialogId, this.options.defaultDialogArgs, res);
            } else {
                var requestData = '';
                req.on('data', (chunk: string) => {
                    requestData += chunk
                });
                req.on('end', () => {
                    try {
                        var msg = JSON.parse(requestData);
                        this.processMessage(msg, this.options.defaultDialogId, this.options.defaultDialogArgs, res);
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
        var msg: IBotConnectorMessage = address;
        msg.type = 'Message';
        if (!msg.from) {
            msg.from = this.options.defaultFrom;
        }

        // Validate args
        if (!msg.to || !msg.from) {
            throw new Error('Invalid address passed to BotConnectorBot.beginDialog().');
        }
        if (!this.hasDialog(dialogId)) {
            throw new Error('Invalid dialog passed to BotConnectorBot.beginDialog().');
        }

        // Dispatch message
        this.processMessage(msg, dialogId, dialogArgs);
    }

    private processMessage(message: IBotConnectorMessage, dialogId: string, dialogArgs: any, res?: IResponse) {
        try {
            // Validate message
            if (!message || !message.type) {
                this.emit('error', new Error('Invalid Bot Framework Message'));
                return res.send(400);
            }

            // Dispatch messages
            this.emit(message.type, message);
            if (message.type == 'Message') {
                // Initialize session
                var ses = new BotConnectorSession({
                    localizer: this.options.localizer,
                    dialogs: this,
                    dialogId: dialogId,
                    dialogArgs: dialogArgs
                });
                ses.on('send', (message: IMessage) => {
                    // Compose reply
                    var reply: IBotConnectorMessage = message || {};
                    reply.botUserData = utils.clone(ses.userData);
                    reply.botConversationData = utils.clone(ses.conversationData);
                    reply.botPerUserInConversationData = utils.clone(ses.perUserInConversationData);
                    reply.botPerUserInConversationData[consts.Data.SessionState] = ses.sessionState;
                    if (reply.text && !reply.language) {
                        reply.language = ses.message.language;
                    }

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
                        this.post('/bot/v1.0/messages', reply, (err) => {
                            this.emit('error', err);
                        });
                    } else {
                        // Start a new conversation
                        reply.from = ses.message.from;
                        reply.to = ses.message.to;
                        this.emit('send', reply);
                        this.post('/bot/v1.0/messages', reply, (err) => {
                            this.emit('error', err);
                        });
                    }
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

                // Unpack data fields
                var sessionState: ISessionState;
                if (message.botUserData) {
                    ses.userData = message.botUserData;
                    delete message.botUserData;
                } else {
                    ses.userData = {};
                }
                if (message.botConversationData) {
                    ses.conversationData = message.botConversationData;
                    delete message.botConversationData;
                } else {
                    ses.conversationData = {};
                }
                if (message.botPerUserInConversationData) {
                    if (message.botPerUserInConversationData.hasOwnProperty(consts.Data.SessionState)) {
                        sessionState = message.botPerUserInConversationData[consts.Data.SessionState];
                        delete message.botPerUserInConversationData[consts.Data.SessionState];
                    }
                    ses.perUserInConversationData = message.botPerUserInConversationData;
                    delete message.botPerUserInConversationData;
                } else {
                    ses.perUserInConversationData = {};
                }

                // Dispatch message
                ses.dispatch(sessionState, message);
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

    protected post(path: string, body: any, callback?: (error: any) => void): void {
        var settings = this.options;
        var options: request.Options = {
            url: settings.endpoint + path,
            body: body
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
        request.post(options, callback);
    }
}

export class BotConnectorSession extends session.Session {
    public conversationData: any;
    public perUserInConversationData: any;
}
