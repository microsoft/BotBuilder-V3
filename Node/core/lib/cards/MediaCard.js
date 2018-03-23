"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Message_1 = require("../Message");
const Keyboard_1 = require("./Keyboard");
class MediaCard extends Keyboard_1.Keyboard {
    constructor(session) {
        super(session);
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
    autoloop(choice) {
        this.data.content.autoloop = choice;
        return this;
    }
    autostart(choice) {
        this.data.content.autostart = choice;
        return this;
    }
    shareable(choice) {
        this.data.content.shareable = choice;
        return this;
    }
    image(image) {
        if (image) {
            this.data.content.image = image.toImage ? image.toImage() : image;
        }
        return this;
    }
    media(list) {
        this.data.content.media = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                var media = list[i];
                this.data.content.media.push(media.toMedia ? media.toMedia() : media);
            }
        }
        return this;
    }
    value(param) {
        this.data.content.value = param;
        return this;
    }
}
exports.MediaCard = MediaCard;
