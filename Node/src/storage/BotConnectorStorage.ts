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

import storage = require('./Storage');
import request = require('request');

export interface IBotConnectorStorageOptions {
    endpoint: string;
    appId: string;
    appSecret: string;
}

export class BotConnectorStorage implements storage.IStorage {
    constructor(private options: IBotConnectorStorageOptions) {
        
    }

    public get(id: string, callback: (err: Error, data: any) => void): void {
        var settings = this.options;
        var options: request.Options = {
            url: settings.endpoint + '/bot/v1.0/bots' + id
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
            try {
                var data: any;
                if (!err && typeof body === 'string') {
                    data = JSON.parse(body);
                }
                callback(err, data);
            } catch (e) {
                callback(e instanceof Error ? e : new Error(e.toString()), null);
            }
        });
    }

    public save(id: string, data: any, callback?: (err: Error) => void): void {
        var settings = this.options;
        var options: request.Options = {
            url: settings.endpoint + '/bot/v1.0/bots' + id,
            body: data
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