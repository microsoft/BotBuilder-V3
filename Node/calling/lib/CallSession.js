"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var dlg = require('./dialogs/Dialog');
var consts = require('./consts');
var sprintf = require('sprintf-js');
var events = require('events');
var utils = require('./utils');
var answer = require('./workflow/AnswerAction');
var hangup = require('./workflow/HangupAction');
var reject = require('./workflow/RejectAction');
var playPrompt = require('./workflow/PlayPromptAction');
var prompt = require('./workflow/Prompt');
exports.CallState = {
    idle: 'idle',
    incoming: 'incoming',
    establishing: 'establishing',
    established: 'established',
    hold: 'hold',
    unhold: 'unhold',
    transferring: 'transferring',
    redirecting: 'redirecting',
    terminating: 'terminating',
    terminated: 'terminated'
};
exports.ModalityType = {
    audio: 'audio',
    video: 'video',
    videoBasedScreenSharing: 'videoBasedScreenSharing'
};
exports.NotificationType = {
    rosterUpdate: 'rosterUpdate',
    callStateChange: 'callStateChange'
};
exports.OperationOutcome = {
    success: 'success',
    failure: 'failure'
};
var CallSession = (function (_super) {
    __extends(CallSession, _super);
    function CallSession(options) {
        _super.call(this);
        this.options = options;
        this.msgSent = false;
        this._isReset = false;
        this.lastSendTime = new Date().getTime();
        this.actions = [];
        this.batchStarted = false;
        this.sendingBatch = false;
        this.library = options.library;
        this.promptDefaults = options.promptDefaults;
        this.recognizeDefaults = options.recognizeDefaults;
        this.recordDefaults = options.recordDefaults;
        if (typeof this.options.autoBatchDelay !== 'number') {
            this.options.autoBatchDelay = 250;
        }
    }
    CallSession.prototype.dispatch = function (sessionState, message) {
        var _this = this;
        var index = 0;
        var middleware = this.options.middleware || [];
        var session = this;
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
        this.message = (message || {});
        this.address = utils.clone(this.message.address);
        next();
        return this;
    };
    CallSession.prototype.error = function (err) {
        var msg = err.toString();
        err = err instanceof Error ? err : new Error(msg);
        this.endConversation(this.options.dialogErrorMessage || 'Oops. Something went wrong and we need to start over.');
        this.emit('error', err);
        return this;
    };
    CallSession.prototype.gettext = function (messageid) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        return this.vgettext(messageid, args);
    };
    CallSession.prototype.ngettext = function (messageid, messageid_plural, count) {
        var tmpl;
        if (this.options.localizer && this.message) {
            tmpl = this.options.localizer.ngettext(this.message.user.locale || '', messageid, messageid_plural, count);
        }
        else if (count == 1) {
            tmpl = messageid;
        }
        else {
            tmpl = messageid_plural;
        }
        return sprintf.sprintf(tmpl, count);
    };
    CallSession.prototype.save = function () {
        this.startBatch();
        return this;
    };
    CallSession.prototype.answer = function () {
        this.msgSent = true;
        this.actions.push(new answer.AnswerAction(this).toAction());
        this.startBatch();
        return this;
    };
    CallSession.prototype.reject = function () {
        this.msgSent = true;
        this.actions.push(new reject.RejectAction(this).toAction());
        this.startBatch();
        return this;
    };
    CallSession.prototype.hangup = function () {
        this.msgSent = true;
        this.actions.push(new hangup.HangupAction(this).toAction());
        this.startBatch();
        return this;
    };
    CallSession.prototype.send = function (action) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        this.msgSent = true;
        if (action) {
            var a;
            if (typeof action == 'string' || Array.isArray(action)) {
                a = this.createPlayPromptAction(action, args);
            }
            else if (action.toAction) {
                a = action.toAction();
            }
            else {
                a = action;
            }
            this.actions.push(a);
        }
        this.startBatch();
        return this;
    };
    CallSession.prototype.messageSent = function () {
        return this.msgSent;
    };
    CallSession.prototype.beginDialog = function (id, args) {
        id = this.resolveDialogId(id);
        var dialog = this.findDialog(id);
        if (!dialog) {
            throw new Error('Dialog[' + id + '] not found.');
        }
        this.pushDialog({ id: id, state: {} });
        this.startBatch();
        dialog.begin(this, args);
        return this;
    };
    CallSession.prototype.replaceDialog = function (id, args) {
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
    CallSession.prototype.endConversation = function (action) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        if (action) {
            var a;
            if (typeof action == 'string' || Array.isArray(action)) {
                a = this.createPlayPromptAction(action, args);
            }
            else if (action.toAction) {
                a = action.toAction();
            }
            else {
                a = action;
            }
            this.msgSent = true;
            this.actions.push(a);
        }
        this.privateConversationData = {};
        this.addCallControl(true);
        var ss = this.sessionState;
        ss.callstack = [];
        this.sendBatch();
        return this;
    };
    CallSession.prototype.endDialog = function (action) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        var cur = this.curDialog();
        if (!cur) {
            console.error('ERROR: Too many calls to session.endDialog().');
            return this;
        }
        if (action) {
            var a;
            if (typeof action == 'string' || Array.isArray(action)) {
                a = this.createPlayPromptAction(action, args);
            }
            else if (action.toAction) {
                a = action.toAction();
            }
            else {
                a = action;
            }
            this.msgSent = true;
            this.actions.push(a);
        }
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
        else {
            this.endConversation();
        }
        return this;
    };
    CallSession.prototype.endDialogWithResult = function (result) {
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
        else {
            this.endConversation();
        }
        return this;
    };
    CallSession.prototype.reset = function (dialogId, dialogArgs) {
        this._isReset = true;
        this.sessionState.callstack = [];
        if (!dialogId) {
            dialogId = this.options.dialogId;
            dialogArgs = this.options.dialogArgs;
        }
        this.beginDialog(dialogId, dialogArgs);
        return this;
    };
    CallSession.prototype.isReset = function () {
        return this._isReset;
    };
    CallSession.prototype.sendBatch = function () {
        var _this = this;
        if (this.sendingBatch) {
            return;
        }
        if (this.batchTimer) {
            clearTimeout(this.batchTimer);
            this.batchTimer = null;
        }
        this.batchStarted = false;
        this.sendingBatch = true;
        this.addCallControl(false);
        var workflow = {
            type: 'workflow',
            agent: consts.agent,
            source: this.address.channelId,
            address: this.address,
            actions: this.actions,
            notificationSubscriptions: ["callStateChange"]
        };
        this.actions = [];
        var cur = this.curDialog();
        if (cur) {
            cur.state = this.dialogData;
        }
        this.options.onSave(function (err) {
            if (!err && workflow.actions.length) {
                _this.options.onSend(workflow, function (err) {
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
    CallSession.prototype.addCallControl = function (alsoEndCall) {
        var hasAnswer = (this.message.type !== 'conversation');
        var hasEndCall = false;
        var hasOtherActions = false;
        this.actions.forEach(function (a) {
            switch (a.action) {
                case 'answer':
                    hasAnswer = true;
                    break;
                case 'hangup':
                case 'reject':
                    hasEndCall = true;
                    break;
                default:
                    hasOtherActions = true;
                    break;
            }
        });
        if (!hasAnswer && hasOtherActions) {
            this.actions.unshift(new answer.AnswerAction(this).toAction());
            hasAnswer = true;
        }
        if (alsoEndCall && !hasEndCall) {
            if (hasAnswer) {
                this.actions.push(new hangup.HangupAction(this).toAction());
            }
            else {
                this.actions.push(new reject.RejectAction(this).toAction());
            }
        }
    };
    CallSession.prototype.startBatch = function () {
        var _this = this;
        this.batchStarted = true;
        if (!this.sendingBatch) {
            if (this.batchTimer) {
                clearTimeout(this.batchTimer);
            }
            this.batchTimer = setTimeout(function () {
                _this.batchTimer = null;
                _this.sendBatch();
            }, this.options.autoBatchDelay);
        }
    };
    CallSession.prototype.createPlayPromptAction = function (text, args) {
        args.unshift(text);
        var p = new prompt.Prompt(this);
        prompt.Prompt.prototype.value.apply(p, args);
        return new playPrompt.PlayPromptAction(this).prompts([p]).toAction();
    };
    CallSession.prototype.routeMessage = function () {
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
    CallSession.prototype.vgettext = function (messageid, args) {
        var tmpl;
        if (this.options.localizer && this.message) {
            tmpl = this.options.localizer.gettext(this.message.user.locale || '', messageid);
        }
        else {
            tmpl = messageid;
        }
        return args && args.length > 0 ? sprintf.vsprintf(tmpl, args) : tmpl;
    };
    CallSession.prototype.validateCallstack = function () {
        var ss = this.sessionState;
        for (var i = 0; i < ss.callstack.length; i++) {
            var id = ss.callstack[i].id;
            if (!this.findDialog(id)) {
                return false;
            }
        }
        return true;
    };
    CallSession.prototype.resolveDialogId = function (id) {
        if (id.indexOf(':') >= 0) {
            return id;
        }
        var cur = this.curDialog();
        var libName = cur ? cur.id.split(':')[0] : consts.Library.default;
        return libName + ':' + id;
    };
    CallSession.prototype.findDialog = function (id) {
        var parts = id.split(':');
        return this.library.findDialog(parts[0] || consts.Library.default, parts[1]);
    };
    CallSession.prototype.pushDialog = function (dialog) {
        var ss = this.sessionState;
        var cur = this.curDialog();
        if (cur) {
            cur.state = this.dialogData || {};
        }
        ss.callstack.push(dialog);
        this.dialogData = dialog.state || {};
        return dialog;
    };
    CallSession.prototype.popDialog = function () {
        var ss = this.sessionState;
        if (ss.callstack.length > 0) {
            ss.callstack.pop();
        }
        var cur = this.curDialog();
        this.dialogData = cur ? cur.state : null;
        return cur;
    };
    CallSession.prototype.curDialog = function () {
        var cur;
        var ss = this.sessionState;
        if (ss.callstack.length > 0) {
            cur = ss.callstack[ss.callstack.length - 1];
        }
        return cur;
    };
    return CallSession;
}(events.EventEmitter));
exports.CallSession = CallSession;
