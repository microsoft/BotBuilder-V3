"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Message_1 = require("../Message");
class CardAction {
    constructor(session) {
        this.session = session;
        this.data = {};
    }
    type(t) {
        if (t) {
            this.data.type = t;
        }
        return this;
    }
    title(text, ...args) {
        if (text) {
            this.data.title = Message_1.fmtText(this.session, text, args);
        }
        return this;
    }
    value(v) {
        if (v) {
            this.data.value = v;
        }
        return this;
    }
    image(url) {
        if (url) {
            this.data.image = url;
        }
        return this;
    }
    text(text, ...args) {
        if (text) {
            this.data.text = Message_1.fmtText(this.session, text, args);
        }
        return this;
    }
    displayText(text, ...args) {
        if (text) {
            this.data.displayText = Message_1.fmtText(this.session, text, args);
        }
        return this;
    }
    toAction() {
        return this.data;
    }
    static call(session, number, title) {
        return new CardAction(session).type('call').value(number).title(title || "Click to call");
    }
    static openUrl(session, url, title) {
        return new CardAction(session).type('openUrl').value(url).title(title || "Click to open website in your browser");
    }
    static openApp(session, url, title) {
        return new CardAction(session).type('openApp').value(url).title(title || "Click to open website in a webview");
    }
    static imBack(session, msg, title) {
        return new CardAction(session).type('imBack').value(msg).title(title || "Click to send response to bot");
    }
    static postBack(session, msg, title) {
        return new CardAction(session).type('postBack').value(msg).title(title || "Click to send response to bot");
    }
    static playAudio(session, url, title) {
        return new CardAction(session).type('playAudio').value(url).title(title || "Click to play audio file");
    }
    static playVideo(session, url, title) {
        return new CardAction(session).type('playVideo').value(url).title(title || "Click to play video");
    }
    static showImage(session, url, title) {
        return new CardAction(session).type('showImage').value(url).title(title || "Click to view image");
    }
    static downloadFile(session, url, title) {
        return new CardAction(session).type('downloadFile').value(url).title(title || "Click to download file");
    }
    static invoke(session, action, data, title) {
        const value = {};
        value[action] = data;
        return new CardAction(session).type('invoke').value(JSON.stringify(value)).title(title || "Click to send response to bot");
    }
    ;
    static dialogAction(session, action, data, title) {
        var value = 'action?' + action;
        if (data) {
            value += '=' + data;
        }
        return new CardAction(session).type('postBack').value(value).title(title || "Click to send response to bot");
    }
    static messageBack(session, msg, title) {
        return new CardAction(session).type('messageBack').value(msg).title(title || "Click to send response to bot");
    }
}
exports.CardAction = CardAction;
