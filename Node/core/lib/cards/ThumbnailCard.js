var msg = require('../Message');
var ThumbnailCard = (function () {
    function ThumbnailCard(session) {
        this.session = session;
        this.data = {
            contentType: 'application/vnd.microsoft.card.thumbnail',
            content: {}
        };
    }
    ThumbnailCard.prototype.title = function (text) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        if (text) {
            this.data.content.title = msg.fmtText(this.session, text, args);
        }
        return this;
    };
    ThumbnailCard.prototype.subtitle = function (text) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        if (text) {
            this.data.content.subtitle = msg.fmtText(this.session, text, args);
        }
        return this;
    };
    ThumbnailCard.prototype.text = function (text) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        if (text) {
            this.data.content.text = msg.fmtText(this.session, text, args);
        }
        return this;
    };
    ThumbnailCard.prototype.images = function (list) {
        this.data.content.images = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                var image = list[i];
                this.data.content.images.push(image.toImage ? image.toImage() : image);
            }
        }
        return this;
    };
    ThumbnailCard.prototype.buttons = function (list) {
        this.data.content.buttons = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                var action = list[i];
                this.data.content.buttons.push(action.toAction ? action.toAction() : action);
            }
        }
        return this;
    };
    ThumbnailCard.prototype.tap = function (action) {
        if (action) {
            this.data.content.tap = action.toAction ? action.toAction() : action;
        }
        return this;
    };
    ThumbnailCard.prototype.toAttachment = function () {
        return this.data;
    };
    return ThumbnailCard;
})();
exports.ThumbnailCard = ThumbnailCard;
