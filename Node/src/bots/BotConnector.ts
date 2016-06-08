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
import events = require('events');
import request = require('request');
import async = require('async');
import url = require('url');

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

export class BotConnector extends events.EventEmitter implements ub.IConnector {
    private handler: (messages: IMessage[], cb?: (err: Error) => void) => void;
    
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

    private dispatch(messages: IMessage|IMessage[], res: IWebResponse) {
        // Dispatch messages/activities
        var list: IMessage[] = Array.isArray(messages) ? messages : [messages];
        list.forEach((msg) => {
            try {
                // Break out address fields
                var address = <IAddress>{};
                moveFields(msg, address, <any>toAddress);
                msg.address = address;

                // Dispatch message
                if (msg.type && msg.type.toLowerCase().indexOf('message') == 0) {
                    this.handler([msg]);
                } else {
                    this.emit(msg.type, msg);
                }
            } catch (e) {
                console.error(e.toString());
            }
        });

        // Acknowledge that we recieved the message(s)
        res.status(202);
        res.end();
    }
    
    public onMessage(handler: (messages: IMessage[], cb?: (err: Error) => void) => void): void {
        this.handler = handler;
    }
    
    public send(messages: IMessage[], cb: (err: Error, conversationId?: string) => void): void {
        var conversationId: string;
        async.eachSeries(messages, (msg, cb) => {
            try {
                var address = <IBotConnectorAddress>msg.address;
                if (address && address.serviceUrl) {
                    delete msg.address;
                    if (!address.conversation && conversationId) {
                        address.conversation = { id: conversationId };
                    }
                    this.postMessage(address, msg, (err, id) => {
                        if (!err && id) {
                            conversationId = id;
                        }
                        cb(err);
                    });
                } else {
                    cb(new Error('Message missing address or serviceUrl.'));
                }
            } catch (e) {
                cb(e);
            }
        }, (err) => {
            cb(err, conversationId);
        });
    }

    private postMessage(address: IBotConnectorAddress, msg: IMessage, cb: (err: Error, conversationId: string) => void): void {
        // Calculate path
        var path = '/api/v3/conversations';
        if (address.conversation && address.conversation.id) {
            path += '/' + encodeURIComponent(address.conversation.id) + '/activities';
            if (address.id) {
                path += '/' + encodeURIComponent(address.id);
            }
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
        request(options, (err, response, body) => {
            var conversationId: string;
            if (!err && body) {
                try {
                    var obj = typeof body === 'string' ? JSON.parse(body) : body;
                    if (obj.hasOwnProperty('conversationId')) {
                        conversationId = obj['conversationId'];
                    }
                } catch (e) {
                    console.error('Error parsing channel response: ' + e.toString());
                }
            }
            cb(err, conversationId);
        });
    }
}

var toAddress = {
    'id': 'id',
    'channelId': 'channelId',
    'from': 'user',
    'to': 'conversation',
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