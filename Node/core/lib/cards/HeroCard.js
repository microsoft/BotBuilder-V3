"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const ThumbnailCard_1 = require("./ThumbnailCard");
class HeroCard extends ThumbnailCard_1.ThumbnailCard {
    constructor(session) {
        super(session);
        this.data.contentType = 'application/vnd.microsoft.card.hero';
    }
}
exports.HeroCard = HeroCard;
