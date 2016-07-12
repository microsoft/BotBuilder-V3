var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var dlg = require('./dialogs/Dialog');
var consts = require('./consts');
var sprintf = require('sprintf-js');
var events = require('events');
var msg = require('./Message');
var logger = require('./logger');
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
        this.library = options.library;
        if (typeof this.options.autoBatchDelay !== 'number') {
            this.options.autoBatchDelay = 250;
        }
    }
    Session.prototype.dispatch = function (sessionState, message) {
        var _this = this;
        var index = 0;
        var session = this;
        var middleware = this.options.middleware || [];
        var next = function () {
            var handler = index < middleware.length ? middleware[index] : null;
            if (handler) {
                index++;
                handler(session, next);
            }
            else {
                _this.routeMessage();
            }
        };
        this.sessionState = sessionState || { callstack: [], lastAccess: 0, version: 0.0 };
        this.sessionState.lastAccess = new Date().getTime();
        var cur = this.curDialog();
        if (cur) {
            this.dialogData = cur.state;
        }
        this.message = (message || { text: '' });
        if (!this.message.type) {
            this.message.type = consts.messageType;
        }
        next();
        return this;
    };
    Session.prototype.error = function (err) {
        err = err instanceof Error ? err : new Error(err.toString());
        logger.info(this, 'session.error()');
        this.endConversation(this.options.dialogErrorMessage || 'Oops. Something went wrong and we need to start over.');
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
            tmpl = this.options.localizer.ngettext(this.message.textLocale || '', messageid, messageid_plural, count);
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
        logger.info(this, 'session.save()');
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
            logger.info(this, 'session.send()');
        }
        this.startBatch();
        return this;
    };
    Session.prototype.messageSent = function () {
        return this.msgSent;
    };
    Session.prototype.beginDialog = function (id, args) {
        logger.info(this, 'session.beginDialog(%s)', id);
        var id = this.resolveDialogId(id);
        var dialog = this.findDialog(id);
        if (!dialog) {
            throw new Error('Dialog[' + id + '] not found.');
        }
        this.pushDialog({ id: id, state: {} });
        this.startBatch();
        dialog.begin(this, args);
        return this;
    };
    Session.prototype.replaceDialog = function (id, args) {
        logger.info(this, 'session.replaceDialog(%s)', id);
        var id = this.resolveDialogId(id);
        var dialog = this.findDialog(id);
        if (!dialog) {
            throw new Error('Dialog[' + id + '] not found.');
        }
        this.popDialog();
        this.pushDialog({ id: id, state: {} });
        this.startBatch();
        dialog.begin(this, args);
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
        this.privateConversationData = {};
        logger.info(this, 'session.endConversation()');
        var ss = this.sessionState;
        ss.callstack = [];
        this.sendBatch();
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
        logger.info(this, 'session.endDialog()');
        var childId = cur.id;
        cur = this.popDialog();
        this.startBatch();
        if (cur) {
            var dialog = this.findDialog(cur.id);
            if (dialog) {
                dialog.dialogResumed(this, { resumed: dlg.ResumeReason.completed, response: true, childId: childId });
            }
            else {
                this.error(new Error("ERROR: Can't resume missing parent dialog '" + cur.id + "'."));
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
            result.resumed = dlg.ResumeReason.completed;
        }
        result.childId = cur.id;
        logger.info(this, 'session.endDialogWithResult()');
        cur = this.popDialog();
        this.startBatch();
        if (cur) {
            var dialog = this.findDialog(cur.id);
            if (dialog) {
                dialog.dialogResumed(this, result);
            }
            else {
                this.error(new Error("ERROR: Can't resume missing parent dialog '" + cur.id + "'."));
            }
        }
        return this;
    };
    Session.prototype.reset = function (dialogId, dialogArgs) {
        logger.info(this, 'session.reset()');
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
    Session.prototype.sendBatch = function () {
        var _this = this;
        logger.info(this, 'session.sendBatch() sending %d messages', this.batch.length);
        if (this.sendingBatch) {
            return;
        }
        if (this.batchTimer) {
            clearTimeout(this.batchTimer);
            this.batchTimer = null;
        }
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
        if (!msg.textLocale && this.message.textLocale) {
            msg.textLocale = this.message.textLocale;
        }
    };
    Session.prototype.routeMessage = function () {
        try {
            var cur = this.curDialog();
            if (!cur) {
                this.beginDialog(this.options.dialogId, this.options.dialogArgs);
            }
            else if (this.validateCallstack()) {
                var dialog = this.findDialog(cur.id);
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
            tmpl = this.options.localizer.gettext(this.message.textLocale || '', messageid);
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
            if (!this.findDialog(id)) {
                return false;
            }
        }
        return true;
    };
    Session.prototype.resolveDialogId = function (id) {
        if (id.indexOf(':') >= 0) {
            return id;
        }
        var cur = this.curDialog();
        var libName = cur ? cur.id.split(':')[0] : consts.Library.default;
        return libName + ':' + id;
    };
    Session.prototype.findDialog = function (id) {
        var parts = id.split(':');
        return this.library.findDialog(parts[0] || consts.Library.default, parts[1]);
    };
    Session.prototype.pushDialog = function (ds) {
        var ss = this.sessionState;
        var cur = this.curDialog();
        if (cur) {
            cur.state = this.dialogData || {};
        }
        ss.callstack.push(ds);
        this.dialogData = ds.state || {};
        return ds;
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
        console.warn("Session.getMessageReceived() is deprecated. Use Session.message.sourceEvent instead.");
        return this.message.sourceEvent;
    };
    return Session;
})(events.EventEmitter);
exports.Session = Session;
