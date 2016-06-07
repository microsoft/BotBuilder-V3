var msg = require('../Message');
var Action = (function () {
    function Action(session) {
        this.session = session;
        this.data = {};
    }
    Action.prototype.type = function (t) {
        if (t) {
            this.data.type = t;
        }
        return this;
    };
    Action.prototype.title = function (text) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        if (text) {
            this.data.title = msg.fmtText(this.session, text, args);
        }
        return this;
    };
    Action.prototype.value = function (v) {
        if (v) {
            this.data.value = v;
        }
        return this;
    };
    Action.prototype.image = function (url) {
        if (url) {
            this.data.image = url;
        }
        return this;
    };
    Action.prototype.toAction = function () {
        return this.data;
    };
    Action.openUrl = function (session, url, title) {
        return new Action(session).type('openUrl').value(url).title(title);
    };
    Action.imBack = function (session, msg, title) {
        return new Action(session).type('imBack').value(msg).title(title);
    };
    Action.postBack = function (session, msg, title) {
        return new Action(session).type('postBack').value(msg).title(title);
    };
    Action.playAudio = function (session, url, title) {
        return new Action(session).type('playAudio').value(url).title(title);
    };
    Action.playVideo = function (session, url, title) {
        return new Action(session).type('playVideo').value(url).title(title);
    };
    Action.showImage = function (session, url, title) {
        return new Action(session).type('showImage').value(url).title(title);
    };
    Action.downloadFile = function (session, url, title) {
        return new Action(session).type('downloadFile').value(url).title(title);
    };
    return Action;
})();
exports.Action = Action;
