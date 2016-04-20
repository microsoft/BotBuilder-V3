var session = require('./Session');
var sprintf = require('sprintf-js');
var Message = (function () {
    function Message() {
    }
    Message.prototype.setLanguage = function (language) {
        var m = this;
        m.language = language;
        return this;
    };
    Message.prototype.setText = function (ses, prompts) {
        var args = [];
        for (var _i = 2; _i < arguments.length; _i++) {
            args[_i - 2] = arguments[_i];
        }
        var m = this;
        var msg = typeof prompts == 'string' ? prompts : Message.randomPrompt(prompts);
        args.unshift(msg);
        m.text = session.Session.prototype.gettext.apply(ses, args);
        return this;
    };
    Message.prototype.setNText = function (ses, msg, msg_plural, count) {
        var m = this;
        m.text = ses.ngettext(msg, msg_plural, count);
        return this;
    };
    Message.prototype.composePrompt = function (ses, prompts) {
        var args = [];
        for (var _i = 2; _i < arguments.length; _i++) {
            args[_i - 2] = arguments[_i];
        }
        var m = this;
        m.text = Message.composePrompt(ses, prompts, args);
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
    Message.randomPrompt = function (prompts) {
        var i = Math.floor(Math.random() * prompts.length);
        return prompts[i];
    };
    Message.composePrompt = function (ses, prompts, args) {
        var connector = '';
        var prompt = '';
        for (var i = 0; i < prompts.length; i++) {
            prompt += connector + ses.gettext(Message.randomPrompt(prompts[1]));
            connector = ' ';
        }
        return args && args.length > 0 ? sprintf.vsprintf(prompt, args) : prompt;
    };
    return Message;
})();
exports.Message = Message;
