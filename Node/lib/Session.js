var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var dialog = require('./dialogs/Dialog');
var sprintf = require('sprintf-js');
var events = require('events');
var Session = (function (_super) {
    __extends(Session, _super);
    function Session(options) {
        _super.call(this);
        this.options = options;
        this.msgSent = false;
        this._isReset = false;
        this.lastSendTime = new Date().getTime();
        this.sendQueue = [];
        this.dialogs = options.dialogs;
        if (typeof this.options.minSendDelay !== 'number') {
            this.options.minSendDelay = 1000;
        }
    }
    Session.prototype.dispatch = function (sessionState, message) {
        var _this = this;
        var index = 0;
        var handlers;
        var session = this;
        var next = function () {
            var handler = index < handlers.length ? handlers[index] : null;
            if (handler) {
                index++;
                handler(session, next);
            }
            else {
                _this.routeMessage();
            }
        };
        this.sessionState = sessionState || { callstack: [], lastAccess: 0 };
        this.sessionState.lastAccess = new Date().getTime();
        this.message = message || { text: '' };
        if (!this.message.type) {
            this.message.type = 'Message';
        }
        handlers = this.dialogs.getMiddleware();
        next();
        return this;
    };
    Session.prototype.error = function (err) {
        err = err instanceof Error ? err : new Error(err.toString());
        console.error('Session Error: ' + err.message);
        this.emit('error', err);
        return this;
    };
    Session.prototype.gettext = function (msgid) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        return this.vgettext(msgid, args);
    };
    Session.prototype.ngettext = function (msgid, msgid_plural, count) {
        var tmpl;
        if (this.options.localizer && this.message) {
            tmpl = this.options.localizer.ngettext(this.message.language || '', msgid, msgid_plural, count);
        }
        else if (count == 1) {
            tmpl = msgid;
        }
        else {
            tmpl = msgid_plural;
        }
        return sprintf.sprintf(tmpl, count);
    };
    Session.prototype.send = function (msg) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        var ss = this.sessionState;
        if (ss.callstack.length > 0) {
            ss.callstack[ss.callstack.length - 1].state = this.dialogData || {};
        }
        var message = typeof msg == 'string' ? this.createMessage(msg, args) : msg;
        this.delayedEmit('send', message);
        return this;
    };
    Session.prototype.getMessageReceived = function () {
        return this.message.channelData;
    };
    Session.prototype.sendMessage = function (msg) {
        return this.send({ channelData: msg });
    };
    Session.prototype.messageSent = function () {
        return this.msgSent;
    };
    Session.prototype.beginDialog = function (id, args) {
        var dialog = this.dialogs.getDialog(id);
        if (!dialog) {
            throw new Error('Dialog[' + id + '] not found.');
        }
        var ss = this.sessionState;
        if (ss.callstack.length > 0) {
            ss.callstack[ss.callstack.length - 1].state = this.dialogData || {};
        }
        var cur = { id: id, state: {} };
        ss.callstack.push(cur);
        this.dialogData = cur.state;
        dialog.begin(this, args);
        return this;
    };
    Session.prototype.replaceDialog = function (id, args) {
        var dialog = this.dialogs.getDialog(id);
        if (!dialog) {
            throw new Error('Dialog[' + id + '] not found.');
        }
        var ss = this.sessionState;
        var cur = { id: id, state: {} };
        ss.callstack.pop();
        ss.callstack.push(cur);
        this.dialogData = cur.state;
        dialog.begin(this, args);
        return this;
    };
    Session.prototype.endDialog = function (result) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        var ss = this.sessionState;
        if (!ss || !ss.callstack || ss.callstack.length == 0) {
            console.error('ERROR: Too many calls to session.endDialog().');
            return this;
        }
        var m;
        var r = {};
        if (result) {
            if (typeof result === 'string') {
                m = this.createMessage(result, args);
            }
            else if (result.hasOwnProperty('text') || result.hasOwnProperty('channelData')) {
                m = result;
            }
            else {
                r = result;
            }
        }
        if (!r.hasOwnProperty('resumed')) {
            r.resumed = dialog.ResumeReason.completed;
        }
        r.childId = ss.callstack[ss.callstack.length - 1].id;
        ss.callstack.pop();
        if (ss.callstack.length > 0) {
            var cur = ss.callstack[ss.callstack.length - 1];
            this.dialogData = cur.state;
            if (m) {
                this.send(m);
            }
            var d = this.dialogs.getDialog(cur.id);
            d.dialogResumed(this, r);
        }
        else {
            this.send(m);
            if (r.error) {
                this.emit('error', r.error);
            }
            else {
                this.delayedEmit('quit');
            }
        }
        return this;
    };
    Session.prototype.compareConfidence = function (language, utterance, score, callback) {
        var comparer = new SessionConfidenceComparor(this, language, utterance, score, callback);
        comparer.next();
    };
    Session.prototype.reset = function (dialogId, dialogArgs) {
        this._isReset = true;
        this.sessionState.callstack = [];
        if (!dialogId) {
            dialogId = this.options.dialogId;
            dialogArgs = dialogArgs || this.options.dialogArgs;
        }
        this.beginDialog(dialogId, dialogArgs);
        return this;
    };
    Session.prototype.isReset = function () {
        return this._isReset;
    };
    Session.prototype.createMessage = function (text, args) {
        var message = {
            text: this.vgettext(text, args)
        };
        if (this.message.language) {
            message.language = this.message.language;
        }
        return message;
    };
    Session.prototype.routeMessage = function () {
        try {
            var ss = this.sessionState;
            if (ss.callstack.length == 0) {
                this.beginDialog(this.options.dialogId, this.options.dialogArgs);
            }
            else if (this.validateCallstack()) {
                var cur = ss.callstack[ss.callstack.length - 1];
                var dialog = this.dialogs.getDialog(cur.id);
                this.dialogData = cur.state;
                dialog.replyReceived(this);
            }
            else {
                console.error('Callstack is invalid, resetting session.');
                this.reset(this.options.dialogId, this.options.dialogArgs);
            }
        }
        catch (e) {
            this.error(e);
        }
    };
    Session.prototype.vgettext = function (msgid, args) {
        var tmpl;
        if (this.options.localizer && this.message) {
            tmpl = this.options.localizer.gettext(this.message.language || '', msgid);
        }
        else {
            tmpl = msgid;
        }
        return args && args.length > 0 ? sprintf.vsprintf(tmpl, args) : tmpl;
    };
    Session.prototype.validateCallstack = function () {
        var ss = this.sessionState;
        for (var i = 0; i < ss.callstack.length; i++) {
            var id = ss.callstack[i].id;
            if (!this.dialogs.hasDialog(id)) {
                return false;
            }
        }
        return true;
    };
    Session.prototype.delayedEmit = function (event, message) {
        var _this = this;
        var now = new Date().getTime();
        var delaySend = function () {
            setTimeout(function () {
                var entry = _this.sendQueue.shift();
                _this.lastSendTime = now = new Date().getTime();
                _this.emit(entry.event, entry.msg);
                if (_this.sendQueue.length > 0) {
                    delaySend();
                }
            }, _this.options.minSendDelay - (now - _this.lastSendTime));
        };
        if (this.sendQueue.length == 0) {
            this.msgSent = true;
            if ((now - this.lastSendTime) >= this.options.minSendDelay) {
                this.lastSendTime = now;
                this.emit(event, message);
            }
            else {
                this.sendQueue.push({ event: event, msg: message });
                delaySend();
            }
        }
        else {
            this.sendQueue.push({ event: event, msg: message });
        }
    };
    return Session;
})(events.EventEmitter);
exports.Session = Session;
var SessionConfidenceComparor = (function () {
    function SessionConfidenceComparor(session, language, utterance, score, callback) {
        this.session = session;
        this.language = language;
        this.utterance = utterance;
        this.score = score;
        this.callback = callback;
        this.index = session.sessionState.callstack.length - 1;
        this.userData = session.userData;
    }
    SessionConfidenceComparor.prototype.next = function () {
        this.index--;
        if (this.index >= 0) {
            this.getDialog().compareConfidence(this, this.language, this.utterance, this.score);
        }
        else {
            this.callback(false);
        }
    };
    SessionConfidenceComparor.prototype.endDialog = function (result) {
        this.session.sessionState.callstack.splice(this.index + 1);
        this.getDialog().dialogResumed(this.session, result || { resumed: dialog.ResumeReason.childEnded });
        this.callback(true);
    };
    SessionConfidenceComparor.prototype.send = function (msg) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        args.splice(0, 0, [msg]);
        Session.prototype.send.apply(this.session, args);
        this.callback(true);
    };
    SessionConfidenceComparor.prototype.getDialog = function () {
        var cur = this.session.sessionState.callstack[this.index];
        this.dialogData = cur.state;
        return this.session.dialogs.getDialog(cur.id);
    };
    return SessionConfidenceComparor;
})();
