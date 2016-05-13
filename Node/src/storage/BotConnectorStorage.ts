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

import storage = require('./BotStorage');
import request = require('request');

export interface IBotConnectorStorageOptions {
    endpoint: string;
    appId: string;
    appSecret: string;
}

export interface IBotConnectorStorageData extends storage.IBotStorageData {
    userDataHash?: string;
    conversationDataHash?: string;    
}

export class BotConnectorStorage implements storage.IBotStorage {
    constructor(private options: IBotConnectorStorageOptions) {
        
    }

    public get(address: storage.IBotStorageAddress, callback: (err: Error, data: storage.IBotStorageData) => void): void {
        var ops = 2;
        var settings = this.options;
        var data: storage.IBotStorageData = {};
        function read(path: string, field: string) {
            if (path) {
                var options: request.Options = {
                    url: settings.endpoint + '/bot/v1.0/bots' + path
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
                request.get(options, (err, response, body) => {
                    if (!err) {
                        try {
                            (<any>data)[field + 'Hash'] = body;
                            (<any>data)[field] = typeof body === 'string' ? JSON.parse(body) : null;
                        } catch (e) {
                            err = e instanceof Error ? e : new Error(e.toString()); 
                        }
                    }
                    if (callback && (err || --ops == 0)) {
                        callback(err, data);
                        callback = null;
                    }
                });
            } else if (callback && --ops == 0) {
                callback(null, data);
            }
        }
        var userPath = address.userId ? '/users/' + address.userId : null;
        var convoPath = address.conversationId ? '/conversations/' + address.conversationId + userPath : null;
        read(userPath, 'userData');
        read(convoPath, 'conversationData');
    }

    public save(address: storage.IBotStorageAddress, data: storage.IBotStorageData, callback?: (err: Error) => void): void {
        var ops = 2;
        var settings = this.options;
        function write(path: string, field: string) {
            if (path) {
                var err: Error;
                var body: string;
                var hashField = field + 'Hash';
                try {
                    body = JSON.stringify((<any>data)[field]);
                } catch (e) {
                    err = e instanceof Error ? e : new Error(e.toString());
                }
                if (!err && (!(<any>data)[hashField] || body !== (<any>data)[hashField])) {
                    (<any>data)[hashField] = body;
                    var options: request.Options = {
                        url: settings.endpoint + '/bot/v1.0/bots' + path,
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
                    request.post(options, (err) => {
                        if (callback && (err || --ops == 0)) {
                            callback(err);
                            callback = null;
                        }
                    });
                } else if (callback && (err || --ops == 0)) {
                    callback(err);
                    callback = null;
                }
            } else if (callback && --ops == 0) {
                callback(null);
            }
        }
        var userPath = address.userId ? '/users/' + address.userId : null;
        var convoPath = address.conversationId ? '/conversations/' + address.conversationId + userPath : null;
        write(userPath, 'userData');
        write(convoPath, 'conversationData');
        
    }
}