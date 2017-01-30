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

import { IConnector } from './UniversalBot';
import { IBotStorage, IBotStorageContext, IBotStorageData } from '../storage/BotStorage';
import { OpenIdMetadata } from './OpenIdMetadata';
import * as utils from '../utils';
import * as logger from '../logger';
import * as consts from '../consts';
import * as events from 'events';
import * as request from 'request';
import * as async from 'async';
import * as url from 'url';
import * as http from 'http';
import * as jwt from 'jsonwebtoken';
import * as zlib from 'zlib';
import urlJoin = require('url-join');

var pjson = require('../../package.json');

var MAX_DATA_LENGTH = 65000;

var USER_AGENT = "Microsoft-BotFramework/3.1 (BotBuilder Node.js/"+ pjson.version +")";

export interface IChatConnectorSettings {
    appId?: string;
    appPassword?: string;
    gzipData?: boolean;
    endpoint?: IChatConnectorEndpoint;
    stateEndpoint?: string;
    openIdMetadata? : string;
}

export interface IChatConnectorEndpoint {
    refreshEndpoint: string;
    refreshScope: string;
    botConnectorOpenIdMetadata: string;
    botConnectorIssuer: string;
    botConnectorAudience: string;
    msaOpenIdMetadata: string;
    msaIssuer: string;
    msaAudience: string;
    emulatorOpenIdMetadata: string;
    emulatorIssuer: string;
    emulatorAudience: string;
    stateEndpoint: string;
}

export interface IChatConnectorAddress extends IAddress {
    id?: string;            // Incoming Message ID
    serviceUrl?: string;    // Specifies the URL to: post messages back, comment, annotate, delete
    useAuth?: string;
}

export class ChatConnector implements IConnector, IBotStorage {
    private onEventHandler: (events: IEvent[], cb?: (err: Error) => void) => void;
    private onInvokeHandler: (event: IEvent, cb?: (err: Error, body: any, status?: number) => void) => void;
    private accessToken: string;
    private accessTokenExpires: number;
    private botConnectorOpenIdMetadata: OpenIdMetadata;
    private msaOpenIdMetadata: OpenIdMetadata;
    private emulatorOpenIdMetadata: OpenIdMetadata;

    constructor(private settings: IChatConnectorSettings = {}) {
        if (!this.settings.endpoint) {
            this.settings.endpoint = {
                refreshEndpoint: 'https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token',
                refreshScope: 'https://api.botframework.com/.default',
                botConnectorOpenIdMetadata: this.settings.openIdMetadata || 'https://login.botframework.com/v1/.well-known/openidconfiguration',
                botConnectorIssuer: 'https://api.botframework.com',
                botConnectorAudience: this.settings.appId,
                msaOpenIdMetadata: 'https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration',
                msaIssuer: 'https://sts.windows.net/72f988bf-86f1-41af-91ab-2d7cd011db47/',
                msaAudience: 'https://graph.microsoft.com',
                emulatorOpenIdMetadata: 'https://login.microsoftonline.com/botframework.com/v2.0/.well-known/openid-configuration',
                emulatorAudience: 'https://sts.windows.net/d6d49420-f39b-4df7-a1dc-d59a935871db/',
                emulatorIssuer: this.settings.appId,
                stateEndpoint: this.settings.stateEndpoint || 'https://state.botframework.com'
            }
        }

        this.botConnectorOpenIdMetadata = new OpenIdMetadata(this.settings.endpoint.botConnectorOpenIdMetadata);
        this.msaOpenIdMetadata = new OpenIdMetadata(this.settings.endpoint.msaOpenIdMetadata);
        this.emulatorOpenIdMetadata = new OpenIdMetadata(this.settings.endpoint.emulatorOpenIdMetadata);
    }

    public listen(): IWebMiddleware {
        return (req: IWebRequest, res: IWebResponse) => {
            if (req.body) {
                this.verifyBotFramework(req, res);
            } else {
                var requestData = '';
                req.on('data', (chunk: string) => {
                    requestData += chunk
                });
                req.on('end', () => {
                    req.body = JSON.parse(requestData);
                    this.verifyBotFramework(req, res);
                });
            }
        };
    }

    private verifyBotFramework(req: IWebRequest, res: IWebResponse): void {
        var token: string;
        var isEmulator = req.body['channelId'] === 'emulator';
        var authHeaderValue = req.headers ? req.headers['authorization'] || req.headers['Authorization'] : null;
        if (authHeaderValue) {
            var auth = authHeaderValue.trim().split(' ');
            if (auth.length == 2 && auth[0].toLowerCase() == 'bearer') {
                token = auth[1];
            }
        }

        // Verify token
        if (token) {
            req.body['useAuth'] = true;

            let decoded = jwt.decode(token, { complete: true });
            var verifyOptions: jwt.VerifyOptions;
            var openIdMetadata: OpenIdMetadata;

            if (isEmulator && decoded.payload.iss == this.settings.endpoint.msaIssuer) {
                // This token came from MSA, so check it via the emulator path
                openIdMetadata = this.msaOpenIdMetadata;
                verifyOptions = {
                    issuer: this.settings.endpoint.msaIssuer,
                    audience: this.settings.endpoint.msaAudience,
                    clockTolerance: 300
                };
            } else if (isEmulator && decoded.payload.iss == this.settings.endpoint.emulatorIssuer) {
                // This token came from the emulator, so check it via the emulator path
                openIdMetadata = this.emulatorOpenIdMetadata;
                verifyOptions = {
                    issuer: this.settings.endpoint.emulatorIssuer,
                    audience: this.settings.endpoint.emulatorAudience,
                    clockTolerance: 300
                };
            } else {
                // This is a normal token, so use our Bot Connector verification
                openIdMetadata = this.botConnectorOpenIdMetadata;
                verifyOptions = {
                    issuer: this.settings.endpoint.botConnectorIssuer,
                    audience: this.settings.endpoint.botConnectorAudience,
                    clockTolerance: 300
                };
            }

            if (isEmulator && decoded.payload.appid != this.settings.appId) {
                logger.error('ChatConnector: receive - invalid token. Requested by unexpected app ID.');
                res.status(403);
                res.end();
                return;
            }

            openIdMetadata.getKey(decoded.header.kid, key => {
                if (key) {
                    try {
                        jwt.verify(token, key, verifyOptions);
                    } catch (err) {
                        logger.error('ChatConnector: receive - invalid token. Check bot\'s app ID & Password.');
                        res.status(403);
                        res.end();
                        return;
                    }
                    
                    this.dispatch(req.body, res);
                } else {
                    logger.error('ChatConnector: receive - invalid signing key or OpenId metadata document.');
                    res.status(500);
                    res.end();
                    return;
                }
            });
        } else if (isEmulator && !this.settings.appId && !this.settings.appPassword) {
            // Emulator running without auth enabled
            logger.warn(req.body, 'ChatConnector: receive - emulator running without security enabled.');
            req.body['useAuth'] = false;
            this.dispatch(req.body, res);
        } else {
            // Token not provided so
            logger.error('ChatConnector: receive - no security token sent.');
            res.status(401);
            res.end();
        }
    }

    public onEvent(handler: (events: IEvent[], cb?: (err: Error) => void) => void): void {
        this.onEventHandler = handler;
    }

    public onInvoke(handler: (event: IEvent, cb?: (err: Error, body: any, status?: number) => void) => void): void {
        this.onInvokeHandler = handler;
    }
    
    public send(messages: IMessage[], done: (err: Error) => void): void {
        async.eachSeries(messages, (msg, cb) => {
            try {
                if (msg.address && (<IChatConnectorAddress>msg.address).serviceUrl) {
                    this.postMessage(msg, cb);
                } else {
                    logger.error('ChatConnector: send - message is missing address or serviceUrl.')
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
                // We use urlJoin to concatenate urls. url.resolve should not be used here, 
                // since it resolves urls as hrefs are resolved, which could result in losing
                // the last fragment of the serviceUrl
                url: urlJoin(address.serviceUrl, '/v3/conversations'),
                body: {
                    bot: address.bot,
                    members: [address.user] 
                },
                json: true
            };
            this.authenticatedRequest(options, (err, response, body) => {
                var adr: IChatConnectorAddress;
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
                if (err) {
                    logger.error('ChatConnector: startConversation - error starting conversation.')
                }
                done(err, adr);
            });
        } else {
            logger.error('ChatConnector: startConversation - address is invalid.')
            done(new Error('Invalid address.'))
        }
    }

    public getData(context: IBotStorageContext, callback: (err: Error, data: IChatConnectorStorageData) => void): void {
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
                        var botData = body.data ? body.data : {};
                        if (typeof botData === 'string') {
                            // Decompress gzipped data
                            zlib.gunzip(new Buffer(botData, 'base64'), (err, result) => {
                                if (!err) {
                                    try {
                                        var txt = result.toString();
                                        (<any>data)[entry.field + 'Hash'] = txt;
                                        (<any>data)[entry.field] = JSON.parse(txt);
                                    } catch (e) {
                                        err = e;
                                    }
                                }
                                cb(err);
                            });
                        } else {
                            try {
                                (<any>data)[entry.field + 'Hash'] = JSON.stringify(botData);
                                (<any>data)[entry.field] = botData;
                            } catch (e) {
                                err = e;
                            }
                            cb(err);
                        }
                    } else {
                        cb(err);
                    }
                });
            }, (err) => {
                if (!err) {
                    callback(null, data);
                } else {
                    var m = err.toString();
                    callback(err instanceof Error ? err : new Error(m), null);
                }
            });
        } catch (e) {
            callback(e instanceof Error ? e : new Error(e.toString()), null);
        }
    }

    public saveData(context: IBotStorageContext, data: IChatConnectorStorageData, callback?: (err: Error) => void): void {
        var list: any[] = [];
        function addWrite(field: string, botData: any, url: string) {
            var hashKey = field + 'Hash'; 
            var hash = JSON.stringify(botData);
            if (!(<any>data)[hashKey] || hash !== (<any>data)[hashKey]) {
                (<any>data)[hashKey] = hash;
                list.push({ botData: botData, url: url, hash: hash });
            }
        }

        try {
            // Build list of write commands
            var root = this.getStoragePath(context.address);
            if (context.userId) {
                if (context.persistUserData) {
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
                if (this.settings.gzipData) {
                    zlib.gzip(entry.hash, (err, result) => {
                        if (!err && result.length > MAX_DATA_LENGTH) {
                            err = new Error("Data of " + result.length + " bytes gzipped exceeds the " + MAX_DATA_LENGTH + " byte limit. Can't post to: " + entry.url);
                            (<any>err).code = consts.Errors.EMSGSIZE;
                        }
                        if (!err) {
                            var options: request.Options = {
                                method: 'POST',
                                url: entry.url,
                                body: { eTag: '*', data: result.toString('base64') },
                                json: true
                            };
                            this.authenticatedRequest(options, (err, response, body) => {
                                cb(err);
                            });
                        } else {
                            cb(err);
                        }
                    });
                } else if (entry.hash.length < MAX_DATA_LENGTH) {
                    var options: request.Options = {
                        method: 'POST',
                        url: entry.url,
                        body: { eTag: '*', data: entry.botData },
                        json: true
                    };
                    this.authenticatedRequest(options, (err, response, body) => {
                        cb(err);
                    });
                } else {
                    var err = new Error("Data of " + entry.hash.length + " bytes exceeds the " + MAX_DATA_LENGTH + " byte limit. Consider setting connectors gzipData option. Can't post to: " + entry.url);
                    (<any>err).code = consts.Errors.EMSGSIZE;
                    cb(err);
                }
            }, (err) => {
                if (callback) {
                    if (!err) {
                        callback(null);
                    } else {
                        var m = err.toString();
                        callback(err instanceof Error ? err : new Error(m));
                    }
                }
            });
        } catch (e) {
            if (callback) {
                var err = e instanceof Error ? e : new Error(e.toString());
                (<any>err).code = consts.Errors.EBADMSG;
                callback(err);
            }
        }
    }

    private dispatch(msg: IMessage, res: IWebResponse) {
        // Dispatch message/activity
        try {
            this.prepIncomingMessage(msg);
            logger.info(msg, 'ChatConnector: message received.');

            if(this.isInvoke(msg)) {
                this.onInvokeHandler(msg, (err: Error, body: any, status?: number) => {
                    if(err) {
                        res.status(500);
                        res.end();
                        logger.error('Received error from invoke handler: ', err.message || '');
                    } else {
                        res.send(status || 200, body);
                    }
                });
            } else {
                // Dispatch message
                this.onEventHandler([msg]);

                // Acknowledge that we recieved the message(s)
                res.status(202);
                res.end();
            }

        } catch (e) {
            console.error(e instanceof Error ? (<Error>e).stack : e.toString());
            res.status(500);
            res.end();
        }
    }

    private isInvoke(message: IMessage): boolean {
        return (message && message.type && message.type.toLowerCase() == consts.invokeType);
    }
    
    
    private postMessage(msg: IMessage, cb: (err: Error) => void): void {
        logger.info(address, 'ChatConnector: sending message.')
        this.prepOutgoingMessage(msg);

        // Apply address fields
        var address = <IChatConnectorAddress>msg.address;
        (<any>msg)['from'] = address.bot;
        (<any>msg)['recipient'] = address.user; 
        delete msg.address;

        // Calculate path
        var path = '/v3/conversations/' + encodeURIComponent(address.conversation.id) + '/activities';
        if (address.id && address.channelId !== 'skype') {
            path += '/' + encodeURIComponent(address.id);
        }
        
        // Issue request
        var options: request.Options = {
            method: 'POST',
            // We use urlJoin to concatenate urls. url.resolve should not be used here, 
            // since it resolves urls as hrefs are resolved, which could result in losing
            // the last fragment of the serviceUrl
            url: urlJoin(address.serviceUrl, path),
            body: msg,
            json: true
        };
        if (address.useAuth) {
            this.authenticatedRequest(options, (err, response, body) => cb(err));
        } else {
            this.addUserAgent(options);
            request(options, (err, response, body) => {
                if (!err && response.statusCode >= 400) {
                    var txt = "Request to '" + options.url + "' failed: [" + response.statusCode + "] " + response.statusMessage;
                    err = new Error(txt);
                }
                cb(err);
            });
        }
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

    private addUserAgent(options: request.Options) : void {
        if (options.headers == null)
        {
            options.headers = {};
        }
        options.headers['User-Agent'] = USER_AGENT;
    }

    private addAccessToken(options: request.Options, cb: (err: Error) => void): void {
        this.addUserAgent(options);
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
                path = this.settings.endpoint.stateEndpoint;
                break;
        }

        // Append base path info.
        return path + '/v3/botstate/' + encodeURIComponent(address.channelId);
    }

    private prepIncomingMessage(msg: IMessage): void {
            // Patch locale and channelData
            utils.moveFieldsTo(msg, msg, { 
                'locale': 'textLocale',
                'channelData': 'sourceEvent'
            });

            // Ensure basic fields are there
            msg.text = msg.text || '';
            msg.attachments = msg.attachments || [];
            msg.entities = msg.entities || [];

            // Break out address fields
            var address = <IChatConnectorAddress>{};
            utils.moveFieldsTo(msg, address, <any>toAddress);
            msg.address = address;
            msg.source = address.channelId;

            // Check for facebook quick replies
            if (msg.source == 'facebook' && msg.sourceEvent && msg.sourceEvent.message && msg.sourceEvent.message.quick_reply) {
                msg.text = msg.sourceEvent.message.quick_reply.payload;
            }
    }

    private prepOutgoingMessage(msg: IMessage): void {
        // Convert attachments
        if (msg.attachments) {
            var attachments = <IAttachment[]>[];
            for (var i = 0; i < msg.attachments.length; i++) {
                var a = msg.attachments[i];
                switch (a.contentType) {
                    case 'application/vnd.microsoft.keyboard':
                        if (msg.address.channelId == 'facebook') {
                            // Convert buttons
                            msg.sourceEvent = { quick_replies: [] };
                            (<IKeyboard>a.content).buttons.forEach((action) => {
                                switch (action.type) {
                                    case 'imBack':
                                    case 'postBack':
                                        msg.sourceEvent.quick_replies.push({
                                            content_type: 'text',
                                            title: action.title,
                                            payload: action.value
                                        });
                                        break;
                                    default:
                                        logger.warn(msg, "Invalid keyboard '%s' button sent to facebook.", action.type);
                                        break;
                                }
                            });
                        } else {
                            a.contentType = 'application/vnd.microsoft.card.hero';
                            attachments.push(a);
                        }
                        break;
                    default:
                        attachments.push(a);
                        break;
                }
            }
            msg.attachments = attachments;
        }

        // Patch message fields
        utils.moveFieldsTo(msg, msg, {
            'textLocale': 'locale',
            'sourceEvent': 'channelData'
        });
        delete msg.agent;
        delete msg.source;
    }
}

var toAddress = {
    'id': 'id',
    'channelId': 'channelId',
    'from': 'user',
    'conversation': 'conversation',
    'recipient': 'bot',
    'serviceUrl': 'serviceUrl',
    'useAuth': 'useAuth'
}

interface IChatConnectorStorageData extends IBotStorageData {
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
