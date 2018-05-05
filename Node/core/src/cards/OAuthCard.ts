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

import { Session } from '../Session';
import { ChatConnector } from '../bots/ChatConnector';
import { fmtText, Message } from '../Message';
import { SigninCard } from './SigninCard';

export class OAuthCard implements IIsAttachment {
    private data = {
        contentType: 'application/vnd.microsoft.card.oauth',
        content: <IOAuthCard>{}
    };
    
    constructor(private session?: Session) {
    }
    
    public connectionName(name: string): this {
        if (name) {
            this.data.content.connectionName = name; 
        }
        return this; 
    }

    public text(prompts: string|string[], ...args: any[]): this {
        if (prompts) {
            this.data.content.text = fmtText(this.session, prompts, args); 
        }
        return this; 
    }
   
    public button(title: string|string[]): this {
        if (title) {
            this.data.content.buttons = [{
                type: 'signin',
                title: fmtText(this.session, title),
                value: undefined
            }];
        }
        return this;
    }
    
    public toAttachment(): IAttachment {
        return this.data;
    }

    public static create(connector: ChatConnector, session: Session, connectionName: string, text: string, buttonTitle: string, done: (err: Error, message: Message) => void): void {
        var msg = new Message(session);

        var asSignInCard: boolean = false;
        switch (session.message.address.channelId) {
            case 'msteams':
            case 'cortana':
            case 'skype':
            case 'skypeforbusiness':
                asSignInCard = true;
                break;
        }

        if (asSignInCard) {
            connector.getSignInLink(session.message.address, connectionName, (getSignInLinkErr: Error, link: string) => {
                if (getSignInLinkErr) {
                    done(getSignInLinkErr, undefined);
                } else {
                    msg.attachments([
                        new SigninCard(session)
                        .text(text)
                        .button(buttonTitle, link)
                    ]);
                    done(undefined, msg);
                }
            });
        } else {
            msg.attachments([ 
                new OAuthCard(session) 
                    .text(text) 
                    .connectionName(connectionName)
                    .button(buttonTitle) 
            ]);
            done(undefined, msg);
        }
    }
}