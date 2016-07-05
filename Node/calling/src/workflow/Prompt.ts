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
import utils = require('../utils');

export var VoiceGender = {
    male: 'male',
    female: 'female'
};

export var SayAs = {
    yearMonthDay: 'yearMonthDay',
    monthDayYear: 'monthDayYear',
    dayMonthYear: 'dayMonthYear',
    yearMonth: 'yearMonth',
    monthYear: 'monthYear',
    monthDay: 'monthDay',
    dayMonth: 'dayMonth',
    day: 'day',
    month: 'month',
    year: 'year',
    cardinal: 'cardinal',
    ordinal: 'ordinal',
    letters: 'letters',
    time12: 'time12',
    time24: 'time24',
    telephone: 'telephone',
    name: 'name',
    phoneticName: 'phoneticName'
};

export class Prompt implements IIsPrompt {
    private data: IPrompt;
    
    constructor(private session?: ses.CallSession) {
        this.data = session ? utils.clone(session.promptDefaults || {}) : {};
    }
    
    public value(text: string|string[], ...args: any[]): this {
        if (text) {
            this.data.value = utils.fmtText(this.session, text, args);
        } else {
            this.data.value = null; // Silence being played.
        }
        return this;
    }

    public fileUri(uri: string): this {
        if (uri) {
            this.data.fileUri = uri;
        }
        return this;
    }

    public voice(gender: string): this {
        if (gender) {
            this.data.voice = gender;
        }
        return this;
    }

    public culture(locale: string): this {
        if (locale) {
            this.data.culture = locale;
        }
        return this;
    }

    public silenceLengthInMilliseconds(time: number): this {
        if (time) {
            this.data.silenceLengthInMilliseconds = time;
        }
        return this;
    }

    public emphasize(flag: boolean): this {
        this.data.emphasize = flag;
        return this;
    } 

    public sayAs(type: string): this {
        if (type) {
            this.data.sayAs = type;
        }
        return this;
    }

    public toPrompt(): IPrompt {
        return this.data;
    }

    static text(session: ses.CallSession, text: string|string[], ...args: any[]): Prompt {
        args.unshift(text);
        var prompt = new Prompt(session);
        Prompt.prototype.value.apply(prompt, args);
        return prompt;
    }

    static file(session: ses.CallSession, uri: string): Prompt {
        return new Prompt(session).fileUri(uri);
    }

    static silence(session: ses.CallSession, time: number): Prompt {
        return new Prompt(session).value(null).silenceLengthInMilliseconds(time);
    }
}