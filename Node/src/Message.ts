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
import hc = require('./cards/HeroCard');
import img = require('./cards/CardImage');
import ca = require('./cards/CardAction');

export interface IChannelDataMap {
    [channelId: string]: any;
}

export var LayoutStyle = {
    auto: <string>null,
    image: 'image',
    moji: 'moji',
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
            if (m.local) {
                this.data.local = m.local;
            }
            if (m.address) {
                this.data.address = m.address;
            }
        }
    }
    
    public layoutStyle(style: string): this {
        this.style = style;
        return this;
    }
    
    public local(local: string): this {
        this.data.local = local;
        return this;
    }
    
    public text(text: string|string[], ...args: any[]): this {
        this.data.text = text ? fmtText(this.session, text, args) : '';
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
        this.data.summary = text ? fmtText(this.session, text, args) : '';
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
            // Upgrade attachment if specified using old schema
            var a: IAttachment = (<IIsAttachment>attachment).toAttachment ? (<IIsAttachment>attachment).toAttachment() : <IAttachment>attachment;
            a = this.upgradeAttachment(a);
            if (!this.data.attachments) {
                this.data.attachments = [a];
            } else {
                this.data.attachments.push(a);
            }
        }
        return this;
    }

    private upgradeAttachment(a: IAttachment): IAttachment {
        var isOldSchema = false;
        for (var prop in a) {
            switch (prop) {
                case 'actions':
                case 'fallbackText':
                case 'title':
                case 'titleLink':
                case 'text':
                case 'thumbnailUrl':
                    isOldSchema = true;
                    break;
            }
        }
        if (isOldSchema) {
            console.warn('Using old attachment schema. Upgrade to new card schema.');
            var v2 = <IAttachmentV2>a;
            var card = new hc.HeroCard();
            if (v2.title) {
                card.title(v2.title);
            }
            if (v2.text) {
                card.text(v2.text);
            }
            if (v2.thumbnailUrl) {
                card.images([new img.CardImage().url(v2.thumbnailUrl)]);
            }
            if (v2.titleLink) {
                card.tap(ca.CardAction.openUrl(null, v2.titleLink));
            }
            if (v2.actions) {
                var list: ca.CardAction[] = [];
                for (var i = 0; i < v2.actions.length; i++) {
                    var old = v2.actions[i];
                    var btn = old.message ? 
                        ca.CardAction.postBack(null, old.message, old.title) : 
                        ca.CardAction.openUrl(null, old.url, old.title);
                    if (old.image) {
                        btn.image(old.image);
                    }
                    list.push(btn);
                }
                card.buttons(list);
            }
            return card.toAttachment();
        } else {
            return a;
        }
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
    
    public address(adr: IAddress): this {
        if (adr) {
            this.data.address = adr;
        }
        return this;
    }
    
    public timestamp(time?: string): this {
        this.data.timestamp = time || new Date().toISOString();
        return this;
    }

    public channelData(map: IChannelDataMap): this {
        if (map) {
            var channelId = this.data.address ? this.data.address.channelId : '*';
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
            } else if (attachments.length > 0) {
                var ct = attachments[0].contentType || '';
                if (ct.indexOf('image') == 0 || ct.indexOf('vnd.microsoft.image') > 0) {
                    style = LayoutStyle.image;
                } else if (ct.indexOf('vnd.microsoft.moji') > 0) {
                    style = LayoutStyle.moji;
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
        console.warn("Message.setLanguage() is deprecated. Use Message.local() instead.");
        return this.local(local);
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
        fmt = session.gettext(fmt);
    }
    return args && args.length > 0 ? sprintf.vsprintf(fmt, args) : fmt; 
}