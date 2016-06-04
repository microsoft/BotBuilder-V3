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

import ses = require('../Session');
import msg = require('../Message');

export class ReceiptItem implements IIsReceiptItem {
    private data = <IReceiptItem>{};
    
    constructor(private session?: ses.Session) {
        
    }
    
    public title(text: string|string[], ...args: any[]): this {
        if (text) {
            this.data.title = msg.fmtText(this.session, text, args);
        }
        return this;
    }

    public subtitle(text: string|string[], ...args: any[]): this {
        if (text) {
            this.data.subtitle = msg.fmtText(this.session, text, args);
        }
        return this;
    }
    
    public text(text: string|string[], ...args: any[]): this {
        if (text) {
            this.data.text = msg.fmtText(this.session, text, args);
        }
        return this;
    }
    
    public image(img: IImage|IIsImage): this {
        if (img) {
            this.data.image = img.hasOwnProperty('toImage') ? (<IIsImage>img).toImage() : <IImage>img;
        }
        return this;
    }

    public price(v: string): this {
        this.data.price = v || '';
        return this;
    }
    
    public quantity(v: string): this {
        this.data.quantity = v || '';
        return this;
    }
    
    public tap(action: IAction|IIsAction): this {
        if (action) {
            this.data.tap = action.hasOwnProperty('toAction') ? (<IIsAction>action).toAction() : <IAction>action;
        }
        return this;
    }
    
    public toItem(): IReceiptItem {
        return this.data;    
    }
}