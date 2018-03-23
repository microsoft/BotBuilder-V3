"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Prompt_1 = require("./Prompt");
const PromptRecognizers_1 = require("./PromptRecognizers");
const consts = require("../consts");
class PromptNumber extends Prompt_1.Prompt {
    constructor(features) {
        super({
            defaultRetryPrompt: 'default_number',
            defaultRetryNamespace: consts.Library.system
        });
        this.updateFeatures(features);
        this.onRecognize((context, cb) => {
            if (context.message.text && !this.features.disableRecognizer) {
                let options = context.dialogData.options;
                let entities = PromptRecognizers_1.PromptRecognizers.recognizeNumbers(context, options);
                let top = PromptRecognizers_1.PromptRecognizers.findTopEntity(entities);
                if (top) {
                    cb(null, top.score, top.entity);
                }
                else {
                    cb(null, 0.0);
                }
            }
            else {
                cb(null, 0.0);
            }
        });
        this.onFormatMessage((session, text, speak, callback) => {
            const context = session.dialogData;
            const options = context.options;
            const hasMinValue = typeof options.minValue === 'number';
            const hasMaxValue = typeof options.maxValue === 'number';
            const hasIntegerOnly = options.integerOnly;
            const turnZero = context.turns === 0 || context.isReprompt;
            if (!turnZero && (hasMinValue || hasMaxValue || hasIntegerOnly)) {
                let errorPrompt;
                let context = session.toRecognizeContext();
                let top = PromptRecognizers_1.PromptRecognizers.findTopEntity(PromptRecognizers_1.PromptRecognizers.recognizeNumbers(context));
                if (top) {
                    let value = top.entity;
                    let bellowMin = hasMinValue && value < options.minValue;
                    let aboveMax = hasMaxValue && value > options.maxValue;
                    let notInteger = hasIntegerOnly && Math.floor(value) !== value;
                    if (hasMinValue && hasMaxValue && (bellowMin || aboveMax)) {
                        errorPrompt = 'number_range_error';
                    }
                    else if (hasMinValue && bellowMin) {
                        errorPrompt = 'number_minValue_error';
                    }
                    else if (hasMaxValue && aboveMax) {
                        errorPrompt = 'number_maxValue_error';
                    }
                    else if (hasIntegerOnly && notInteger) {
                        errorPrompt = 'number_integer_error';
                    }
                }
                if (errorPrompt) {
                    let text = Prompt_1.Prompt.gettext(session, errorPrompt, consts.Library.system);
                    let msg = { text: session.gettext(text, options) };
                    if (speak) {
                        msg.speak = Prompt_1.Prompt.gettext(session, speak);
                    }
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
exports.PromptNumber = PromptNumber;
