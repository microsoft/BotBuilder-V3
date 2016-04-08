var session = require('./Session');
var Message = (function () {
    function Message() {
    }
    Message.prototype.setLanguage = function (language) {
        var m = this;
        m.language = language;
        return this;
    };
    Message.prototype.setText = function (ses, msg) {
        var args = [];
        for (var _i = 2; _i < arguments.length; _i++) {
            args[_i - 2] = arguments[_i];
        }
        var m = this;
        args.unshift(msg);
        m.text = session.Session.prototype.gettext.apply(ses, args);
        return this;
    };
    Message.prototype.setNText = function (ses, msg, msg_plural, count) {
        var m = this;
        m.text = ses.ngettext(msg, msg_plural, count);
        return this;
    };
    Message.prototype.addAttachment = function (attachment) {
        var m = this;
        if (!m.attachments) {
            m.attachments = [];
        }
        m.attachments.push(attachment);
        return this;
    };
    Message.prototype.setChannelData = function (data) {
        var m = this;
        m.channelData = data;
        return this;
    };
    return Message;
})();
exports.Message = Message;
