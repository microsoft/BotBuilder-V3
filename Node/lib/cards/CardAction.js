var msg = require('../Message');
var CardAction = (function () {
    function CardAction(session) {
        this.session = session;
        this.data = {};
    }
    CardAction.prototype.type = function (t) {
        if (t) {
            this.data.type = t;
        }
        return this;
    };
    CardAction.prototype.title = function (text) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        if (text) {
            this.data.title = msg.fmtText(this.session, text, args);
        }
        return this;
    };
    CardAction.prototype.value = function (v) {
        if (v) {
            this.data.value = v;
        }
        return this;
    };
    CardAction.prototype.image = function (url) {
        if (url) {
            this.data.image = url;
        }
        return this;
    };
    CardAction.prototype.toAction = function () {
        return this.data;
    };
    CardAction.openUrl = function (session, url, title) {
        return new CardAction(session).type('openUrl').value(url).title(title);
    };
    CardAction.imBack = function (session, msg, title) {
        return new CardAction(session).type('imBack').value(msg).title(title);
    };
    CardAction.postBack = function (session, msg, title) {
        return new CardAction(session).type('postBack').value(msg).title(title);
    };
    CardAction.playAudio = function (session, url, title) {
        return new CardAction(session).type('playAudio').value(url).title(title);
    };
    CardAction.playVideo = function (session, url, title) {
        return new CardAction(session).type('playVideo').value(url).title(title);
    };
    CardAction.showImage = function (session, url, title) {
        return new CardAction(session).type('showImage').value(url).title(title);
    };
    CardAction.downloadFile = function (session, url, title) {
        return new CardAction(session).type('downloadFile').value(url).title(title);
    };
    return CardAction;
})();
exports.CardAction = CardAction;
