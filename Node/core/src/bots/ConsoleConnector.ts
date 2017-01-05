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
import { Message } from '../Message';
import * as utils from '../utils';
import * as readline from 'readline';

export class ConsoleConnector implements IConnector {
    private onEventHandler: (events: IEvent[], cb?: (err: Error) => void) => void;
    private onInvokeHandler: (event: IEvent, cb?: (err: Error, body: any, status?: number) => void) => void;
    private rl: readline.ReadLine;
    private replyCnt = 0;

    public listen(): this {
        this.rl = readline.createInterface({ input: process.stdin, output: process.stdout, terminal: false });
        this.rl.on('line', (line: string) => {
            this.replyCnt = 0;
            line = line || '';
            if (line.toLowerCase() == 'quit') {
                this.rl.close();
                process.exit();
            } else {
                this.processMessage(line);
            }
        });
        return this;
    }

    public processMessage(line: string): this {
        if (this.onEventHandler) {
            // TODO: Add some sort of logic to support attachment uploads.
            var msg = new Message()
                .address({
                    channelId: 'console',
                    user: { id: 'user', name: 'User1' },
                    bot: { id: 'bot', name: 'Bot' },
                    conversation: { id: 'Convo1' }
                })
                .timestamp()
                .text(line);
            this.onEventHandler([msg.toMessage()]);
        }
        return this;
    }

    public onEvent(handler: (events: IEvent[], cb?: (err: Error) => void) => void): void {
        this.onEventHandler = handler;
    }

    public onInvoke(handler: (event: IEvent, cb?: (err: Error, body: any, status?: number) => void) => void): void {
        this.onInvokeHandler = handler;
    }
    public send(messages: IMessage[], done: (err: Error) => void): void {
        for (var i = 0; i < messages.length; i++ ){
            if (this.replyCnt++ > 0) {
                console.log();
            }
            var msg = messages[i];
            if (msg.text) {
                log(msg.text);
            }
            if (msg.attachments && msg.attachments.length > 0) {
                for (var j = 0; j < msg.attachments.length; j++) {
                    if (j > 0) {
                        console.log();
                    }
                    renderAttachment(msg.attachments[j]);
                }
            }
        }

        done(null);
    }

    public startConversation(address: IAddress, cb: (err: Error, address?: IAddress) => void): void {
        var adr = utils.clone(address);
        adr.conversation = { id: 'Convo1' };
        cb(null, adr);
    }
}

function renderAttachment(a: IAttachment) {
    switch (a.contentType) {
        case 'application/vnd.microsoft.card.hero':
        case 'application/vnd.microsoft.card.thumbnail':
            var tc: IThumbnailCard = a.content;
            if (tc.title) {
                if (tc.title.length <= 40) {
                    line('=', 60, tc.title);
                } else {
                    line('=', 60);
                    wrap(tc.title, 60, 3);
                }
            }
            if (tc.subtitle) {
                wrap(tc.subtitle, 60, 3);
            }
            if (tc.text) {
                wrap(tc.text, 60, 3);
            }
            renderImages(tc.images);
            renderButtons(tc.buttons);
            break;
        case 'application/vnd.microsoft.card.signin':
        case 'application/vnd.microsoft.card.receipt':
        default:
            line('.', 60, a.contentType);
            if (a.contentUrl) {
                wrap(a.contentUrl, 60, 3);
            } else {
                log(JSON.stringify(a.content));
            }
            break;
    }
}

function renderImages(images: ICardImage[]) {
    if (images && images.length) {
        line('.', 60, 'images');
        var bullet = images.length > 1 ? '* ' : '';
        for (var i = 0; i < images.length; i++) {
            var img = images[i];
            if (img.alt) {
                wrap(bullet + img.alt + ': ' + img.url, 60, 3);
            } else {
                wrap(bullet + img.url, 60, 3);
            }
        }
    }
}

function renderButtons(actions: ICardAction[]) {
    if (actions && actions.length) {
        line('.', 60, 'buttons');
        var bullet = actions.length > 1 ? '* ' : '';
        for (var i = 0; i < actions.length; i++) {
            var a = actions[i];
            if (a.title == a.value) {
                wrap(bullet + a.title, 60, 3);
            } else {
                wrap(bullet + a.title + ' [' + a.value + ']', 60, 3);
            }
        }
    }
}

function line(char: string, length: number, title?: string) {
    if (title) {
        var txt = repeat(char, 2);
        txt += '[' + title + ']';
        if (length > txt.length) {
            txt += repeat(char, length - txt.length);
        }
        log(txt);
    } else {
        log(repeat(char, length));
    }
}

function wrap(text: string, length: number, indent = 0) {
    var buffer = '';
    var pad = indent ? repeat(' ', indent) : '';
    var tokens = text.split(' ');
    length -= pad.length;
    for (var i = 0; i < tokens.length; i++) {
        var t = tokens[i];
        if (buffer.length) {
            if ((buffer.length + 1 + t.length) > length) {
                log(pad + buffer);
                buffer = t;
            } else {
                buffer += ' ' + t;
            }
        } else if (t.length < length) {
            buffer = t;
        } else {
            log(pad + t);
        }
    }
    if (buffer.length) {
        log(pad + buffer);
    }
}

function repeat(char: string, length: number): string {
    var txt = '';
    for (var i = 0; i < length; i++) {
        txt += char;
    }
    return txt;
}

function log(text: string) {
    console.log(text);
}