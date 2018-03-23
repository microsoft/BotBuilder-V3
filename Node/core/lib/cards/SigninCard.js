"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Message_1 = require("../Message");
class SigninCard {
    constructor(session) {
        this.session = session;
        this.data = {
            contentType: 'application/vnd.microsoft.card.signin',
            content: {}
        };
    }
    text(prompts, ...args) {
        if (prompts) {
            this.data.content.text = Message_1.fmtText(this.session, prompts, args);
        }
        return this;
    }
    button(title, url) {
        if (title && url) {
            this.data.content.buttons = [{
                    type: 'signin',
                    title: Message_1.fmtText(this.session, title),
                    value: url
                }];
        }
        return this;
    }
    toAttachment() {
        return this.data;
    }
}
exports.SigninCard = SigninCard;
