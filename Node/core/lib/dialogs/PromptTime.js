"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Prompt_1 = require("./Prompt");
const PromptRecognizers_1 = require("./PromptRecognizers");
const consts = require("../consts");
class PromptTime extends Prompt_1.Prompt {
    constructor(features) {
        super({
            defaultRetryPrompt: 'default_time',
            defaultRetryNamespace: consts.Library.system
        });
        this.updateFeatures(features);
        this.onRecognize((context, cb) => {
            if (context.message.text && !this.features.disableRecognizer) {
                let options = context.dialogData.options;
                let entities = PromptRecognizers_1.PromptRecognizers.recognizeTimes(context, options);
                let top = PromptRecognizers_1.PromptRecognizers.findTopEntity(entities);
                if (top) {
                    cb(null, top.score, top);
                }
                else {
                    cb(null, 0.0);
                }
            }
            else {
                cb(null, 0.0);
            }
        });
        this.matches(consts.Intents.Repeat, (session) => {
            session.dialogData.turns = 0;
            this.sendPrompt(session);
        });
    }
}
exports.PromptTime = PromptTime;
