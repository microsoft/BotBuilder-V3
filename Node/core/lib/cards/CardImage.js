"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Message_1 = require("../Message");
class CardImage {
    constructor(session) {
        this.session = session;
        this.data = {};
    }
    url(u) {
        if (u) {
            this.data.url = u;
        }
        return this;
    }
    alt(text, ...args) {
        if (text) {
            this.data.alt = Message_1.fmtText(this.session, text, args);
        }
        return this;
    }
    tap(action) {
        if (action) {
            this.data.tap = action.toAction ? action.toAction() : action;
        }
        return this;
    }
    toImage() {
        return this.data;
    }
    static create(session, url) {
        return new CardImage(session).url(url);
    }
}
exports.CardImage = CardImage;
