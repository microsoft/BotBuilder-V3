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
import dlg = require('./Dialog');
import actions = require('./DialogAction');
import consts = require('../consts');
import logger = require('../logger');
import async = require('async');

export enum RecognizeOrder { parallel, series }

export enum RecognizeMode { onBegin, onBeginIfRoot, onReply }

export interface IIntentDialogOptions {
    intentThreshold?: number;
    recognizeMode?: RecognizeMode;
    recognizeOrder?: RecognizeOrder;
    recognizers?: IIntentRecognizer[];
    processLimit?: number;
} 

export interface IIntentRecognizer {
    recognize(context: dlg.IRecognizeContext, cb: (err: Error, result: IIntentRecognizerResult) => void): void;
}

export interface IIntentRecognizerResult extends dlg.IRecognizeResult {
    intent: string;
    expression?: RegExp;
    matched?: string[]; 
    intents?: IIntent[];
    entities?: IEntity[];
}

export class IntentDialog extends dlg.Dialog {
    private beginDialog: IBeginDialogHandler;
    private handlers = <IIntentHandlerMap>{};
    private expressions = <RegExp[]>[];

    constructor(private options: IIntentDialogOptions = {}) {
        super();
        if (typeof this.options.intentThreshold !== 'number') {
            this.options.intentThreshold = 0.1;
        }
        if (!this.options.hasOwnProperty('recognizeMode')) {
            this.options.recognizeMode = RecognizeMode.onBeginIfRoot;
        }
        if (!this.options.hasOwnProperty('recognizeOrder')) {
            this.options.recognizeOrder = RecognizeOrder.parallel;
        }
        if (!this.options.recognizers) {
            this.options.recognizers = [];
        }
        if (!this.options.processLimit) {
            this.options.processLimit = 4;
        }
    }

    public begin<T>(session: ses.Session, args: any): void {
        var mode = this.options.recognizeMode;
        var isRoot = (session.sessionState.callstack.length == 1);
        var recognize = (mode == RecognizeMode.onBegin || (isRoot && mode == RecognizeMode.onBeginIfRoot)); 
        if (this.beginDialog) {
            try {
                logger.info(session, 'IntentDialog.begin()');
                this.beginDialog(session, args, () => {
                    if (recognize) {
                        this.replyReceived(session);
                    }
                });
            } catch (e) {
                this.emitError(session, e);
            }
        } else if (recognize) {
            this.replyReceived(session);
        }
    }

    public replyReceived(session: ses.Session, recognizeResult?: dlg.IRecognizeResult): void {
        if (!recognizeResult) {
            this.recognize({ message: session.message, dialogData: session.dialogData, activeDialog: true }, (err, result) => {
                if (!err) {
                    this.invokeIntent(session, <IIntentRecognizerResult>result);
                } else {
                    this.emitError(session, err);
                }
            });
        } else {
            this.invokeIntent(session, <IIntentRecognizerResult>recognizeResult);
        }
    }

    public dialogResumed(session: ses.Session, result: dlg.IDialogResult<any>): void {
        var activeIntent: string = session.dialogData[consts.Data.Intent];
        if (activeIntent && this.handlers.hasOwnProperty(activeIntent)) {
            try {
                this.handlers[activeIntent](session, result);
            } catch (e) {
                this.emitError(session, e);
            }
        } else {
            super.dialogResumed(session, result);
        }
    }

    public recognize(context: dlg.IRecognizeContext, cb: (err: Error, result: dlg.IRecognizeResult) => void): void {
        function done(err: Error, r: IIntentRecognizerResult) {
            if (!err) {
                if (r.score > result.score) {
                    cb(null, r);
                } else {
                    cb(null, result);
                }
            } else {
                cb(err, null);
            }
        }

        var result: IIntentRecognizerResult = { score: 0.0, intent: null };
        if (context.message && context.message.text) {
            // Match regular expressions first
            if (this.expressions) {
                for (var i = 0; i < this.expressions.length; i++ ) {
                    var exp = this.expressions[i];
                    var matches = exp.exec(context.message.text);
                    if (matches && matches.length) {
                        var matched = matches[0];
                        var score = matched.length / context.message.text.length;
                        if (score > result.score && score >= this.options.intentThreshold) {
                            result.score = score;
                            result.intent = exp.toString();
                            result.expression = exp;
                            result.matched = matches;
                            if (score == 1.0) {
                                break;
                            }
                        }
                    }
                }
            }

            // Match using registered recognizers 
            if (result.score < 1.0 && this.options.recognizers.length) {
                switch (this.options.recognizeOrder) {
                    default:
                    case RecognizeOrder.parallel:
                        this.recognizeInParallel(context, done);
                        break;
                    case RecognizeOrder.series:
                        this.recognizeInSeries(context, done);
                        break;
                }
            } else {
                cb(null, result);
            }
        } else {
            cb(null, result);
        }
    }

    public onBegin(handler: IBeginDialogHandler): this {
        this.beginDialog = handler;
        return this;
    }

    public matches(intent: string|RegExp, dialogId: string|actions.IDialogWaterfallStep[]|actions.IDialogWaterfallStep, dialogArgs?: any): this {
        // Find ID and verify unique
        var id: string;
        if (intent) {
            if (typeof intent === 'string') {
                id = intent;
            } else {
                id = (<RegExp>intent).toString();
                this.expressions.push(intent);
            }
        }
        if (this.handlers.hasOwnProperty(id)) {
            throw new Error("A handler for '" + id + "' already exists.");
        }

        // Register handler
        if (Array.isArray(dialogId)) {
            this.handlers[id] = actions.waterfall(dialogId);
        } else if (typeof dialogId === 'string') {
            this.handlers[id] = actions.DialogAction.beginDialog(<string>dialogId, dialogArgs);
        } else {
            this.handlers[id] = actions.waterfall([<actions.IDialogWaterfallStep>dialogId]);
        }
        return this;
    }

    public matchesAny(intents: string[]|RegExp[], dialogId: string|actions.IDialogWaterfallStep[]|actions.IDialogWaterfallStep, dialogArgs?: any): this {
        for (var i = 0; i < intents.length; i++) {
            this.matches(intents[i], dialogId, dialogArgs);
        }
        return this;
    }

    public onDefault(dialogId: string|actions.IDialogWaterfallStep[]|actions.IDialogWaterfallStep, dialogArgs?: any): this {
        // Register handler
        if (Array.isArray(dialogId)) {
            this.handlers[consts.Intents.Default] = actions.waterfall(dialogId);
        } else if (typeof dialogId === 'string') {
            this.handlers[consts.Intents.Default] = actions.DialogAction.beginDialog(<string>dialogId, dialogArgs);
        } else {
            this.handlers[consts.Intents.Default] = actions.waterfall([<actions.IDialogWaterfallStep>dialogId]);
        }
        return this;
    }

    private recognizeInParallel(context: dlg.IRecognizeContext, done: (err: Error, result: IIntentRecognizerResult) => void): void {
        var result: IIntentRecognizerResult = { score: 0.0, intent: null };
        async.eachLimit(this.options.recognizers, this.options.processLimit, (recognizer, cb) => {
            try {
                recognizer.recognize(context, (err, r) => {
                    if (!err && r && r.score > result.score && r.score >= this.options.intentThreshold) {
                        result = r;
                    }
                    cb(err);
                });
            } catch (e) {
                cb(e);
            }
        }, (err) => {
            if (!err) {
                done(null, result);
            } else {
                done(err instanceof Error ? err : new Error(err.toString()), null);
            }
        });
    }

    private recognizeInSeries(context: dlg.IRecognizeContext, done: (err: Error, result: IIntentRecognizerResult) => void): void {
        var i = 0;
        var result: IIntentRecognizerResult = { score: 0.0, intent: null };
        async.whilst(() => {
            return (i < this.options.recognizers.length && result.score < 1.0);
        }, (cb) => {
            try {
                var recognizer = this.options.recognizers[i++];
                recognizer.recognize(context, (err, r) => {
                    if (!err && r && r.score > result.score && r.score >= this.options.intentThreshold) {
                        result = r;
                    }
                    cb(err);
                });
            } catch (e) {
                cb(e);
            }
        }, (err) => {
            if (!err) {
                done(null, result);
            } else {
                done(err instanceof Error ? err : new Error(err.toString()), null);
            }
        });
    }

    private invokeIntent(session: ses.Session, recognizeResult: IIntentRecognizerResult): void {
        var activeIntent: string;
        if (recognizeResult.intent && this.handlers.hasOwnProperty(recognizeResult.intent)) {
            logger.info(session, 'IntentDialog.matches(%s)', recognizeResult.intent);
            activeIntent = recognizeResult.intent;                
        } else if (this.handlers.hasOwnProperty(consts.Intents.Default)) {
            logger.info(session, 'IntentDialog.onDefault()');
            activeIntent = consts.Intents.Default;
        }
        if (activeIntent) {
            try {
                session.dialogData[consts.Data.Intent] = activeIntent;
                this.handlers[activeIntent](session, recognizeResult);
            } catch (e) {
                this.emitError(session, e);
            }
        } else {
            logger.warn(session, 'IntentDialog - no intent handler found for %s', recognizeResult.intent);
        }
    }

    private emitError(session: ses.Session, err: Error): void {
        err = err instanceof Error ? err : new Error(err.toString());
        session.error(err);
    }
}

interface IIntentHandlerMap {
    [id: string]: IDialogHandler<any>;
}
