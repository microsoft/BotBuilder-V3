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

export class ReceiptCard implements IIsAttachment {
    protected data = {
        contentType: 'application/vnd.microsoft.card.receipt',
        content: <IReceiptCard>{}
    };
    
    constructor(protected session?: ses.Session) {
        
    }
    
    public title(text: string|string[], ...args: any[]): this {
        if (text) {
            this.data.content.title = msg.fmtText(this.session, text, args);
        }
        return this;
    }
    
    public items(list: IReceiptItem[]|IIsReceiptItem[]): this {
        this.data.content.items = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                var item = list[i];
                this.data.content.items.push(item.hasOwnProperty('toItem') ? (<IIsReceiptItem>item).toItem() : <IReceiptItem>item);    
            }
        }
        return this;
    }

    public facts(list: IFact[]|IIsFact[]): this {
        this.data.content.facts = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                var fact = list[i];
                this.data.content.facts.push(fact.hasOwnProperty('toFact') ? (<IIsFact>fact).toFact() : <IFact>fact);    
            }
        }
        return this;
    }

    public tap(action: IAction|IIsAction): this {
        if (action) {
            this.data.content.tap = action.hasOwnProperty('toAction') ? (<IIsAction>action).toAction() : <IAction>action;
        }
        return this;
    }
    
    public total(v: string): this {
        this.data.content.total = v || '';
        return this;
    }

    public tax(v: string): this {
        this.data.content.tax = v || '';
        return this;
    }

    public vat(v: string): this {
        this.data.content.vat = v || '';
        return this;
    }

    public buttons(list: IAction[]|IIsAction[]): this {
        this.data.content.buttons = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                var action = list[i];
                this.data.content.buttons.push(action.hasOwnProperty('toAction') ? (<IIsAction>action).toAction() : <IAction>action);    
            }
        }
        return this;
    }

    public toAttachment(): IAttachment {
        return this.data;
    }
}