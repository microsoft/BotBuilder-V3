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

import ucb = require('./UniversalCallBot');
import bs = require('../storage/BotStorage');
import events = require('events');
import request = require('request');
import async = require('async');
import url = require('url');
import http = require('http');
import utils = require('../utils');
import consts = require('../consts');
var Busboy = require('busboy');

export interface ICallConnectorSettings {
    callbackUri: string;
    appId?: string;
    appPassword?: string;
    endpoint?: ICallConnectorEndpoint;
    serviceUri?: string;
    stateUri?: string;
}

export interface ICallConnectorEndpoint {
    refreshEndpoint: string;
    refreshScope: string;
    verifyEndpoint: string;
    verifyIssuer: string;
    stateEndpoint: string;
}

export class CallConnector implements ucb.ICallConnector, bs.IBotStorage {
    private handler: (event: IEvent, cb?: (err: Error) => void) => void;
    private accessToken: string;
    private accessTokenExpires: number;
    private responses: { [id:string]: (err: Error, repsonse?: any) => void; } = {};

    constructor(private settings: ICallConnectorSettings) {
        if (!this.settings.endpoint) {
            this.settings.endpoint = {
                refreshEndpoint: 'https://login.microsoftonline.com/common/oauth2/v2.0/token',
                refreshScope: 'https://graph.microsoft.com/.default',
                verifyEndpoint: 'https://api.botframework.com/api/.well-known/OpenIdConfiguration',
                verifyIssuer: 'https://api.botframework.com',
                stateEndpoint: this.settings.stateUri || 'https://state.botframework.com'
            }
        }
    }

    public listen(): IWebMiddleware {
        return (req: IWebRequest, res: IWebResponse) => {
            var correlationId = req.headers['X-Microsoft-Skype-Chain-ID'];
            var callback = this.responseCallback(req, res);
            if (req.is('application/json')) {
                this.parseBody(req, (err, body) => {
                    if (!err) {
                        body.correlationId = correlationId;
                        this.dispatch(body, callback);
                    } else {
                        callback(err);
                    }
                });
            } else if (req.is('multipart/form-data')) {
                this.parseFormData(req, (err, body) => {
                    if (!err) {
                        body.correlationId = correlationId;
                        this.dispatch(body, callback);
                    } else {
                        callback(err);
                    }
                });
            } else {
                callback(new Error('Invalid content type.'));
            }
        };
    }

    public verifyBotFramework(): IWebMiddleware {
        return (req: IWebRequest, res: IWebResponse, next: Function) => {
            // TODO: Add logic to verify framework calls.
            next();
        };
    }

    public onEvent(handler: (event: IEvent, cb?: (err: Error) => void) => void): void {
        this.handler = handler;
    }

    public send(event: IEvent, cb: (err: Error) => void): void {
        if (event.type == 'workflow' && event.address && event.address.conversation) {
            var conversation = event.address.conversation;
            if (this.responses.hasOwnProperty(conversation.id)) {
                // Pop callback off responses list
                var callback = this.responses[conversation.id];
                delete this.responses[conversation.id];
                
                // Deliver message
                var response: IWorkflow = utils.clone(event);
                response.links = { 'callback': this.settings.callbackUri };
                (<any>response).appState = JSON.stringify(response.address);
                delete response.type;
                delete response.address;
                callback(null, response);
            }
        } else {
            cb(new Error('Invalid message sent to CallConnector.send().'));
        }
    }

    public getData(context: bs.IBotStorageContext, callback: (err: Error, data: ICallConnectorStorageData) => void): void {
        try {
            // Build list of read commands
            var root = this.getStoragePath();
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
            var data: ICallConnectorStorageData = {};
            async.each(list, (entry, cb) => {
                var options: request.Options = {
                    method: 'GET',
                    url: entry.url,
                    json: true
                };
                this.authenticatedRequest(options, (err: Error, response: http.IncomingMessage, body: ICallConnectorState) => {
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

    public saveData(context: bs.IBotStorageContext, data: ICallConnectorStorageData, callback?: (err: Error) => void): void {
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
            var root = this.getStoragePath();
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

    private dispatch(body: any, response: (err: Error, response?: any) => void) {
        if ((<IConversationResult>body).callState == 'terminated') {
            return response(null);
        }
        var event = <IEvent>{};
        event.agent = consts.agent;
        event.source = 'skype';
        event.sourceEvent = body;
        this.responses[body.id] = response;
        if (event.hasOwnProperty('participants')) {
            // Initialize event
            var convo = <ISkypeConversation>body;
            event.type = 'conversation';
            utils.copyFieldsTo(convo, event, 'callState|links|presentedModalityTypes');

            // Populate address
            var address = event.address = <ICallConnectorAddress>{};
            address.channelId = event.source;
            address.correlationId = convo.correlationId;
            address.serviceUri = this.settings.serviceUri || '';
            address.conversation = { id: convo.id, isGroup: convo.isMultiparty };
            utils.copyFieldsTo(convo, address, 'threadId|subject');
            if (address.subject) {
                address.conversation.name = address.subject;
            }
            address.participants = [];
            convo.participants.forEach((p) => {
                var identity = <IIdentity>{
                    id: p.identity,
                    name: p.displayName,
                    locale: p.languageId,
                    originator: p.originator
                };
                address.participants.push(identity);
                if (identity.originator) {
                    address.user = identity;
                } else if (identity.id.indexOf('28:') == 0) {
                    address.bot = identity;
                }
            });
        } else {
            // Initialize event
            var result = <ISkypeConversationResult>body;
            event.type = 'conversationResult';
            event.address = <ICallConnectorAddress>JSON.parse(result.appState);
            utils.copyFieldsTo(result, event, 'links|operationOutcome|recordedAudio');
            if (result.id !== event.address.conversation.id) {
                console.warn("CallConnector received a 'conversationResult' with an invalid conversation id.");
            }
        }
        this.handler(event, (err) => {
            if (err && this.responses.hasOwnProperty(body.id)) {
                delete this.responses[body.id];
                response(err);
            }
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

    public getAccessToken(cb: (err: Error, accessToken: string) => void): void {
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
                        cb(null, this.accessToken);
                    } else {
                        cb(new Error('Refresh access token failed with status code: ' + response.statusCode), null);
                    }
                } else {
                    cb(err, null);
                }
            });
        } else {
            cb(null, this.accessToken);
        }
    }

    private addAccessToken(options: request.Options, cb: (err: Error) => void): void {
        if (this.settings.appId && this.settings.appPassword) {
            this.getAccessToken((err, token) => {
                if (!err && token) {
                    options.headers = {
                        'Authorization': 'Bearer ' + token
                    };
                    cb(null);
                } else {
                    cb(err);
                }
            });
        } else {
            cb(null);
        }
    }

    private getStoragePath(): string {
        return url.resolve(this.settings.endpoint.stateEndpoint, '/v3/botstate/skype/');
    }

    private parseBody(req: IWebRequest, cb: (err: Error, body: any) => void): void {
        if (typeof req.body === 'undefined') {
            var data = '';
            req.on('data', (chunk: string) => data += chunk);
            req.on('end', () => {
                var err: Error;
                var body: any;
                try {
                    body = JSON.parse(data);
                } catch (e) {
                    err = e;
                }
                cb(err, body);
            });
        } else {
            cb(null, req.body);
        }
    }

    private parseFormData(req: IWebRequest, cb: (err: Error, body: any) => void): void {
        var busboy = new Busboy({ headers: req.headers, defCharset: 'binary' });

        var result: IConversationResult;
        var recordedAudio: Buffer;
        busboy.on('field', (fieldname: string, val: any, fieldnameTruncated: string, valTruncated: any, encoding: string, mimetype: string) => {
            if (fieldname === 'recordedAudio') {
                recordedAudio = new Buffer(val, 'binary');
            } else if (fieldname === 'conversationResult') {
                result = JSON.parse(val);
            }
        });
        busboy.on('finish', () => {
            if (result && recordedAudio) {
                result.recordedAudio = recordedAudio;
            }
            cb(null, result);
        });
        req.pipe(busboy);
    }

    private responseCallback(req: IWebRequest, res: IWebResponse) {
        return function(err: Error, response?: any) {
            if (err) {
                res.status(500);
                res.end();
            } else {
                res.status(200);
                res.send(response);
            }
        };
    }
}

interface ICallConnectorStorageData extends bs.IBotStorageData {
    userDataHash?: string;
    conversationDataHash?: string;
    privateConversationDataHash?: string;
}

interface ICallConnectorState {
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
    pipe(stream: any): void;
    is(contentType: string): boolean;
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

interface ISkypeConversation {
    id: string;
    callState: string;
    appId?: string;
    links?: any;
    presentedModalityTypes: string[];
    participants: ISkypeParticipant[];
    threadId?: string;
    subject?: string;
    correlationId?: string;
    isMultiparty?: boolean;
}

interface ISkypeParticipant {
    identity: string;
    displayName: string;
    languageId: string;
    originator: boolean;
}

interface ISkypeConversationResult {
    id: string;
    appId?: string;
    appState?: any;
    links?: any;
    operationOutcome: IActionOutcome; 
    recordedAudio?: Buffer;
}
