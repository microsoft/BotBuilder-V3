"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const WaterfallDialog_1 = require("./WaterfallDialog");
const DialogAction_1 = require("./DialogAction");
const Dialog_1 = require("./Dialog");
const IntentRecognizerSet_1 = require("./IntentRecognizerSet");
const RegExpRecognizer_1 = require("./RegExpRecognizer");
const Message_1 = require("../Message");
const consts = require("../consts");
var PromptType;
(function (PromptType) {
    PromptType[PromptType["text"] = 0] = "text";
    PromptType[PromptType["number"] = 1] = "number";
    PromptType[PromptType["confirm"] = 2] = "confirm";
    PromptType[PromptType["choice"] = 3] = "choice";
    PromptType[PromptType["time"] = 4] = "time";
    PromptType[PromptType["attachment"] = 5] = "attachment";
})(PromptType = exports.PromptType || (exports.PromptType = {}));
var ListStyle;
(function (ListStyle) {
    ListStyle[ListStyle["none"] = 0] = "none";
    ListStyle[ListStyle["inline"] = 1] = "inline";
    ListStyle[ListStyle["list"] = 2] = "list";
    ListStyle[ListStyle["button"] = 3] = "button";
    ListStyle[ListStyle["auto"] = 4] = "auto";
})(ListStyle = exports.ListStyle || (exports.ListStyle = {}));
class Prompt extends Dialog_1.Dialog {
    constructor(features) {
        super();
        this.features = features;
        this.recognizers = new IntentRecognizerSet_1.IntentRecognizerSet();
        this.handlers = {};
        this._onPrompt = [];
        this._onFormatMessage = [];
        this._onRecognize = [];
        if (!this.features) {
            this.features = {};
        }
    }
    begin(session, options) {
        let dc = session.dialogData;
        dc.options = options || {};
        dc.turns = 0;
        dc.lastTurn = new Date().getTime();
        dc.isReprompt = false;
        if (!options.hasOwnProperty('promptAfterAction')) {
            options.promptAfterAction = true;
        }
        function resolvePrompt(prompt) {
            if (typeof prompt == 'object' && prompt.toMessage) {
                return prompt.toMessage();
            }
            return prompt;
        }
        options.prompt = resolvePrompt(options.prompt);
        options.retryPrompt = resolvePrompt(options.retryPrompt);
        if (!options.libraryNamespace) {
            if (options.localizationNamespace) {
                options.libraryNamespace = options.localizationNamespace;
            }
            else {
                const stack = session.dialogStack();
                if (stack.length >= 2) {
                    options.libraryNamespace = stack[stack.length - 2].id.split(':')[0];
                }
                else {
                    options.libraryNamespace = consts.Library.default;
                }
            }
        }
        let attachments = options.attachments || [];
        for (let i = 0; i < attachments.length; i++) {
            if (attachments[i].toAttachment) {
                attachments[i] = attachments[i].toAttachment();
            }
        }
        this.sendPrompt(session);
    }
    recognize(context, cb) {
        let dc = context.dialogData;
        dc.turns++;
        dc.lastTurn = new Date().getTime();
        dc.isReprompt = false;
        dc.activeIntent = null;
        let recognizers = this.recognizers;
        function finalRecognize() {
            recognizers.recognize(context, (err, r) => {
                if (!err && r.score > result.score) {
                    result = r;
                }
                cb(err, result);
            });
        }
        let idx = 0;
        const handlers = this._onRecognize;
        let result = { score: 0.0, intent: null };
        function next() {
            try {
                if (idx < handlers.length) {
                    handlers[idx++](context, (err, score, response) => {
                        if (err) {
                            return cb(err, null);
                        }
                        let r = {
                            score: score,
                            intent: consts.Intents.Response,
                            entities: [{
                                    type: consts.Entities.Response,
                                    entity: response
                                }]
                        };
                        if (r.score > result.score) {
                            result = r;
                        }
                        if (result.score >= 1.0) {
                            cb(null, result);
                        }
                        else {
                            next();
                        }
                    });
                }
                else {
                    finalRecognize();
                }
            }
            catch (e) {
                cb(e, null);
            }
        }
        next();
    }
    replyReceived(session, recognizeResult) {
        if (recognizeResult && recognizeResult.score > 0.0) {
            this.invokeIntent(session, recognizeResult);
        }
        else {
            this.sendPrompt(session);
        }
    }
    dialogResumed(session, result) {
        let dc = session.dialogData;
        if (dc.activeIntent && this.handlers.hasOwnProperty(dc.activeIntent)) {
            try {
                this.handlers[dc.activeIntent](session, result);
            }
            catch (e) {
                session.error(e);
            }
        }
        else if (dc.options.promptAfterAction) {
            dc.isReprompt = (result.resumed === Dialog_1.ResumeReason.reprompt);
            this.sendPrompt(session);
        }
    }
    sendPrompt(session) {
        const _that = this;
        function defaultSend() {
            if (typeof options.maxRetries === 'number' && context.turns > options.maxRetries) {
                session.endDialogWithResult({ resumed: Dialog_1.ResumeReason.notCompleted });
            }
            else {
                let prompt = !turnZero ? _that.getRetryPrompt(session) : options.prompt;
                if (Array.isArray(prompt) || typeof prompt === 'string') {
                    let speak = !turnZero ? options.retrySpeak : options.speak;
                    _that.formatMessage(session, prompt, speak, (err, msg) => {
                        if (!err) {
                            sendMsg(msg);
                        }
                        else {
                            session.error(err);
                        }
                    });
                }
                else {
                    sendMsg(prompt);
                }
            }
        }
        function sendMsg(msg) {
            if (turnZero) {
                if (options.attachments) {
                    if (!msg.attachments) {
                        msg.attachments = [];
                    }
                    options.attachments.forEach((value) => {
                        if (value.toAttachment) {
                            msg.attachments.push(value.toAttachment());
                        }
                        else {
                            msg.attachments.push(value);
                        }
                    });
                }
                ['attachmentLayout', 'entities', 'textFormat', 'inputHint'].forEach((key) => {
                    if (!msg.hasOwnProperty(key)) {
                        msg[key] = options[key];
                    }
                });
            }
            if (!msg.inputHint) {
                msg.inputHint = Message_1.InputHint.expectingInput;
            }
            session.send(msg);
        }
        let idx = 0;
        const handlers = this._onPrompt;
        const context = session.dialogData;
        const options = context.options;
        const turnZero = context.turns === 0 || context.isReprompt;
        function next() {
            try {
                if (idx < handlers.length) {
                    handlers[idx++](session, next);
                }
                else {
                    defaultSend();
                }
            }
            catch (e) {
                session.error(e);
            }
        }
        next();
    }
    formatMessage(session, text, speak, callback) {
        let idx = 0;
        const handlers = this._onFormatMessage;
        function next(err, msg) {
            if (err || msg) {
                callback(err, msg);
            }
            else {
                try {
                    if (idx < handlers.length) {
                        handlers[idx++](session, text, speak, next);
                    }
                    else {
                        msg = { text: Prompt.gettext(session, text) };
                        if (speak) {
                            msg.speak = Prompt.gettext(session, speak);
                        }
                        callback(null, msg);
                    }
                }
                catch (e) {
                    callback(e, null);
                }
            }
        }
        next(null, null);
    }
    onPrompt(handler) {
        this._onPrompt.unshift(handler);
        return this;
    }
    onFormatMessage(handler) {
        this._onFormatMessage.unshift(handler);
        return this;
    }
    onRecognize(handler) {
        this._onRecognize.unshift(handler);
        return this;
    }
    matches(intent, dialogId, dialogArgs) {
        let id;
        if (intent) {
            if (typeof intent === 'string') {
                id = intent;
            }
            else {
                id = intent.toString();
                this.recognizers.recognizer(new RegExpRecognizer_1.RegExpRecognizer(id, intent));
            }
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
        for (let i = 0; i < intents.length; i++) {
            this.matches(intents[i], dialogId, dialogArgs);
        }
        return this;
    }
    recognizer(plugin) {
        this.recognizers.recognizer(plugin);
        return this;
    }
    updateFeatures(features) {
        if (features) {
            for (let key in features) {
                if (features.hasOwnProperty(key)) {
                    this.features[key] = features[key];
                }
            }
        }
        return this;
    }
    static gettext(session, text, namespace) {
        let locale = session.preferredLocale();
        let options = session.dialogData.options;
        if (!namespace && options && options.libraryNamespace) {
            namespace = options.libraryNamespace;
        }
        return session.localizer.gettext(locale, Message_1.Message.randomPrompt(text), namespace);
    }
    invokeIntent(session, recognizeResult) {
        if (recognizeResult.intent === consts.Intents.Response) {
            let response = recognizeResult.entities && recognizeResult.entities.length == 1 ? recognizeResult.entities[0].entity : null;
            session.logger.log(session.dialogStack(), 'Prompt.returning(' + response + ')');
            session.endDialogWithResult({ resumed: Dialog_1.ResumeReason.completed, response: response });
        }
        else if (this.handlers.hasOwnProperty(recognizeResult.intent)) {
            try {
                session.logger.log(session.dialogStack(), 'Prompt.matches(' + recognizeResult.intent + ')');
                let dc = session.dialogData;
                dc.activeIntent = recognizeResult.intent;
                this.handlers[dc.activeIntent](session, recognizeResult);
            }
            catch (e) {
                session.error(e);
            }
        }
        else {
            session.logger.warn(session.dialogStack(), 'Prompt - no intent handler found for ' + recognizeResult.intent);
            this.sendPrompt(session);
        }
    }
    getRetryPrompt(session) {
        let options = session.dialogData.options;
        if (options.retryPrompt) {
            return options.retryPrompt;
        }
        else if (this.features.defaultRetryPrompt) {
            let prompt = this.features.defaultRetryPrompt;
            if (Array.isArray(prompt) || typeof prompt === 'string') {
                let locale = session.preferredLocale();
                return session.localizer.gettext(locale, Message_1.Message.randomPrompt(prompt), this.features.defaultRetryNamespace || consts.Library.default);
            }
            else {
                return prompt;
            }
        }
        else {
            return options.prompt;
        }
    }
}
exports.Prompt = Prompt;
var prompt = new Prompt({});
