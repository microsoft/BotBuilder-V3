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

import ses = require('../CallSession');
import uuid = require('node-uuid');
import prompt = require('./Prompt');

export class PlayPromptAction implements IIsAction {
    private data = <IPlayPromptAction>{};
    
    constructor(private session?: ses.CallSession) {
        this.data.action = 'playPrompt';
        this.data.operationId = uuid.v4();
    }
    
    public prompts(list: IPrompt[]|IIsPrompt[]): this {
        this.data.prompts = [];
        if (list) {
           for (var i = 0; i < list.length; i++) {
               var p = list[i];
               this.data.prompts.push((<IIsPrompt>p).toPrompt ? (<IIsPrompt>p).toPrompt() : p);
           }
        }
        return this;
    }

    public toAction(): IAction {
        return this.data;
    }

    static text(session: ses.CallSession, text: string|string[], ...args: any[]): PlayPromptAction {
        args.unshift(text);
        var p = new prompt.Prompt(session);
        prompt.Prompt.prototype.value.apply(p, args);
        return new PlayPromptAction(session).prompts([p]);
    }

    static file(session: ses.CallSession, uri: string): PlayPromptAction {
        return new PlayPromptAction(session).prompts([prompt.Prompt.file(session, uri)]);
    }
    
    static silence(session: ses.CallSession, time: number): PlayPromptAction {
        return new PlayPromptAction(session).prompts([prompt.Prompt.silence(session, time)]);
    }
}