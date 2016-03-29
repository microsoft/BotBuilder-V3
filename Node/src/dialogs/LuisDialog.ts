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

import intent = require('./IntentDialog');
import dialog = require('./Dialog');
import utils = require('../utils');
import request = require('request');
import sprintf = require('sprintf-js');

interface ILuisResults {
    query: string;
    intents: IIntent[];
    entities: IEntity[];
}

export class LuisDialog extends intent.IntentDialog {
    constructor(private serviceUri: string) {
        super();
    }

    protected recognizeIntents(language: string, utterance: string, callback: (err: Error, intents?: IIntent[], entities?: IEntity[]) => void): void {
        LuisDialog.recognize(utterance, this.serviceUri, callback);
    }

    static recognize(utterance: string, serviceUri: string, callback: (err: Error, intents?: IIntent[], entities?: IEntity[]) => void): void {
        var uri = serviceUri.trim();
        if (uri.lastIndexOf('&q=') != uri.length - 3) {
            uri += '&q=';
        }
        uri += encodeURIComponent(utterance || '');
        request.get(uri, (err: Error, res: any, body: string) => {
            try {
                if (!err) {
                    var result: ILuisResults = JSON.parse(body);
                    if (result.intents.length == 1 && !result.intents[0].hasOwnProperty('score')) {
                        // Intents for the builtin Cortana app don't return a score.
                        result.intents[0].score = 1.0;
                    }
                    callback(null, result.intents, result.entities);
                } else {
                    callback(err);
                }
            } catch (e) {
                callback(e);
            }
        });
    }
}

interface ILuisDateTimeEntity extends IEntity {
    resolution: ILuisDateTimeResolution;
}

interface ILuisDateTimeResolution {
    resolution_type: string;
    date?: string;
    time?: string;
    comment?: string;
    duration?: string;
}

export class LuisEntityResolver {

    static findEntity(entities: IEntity[], type: string): IEntity {
        for (var i = 0; i < entities.length; i++) {
            if (entities[i].type == type) {
                return entities[i];
            }
        }
        return null;
    }

    static findAllEntities(entities: IEntity[], type: string): IEntity[] {
        var found: IEntity[] = [];
        for (var i = 0; i < entities.length; i++) {
            if (entities[i].type == type) {
                found.push(entities[i]);
            }
        }
        return found;
    }

    static resolveDate(entities: IEntity[], timezoneOffset?: number): Date {
        var now = new Date();
        var date: string;
        var time: string;
        for (var i = 0; i < entities.length; i++) {
            var entity = <ILuisDateTimeEntity>entities[i];
            if (entity.resolution) {
                switch (entity.resolution.resolution_type) {
                    case 'builtin.datetime.date':
                        if (!date) {
                            date = entity.resolution.date;
                        }
                        break;
                    case 'builtin.datetime.time':
                        if (!time) {
                            time = entity.resolution.time;
                            if (time.length == 3) {
                                time = time + ':00:00';
                            } else if (time.length == 6) {
                                time = time + ':00';
                            }
                            // TODO: resolve "ampm" comments
                        }
                        break;
                }
            }
        }
        if (date || time) {
            // The user can just say "at 9am" so we'll use today if no date.
            if (!date) {
                date = utils.toDate8601(now);
            }
            if (time) {
                // Append time but adjust timezone. Default is to use bots timezone.
                if (typeof timezoneOffset !== 'number') {
                    timezoneOffset = now.getTimezoneOffset() / 60;
                }
                date = sprintf.sprintf('%s%s%s%02d:00', date, time, (timezoneOffset > 0 ? '-' : '+'), timezoneOffset);
            }
            return new Date(date);
        } else {
            return null;
        }
    }
}