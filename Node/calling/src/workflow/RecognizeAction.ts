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
import utils = require('../utils');

export var RecognitionCompletionReason = {
    unknown: 'unknown',
    initialSilenceTimeout: 'initialSilenceTimeout',
    incorrectDtmf: 'incorrectDtmf',
    interdigitTimeout: 'interdigitTimeout',
    speechOptionMatched: 'speechOptionMatched',
    dtmfOptionMatched: 'dtmfOptionMatched',
    callTerminated: 'callTerminated',
    temporarySystemFailure: 'temporarySystemFailure'
};

export var DigitCollectionCompletionReason = {
    unknown: 'unknown',
    initialSilenceTimeout: 'initialSilenceTimeout',
    interdigitTimeout: 'interdigitTimeout',
    completedStopToneDetected: 'completedStopToneDetected',
    callTerminated: 'callTerminated',
    temporarySystemFailure: 'temporarySystemFailure'
};

export class RecognizeAction implements IIsAction {
    private data: IRecognizeAction;
    
    constructor(private session?: ses.CallSession) {
        this.data = session ? utils.clone(session.recognizeDefaults || {}) : {};
        this.data.action = 'recognize';
        this.data.operationId = uuid.v4();
    }

    public playPrompt(action: IAction|IIsAction): this {
        if (action) {
            this.data.playPrompt = (<IIsAction>action).toAction ? (<IIsAction>action).toAction() : <any>action;
        }
        return this;
    }

    public bargeInAllowed(flag: boolean): this {
        this.data.bargeInAllowed = flag;
        return this;
    }

    public culture(locale: string): this {
        if (locale) {
            this.data.culture = locale;
        }
        return this;
    }

    public initialSilenceTimeoutInSeconds(time: number): this {
        if (time) {
            this.data.initialSilenceTimeoutInSeconds = time;
        }
        return this;
    }

    public interdigitTimeoutInSeconds(time: number): this {
        if (time) {
            this.data.interdigitTimeoutInSeconds = time;
        }
        return this;
    }

    public choices(list: IRecognitionChoice[]): this {
        this.data.choices = list || [];
        return this;
    }

    public collectDigits(options: ICollectDigits): this {
        if (options) {
            this.data.collectDigits = options;
        }
        return this;
    }

    public toAction(): IAction {
        return this.data;
    }
}