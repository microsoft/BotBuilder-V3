"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const MediaCard_1 = require("./MediaCard");
class AnimationCard extends MediaCard_1.MediaCard {
    constructor(session) {
        super(session);
        this.data.contentType = 'application/vnd.microsoft.card.animation';
    }
}
exports.AnimationCard = AnimationCard;
