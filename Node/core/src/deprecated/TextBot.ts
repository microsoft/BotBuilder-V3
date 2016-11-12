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

import { UniversalBot, IUniversalBotSettings } from '../bots/UniversalBot';
import { ConsoleConnector } from '../bots/ConsoleConnector';
import { Dialog } from '../dialogs/Dialog';
import { IDialogWaterfallStep } from '../dialogs/SimpleDialog';

export interface IConsoleConnectorOptions {
    appId?: string;
    appSecret?: string;
    localizer?: ILocalizer;
    minSendDelay?: number;
    defaultDialogId?: string;
    defaultDialogArgs?: any;
    groupWelcomeMessage?: string;
    userWelcomeMessage?: string;
    goodbyeMessage?: string;

    // Ignored
    endpoint?: string;
    defaultFrom?: any;

    // Unsupported options
    userStore?: any;
    sessionStore?: any;
}

export class TextBot  {
    private connector: ConsoleConnector;
    private bot: UniversalBot;
    private groupWelcomeMessage: string;
    private userWelcomeMessage: string;
    private goodbyeMessage: string;

    constructor(options: IConsoleConnectorOptions = {}) {
        console.warn('TextBot class is deprecated. Use UniversalBot with a ConsoleConnector class.')

        // Map options into settings
        var oBot: IUniversalBotSettings = {};
        for (var key in options) {
            switch (key) {
                case 'defaultDialogId':
                    oBot.defaultDialogId = options.defaultDialogId;
                    break;
                case 'defaultDialogArgs':
                    oBot.defaultDialogArgs = options.defaultDialogArgs;
                    break;
                 case 'groupWelcomeMessage':
                    this.groupWelcomeMessage = options.groupWelcomeMessage;
                    break;
                case 'userWelcomeMessage':
                    this.userWelcomeMessage = options.userWelcomeMessage;
                    break;
                case 'goodbyeMessage':
                    this.goodbyeMessage = options.goodbyeMessage;
                    break;
                
                case 'userStore':
                case 'sessionStore':
                    console.error('TextBot custom stores no longer supported. Use UniversalBot with a custom IBotStorage implementation instead.');
                    throw new Error('TextBot custom stores no longer supported.');
            }
        }

        // Initialize connector & universal bot
        this.connector = new ConsoleConnector();
        this.bot = new UniversalBot(this.connector, oBot);
    }

    public on(event: string, listener: Function): this {
        this.bot.on(event, listener);
        return this;
    }

    public add(id: string, dialog?: Dialog | IDialogWaterfallStep[] | IDialogWaterfallStep): this {
        this.bot.dialog(id, dialog); 
        return this;
    }

    public configure(options: IConsoleConnectorOptions) {
        console.error("TextBot.configure() is no longer supported. You should either pass all options into the constructor or update code to use the new UniversalBot class.");
        throw new Error("TextBot.configure() is no longer supported.");
    }

    public listenStdin(): any {
        return this.connector.listen();
    }

    public beginDialog(address: any, dialogId: string, dialogArgs?: any): void {
        console.error("TextBot.beginDialog() is no longer supported. The schema for sending proactive messages has changed and you should update your code to use the new UniversalBot class.");
        throw new Error("TextBot.beginDialog() is no longer supported.");
    }

    public processMessage(message: IMessage, callback?: (err: Error, reply: IMessage) => void): void {
        console.error("TextBot.processMessage() is no longer supported. The schema for messages has changed and you should update your code to use the new UniversalBot class.");
        throw new Error("TextBot.processMessage() is no longer supported.");
    }
}
