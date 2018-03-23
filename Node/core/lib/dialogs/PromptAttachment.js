"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Prompt_1 = require("./Prompt");
const consts = require("../consts");
class PromptAttachment extends Prompt_1.Prompt {
    constructor(features) {
        super({
            defaultRetryPrompt: 'default_file',
            defaultRetryNamespace: consts.Library.system,
            recognizeScore: 1.0
        });
        this.updateFeatures(features);
        this.onRecognize((context, cb) => {
            if (context.message.attachments && !this.features.disableRecognizer) {
                const options = context.dialogData.options;
                let contentTypes = typeof options.contentTypes == 'string' ? options.contentTypes.split('|') : options.contentTypes;
                let attachments = [];
                context.message.attachments.forEach((value) => {
                    if (this.allowed(value, contentTypes)) {
                        attachments.push(value);
                    }
                });
                if (attachments.length > 0) {
                    cb(null, this.features.recognizeScore, attachments);
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
    allowed(attachment, contentTypes) {
        let allowed = false;
        if (contentTypes && contentTypes.length > 0) {
            const type = attachment.contentType.toLowerCase();
            for (let i = 0; !allowed && i < contentTypes.length; i++) {
                const filter = contentTypes[i].toLowerCase();
                if (filter.charAt(filter.length - 1) == '*') {
                    if (type.indexOf(filter.substr(0, filter.length - 1)) == 0) {
                        allowed = true;
                    }
                }
                else if (type === filter) {
                    allowed = true;
                }
            }
        }
        else {
            allowed = true;
        }
        return allowed;
    }
}
exports.PromptAttachment = PromptAttachment;
