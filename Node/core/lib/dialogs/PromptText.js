"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Prompt_1 = require("./Prompt");
const consts = require("../consts");
class PromptText extends Prompt_1.Prompt {
    constructor(features) {
        super({
            defaultRetryPrompt: 'default_text',
            defaultRetryNamespace: consts.Library.system,
            recognizeScore: 0.5
        });
        this.updateFeatures(features);
        this.onRecognize((context, cb) => {
            const text = context.message.text;
            if (text && !this.features.disableRecognizer) {
                const options = context.dialogData.options;
                if ((options.minLength && text.length < Number(options.minLength)) ||
                    (options.maxLength && text.length > Number(options.maxLength))) {
                    cb(null, 0.0);
                }
                else {
                    cb(null, this.features.recognizeScore, text);
                }
            }
            else {
                cb(null, 0.0);
            }
        });
        this.onFormatMessage((session, text, speak, callback) => {
            const context = session.dialogData;
            const options = context.options;
            const turnZero = context.turns === 0 || context.isReprompt;
            const message = session.message.text;
            if (!turnZero && (options.minLength || options.maxLength)) {
                var errorPrompt;
                if (options.minLength && message.length < Number(options.minLength)) {
                    errorPrompt = 'text_minLength_error';
                }
                else if (options.maxLength && message.length > Number(options.maxLength)) {
                    errorPrompt = 'text_maxLength_error';
                }
                if (errorPrompt) {
                    let text = Prompt_1.Prompt.gettext(session, errorPrompt, consts.Library.system);
                    let msg = { text: session.gettext(text, options) };
                    callback(null, msg);
                }
                else {
                    callback(null, null);
                }
            }
            else {
                callback(null, null);
            }
        });
        this.matches(consts.Intents.Repeat, (session) => {
            session.dialogData.turns = 0;
            this.sendPrompt(session);
        });
    }
}
exports.PromptText = PromptText;
