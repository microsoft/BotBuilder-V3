var msg = require('../Message');
var Image = (function () {
    function Image(session) {
        this.session = session;
        this.data = {};
    }
    Image.prototype.url = function (u) {
        if (u) {
            this.data.url = u;
        }
        return this;
    };
    Image.prototype.alt = function (text) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        if (text) {
            this.data.alt = msg.fmtText(this.session, text, args);
        }
        return this;
    };
    Image.prototype.tap = function (action) {
        if (action) {
            this.data.tap = action.toAction ? action.toAction() : action;
        }
        return this;
    };
    Image.prototype.toImage = function () {
        return this.data;
    };
    Image.create = function (session, url) {
        return new Image(session).url(url);
    };
    return Image;
})();
exports.Image = Image;
