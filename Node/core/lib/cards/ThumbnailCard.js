"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Message_1 = require("../Message");
const Keyboard_1 = require("./Keyboard");
class ThumbnailCard extends Keyboard_1.Keyboard {
    constructor(session) {
        super(session);
        this.data.contentType = 'application/vnd.microsoft.card.thumbnail';
    }
    title(text, ...args) {
        if (text) {
            this.data.content.title = Message_1.fmtText(this.session, text, args);
        }
        return this;
    }
    subtitle(text, ...args) {
        if (text) {
            this.data.content.subtitle = Message_1.fmtText(this.session, text, args);
        }
        return this;
    }
    text(text, ...args) {
        if (text) {
            this.data.content.text = Message_1.fmtText(this.session, text, args);
        }
        return this;
    }
    images(list) {
        this.data.content.images = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                var image = list[i];
                this.data.content.images.push(image.toImage ? image.toImage() : image);
            }
        }
        return this;
    }
    tap(action) {
        if (action) {
            this.data.content.tap = action.toAction ? action.toAction() : action;
        }
        return this;
    }
}
exports.ThumbnailCard = ThumbnailCard;
