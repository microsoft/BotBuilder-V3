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

import { IRecognizeContext } from './IntentRecognizerSet';
import { IIntentRecognizer, IIntentRecognizerResult } from './IntentRecognizerSet';
import * as utils from '../utils';

export interface IRegExpMap {
    [local: string]: RegExp;
}

export class RegExpRecognizer implements IIntentRecognizer {
    private expressions: IRegExpMap;

    constructor(public intent: string, expressions: RegExp|IRegExpMap) {
        if (expressions instanceof RegExp) {
            this.expressions = { '*': <RegExp>expressions };
        } else {
            this.expressions = <IRegExpMap>(expressions || {});
        }
    }

    public recognize(context: IRecognizeContext, cb: (err: Error, result: IIntentRecognizerResult) => void): void {
        var result: IIntentRecognizerResult = { score: 0.0, intent: null };
        if (context && context.message && context.message.text) {
            var utterance = context.message.text;
            var locale = context.locale || '*';
            var exp = this.expressions.hasOwnProperty(locale) ? this.expressions[locale] : this.expressions['*'];
            if (exp) {
                var matches = exp.exec(context.message.text);
                if (matches && matches.length) {
                    var matched = matches[0];
                    result.score = matched.length / context.message.text.length;
                    result.intent = this.intent;
                    result.expression = exp;
                    result.matched = matches;
                }
                cb(null, result);
            } else {
                cb(new Error("Expression not found for locale '" + locale + "'."), null);
            }
        } else {
            cb(null, result);
        }
    }
}
