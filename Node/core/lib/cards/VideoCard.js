"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Message_1 = require("../Message");
const MediaCard_1 = require("./MediaCard");
class VideoCard extends MediaCard_1.MediaCard {
    constructor(session) {
        super(session);
        this.data.contentType = 'application/vnd.microsoft.card.video';
    }
    aspect(text, ...args) {
        if (text) {
            this.data.content.aspect = Message_1.fmtText(this.session, text, args);
        }
        return this;
    }
}
exports.VideoCard = VideoCard;
