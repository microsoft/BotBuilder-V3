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

import dlg = require('./Dialog');
import utils = require('../utils');
import request = require('request');
import url = require('url');
import http = require('http');
import actions = require('./DialogAction');

export interface IDialogManagerSettings {
    agentId: string;
    appId: string;
    endpoint?: string;
}

export interface IDialogManagerResponse {
    _type: string;
    instrumentation: any;
    text?: string;
    state: string;
}

export class DialogManager extends dlg.Dialog {
    private defaultHandler: IDialogHandler<any>;

    constructor(private settings: IDialogManagerSettings) {
        super();
        if (!this.settings.endpoint) {
            this.settings.endpoint = 'https://www.bingapis.com';
        }
    }

    public replyReceived(session: ISession, recognizeResult?: dlg.IRecognizeResult): void {
        var msg = session.message;
        this.process(msg.text, msg.address.conversation.id, (err, response, body) => {
            if (!err) {
                var handled = false;
                if (body && body.state) {
                    switch (body.state) {
                        case 'Completed':
                        case 'PartialResponse':
                        case 'ChatAnswer':
                            if (body.text) {
                                handled = true;
                                session.send(body.text);
                            }
                            break;                        
                    }
                }
                if (!handled && this.defaultHandler) {
                    try {
                        this.defaultHandler(session, body);
                    } catch (e) {
                        session.error(e);
                    }
                }
            } else {
                session.error(err);
            }
        });
    }

    public dialogResumed<T>(session: ISession, result: dlg.IDialogResult<T>): void {
        if (this.defaultHandler) {
            try {
                this.defaultHandler(session, result);
            } catch (e) {
                session.error(e);
            }
        } else {
            super.dialogResumed(session, result);
        }
    }

    public process(utterance: string, conversationId: string, callback: (err: Error, response: http.IncomingMessage, body: IDialogManagerResponse) => void): void {
        try {
            var path = '/api/v5/dialog/agents/' +
                encodeURIComponent(this.settings.agentId) +
                '/conversations/' +
                encodeURIComponent(conversationId) +
                '/messages?appid=' +
                encodeURIComponent(this.settings.appId) +
                '&includeSemanticFrames=false&includeTaskFrameStates=false';
            var options: request.Options = {
                method: 'POST',
                url: url.resolve(this.settings.endpoint, path),
                body: { Text: utterance },
                json: true
            };
            request(options, (err, response, body) => {
                try {
                    if (!err) {
                        if (response.statusCode < 400) {
                            callback(null, response, body);
                        } else {
                            var txt = "Request to '" + options.url + "' failed: [" + response.statusCode + "] " + response.statusMessage;
                            callback(new Error(txt), response, null);
                        }
                    } else {
                        callback(err, null, null);
                    }
                } catch (e) {
                    console.error(e.toString());
                }
            });
        } catch (err) {
            callback(err instanceof Error ? err : new Error(err.toString()), null, null);
        }
    }

    public onDefault(dialogId: string|actions.IDialogWaterfallStep[]|actions.IDialogWaterfallStep, dialogArgs?: any): this {
        // Register handler
        if (Array.isArray(dialogId)) {
            this.defaultHandler = actions.waterfall(dialogId);
        } else if (typeof dialogId === 'string') {
            this.defaultHandler = actions.DialogAction.beginDialog(<string>dialogId, dialogArgs);
        } else {
            this.defaultHandler = actions.waterfall([<actions.IDialogWaterfallStep>dialogId]);
        }
        return this;
    }
}
