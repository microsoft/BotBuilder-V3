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

import { IConnector } from '../Session';
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

var USER_AGENT = "Microsoft-BotFramework/3.1 (BotBuilder Node.js/" + pjson.version + ")";

export interface IChatConnectorSettings {
    appId?: string;
    appPassword?: string;
    gzipData?: boolean;
    endpoint?: IChatConnectorEndpoint;
    stateEndpoint?: string;
    openIdMetadata?: string;
}

export interface IChatConnectorEndpoint {
    refreshEndpoint: string;
    refreshScope: string;
    botConnectorOpenIdMetadata: string;
    botConnectorIssuer: string;
    botConnectorAudience: string;
    emulatorOpenIdMetadata: string;
    emulatorAuthV31IssuerV1: string;
    emulatorAuthV31IssuerV2: string;
    emulatorAuthV32IssuerV1: string;
    emulatorAuthV32IssuerV2: string;
    emulatorAudience: string;
    stateEndpoint: string;
}

export interface IChatConnectorAddress extends IAddress {
    id?: string;            // Incoming Message ID
    serviceUrl?: string;    // Specifies the URL to: post messages back, comment, annotate, delete
}

export interface IStartConversationAddress extends IChatConnectorAddress {
    activity?: any;
    channelData?: any;
    isGroup?: boolean;
    members?: IIdentity[];
    topicName?: string;
}

export class ChatConnector implements IConnector, IBotStorage {
    private onEventHandler: (events: IEvent[], cb?: (err: Error) => void) => void;
    private onInvokeHandler: (event: IEvent, cb?: (err: Error, body: any, status?: number) => void) => void;
    private accessToken: string;
    private accessTokenExpires: number;
    private botConnectorOpenIdMetadata: OpenIdMetadata;
    private emulatorOpenIdMetadata: OpenIdMetadata;

    constructor(protected settings: IChatConnectorSettings = {}) {
        if (!this.settings.endpoint) {
            this.settings.endpoint = {
                refreshEndpoint: 'https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token',
                refreshScope: 'https://api.botframework.com/.default',
                botConnectorOpenIdMetadata: this.settings.openIdMetadata || 'https://login.botframework.com/v1/.well-known/openidconfiguration',
                botConnectorIssuer: 'https://api.botframework.com',
                botConnectorAudience: this.settings.appId,
                emulatorOpenIdMetadata: 'https://login.microsoftonline.com/botframework.com/v2.0/.well-known/openid-configuration',
                emulatorAudience: this.settings.appId,
                emulatorAuthV31IssuerV1: 'https://sts.windows.net/d6d49420-f39b-4df7-a1dc-d59a935871db/',
                emulatorAuthV31IssuerV2: 'https://login.microsoftonline.com/d6d49420-f39b-4df7-a1dc-d59a935871db/v2.0',
                emulatorAuthV32IssuerV1: 'https://sts.windows.net/f8cdef31-a31e-4b4a-93e4-5f571e91255a/',
                emulatorAuthV32IssuerV2: 'https://login.microsoftonline.com/f8cdef31-a31e-4b4a-93e4-5f571e91255a/v2.0',
                stateEndpoint: this.settings.stateEndpoint || 'https://state.botframework.com'
            }
        }

        this.botConnectorOpenIdMetadata = new OpenIdMetadata(this.settings.endpoint.botConnectorOpenIdMetadata);
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
            let decoded = jwt.decode(token, { complete: true });
            var verifyOptions: jwt.VerifyOptions;
            var openIdMetadata: OpenIdMetadata;
            const algorithms: string[] = ['RS256', 'RS384', 'RS512'];

            if (isEmulator) {

                // validate the claims from the emulator
                if ((decoded.payload.ver === '2.0' && decoded.payload.azp !== this.settings.appId) ||
                    (decoded.payload.ver !== '2.0' && decoded.payload.appid !== this.settings.appId)) {
                    logger.error('ChatConnector: receive - invalid token. Requested by unexpected app ID.');
                    res.status(403);
                    res.end();
                    return;
                }

                // the token came from the emulator, so ensure the correct issuer is used
                let issuer: string;
                if (decoded.payload.ver === '1.0' && decoded.payload.iss == this.settings.endpoint.emulatorAuthV31IssuerV1) {
                    // This token came from the emulator as a v1 token using the Auth v3.1 issuer
                    issuer = this.settings.endpoint.emulatorAuthV31IssuerV1;
                } else if (decoded.payload.ver === '2.0' && decoded.payload.iss == this.settings.endpoint.emulatorAuthV31IssuerV2) {
                    // This token came from the emulator as a v2 token using the Auth v3.1 issuer
                    issuer = this.settings.endpoint.emulatorAuthV31IssuerV2;
                } else if (decoded.payload.ver === '1.0' && decoded.payload.iss == this.settings.endpoint.emulatorAuthV32IssuerV1) {
                    // This token came from the emulator as a v1 token using the Auth v3.2 issuer
                    issuer = this.settings.endpoint.emulatorAuthV32IssuerV1;
                } else if (decoded.payload.ver === '2.0' && decoded.payload.iss == this.settings.endpoint.emulatorAuthV32IssuerV2) {
                    // This token came from the emulator as a v2 token using the Auth v3.2 issuer
                    issuer = this.settings.endpoint.emulatorAuthV32IssuerV2;
                }

                if (issuer) {
                    openIdMetadata = this.emulatorOpenIdMetadata;
                    verifyOptions = {
                        algorithms: algorithms,
                        issuer: issuer,
                        audience: this.settings.endpoint.emulatorAudience,
                        clockTolerance: 300
                    };
                }
            }

            if (!verifyOptions) {
                // This is a normal token, so use our Bot Connector verification
                openIdMetadata = this.botConnectorOpenIdMetadata;
                verifyOptions = {
                    issuer: this.settings.endpoint.botConnectorIssuer,
                    audience: this.settings.endpoint.botConnectorAudience,
                    clockTolerance: 300
                };
            }

            openIdMetadata.getKey(decoded.header.kid, key => {
                if (key) {
                    try {
                        jwt.verify(token, key.key, verifyOptions);

                        // enforce endorsements in openIdMetadadata if there is any endorsements associated with the key
                        if (typeof req.body.channelId !== 'undefined' &&
                            typeof key.endorsements !== 'undefined' &&
                            key.endorsements.lastIndexOf(req.body.channelId) === -1) {
                            const errorDescription: string = `channelId in req.body: ${req.body.channelId} didn't match the endorsements: ${key.endorsements.join(',')}.`;
                            logger.error(`ChatConnector: receive - endorsements validation failure. ${errorDescription}`);
                            throw new Error(errorDescription);
                        }

                        // validate service url using token's serviceurl payload
                        if (typeof decoded.payload.serviceurl !== 'undefined' &&
                            typeof req.body.serviceUrl !== 'undefined' &&
                            decoded.payload.serviceurl !== req.body.serviceUrl) {
                            const errorDescription: string = `ServiceUrl in payload of token: ${decoded.payload.serviceurl} didn't match the request's serviceurl: ${req.body.serviceUrl}.`;
                            logger.error(`ChatConnector: receive - serviceurl mismatch. ${errorDescription}`);
                            throw new Error(errorDescription);
                        }
                    } catch (err) {
                        logger.error('ChatConnector: receive - invalid token. Check bot\'s app ID & Password.');
                        res.send(403, err);
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

    public send(messages: IMessage[], done: (err: Error, addresses?: IAddress[]) => void): void {
        let addresses: IAddress[] = [];
        async.forEachOfSeries(messages, (msg, idx, cb) => {
            try {
                if (msg.type == 'delay') {
                    setTimeout(cb, (<any>msg).value);
                } else if (msg.address && (<IChatConnectorAddress>msg.address).serviceUrl) {
                    this.postMessage(msg, (idx == messages.length - 1), (err, address) => {
                        addresses.push(address);
                        cb(err);
                    });
                } else {
                    logger.error('ChatConnector: send - message is missing address or serviceUrl.')
                    cb(new Error('Message missing address or serviceUrl.'));
                }
            } catch (e) {
                cb(e);
            }
        }, (err) => done(err, !err ? addresses : null));
    }

    public startConversation(address: IStartConversationAddress, done: (err: Error, address?: IAddress) => void): void {
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
                    members: address.members || [address.user]
                },
                json: true
            };
            if (address.activity) { options.body.activity = address.activity }
            if (address.channelData) { options.body.channelData = address.channelData }
            if (address.isGroup !== undefined) { options.body.isGroup = address.isGroup }
            if (address.topicName) { options.body.topicName = address.topicName }
            this.authenticatedRequest(options, (err, response, body) => {
                var adr: IChatConnectorAddress;
                if (!err) {
                    try {
                        var obj = typeof body === 'string' ? JSON.parse(body) : body;
                        if (obj && obj.hasOwnProperty('id')) {
                            adr = utils.clone(address);
                            adr.conversation = { id: obj['id'] };
                            if (obj['serviceUrl']) { adr.serviceUrl = obj['serviceUrl'] }
                            if (adr.id) { delete adr.id }
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

    public update(message: IMessage, done: (err: Error, address?: IAddress) => void): void {
        let address = <IChatConnectorAddress>message.address;
        if (message.address && address.serviceUrl) {
            (<any>message).id = address.id;
            this.postMessage(message, true, done, 'PUT');
        } else {
            logger.error('ChatConnector: updateMessage - message is missing address or serviceUrl.')
            done(new Error('Message missing address or serviceUrl.'), null);
        }
    }

    public delete(address: IChatConnectorAddress, done: (err: Error) => void): void {
        // Calculate path
        var path = '/v3/conversations/' + encodeURIComponent(address.conversation.id) +
            '/activities/' + encodeURIComponent(address.id);

        // Issue request
        var options: request.Options = {
            method: 'DELETE',
            // We use urlJoin to concatenate urls. url.resolve should not be used here, 
            // since it resolves urls as hrefs are resolved, which could result in losing
            // the last fragment of the serviceUrl
            url: urlJoin(address.serviceUrl, path),
            json: true
        };
        this.authenticatedRequest(options, (err, response, body) => done(err));
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

    protected onDispatchEvents(events: IEvent[], callback: (err: Error, body: any, status?: number) => void): void {
        if (events && events.length > 0) {
            if (this.isInvoke(events[0])) {
                this.onInvokeHandler(events[0], callback);
            } else {
                // Dispatch message
                this.onEventHandler(events);

                // Acknowledge that we received the events
                callback(null, null, 202);
            }
        }
    }

    private dispatch(msg: IMessage, res: IWebResponse) {
        // Dispatch message/activity
        try {
            this.prepIncomingMessage(msg);
            logger.info(msg, 'ChatConnector: message received.');

            this.onDispatchEvents([msg], (err, body, status) => {
                if (err) {
                    res.status(500);
                    res.end();
                    logger.error('ChatConnector: error dispatching event(s) - ', err.message || '');
                } else if (body) {
                    res.send(status || 200, body);
                } else {
                    res.status(status || 200);
                    res.end();
                }
            })
        } catch (e) {
            console.error(e instanceof Error ? (<Error>e).stack : e.toString());
            res.status(500);
            res.end();
        }
    }

    private isInvoke(event: IEvent): boolean {
        return (event && event.type && event.type.toLowerCase() == consts.invokeType);
    }

    private postMessage(msg: IMessage, lastMsg: boolean, cb: (err: Error, address: IAddress) => void, method = 'POST'): void {
        logger.info(address, 'ChatConnector: sending message.')
        this.prepOutgoingMessage(msg);

        // Apply address fields
        var address = <IChatConnectorAddress>msg.address;
        (<any>msg)['from'] = address.bot;
        (<any>msg)['recipient'] = address.user;
        delete msg.address;

        // Patch inputHint
        if (msg.type === 'message' && !msg.inputHint) {
            msg.inputHint = lastMsg ? 'acceptingInput' : 'ignoringInput';
        }

        // Calculate path
        var path = '/v3/conversations/' + encodeURIComponent(address.conversation.id) + '/activities';
        if (address.id && address.channelId !== 'skype') {
            path += '/' + encodeURIComponent(address.id);
        }

        // Issue request
        var options: request.Options = {
            method: method,
            // We use urlJoin to concatenate urls. url.resolve should not be used here, 
            // since it resolves urls as hrefs are resolved, which could result in losing
            // the last fragment of the serviceUrl
            url: urlJoin(address.serviceUrl, path),
            body: msg,
            json: true
        };
        this.authenticatedRequest(options, (err, response, body) => {
            if (!err) {
                if (body && body.id) {
                    // Return a new address object for the sent message
                    let newAddress = utils.clone(address);
                    newAddress.id = body.id;
                    cb(null, newAddress);
                } else {
                    cb(null, address);
                }
            } else {
                cb(err, null);
            }
        });
    }

    private authenticatedRequest(options: request.Options, callback: (error: any, response: http.IncomingMessage, body: any) => void, refresh = false): void {
        if (refresh) {
            this.accessToken = null;
        }
        this.addUserAgent(options);
        this.addAccessToken(options, (err) => {
            if (!err) {
                request(options, (err, response, body) => {
                    if (!err) {
                        switch (response.statusCode) {
                            case 401:
                            case 403:
                                if (!refresh && this.settings.appId && this.settings.appPassword) {
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

    private tokenExpired(): boolean {
        return Date.now() >= this.accessTokenExpires;
    }

    private tokenHalfWayExpired(secondstoHalfWayExpire: number = 1800, secondsToExpire: number = 300): boolean {
        var timeToExpiration = (this.accessTokenExpires - Date.now())/1000;
        return timeToExpiration < secondstoHalfWayExpire 
            && timeToExpiration > secondsToExpire;
    }

    private refreshAccessToken(cb: (err: Error, accessToken: string) => void): void {
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
        this.addUserAgent(opt);
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
    }

    public getAccessToken(cb: (err: Error, accessToken: string) => void): void {
        if (this.accessToken == null || this.tokenExpired()) {
            // Refresh access token with error handling
            this.refreshAccessToken((err, token) => {
                cb(err, this.accessToken);
            });
         
        }
        else if (this.tokenHalfWayExpired()) {
            // Refresh access token without error handling
            var oldToken = this.accessToken;
            this.refreshAccessToken((err, token) => {
                if (!err)
                    cb(null, this.accessToken);
                else
                    cb(null, oldToken);
            });
        }
        else
            cb(null, this.accessToken);
    }

    private addUserAgent(options: request.Options): void {
        if (!options.headers) {
            options.headers = {};
        }
        options.headers['User-Agent'] = USER_AGENT;
    }

    private addAccessToken(options: request.Options, cb: (err: Error) => void): void {
        if (this.settings.appId && this.settings.appPassword) {
            this.getAccessToken((err, token) => {
                if (!err && token) {
                    if (!options.headers) {
                        options.headers = {};
                    } 
                    options.headers['Authorization'] = 'Bearer ' + token
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

        // Ensure local timestamp
        if (!msg.localTimestamp) {
            msg.localTimestamp = new Date().toISOString();
        }
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

