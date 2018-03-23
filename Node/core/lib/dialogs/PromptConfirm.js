"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Prompt_1 = require("./Prompt");
const PromptChoice_1 = require("./PromptChoice");
const PromptRecognizers_1 = require("./PromptRecognizers");
const consts = require("../consts");
class PromptConfirm extends PromptChoice_1.PromptChoice {
    constructor(features) {
        super({
            defaultRetryPrompt: 'default_confirm',
            defaultRetryNamespace: consts.Library.system,
            recognizeNumbers: false,
            recognizeOrdinals: false,
            recognizeChoices: false,
            defaultListStyle: Prompt_1.ListStyle.none
        });
        this.updateFeatures(features);
        this.onRecognize((context, cb) => {
            if (context.message.text && !this.features.disableRecognizer) {
                let entities = PromptRecognizers_1.PromptRecognizers.recognizeBooleans(context);
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
        this.onChoices((context, callback) => {
            let options = context.dialogData.options;
            if (options.choices) {
                callback(null, options.choices);
            }
            else {
                let locale = context.preferredLocale();
                callback(null, [
                    { value: context.localizer.gettext(locale, 'confirm_yes', consts.Library.system) },
                    { value: context.localizer.gettext(locale, 'confirm_no', consts.Library.system) }
                ]);
            }
        });
    }
}
exports.PromptConfirm = PromptConfirm;
