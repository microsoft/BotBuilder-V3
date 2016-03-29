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

import session = require('../Session');
import dialog = require('./Dialog');
import actions = require('./DialogAction');
import consts = require('../consts');

export interface IIntentHandler {
    (session: ISession, entities?: IEntity[], intents?: IIntent[]): void;
}

export interface ICaptureIntentHandler {
    (action: ISessionAction, intent: IIntent, entities?: IEntity[]): void;
}

interface ICaptureResult extends dialog.IDialogResult<any> {
    captured: {
        intents: IIntent[];
        entities: IEntity[];
    };
}

export interface IIntentArgs {
    intents: IIntent[];
    entities: IEntity[];
}

interface IHandlerMatch {
    groupId: string;
    handler: IDialogHandler<IIntentArgs>;
}

export abstract class IntentDialog extends dialog.Dialog {
    private static CAPTURE_THRESHOLD = 0.6;

    private groups: { [id: string]: IntentGroup; } = {};
    private beginDialog: IBeginDialogHandler;
    private captureIntent: ICaptureIntentHandler;
    private intentThreshold = 0.1;

    public begin<T>(session: ISession, args: IntentGroup): void {
        if (this.beginDialog) {
            this.beginDialog(session, args, () => {
                super.begin(session, args);
            });
        } else {
            super.begin(session, args);
        }
    }

    public replyReceived(session: ISession): void {
        var msg = session.message;
        this.recognizeIntents(msg.language, msg.text, (err, intents, entities) => {
            if (!err) {
                var topIntent = this.findTopIntent(intents);
                var score = topIntent ? topIntent.score : 0;
                session.compareConfidence(msg.language, msg.text, score, (handled) => {
                    if (!handled) {
                        this.invokeIntent(session, intents, entities);
                    }
                });
            } else {
                session.endDialog({ error: new Error('Intent recognition error: ' + err.message) });
            }
        });
    }

    public dialogResumed(session: ISession, result: ICaptureResult): void {
        if (result.captured) {
            this.invokeIntent(session, result.captured.intents, result.captured.entities);
        } else {
            var activeGroup: string = session.dialogData[consts.Data.Group];
            var activeIntent: string = session.dialogData[consts.Data.Intent];
            var group = activeGroup ? this.groups[activeGroup] : null;
            var handler = group && activeIntent ? group._intentHandler(activeIntent) : null;
            if (handler) {
                handler(session, <any>result);
            } else {
                super.dialogResumed(session, result);
            }
        }
    }

    public compareConfidence(action: ISessionAction, language: string, utterance: string, score: number): void {
        // First check to see if the childs confidence is low and that we have a capture handler.
        if (score < IntentDialog.CAPTURE_THRESHOLD && this.captureIntent) {
            this.recognizeIntents(language, utterance, (err, intents, entities) => {
                var handled = false;
                if (!err) {
                    // Ensure capture handler is worth invoking. Requirements are the top intents
                    // score should be greater then the childs score and there should be a handler
                    // registered for that intent. The last requirement addresses the fact that the
                    // 'None' intent from LUIS is likely to have a score that's greater then the 
                    // intent threshold.
                    var matches: IHandlerMatch;
                    var topIntent = this.findTopIntent(intents);
                    if (topIntent && topIntent.score > this.intentThreshold && topIntent.score > score) {
                        matches = this.findHandler(topIntent);
                    }
                    if (matches) {
                        this.captureIntent({
                            next: action.next,
                            userData: action.userData,
                            dialogData: action.dialogData,
                            endDialog: () => {
                                action.endDialog({
                                    resumed: dialog.ResumeReason.completed,
                                    captured: {
                                        intents: intents,
                                        entities: entities
                                    }
                                });
                            },
                            send: action.send 
                        }, topIntent, entities);
                    } else {
                        action.next();
                    }
                } else {
                    console.error('Intent recognition error: ' + err.message);
                    action.next();
                }
            });
        } else {
            action.next();
        }
    }

    public addGroup(group: IntentGroup): this {
        var id = group.getId();
        if (!this.groups.hasOwnProperty(id)) {
            this.groups[id] = group;
        } else {
            throw "Group of " + id + " already exists within the dialog.";
        }
        return this;
    }

    public onBegin(handler: IBeginDialogHandler): this {
        this.beginDialog = handler;
        return this;
    }

    public on(intent: string, fn: IDialogHandler<IIntentArgs>): this;
    public on(intent: string, waterfall: actions.IDialogWaterfallStep[]): this;
    public on(intent: string, dialogId: string, dialogArgs?: any): this;
    public on(intent: string, dialogId: any, dialogArgs?: any): this {
        this.getDefaultGroup().on(intent, dialogId, dialogArgs);
        return this;
    }

    public onDefault(fn: IDialogHandler<IIntentArgs>): this;
    public onDefault(waterfall: actions.IDialogWaterfallStep[]): this;
    public onDefault(dialogId: string, dialogArgs?: any): this;
    public onDefault(dialogId: any, dialogArgs?: any): this {
        this.getDefaultGroup().on(consts.Intents.Default, dialogId, dialogArgs);
        return this;
    }

    public getThreshold(): number {
        return this.intentThreshold;
    }

    public setThreshold(score: number): this {
        this.intentThreshold = score;
        return this;
    }

    private invokeIntent(session: ISession, intents: IIntent[], entities: IEntity[]): void {
        try {
            // Find top intent, group, and handler;
            var match: IHandlerMatch;
            var topIntent = this.findTopIntent(intents);
            if (topIntent && topIntent.score > this.intentThreshold) {
                match = this.findHandler(topIntent);
            }
            if (!match) {
                match = {
                    groupId: consts.Id.DefaultGroup,
                    handler: this.getDefaultGroup()._intentHandler(consts.Intents.Default)
                };
            }

            // Invoke handler
            if (match) {
                session.dialogData[consts.Data.Group] = match.groupId;
                session.dialogData[consts.Data.Intent] = topIntent.intent;
                match.handler(session, { intents: intents, entities: entities });
            } else {
                session.send();
            }
        } catch (e) {
            session.endDialog({ error: new Error('Exception handling intent: ' + e.message) });
        }
    }

    private findTopIntent(intents: IIntent[]): IIntent {
        var topIntent: IIntent;
        for (var i = 0; i < intents.length; i++) {
            var intent = intents[i];
            if (!topIntent) {
                topIntent = intent;
            } else if (intent.score > topIntent.score) {
                topIntent = intent;
            }
        }
        return topIntent;
    }

    private findHandler(intent: IIntent): IHandlerMatch {
        for (var groupId in this.groups) {
            var handler = this.groups[groupId]._intentHandler(intent.intent);
            if (handler) {
                return { groupId: groupId, handler: handler };
            }
        }
        return null;
    }

    private getDefaultGroup(): IntentGroup {
        var group = this.groups[consts.Id.DefaultGroup];
        if (!group) {
            this.groups[consts.Id.DefaultGroup] = group = new IntentGroup(consts.Id.DefaultGroup);
        }
        return group;
    }

    protected abstract recognizeIntents(language: string, utterance: string, callback: (err: Error, intents?: IIntent[], entities?: IEntity[]) => void): void;
}

export class IntentGroup {
    private handlers: { [id: string]: IDialogHandler<IIntentArgs>; } = {};

    constructor(private id: string) {
    }

    public getId(): string {
        return this.id;
    }

    /** Returns the handler registered for an intent if it exists. */
    public _intentHandler(intent: string): IDialogHandler<IIntentArgs> {
        return this.handlers[intent];
    }

    public on(intent: string, fn: IDialogHandler<IIntentArgs>): this;
    public on(intent: string, waterfall: actions.IDialogWaterfallStep[]): this;
    public on(intent: string, dialogId: string, dialogArgs?: any): this;
    public on(intent: string, dialogId: any, dialogArgs?: any): this {
        if (!this.handlers.hasOwnProperty(intent)) {
            if (Array.isArray(dialogId)) {
                this.handlers[intent] = actions.DialogAction.waterfall(dialogId);
            } else if (typeof dialogId == 'string') {
                this.handlers[intent] = actions.DialogAction.beginDialog(dialogId, dialogArgs);
            } else {
                this.handlers[intent] = dialogId;
            }
        } else {
            throw new Error('Intent[' + intent + '] already exists.');
        }
        return this;
    }
}
