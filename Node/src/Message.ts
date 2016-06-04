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

import ses = require('./Session');
import sprintf = require('sprintf-js');
import utils = require('./utils');

export interface IChannelDataMap {
    [channelId: string]: any;
}

export var LayoutStyle = {
    auto: <string>null,
    card: 'card',
    signinCard: 'card.signin',
    receiptCard: 'card.receipt',
    carouselCard: 'card.carousel'
};

export class Message implements IIsMessage {
    private data = <IMessage>{};
    private style = LayoutStyle.auto;
    
    constructor(private session?: ses.Session) {
        this.data.type = 'message';
        if (this.session) {
            var m = this.session.message;
            if (m.language) {
                this.data.language = m.language;
            }
        }
    }
    
    public layoutStyle(style: string): this {
        this.style = style;
        return this;
    }
    
    public language(local: string): this {
        if (local) {
            this.data.language = local;
        } else if (this.data.hasOwnProperty('language')) {
            delete this.data.language;
        }
        return this;
    }
    
    public text(text: string|string[], ...args: any[]): this {
        if (text) {
            this.data.text = fmtText(this.session, text, args);
        }
        return this; 
    }
    
    public ntext(msg: string|string[], msg_plural: string|string[], count: number): this {
        var fmt = count == 1 ? Message.randomPrompt(msg) : Message.randomPrompt(msg_plural);
        if (this.session) {
            // Run prompt through localizer
            fmt = this.session.gettext(fmt);
        }
        this.data.text = sprintf.sprintf(fmt, count);
        return this;
    }

    public compose(prompts: string[][], ...args: any[]): this {
        if (prompts) {
            this.data.text = Message.composePrompt(this.session, prompts, args);
        }
        return this;
    }

    public summary(text: string|string[], ...args: any[]): this {
        if (text) {
            this.data.summary = fmtText(this.session, text, args);
        }
        return this; 
    }

    public attachments(list: IAttachment[]|IIsAttachment[]): this {
        this.data.attachments = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                this.addAttachment(list[i]);
            }
        }
        return this;
    }
       
    public addAttachment(attachment: IAttachment|IIsAttachment): this {
        if (attachment) {
            var a: IAttachment = attachment.hasOwnProperty('toAttachment') ? (<IIsAttachment>attachment).toAttachment() : <IAttachment>attachment; 
            if (!this.data.attachments) {
                this.data.attachments = [a];
            } else {
                this.data.attachments.push(a);
            }
        }
        return this;
    }
    
    public entities(list: Object[]): this {
        this.data.entities = list || [];
        return this;
    }
    
    public addEntity(obj: Object): this {
        if (obj) {
            if (!this.data.entities) {
                this.data.entities = [obj];
            } else {
                this.data.entities.push(obj);
            }
        }
        return this;
    }

    public channelData(map: IChannelDataMap): this {
        if (map) {
            var channelId = this.session ? this.session.message.channelId : '*';
            if (map.hasOwnProperty(channelId)) {
                this.data.channelData = map[channelId];
            } else if (map.hasOwnProperty('*')) {
                this.data.channelData = map['*'];
            }
        }
        return this;
    }
    
    public toMessage(): IMessage {
        // Find layout style
        var style = this.style;
        if (!style && this.data.attachments) {
            var cards = 0;
            var hasReceipt = false;
            var hasSignin = false;
            var attachments = this.data.attachments;
            for (var i = 0; i < attachments.length; i++) {
                var ct = attachments[i].contentType || '';
                if (ct.indexOf('vnd.microsoft.card') > 0) {
                    cards++;
                    if (ct.indexOf('card.signin') > 0) {
                        hasSignin = true;
                    } else if (ct.indexOf('card.receipt') > 0) {
                        hasReceipt = true;
                    }
                }
            }
            if (cards > 0) {
                if (cards > 1) {
                    style = LayoutStyle.carouselCard;
                } else if (hasSignin) {
                    style = LayoutStyle.signinCard;
                } else if (hasReceipt) {
                    style = LayoutStyle.receiptCard;
                } else {
                    style = LayoutStyle.card;
                }
            }
        } 
        
        // Return cloned message
        var msg: IMessage = utils.clone(this.data);
        if (style) {
            msg.type += '/' + style;
        }
        return msg;
    }

    static randomPrompt(prompts: string|string[]): string {
        if (Array.isArray(prompts)) {
            var i = Math.floor(Math.random() * prompts.length);
            return prompts[i];
        } else {
            return prompts;
        }
    }
    
    static composePrompt(session: ses.Session, prompts: string[][], args?: any[]): string {
        var connector = '';
        var prompt = '';
        for (var i = 0; i < prompts.length; i++) {
            var txt = Message.randomPrompt(prompts[1]);
            prompt += connector + (session ? session.gettext(txt) : txt);
            connector = ' ';
        }
        return args && args.length > 0 ? sprintf.vsprintf(prompt, args) : prompt;
    }

    //-------------------
    // DEPRECATED METHODS
    //-------------------
    
    public setLanguage(local: string): this {
        console.warn("Message.setLanguage() is deprecated. Use Message.language() instead.");
        return this.language(local);
    }
    
    public setText(session: ses.Session, prompts: string|string[], ...args: any[]): this {
        console.warn("Message.setText() is deprecated. Use Message.text() instead.");
        if (session && !this.session) {
            this.session = session;
        }
        args.unshift(prompts);
        return Message.prototype.text.apply(this, args);
    }

    public setNText(session: ses.Session, msg: string, msg_plural: string, count: number): this {
        console.warn("Message.setNText() is deprecated. Use Message.ntext() instead.");
        if (session && !this.session) {
            this.session = session;
        }
        return this.ntext(msg, msg_plural, count);
    }
    
    public composePrompt(session: ses.Session, prompts: string[][], ...args: any[]): this {
        console.warn("Message.composePrompt() is deprecated. Use Message.compose() instead.");
        if (session && !this.session) {
            this.session = session;
        }
        args.unshift(prompts);
        return Message.prototype.compose.apply(this, args);
    }

    public setChannelData(data: any): this {
        console.warn("Message.setChannelData() is deprecated. Use Message.channelData() instead.");
        return this.channelData({ '*': data });
    }
}

export function fmtText(session: ses.Session, prompts: string|string[], args?: any[]): string {
    var fmt = Message.randomPrompt(prompts);
    if (session) {
        // Run prompt through localizer
        fmt = this.session.gettext(fmt);
    }
    return args.length > 0 ? sprintf.vsprintf(fmt, args) : fmt; 
}