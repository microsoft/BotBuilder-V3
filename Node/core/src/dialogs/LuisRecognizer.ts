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

import { IIntentRecognizer, IRecognizeContext, IIntentRecognizerResult } from './IntentRecognizerSet';
import * as utils from '../utils';
import * as request from 'request';

export interface ILuisModelMap {
    [local: string]: string;
}

export class LuisRecognizer implements IIntentRecognizer {
    private models: ILuisModelMap;

    constructor(models: string|ILuisModelMap) {
        if (typeof models == 'string') {
            this.models = { '*': <string>models };
        } else {
            this.models = <ILuisModelMap>(models || {});
        }
    }

    public recognize(context: IRecognizeContext, cb: (err: Error, result: IIntentRecognizerResult) => void): void {
        var result: IIntentRecognizerResult = { score: 0.0, intent: null };
        if (context && context.message && context.message.text) {
            var utterance = context.message.text;
            var locale = context.locale || '*';
            var model = this.models.hasOwnProperty(locale) ? this.models[locale] : this.models['*'];
            if (model) {
                LuisRecognizer.recognize(utterance, model, (err, intents, entities) => {
                    if (!err) {
                        result.intents = intents;
                        result.entities = entities;

                        // Return top intent
                        var top: IIntent;
                        intents.forEach((intent) => {
                            if (top) {
                                if (intent.score > top.score) {
                                    top = intent;
                                }
                            } else {
                                top = intent;
                            }
                        });
                        if (top) {
                            result.score = top.score;
                            result.intent = top.intent;

                            // Correct score for 'none' intent
                            // - The 'none' intent often has a score of 1.0 which
                            //   causes issues when trying to recognize over multiple
                            //   model. Setting to 0.1 lets the intent still be 
                            //   triggered but keeps it from trompling other models.
                            switch (top.intent.toLowerCase()) {
                                case 'builtin.intent.none':
                                case 'none':
                                    result.score = 0.1;
                                    break;
                            }
                        }
                        cb(null, result);
                    } else {
                        cb(err, null);
                    }
                });
            } else {
                cb(new Error("LUIS model not found for locale '" + locale + "'."), null);
            }
        } else {
            cb(null, result);
        }
    }

    static recognize(utterance: string, modelUrl: string, callback: (err: Error, intents?: IIntent[], entities?: IEntity[]) => void): void {
        try {
            var uri = modelUrl.trim();
            if (uri.lastIndexOf('&q=') != uri.length - 3) {
                uri += '&q=';
            }
            uri += encodeURIComponent(utterance || '');
            request.get(uri, (err: Error, res: any, body: string) => {
                // Parse result
                var result: ILuisResults;
                try {
                    if (!err) {
                        result = JSON.parse(body);
                        result.intents = result.intents || [];
                        result.entities = result.entities || [];
                        if (result.topScoringIntent && result.intents.length == 0) {
                            result.intents.push(result.topScoringIntent);
                        }
                        if (result.intents.length == 1 && typeof result.intents[0].score !== 'number') {
                            // Intents for the builtin Cortana app don't return a score.
                            result.intents[0].score = 1.0;
                        }
                    }
                } catch (e) {
                    err = e;
                }

                // Return result
                try {
                    if (!err) {
                        callback(null, result.intents, result.entities);
                    } else {
                        var m = err.toString();
                        callback(err instanceof Error ? err : new Error(m));
                    }
                } catch (e) {
                    console.error(e.toString());
                }
            });
        } catch (err) {
            callback(err instanceof Error ? err : new Error(err.toString()));
        }
    }
}

interface ILuisResults {
    query: string;
    topScoringIntent: IIntent;
    intents: IIntent[];
    entities: IEntity[];
}
