var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var dialog = require('./dialogs/Dialog');
var sprintf = require('sprintf-js');
var events = require('events');
var msg = require('./Message');
var Session = (function (_super) {
    __extends(Session, _super);
    function Session(options) {
        _super.call(this);
        this.options = options;
        this.msgSent = false;
        this._isReset = false;
        this.lastSendTime = new Date().getTime();
        this.batch = [];
        this.batchStarted = false;
        this.sendingBatch = false;
        this.dialogs = options.dialogs;
        if (typeof this.options.autoBatchDelay !== 'number') {
            this.options.autoBatchDelay = 150;
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
        this.message = (message || { text: '' });
        if (!this.message.type) {
            this.message.type = 'Message';
        }
        handlers = this.dialogs.getMiddleware();
        next();
        return this;
    };
    Session.prototype.error = function (err) {
        err = err instanceof Error ? err : new Error(err.toString());
        console.error('ERROR: Session Error: ' + err.message);
        this.emit('error', err);
        return this;
    };
    Session.prototype.gettext = function (messageid) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        return this.vgettext(messageid, args);
    };
    Session.prototype.ngettext = function (messageid, messageid_plural, count) {
        var tmpl;
        if (this.options.localizer && this.message) {
            tmpl = this.options.localizer.ngettext(this.message.local || '', messageid, messageid_plural, count);
        }
        else if (count == 1) {
            tmpl = messageid;
        }
        else {
            tmpl = messageid_plural;
        }
        return sprintf.sprintf(tmpl, count);
    };
    Session.prototype.save = function () {
        this.startBatch();
        return this;
    };
    Session.prototype.send = function (message) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        this.msgSent = true;
        if (message) {
            var m;
            if (typeof message == 'string' || Array.isArray(message)) {
                m = this.createMessage(message, args);
            }
            else if (message.toMessage) {
                m = message.toMessage();
            }
            else {
                m = message;
            }
            this.prepareMessage(m);
            this.batch.push(m);
        }
        this.startBatch();
        return this;
    };
    Session.prototype.sendMessage = function (message) {
        this.msgSent = true;
        if (message) {
            var m = message.toMessage ? message.toMessage() : message;
            this.prepareMessage(m);
            this.batch.push(m);
        }
        this.startBatch();
        return this;
    };
    Session.prototype.messageSent = function () {
        return this.msgSent;
    };
    Session.prototype.beginDialog = function (id, args) {
        var dlg = this.dialogs.getDialog(id);
        if (!dlg) {
            throw new Error('Dialog[' + id + '] not found.');
        }
        this.pushDialog({ id: id, state: {} });
        this.startBatch();
        dlg.begin(this, args);
        return this;
    };
    Session.prototype.replaceDialog = function (id, args) {
        var dlg = this.dialogs.getDialog(id);
        if (!dlg) {
            throw new Error('Dialog[' + id + '] not found.');
        }
        this.popDialog();
        this.pushDialog({ id: id, state: {} });
        this.startBatch();
        dlg.begin(this, args);
        return this;
    };
    Session.prototype.endConversation = function (message) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        var m;
        if (message) {
            if (typeof message == 'string' || Array.isArray(message)) {
                m = this.createMessage(message, args);
            }
            else if (message.toMessage) {
                m = message.toMessage();
            }
            else {
                m = message;
            }
            this.msgSent = true;
            this.prepareMessage(m);
            this.batch.push(m);
        }
        var ss = this.sessionState;
        ss.callstack = [];
        this.startBatch();
        return this;
    };
    Session.prototype.endDialog = function (message) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        if (typeof message === 'object' && (message.hasOwnProperty('response') || message.hasOwnProperty('resumed') || message.hasOwnProperty('error'))) {
            console.warn('Returning results via Session.endDialog() is deprecated. Use Session.endDialogWithResult() instead.');
            return this.endDialogWithResult(message);
        }
        var cur = this.curDialog();
        if (!cur) {
            console.error('ERROR: Too many calls to session.endDialog().');
            return this;
        }
        var m;
        if (message) {
            if (typeof message == 'string' || Array.isArray(message)) {
                m = this.createMessage(message, args);
            }
            else if (message.toMessage) {
                m = message.toMessage();
            }
            else {
                m = message;
            }
            this.msgSent = true;
            this.prepareMessage(m);
            this.batch.push(m);
        }
        var childId = cur.id;
        cur = this.popDialog();
        this.startBatch();
        if (cur) {
            var dlg = this.dialogs.getDialog(cur.id);
            if (dlg) {
                dlg.dialogResumed(this, { resumed: dialog.ResumeReason.completed, response: true, childId: childId });
            }
            else {
                this.endDialogWithResult({ resumed: dialog.ResumeReason.notCompleted, error: new Error("ERROR: Can't resume missing parent dialog '" + cur.id + "'.") });
            }
        }
        return this;
    };
    Session.prototype.endDialogWithResult = function (result) {
        var cur = this.curDialog();
        if (!cur) {
            console.error('ERROR: Too many calls to session.endDialog().');
            return this;
        }
        result = result || {};
        if (!result.hasOwnProperty('resumed')) {
            result.resumed = dialog.ResumeReason.completed;
        }
        result.childId = cur.id;
        cur = this.popDialog();
        this.startBatch();
        if (cur) {
            var dlg = this.dialogs.getDialog(cur.id);
            if (dlg) {
                dlg.dialogResumed(this, result);
            }
            else {
                this.endDialogWithResult({ resumed: dialog.ResumeReason.notCompleted, error: new Error("ERROR: Can't resume missing parent dialog '" + cur.id + "'.") });
            }
        }
        return this;
    };
    Session.prototype.reset = function (dialogId, dialogArgs) {
        this._isReset = true;
        this.sessionState.callstack = [];
        if (!dialogId) {
            dialogId = this.options.dialogId;
            dialogArgs = this.options.dialogArgs;
        }
        this.beginDialog(dialogId, dialogArgs);
        return this;
    };
    Session.prototype.isReset = function () {
        return this._isReset;
    };
    Session.prototype.startBatch = function () {
        var _this = this;
        this.batchStarted = true;
        if (!this.sendingBatch) {
            if (this.batchTimer) {
                clearTimeout(this.batchTimer);
            }
            this.batchTimer = setTimeout(function () {
                _this.sendBatch();
            }, this.options.autoBatchDelay);
        }
    };
    Session.prototype.sendBatch = function () {
        var _this = this;
        this.batchTimer = null;
        var batch = this.batch;
        this.batch = [];
        this.batchStarted = false;
        this.sendingBatch = true;
        var cur = this.curDialog();
        if (cur) {
            cur.state = this.dialogData;
        }
        this.options.onSave(function (err) {
            if (!err && batch.length) {
                _this.options.onSend(batch, function (err) {
                    _this.sendingBatch = false;
                    if (_this.batchStarted) {
                        _this.startBatch();
                    }
                });
            }
            else {
                _this.sendingBatch = false;
                if (_this.batchStarted) {
                    _this.startBatch();
                }
            }
        });
    };
    Session.prototype.createMessage = function (text, args) {
        args.unshift(text);
        var message = new msg.Message(this);
        msg.Message.prototype.text.apply(message, args);
        return message.toMessage();
    };
    Session.prototype.prepareMessage = function (msg) {
        if (!msg.type) {
            msg.type = 'message';
        }
        if (!msg.address) {
            msg.address = this.message.address;
        }
        if (!msg.local && this.message.local) {
            msg.local = this.message.local;
        }
    };
    Session.prototype.routeMessage = function () {
        try {
            var cur = this.curDialog();
            if (!cur) {
                this.beginDialog(this.options.dialogId, this.options.dialogArgs);
            }
            else if (this.validateCallstack()) {
                var dialog = this.dialogs.getDialog(cur.id);
                this.dialogData = cur.state;
                dialog.replyReceived(this);
            }
            else {
                console.warn('Callstack is invalid, resetting session.');
                this.reset(this.options.dialogId, this.options.dialogArgs);
            }
        }
        catch (e) {
            this.error(e);
        }
    };
    Session.prototype.vgettext = function (messageid, args) {
        var tmpl;
        if (this.options.localizer && this.message) {
            tmpl = this.options.localizer.gettext(this.message.local || '', messageid);
        }
        else {
            tmpl = messageid;
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
    Session.prototype.pushDialog = function (dlg) {
        var ss = this.sessionState;
        var cur = this.curDialog();
        if (cur) {
            cur.state = this.dialogData || {};
        }
        ss.callstack.push(dlg);
        this.dialogData = dlg.state || {};
        return dlg;
    };
    Session.prototype.popDialog = function () {
        var ss = this.sessionState;
        if (ss.callstack.length > 0) {
            ss.callstack.pop();
        }
        var cur = this.curDialog();
        this.dialogData = cur ? cur.state : null;
        return cur;
    };
    Session.prototype.curDialog = function () {
        var cur;
        var ss = this.sessionState;
        if (ss.callstack.length > 0) {
            cur = ss.callstack[ss.callstack.length - 1];
        }
        return cur;
    };
    Session.prototype.getMessageReceived = function () {
        console.warn("Session.getMessageReceived() is deprecated. Use Session.message.channelData instead.");
        return this.message.channelData;
    };
    return Session;
})(events.EventEmitter);
exports.Session = Session;
