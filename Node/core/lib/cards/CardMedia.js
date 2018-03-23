"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class CardMedia {
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
    profile(text) {
        if (text) {
            this.data.profile = text;
        }
        return this;
    }
    toMedia() {
        return this.data;
    }
    static create(session, url) {
        return new CardMedia(session).url(url);
    }
}
exports.CardMedia = CardMedia;
