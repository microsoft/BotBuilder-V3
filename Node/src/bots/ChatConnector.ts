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

import ub = require('./UniversalBot');
import bs = require('../storage/BotStorage');
import events = require('events');
import request = require('request');
import async = require('async');
import url = require('url');
import http = require('http');
import utils = require('../utils');

export interface IChatConnectorSettings {
    botId: string;
    appId?: string;
    appPassword?: string;
    endpoint?: IChatConnectorEndpoint;
}

export interface IChatConnectorEndpoint {
    refreshEndpoint: string;
    refreshScope: string;
    verifyEndpoint: string;
    verifyIssuer: string;
}

export class ChatConnector implements ub.IConnector, bs.IBotStorage {
    private handler: (messages: IMessage[], cb?: (err: Error) => void) => void;
    private accessToken: string;
    private accessTokenExpires: number;

    constructor(private settings: IChatConnectorSettings) {
        if (!this.settings.endpoint) {
            this.settings.endpoint = {
                refreshEndpoint: 'https://login.microsoftonline.com/common/oauth2/v2.0/token',
                refreshScope: 'https://graph.microsoft.com/.default',
                verifyEndpoint: 'https://api.botframework.com/api/.well-known/OpenIdConfiguration',
                verifyIssuer: 'https://api.botframework.com'
            }
        }
    }

    public listen(): IWebMiddleware {
        return (req: IWebRequest, res: IWebResponse) => {
            if (req.body) {
                this.dispatch(req.body, res);
            } else {
                var requestData = '';
                req.on('data', (chunk: string) => {
                    requestData += chunk
                });
                req.on('end', () => {
                    var body = JSON.parse(requestData);
                    this.dispatch(body, res);
                });
            }
        };
    }

    public verifyBotFramework(): IWebMiddleware {
        return (req: IWebRequest, res: IWebResponse, next: Function) => {
            // TODO: Add logic to verify framework calls.
            next();
        };
    }

    public onMessage(handler: (messages: IMessage[], cb?: (err: Error) => void) => void): void {
        this.handler = handler;
    }
    
    public send(messages: IMessage[], done: (err: Error) => void): void {
        var conversationId: string;
        async.eachSeries(messages, (msg, cb) => {
            try {
                var address = <IChatConnectorAddress>msg.address;
                if (address && address.serviceUrl) {
                    delete msg.address;
                    if (!address.conversation && conversationId) {
                        address.conversation = { id: conversationId };
                    }
                    this.postMessage(address, msg, cb);
                } else {
                    cb(new Error('Message missing address or serviceUrl.'));
                }
            } catch (e) {
                cb(e);
            }
        }, done);
    }

    public startConversation(address: IChatConnectorAddress, done: (err: Error, address?: IAddress) => void): void {
        if (address && address.user && address.bot && address.serviceUrl) {
            // Issue request
            var options: request.Options = {
                method: 'POST',
                url: url.resolve(address.serviceUrl, '/v3/conversations'),
                body: {
                    bot: address.bot,
                    members: [address.user] 
                },
                json: true
            };
            this.authenticatedRequest(options, (err, response, body) => {
                var adr: IAddress;
                if (!err) {
                    try {
                        var obj = typeof body === 'string' ? JSON.parse(body) : body;
                        if (obj && obj.hasOwnProperty('id')) {
                            adr = utils.clone(address);
                            adr.conversation = { id: obj['id'] };
                            if (adr.id) {
                                delete adr.id;
                            }
                        } else {
                            err = new Error('Failed to start conversation: no conversation ID returned.')
                        }
                    } catch (e) {
                        err = e instanceof Error ? e : new Error(e.toString());
                    }
                }
                done(err, adr);
            });
        }
    }

    public getData(context: bs.IBotStorageContext, callback: (err: Error, data: IChatConnectorStorageData) => void): void {
        try {
            // Build list of read commands
            var root = this.getStoragePath(context.address);
            var list: any[] = [];
            if (context.userId) {
                // Read userData
                if (context.persistUserData) {
                    list.push({ 
                        field: 'userData', 
                        url: root + '/users/' + encodeURIComponent(context.userId) 
                    });
                }
                if (context.conversationId) {
                    // Read privateConversationData
                    list.push({ 
                        field: 'privateConversationData',
                        url: root + '/conversations/' + encodeURIComponent(context.conversationId) +
                                    '/users/' + encodeURIComponent(context.userId)
                    });
                }
            }
            if (context.persistConversationData && context.conversationId) {
                // Read conversationData
                list.push({ 
                    field: 'conversationData',
                    url: root + '/conversations/' + encodeURIComponent(context.conversationId)
                });
            }

            // Execute reads in parallel
            var data: IChatConnectorStorageData = {};
            async.each(list, (entry, cb) => {
                var options: request.Options = {
                    method: 'GET',
                    url: entry.url,
                    json: true
                };
                this.authenticatedRequest(options, (err: Error, response: http.IncomingMessage, body: IChatConnectorState) => {
                    if (!err && body) {
                        try {
                            var botData = body.data ? body.data : {};
                            (<any>data)[entry.field + 'Hash'] = JSON.stringify(botData);
                            (<any>data)[entry.field] = botData;
                        } catch (e) {
                            err = e;
                        }
                    }
                    cb(err);
                });
            }, (err) => {
                if (!err) {
                    callback(null, data);
                } else {
                    callback(err instanceof Error ? err : new Error(err.toString()), null);
                }
            });
        } catch (e) {
            callback(e instanceof Error ? e : new Error(e.toString()), null);
        }
    }

    public saveData(context: bs.IBotStorageContext, data: IChatConnectorStorageData, callback?: (err: Error) => void): void {
        var list: any[] = [];
        function addWrite(field: string, botData: any, url: string) {
            var hashKey = field + 'Hash'; 
            var hash = JSON.stringify(botData);
            if (!(<any>data)[hashKey] || hash !== (<any>data)[hashKey]) {
                (<any>data)[hashKey] = hash;
                list.push({ botData: botData, url: url });
            }
        }
        
        try {
            // Build list of write commands
            var root = this.getStoragePath(context.address);
            if (context.userId) {
                if (context.persistUserData)
                {
                    // Write userData
                    addWrite('userData', data.userData || {}, root + '/users/' + encodeURIComponent(context.userId));
                }
                if (context.conversationId) {
                    // Write privateConversationData
                    var url = root + '/conversations/' + encodeURIComponent(context.conversationId) +
                                     '/users/' + encodeURIComponent(context.userId);
                    addWrite('privateConversationData', data.privateConversationData || {}, url);
                }
            }
            if (context.persistConversationData && context.conversationId) {
                // Write conversationData
                addWrite('conversationData', data.conversationData || {}, root + '/conversations/' + encodeURIComponent(context.conversationId));
            }

            // Execute writes in parallel
            async.each(list, (entry, cb) => {
                var options: request.Options = {
                    method: 'POST',
                    url: entry.url,
                    body: { eTag: '*', data: entry.botData },
                    json: true
                };
                this.authenticatedRequest(options, (err, response, body) => {
                    cb(err);
                });
            }, (err) => {
                if (callback) {
                    if (!err) {
                        callback(null);
                    } else {
                        callback(err instanceof Error ? err : new Error(err.toString()));
                    }
                }
            });
        } catch (e) {
            if (callback) {
                callback(e instanceof Error ? e : new Error(e.toString()));
            }
        }
    }

    private dispatch(messages: IMessage|IMessage[], res: IWebResponse) {
        // Dispatch messages/activities
        var list: IMessage[] = Array.isArray(messages) ? messages : [messages];
        list.forEach((msg) => {
            try {
                // Break out address fields
                var address = <IChatConnectorAddress>{};
                moveFields(msg, address, <any>toAddress);
                msg.address = address;

                // Patch serviceUrl
                if (address.serviceUrl) {
                    try {
                        var u = url.parse(address.serviceUrl);
                        address.serviceUrl = u.protocol + '//' + u.host;
                    } catch (e) {
                        console.error("ChatConnector error parsing '" + address.serviceUrl + "': " + e.toString());
                    }
                }

                // Dispatch message
                this.handler([msg]);
            } catch (e) {
                console.error(e.toString());
            }
        });

        // Acknowledge that we recieved the message(s)
        res.status(202);
        res.end();
    }

    private postMessage(address: IChatConnectorAddress, msg: IMessage, cb: (err: Error) => void): void {
        // Calculate path
        var path = '/v3/conversations/' + encodeURIComponent(address.conversation.id) + '/activities';
        if (address.id && address.channelId !== 'skype') {
            path += '/' + encodeURIComponent(address.id);
        }

        // Update message with address info
        (<any>msg)['from'] = address.bot;
        (<any>msg)['recipient'] = address.user; 

        // Issue request
        var options: request.Options = {
            method: 'POST',
            url: url.resolve(address.serviceUrl, path),
            body: msg,
            json: true
        };
        this.authenticatedRequest(options, (err, response, body) => {
            cb(err);
        });
    }

    private authenticatedRequest(options: request.Options, callback: (error: any, response: http.IncomingMessage, body: any) => void, refresh = false): void {
        if (refresh) {
            this.accessToken = null;
        }
        this.addAccessToken(options, (err) => {
            if (!err) {
                request(options, (err, response, body) => {
                    if (!err) {
                        switch (response.statusCode) {
                            case 401:
                            case 403:
                                if (!refresh) {
                                    this.authenticatedRequest(options, callback, true);
                                } else {
                                    callback(null, response, body);
                                }
                                break;
                            default:
                                if (response.statusCode < 400) {
                                    callback(null, response, body);
                                } else {
                                    var txt = "Request to '" + options.url + "' failed: [" + response.statusCode + "] " + response.statusMessage;
                                    callback(new Error(txt), response, null);
                                }
                                break;
                        }
                    } else {
                        callback(err, null, null);
                    }
                });
            } else {
                callback(err, null, null);
            }
        });
    }

    private addAccessToken(options: request.Options, cb: (err: Error) => void): void {
        if (this.settings.appId && this.settings.appPassword) {
            if (!this.accessToken || new Date().getTime() >= this.accessTokenExpires) {
                // Refresh access token
                var opt: request.Options = {
                    method: 'POST',
                    url: this.settings.endpoint.refreshEndpoint,
                    form: {
                        grant_type: 'client_credentials',
                        client_id: this.settings.appId,
                        client_secret: this.settings.appPassword,
                        scope: this.settings.endpoint.refreshScope
                    }
                };
                request(opt, (err, response, body) => {
                    if (!err) {
                        if (body && response.statusCode < 300) {
                            // Subtract 5 minutes from expires_in so they'll we'll get a
                            // new token before it expires.
                            var oauthResponse = JSON.parse(body);
                            this.accessToken = oauthResponse.access_token;
                            this.accessTokenExpires = new Date().getTime() + ((oauthResponse.expires_in - 300) * 1000); 
                            options.headers = {
                                'Authorization': 'Bearer ' + this.accessToken
                            };
                            cb(null);
                        } else {
                            cb(new Error('Refresh access token failed with status code: ' + response.statusCode));
                        }
                    } else {
                        cb(err);
                    }
                });
            } else {
                options.headers = {
                    'Authorization': 'Bearer ' + this.accessToken
                };
                cb(null);
            }
        } else {
            cb(null);
        }
    }

    private getStoragePath(address: IChatConnectorAddress): string {
        // Calculate host
        var path: string;
        switch (address.channelId) {
            case 'emulator':
            //case 'skype-teams':
                if (address.serviceUrl) {
                    path = address.serviceUrl;
                } else {
                    throw new Error('ChatConnector.getStoragePath() missing address.serviceUrl.');
                }
                break;
            default:
                path = 'https://api.botframework.com'
                break;
        }

        // Append base path info.
        return path + '/v3/botstate/' + 
            encodeURIComponent(this.settings.botId) + '/' +
            encodeURIComponent(address.channelId);
    }
}

var toAddress = {
    'id': 'id',
    'channelId': 'channelId',
    'from': 'user',
    'conversation': 'conversation',
    'recipient': 'bot',
    'serviceUrl': 'serviceUrl'
}

function moveFields(frm: Object, to: Object, map: { [key:string]: string; }): void {
    if (frm && to) {
        for (var key in map) {
            if (frm.hasOwnProperty(key)) {
                (<any>to)[map[key]] = (<any>frm)[key];
                delete (<any>frm)[key];
            }
        }
    }
}

interface IChatConnectorAddress extends IAddress {
    serviceUrl?: string;    // Specifies the URL to: post messages back, comment, annotate, delete 
}

interface IChatConnectorStorageData extends bs.IBotStorageData {
    userDataHash?: string;
    conversationDataHash?: string;
    privateConversationDataHash?: string;
}

interface IChatConnectorState {
    eTag: string;
    data?: any;
}

/** Express or Restify Request object. */
interface IWebRequest {
    body: any;
    headers: {
        [name: string]: string;
    };
    on(event: string, ...args: any[]): void;
}

/** Express or Restify Response object. */
interface IWebResponse {
    end(): this;
    send(status: number, body?: any): this;
    send(body: any): this;
    status(code: number): this;
}

/** Express or Restify Middleware Function. */
interface IWebMiddleware {
    (req: IWebRequest, res: IWebResponse, next?: Function): void;
}
