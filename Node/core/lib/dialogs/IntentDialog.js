"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const WaterfallDialog_1 = require("./WaterfallDialog");
const DialogAction_1 = require("./DialogAction");
const Dialog_1 = require("./Dialog");
const IntentRecognizerSet_1 = require("./IntentRecognizerSet");
const RegExpRecognizer_1 = require("./RegExpRecognizer");
const consts = require("../consts");
var RecognizeMode;
(function (RecognizeMode) {
    RecognizeMode[RecognizeMode["onBegin"] = 0] = "onBegin";
    RecognizeMode[RecognizeMode["onBeginIfRoot"] = 1] = "onBeginIfRoot";
    RecognizeMode[RecognizeMode["onReply"] = 2] = "onReply";
})(RecognizeMode = exports.RecognizeMode || (exports.RecognizeMode = {}));
class IntentDialog extends Dialog_1.Dialog {
    constructor(options = {}) {
        super();
        this.handlers = {};
        this.recognizers = new IntentRecognizerSet_1.IntentRecognizerSet(options);
        if (typeof options.recognizeMode !== "undefined") {
            this.recognizeMode = options.recognizeMode;
        }
        else {
            this.recognizeMode = RecognizeMode.onBeginIfRoot;
        }
    }
    begin(session, args) {
        var mode = this.recognizeMode;
        var isRoot = (session.sessionState.callstack.length == 1);
        var recognize = (mode == RecognizeMode.onBegin || (isRoot && mode == RecognizeMode.onBeginIfRoot));
        if (this.beginDialog) {
            try {
                session.logger.log(session.dialogStack(), 'IntentDialog.begin()');
                this.beginDialog(session, args, () => {
                    if (recognize) {
                        this.replyReceived(session);
                    }
                });
            }
            catch (e) {
                this.emitError(session, e);
            }
        }
        else if (recognize) {
            this.replyReceived(session);
        }
    }
    replyReceived(session, recognizeResult) {
        if (!recognizeResult) {
            var locale = session.preferredLocale();
            var context = session.toRecognizeContext();
            context.dialogData = session.dialogData;
            context.activeDialog = true;
            this.recognize(context, (err, result) => {
                if (!err) {
                    this.invokeIntent(session, result);
                }
                else {
                    this.emitError(session, err);
                }
            });
        }
        else {
            this.invokeIntent(session, recognizeResult);
        }
    }
    dialogResumed(session, result) {
        var activeIntent = session.dialogData[consts.Data.Intent];
        if (activeIntent && this.handlers.hasOwnProperty(activeIntent)) {
            try {
                this.handlers[activeIntent](session, result);
            }
            catch (e) {
                this.emitError(session, e);
            }
        }
        else {
            super.dialogResumed(session, result);
        }
    }
    recognize(context, cb) {
        this.recognizers.recognize(context, cb);
    }
    onBegin(handler) {
        this.beginDialog = handler;
        return this;
    }
    matches(intent, dialogId, dialogArgs) {
        var id;
        if (intent) {
            if (typeof intent === 'string') {
                id = intent;
            }
            else {
                id = intent.toString();
                this.recognizers.recognizer(new RegExpRecognizer_1.RegExpRecognizer(id, intent));
            }
        }
        if (this.handlers.hasOwnProperty(id)) {
            throw new Error("A handler for '" + id + "' already exists.");
        }
        if (Array.isArray(dialogId)) {
            this.handlers[id] = WaterfallDialog_1.WaterfallDialog.createHandler(dialogId);
        }
        else if (typeof dialogId === 'string') {
            this.handlers[id] = DialogAction_1.DialogAction.beginDialog(dialogId, dialogArgs);
        }
        else {
            this.handlers[id] = WaterfallDialog_1.WaterfallDialog.createHandler([dialogId]);
        }
        return this;
    }
    matchesAny(intents, dialogId, dialogArgs) {
        for (var i = 0; i < intents.length; i++) {
            this.matches(intents[i], dialogId, dialogArgs);
        }
        return this;
    }
    onDefault(dialogId, dialogArgs) {
        if (Array.isArray(dialogId)) {
            this.handlers[consts.Intents.Default] = WaterfallDialog_1.WaterfallDialog.createHandler(dialogId);
        }
        else if (typeof dialogId === 'string') {
            this.handlers[consts.Intents.Default] = DialogAction_1.DialogAction.beginDialog(dialogId, dialogArgs);
        }
        else {
            this.handlers[consts.Intents.Default] = WaterfallDialog_1.WaterfallDialog.createHandler([dialogId]);
        }
        return this;
    }
    recognizer(plugin) {
        this.recognizers.recognizer(plugin);
        return this;
    }
    invokeIntent(session, recognizeResult) {
        var activeIntent;
        if (recognizeResult.intent && this.handlers.hasOwnProperty(recognizeResult.intent)) {
            session.logger.log(session.dialogStack(), 'IntentDialog.matches(' + recognizeResult.intent + ')');
            activeIntent = recognizeResult.intent;
        }
        else if (this.handlers.hasOwnProperty(consts.Intents.Default)) {
            session.logger.log(session.dialogStack(), 'IntentDialog.onDefault()');
            activeIntent = consts.Intents.Default;
        }
        if (activeIntent) {
            try {
                session.dialogData[consts.Data.Intent] = activeIntent;
                this.handlers[activeIntent](session, recognizeResult);
            }
            catch (e) {
                this.emitError(session, e);
            }
        }
        else {
            session.logger.warn(session.dialogStack(), 'IntentDialog - no intent handler found for ' + recognizeResult.intent);
        }
    }
    emitError(session, err) {
        var m = err.toString();
        err = err instanceof Error ? err : new Error(m);
        session.error(err);
    }
}
exports.IntentDialog = IntentDialog;
