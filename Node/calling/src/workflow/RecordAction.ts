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

export var RecordingFormat = {
    wma: 'wma',
    wav: 'wav',
    mp3: 'mp3'
};

export var RecordingCompletionReason = {
    unknown: 'unknown',
    initialSilenceTimeout: 'initialSilenceTimeout',
    maxRecordingTimeout: 'maxRecordingTimeout',
    completedSilenceDetected: 'completedSilenceDetected',
    completedStopToneDetected: 'completedStopToneDetected',
    callTerminated: 'callTerminated',
    temporarySystemFailure: 'temporarySystemFailure'
};

export interface IRecording {
    recordedAudio: Buffer;
    lengthOfRecordingInSecs: number;
}

export class RecordAction implements IIsAction {
    private data: IRecordAction;
    
    constructor(private session?: ses.CallSession) {
        this.data = session ? utils.clone(session.recordDefaults || {}) : {};
        this.data.action = 'record';
        this.data.operationId = uuid.v4();
    }

    public playPrompt(action: IAction|IIsAction): this {
        if (action) {
            this.data.playPrompt = (<IIsAction>action).toAction ? (<IIsAction>action).toAction() : <any>action;
        }
        return this;
    }

    public maxDurationInSeconds(time: number): this {
        if (time) {
            this.data.maxDurationInSeconds = time;
        }
        return this;
    }

    public initialSilenceTimeoutInSeconds(time: number): this {
        if (time) {
            this.data.initialSilenceTimeoutInSeconds = time;
        }
        return this;
    }

    public maxSilenceTimeoutInSeconds(time: number): this {
        if (time) {
            this.data.maxSilenceTimeoutInSeconds = time;
        }
        return this;
    }

    public recordingFormat(fmt: string): this {
        if (fmt) {
            this.data.recordingFormat = fmt;
        }
        return this;
    }

    public playBeep(flag: boolean): this {
        this.data.playBeep = flag;
        return this;
    }

    public stopTones(dtmf: string[]): this {
        if (dtmf) {
            this.data.stopTones = dtmf;
        }
        return this;
    }
    
    public toAction(): IAction {
        return this.data;
    }
}