"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Dialog_1 = require("./dialogs/Dialog");
const Message_1 = require("./Message");
const consts = require("./consts");
const sprintf = require("sprintf-js");
const events = require("events");
const async = require("async");
class Session extends events.EventEmitter {
    constructor(options) {
        super();
        this.options = options;
        this.msgSent = false;
        this._hasError = false;
        this._isReset = false;
        this.lastSendTime = new Date().getTime();
        this.batch = [];
        this.batchStarted = false;
        this.sendingBatch = false;
        this.inMiddleware = false;
        this._locale = null;
        this.localizer = null;
        this.logger = null;
        this.connector = options.connector;
        this.library = options.library;
        this.localizer = options.localizer;
        this.logger = options.logger;
        if (typeof this.options.autoBatchDelay !== 'number') {
            this.options.autoBatchDelay = 250;
        }
    }
    toRecognizeContext() {
        return {
            message: this.message,
            userData: this.userData,
            conversationData: this.conversationData,
            privateConversationData: this.privateConversationData,
            dialogData: this.dialogData,
            localizer: this.localizer,
            logger: this.logger,
            dialogStack: () => { return this.dialogStack(); },
            preferredLocale: () => { return this.preferredLocale(); },
            gettext: (...args) => { return Session.prototype.gettext.call(this, args); },
            ngettext: (...args) => { return Session.prototype.ngettext.call(this, args); },
            locale: this.preferredLocale()
        };
    }
    dispatch(sessionState, message, done) {
        var index = 0;
        var session = this;
        var now = new Date().getTime();
        var middleware = this.options.middleware || [];
        var next = () => {
            var handler = index < middleware.length ? middleware[index] : null;
            if (handler) {
                index++;
                handler(session, next);
            }
            else {
                this.inMiddleware = false;
                this.sessionState.lastAccess = now;
                done();
            }
        };
        this.sessionState = sessionState || { callstack: [], lastAccess: now, version: 0.0 };
        var cur = this.curDialog();
        if (cur) {
            this.dialogData = cur.state;
        }
        this.inMiddleware = true;
        this.message = (message || { text: '' });
        if (!this.message.type) {
            this.message.type = consts.messageType;
        }
        var locale = this.preferredLocale();
        this.localizer.load(locale, (err) => {
            if (err) {
                this.error(err);
            }
            else {
                next();
            }
        });
        return this;
    }
    error(err) {
        var m = err.toString();
        err = err instanceof Error ? err : new Error(m);
        this.emit('error', err);
        this.logger.error(this.dialogStack(), err);
        this._hasError = true;
        if (this.options.dialogErrorMessage) {
            this.endConversation(this.options.dialogErrorMessage);
        }
        else {
            var locale = this.preferredLocale();
            this.endConversation(this.localizer.gettext(locale, 'default_error', consts.Library.system));
        }
        return this;
    }
    preferredLocale(locale, callback) {
        if (locale) {
            this._locale = locale;
            if (this.userData) {
                this.userData[consts.Data.PreferredLocale] = locale;
            }
            if (this.localizer) {
                this.localizer.load(locale, callback);
            }
        }
        else if (!this._locale) {
            if (this.userData && this.userData[consts.Data.PreferredLocale]) {
                this._locale = this.userData[consts.Data.PreferredLocale];
            }
            else if (this.message && this.message.textLocale) {
                this._locale = this.message.textLocale;
            }
            else if (this.localizer) {
                this._locale = this.localizer.defaultLocale();
            }
        }
        return this._locale;
    }
    gettext(messageid, ...args) {
        return this.vgettext(this.curLibraryName(), messageid, args);
    }
    ngettext(messageid, messageid_plural, count) {
        var tmpl;
        if (this.localizer && this.message) {
            tmpl = this.localizer.ngettext(this.preferredLocale(), messageid, messageid_plural, count, this.curLibraryName());
        }
        else if (count == 1) {
            tmpl = messageid;
        }
        else {
            tmpl = messageid_plural;
        }
        return sprintf.sprintf(tmpl, count);
    }
    save() {
        this.logger.log(this.dialogStack(), 'Session.save()');
        this.startBatch();
        return this;
    }
    send(message, ...args) {
        args.unshift(this.curLibraryName(), message);
        return Session.prototype.sendLocalized.apply(this, args);
    }
    sendLocalized(libraryNamespace, message, ...args) {
        this.msgSent = true;
        if (message) {
            var m;
            if (typeof message == 'string' || Array.isArray(message)) {
                m = this.createMessage(libraryNamespace, message, args);
            }
            else if (message.toMessage) {
                m = message.toMessage();
            }
            else {
                m = message;
            }
            this.prepareMessage(m);
            this.batch.push(m);
            this.logger.log(this.dialogStack(), 'Session.send()');
        }
        this.startBatch();
        return this;
    }
    say(text, speak, options) {
        if (typeof speak === 'object') {
            options = speak;
            speak = null;
        }
        return this.sayLocalized(this.curLibraryName(), text, speak, options);
    }
    sayLocalized(libraryNamespace, text, speak, options) {
        this.msgSent = true;
        let msg = new Message_1.Message(this).text(text).speak(speak).toMessage();
        if (options) {
            ['attachments', 'attachmentLayout', 'entities', 'textFormat', 'inputHint'].forEach((field) => {
                if (options.hasOwnProperty(field)) {
                    msg[field] = options[field];
                }
            });
        }
        return this.sendLocalized(libraryNamespace, msg);
    }
    sendTyping() {
        this.msgSent = true;
        var m = { type: 'typing' };
        this.prepareMessage(m);
        this.batch.push(m);
        this.logger.log(this.dialogStack(), 'Session.sendTyping()');
        return this;
    }
    delay(delay) {
        this.msgSent = true;
        var m = { type: 'delay', value: delay };
        this.prepareMessage(m);
        this.batch.push(m);
        this.logger.log(this.dialogStack(), 'Session.delay(%d)', delay);
        return this;
    }
    messageSent() {
        return this.msgSent;
    }
    beginDialog(id, args) {
        this.logger.log(this.dialogStack(), 'Session.beginDialog(' + id + ')');
        var id = this.resolveDialogId(id);
        var dialog = this.findDialog(id);
        if (!dialog) {
            throw new Error('Dialog[' + id + '] not found.');
        }
        this.pushDialog({ id: id, state: {} });
        this.startBatch();
        dialog.begin(this, args);
        return this;
    }
    replaceDialog(id, args) {
        this.logger.log(this.dialogStack(), 'Session.replaceDialog(' + id + ')');
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
    }
    endConversation(message, ...args) {
        var m;
        if (message) {
            if (typeof message == 'string' || Array.isArray(message)) {
                m = this.createMessage(this.curLibraryName(), message, args);
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
        this.conversationData = {};
        this.privateConversationData = {};
        let code = this._hasError ? 'unknown' : 'completedSuccessfully';
        let mec = { type: 'endOfConversation', code: code };
        this.prepareMessage(mec);
        this.batch.push(mec);
        this.logger.log(this.dialogStack(), 'Session.endConversation()');
        var ss = this.sessionState;
        ss.callstack = [];
        this.sendBatch();
        return this;
    }
    endDialog(message, ...args) {
        if (typeof message === 'object' && (message.hasOwnProperty('response') || message.hasOwnProperty('resumed') || message.hasOwnProperty('error'))) {
            console.warn('Returning results via Session.endDialog() is deprecated. Use Session.endDialogWithResult() instead.');
            return this.endDialogWithResult(message);
        }
        var cur = this.curDialog();
        if (cur) {
            var m;
            if (message) {
                if (typeof message == 'string' || Array.isArray(message)) {
                    m = this.createMessage(this.curLibraryName(), message, args);
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
            this.logger.log(this.dialogStack(), 'Session.endDialog()');
            var childId = cur.id;
            cur = this.popDialog();
            this.startBatch();
            if (cur) {
                var dialog = this.findDialog(cur.id);
                if (dialog) {
                    dialog.dialogResumed(this, { resumed: Dialog_1.ResumeReason.completed, response: true, childId: childId });
                }
                else {
                    this.error(new Error("Can't resume missing parent dialog '" + cur.id + "'."));
                }
            }
        }
        return this;
    }
    endDialogWithResult(result) {
        var cur = this.curDialog();
        if (cur) {
            result = result || {};
            if (!result.hasOwnProperty('resumed')) {
                result.resumed = Dialog_1.ResumeReason.completed;
            }
            result.childId = cur.id;
            this.logger.log(this.dialogStack(), 'Session.endDialogWithResult()');
            cur = this.popDialog();
            this.startBatch();
            if (cur) {
                var dialog = this.findDialog(cur.id);
                if (dialog) {
                    dialog.dialogResumed(this, result);
                }
                else {
                    this.error(new Error("Can't resume missing parent dialog '" + cur.id + "'."));
                }
            }
        }
        return this;
    }
    cancelDialog(dialogId, replaceWithId, replaceWithArgs) {
        var childId = typeof dialogId === 'number' ? this.sessionState.callstack[dialogId].id : dialogId;
        var cur = this.deleteDialogs(dialogId);
        if (replaceWithId) {
            this.logger.log(this.dialogStack(), 'Session.cancelDialog(' + replaceWithId + ')');
            var id = this.resolveDialogId(replaceWithId);
            var dialog = this.findDialog(id);
            this.pushDialog({ id: id, state: {} });
            this.startBatch();
            dialog.begin(this, replaceWithArgs);
        }
        else {
            this.logger.log(this.dialogStack(), 'Session.cancelDialog()');
            this.startBatch();
            if (cur) {
                var dialog = this.findDialog(cur.id);
                if (dialog) {
                    dialog.dialogResumed(this, { resumed: Dialog_1.ResumeReason.canceled, response: null, childId: childId });
                }
                else {
                    this.error(new Error("Can't resume missing parent dialog '" + cur.id + "'."));
                }
            }
        }
        return this;
    }
    reset(dialogId, dialogArgs) {
        this.logger.log(this.dialogStack(), 'Session.reset()');
        this._isReset = true;
        this.sessionState.callstack = [];
        if (!dialogId) {
            dialogId = this.options.dialogId;
            dialogArgs = this.options.dialogArgs;
        }
        this.beginDialog(dialogId, dialogArgs);
        return this;
    }
    isReset() {
        return this._isReset;
    }
    sendBatch(done) {
        this.logger.log(this.dialogStack(), 'Session.sendBatch() sending ' + this.batch.length + ' message(s)');
        if (this.sendingBatch) {
            this.batchStarted = true;
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
        this.onSave((err) => {
            if (!err) {
                this.onSend(batch, (err, addresses) => {
                    this.onFinishBatch(() => {
                        if (this.batchStarted) {
                            this.startBatch();
                        }
                        if (done) {
                            done(err, addresses);
                        }
                    });
                });
            }
            else {
                this.onFinishBatch(() => {
                    if (done) {
                        done(err, null);
                    }
                });
            }
        });
    }
    dialogStack(newStack) {
        var stack;
        if (newStack) {
            stack = this.sessionState.callstack = newStack;
            this.dialogData = stack.length > 0 ? stack[stack.length - 1].state : null;
        }
        else {
            stack = this.sessionState.callstack || [];
            if (stack.length > 0) {
                stack[stack.length - 1].state = this.dialogData || {};
            }
        }
        return stack.slice(0);
    }
    clearDialogStack() {
        this.sessionState.callstack = [];
        this.dialogData = null;
        return this;
    }
    static forEachDialogStackEntry(stack, reverse, fn) {
        var step = reverse ? -1 : 1;
        var l = stack ? stack.length : 0;
        for (var i = step > 0 ? 0 : l - 1; i >= 0 && i < l; i += step) {
            fn(stack[i], i);
        }
    }
    static findDialogStackEntry(stack, dialogId, reverse = false) {
        var step = reverse ? -1 : 1;
        var l = stack ? stack.length : 0;
        for (var i = step > 0 ? 0 : l - 1; i >= 0 && i < l; i += step) {
            if (stack[i].id === dialogId) {
                return i;
            }
        }
        return -1;
    }
    static activeDialogStackEntry(stack) {
        return stack && stack.length > 0 ? stack[stack.length - 1] : null;
    }
    static pushDialogStackEntry(stack, entry) {
        if (!entry.state) {
            entry.state = {};
        }
        stack = stack || [];
        stack.push(entry);
        return entry;
    }
    static popDialogStackEntry(stack) {
        if (stack && stack.length > 0) {
            stack.pop();
        }
        return Session.activeDialogStackEntry(stack);
    }
    static pruneDialogStack(stack, start) {
        if (stack && stack.length > 0) {
            stack.splice(start);
        }
        return Session.activeDialogStackEntry(stack);
    }
    static validateDialogStack(stack, root) {
        let valid = true;
        Session.forEachDialogStackEntry(stack, false, (entry) => {
            var pair = entry.id.split(':');
            if (!root.findDialog(pair[0], pair[1])) {
                valid = false;
            }
        });
        return valid;
    }
    routeToActiveDialog(recognizeResult) {
        var dialogStack = this.dialogStack();
        if (Session.validateDialogStack(dialogStack, this.library)) {
            var active = Session.activeDialogStackEntry(dialogStack);
            if (active) {
                var dialog = this.findDialog(active.id);
                dialog.replyReceived(this, recognizeResult);
            }
            else {
                this.beginDialog(this.options.dialogId, this.options.dialogArgs);
            }
        }
        else {
            this.error(new Error('Invalid Dialog Stack.'));
        }
    }
    watch(variable, enable = true) {
        let name = variable.toLowerCase();
        if (!this.userData.hasOwnProperty(consts.Data.DebugWatches)) {
            this.userData[consts.Data.DebugWatches] = {};
        }
        if (watchableHandlers.hasOwnProperty(name)) {
            var entry = watchableHandlers[name];
            this.userData[consts.Data.DebugWatches][entry.name] = enable;
        }
        else {
            throw new Error("Invalid watch statement. '" + variable + "' isn't watchable");
        }
        return this;
    }
    watchList() {
        var watches = [];
        if (this.userData.hasOwnProperty(consts.Data.DebugWatches)) {
            for (let name in this.userData[consts.Data.DebugWatches]) {
                if (this.userData[consts.Data.DebugWatches][name]) {
                    watches.push(name);
                }
            }
        }
        return watches;
    }
    static watchable(variable, handler) {
        if (handler) {
            watchableHandlers[variable.toLowerCase()] = { name: variable, handler: handler };
        }
        else {
            let entry = watchableHandlers[variable.toLowerCase()];
            if (entry) {
                handler = entry.handler;
            }
        }
        return handler;
    }
    static watchableList() {
        let variables = [];
        for (let name in watchableHandlers) {
            if (watchableHandlers.hasOwnProperty(name)) {
                variables.push(watchableHandlers[name].name);
            }
        }
        return variables;
    }
    onSave(cb) {
        this.options.onSave((err) => {
            if (err) {
                this.logger.error(this.dialogStack(), err);
                switch (err.code || '') {
                    case consts.Errors.EBADMSG:
                    case consts.Errors.EMSGSIZE:
                        this.userData = {};
                        this.batch = [];
                        this.endConversation(this.options.dialogErrorMessage || 'Oops. Something went wrong and we need to start over.');
                        break;
                }
            }
            cb(err);
        });
    }
    onSend(batch, cb) {
        if (batch && batch.length > 0) {
            this.options.onSend(batch, (err, responses) => {
                if (err) {
                    this.logger.error(this.dialogStack(), err);
                }
                cb(err, responses);
            });
        }
        else {
            cb(null, null);
        }
    }
    onFinishBatch(cb) {
        var ctx = this.toRecognizeContext();
        async.each(this.watchList(), (variable, cb) => {
            let entry = watchableHandlers[variable.toLowerCase()];
            if (entry && entry.handler) {
                try {
                    entry.handler(ctx, (err, value) => {
                        if (!err) {
                            this.logger.dump(variable, value);
                        }
                        cb(err);
                    });
                }
                catch (e) {
                    cb(e);
                }
            }
            else {
                cb(new Error("'" + variable + "' isn't watchable."));
            }
        }, (err) => {
            if (err) {
                this.logger.error(this.dialogStack(), err);
            }
            this.logger.flush((err) => {
                this.sendingBatch = false;
                if (err) {
                    console.error(err);
                }
                cb();
            });
        });
    }
    startBatch() {
        this.batchStarted = true;
        if (!this.sendingBatch) {
            if (this.batchTimer) {
                clearTimeout(this.batchTimer);
            }
            this.batchTimer = setTimeout(() => {
                this.sendBatch();
            }, this.options.autoBatchDelay);
        }
    }
    createMessage(localizationNamespace, text, args) {
        var message = new Message_1.Message(this)
            .text(this.vgettext(localizationNamespace, Message_1.Message.randomPrompt(text), args));
        return message.toMessage();
    }
    prepareMessage(msg) {
        if (!msg.type) {
            msg.type = 'message';
        }
        if (!msg.address) {
            msg.address = this.message.address;
        }
        if (!msg.textLocale && this.message.textLocale) {
            msg.textLocale = this.message.textLocale;
        }
    }
    vgettext(localizationNamespace, messageid, args) {
        var tmpl;
        if (this.localizer && this.message) {
            tmpl = this.localizer.gettext(this.preferredLocale(), messageid, localizationNamespace);
        }
        else {
            tmpl = messageid;
        }
        return args && args.length > 0 ? sprintf.vsprintf(tmpl, args) : tmpl;
    }
    validateCallstack() {
        var ss = this.sessionState;
        for (var i = 0; i < ss.callstack.length; i++) {
            var id = ss.callstack[i].id;
            if (!this.findDialog(id)) {
                return false;
            }
        }
        return true;
    }
    resolveDialogId(id) {
        return id.indexOf(':') >= 0 ? id : this.curLibraryName() + ':' + id;
    }
    curLibraryName() {
        var cur = this.curDialog();
        return cur && !this.inMiddleware ? cur.id.split(':')[0] : this.library.name;
    }
    findDialog(id) {
        var parts = id.split(':');
        return this.library.findDialog(parts[0] || this.library.name, parts[1]);
    }
    pushDialog(ds) {
        var ss = this.sessionState;
        var cur = this.curDialog();
        if (cur) {
            cur.state = this.dialogData || {};
        }
        ss.callstack.push(ds);
        this.dialogData = ds.state || {};
        return ds;
    }
    popDialog() {
        var ss = this.sessionState;
        if (ss.callstack.length > 0) {
            ss.callstack.pop();
        }
        var cur = this.curDialog();
        this.dialogData = cur ? cur.state : null;
        return cur;
    }
    deleteDialogs(dialogId) {
        var ss = this.sessionState;
        var index = -1;
        if (typeof dialogId === 'string') {
            for (var i = ss.callstack.length - 1; i >= 0; i--) {
                if (ss.callstack[i].id == dialogId) {
                    index = i;
                    break;
                }
            }
        }
        else {
            index = dialogId;
        }
        if (index < 0 && index < ss.callstack.length) {
            throw new Error('Unable to cancel dialog. Dialog[' + dialogId + '] not found.');
        }
        ss.callstack.splice(index);
        var cur = this.curDialog();
        this.dialogData = cur ? cur.state : null;
        return cur;
    }
    curDialog() {
        var cur;
        var ss = this.sessionState;
        if (ss.callstack.length > 0) {
            cur = ss.callstack[ss.callstack.length - 1];
        }
        return cur;
    }
    getMessageReceived() {
        console.warn("Session.getMessageReceived() is deprecated. Use Session.message.sourceEvent instead.");
        return this.message.sourceEvent;
    }
}
exports.Session = Session;
let watchableHandlers = {
    'userdata': { name: 'userData', handler: (ctx, cb) => cb(null, ctx.userData) },
    'conversationdata': { name: 'conversationData', handler: (ctx, cb) => cb(null, ctx.conversationData) },
    'privateconversationdata': { name: 'privateConversationData', handler: (ctx, cb) => cb(null, ctx.privateConversationData) },
    'dialogdata': { name: 'dialogData', handler: (ctx, cb) => cb(null, ctx.dialogData) },
    'dialogstack': { name: 'dialogStack', handler: (ctx, cb) => cb(null, ctx.dialogStack()) },
    'preferredlocale': { name: 'preferredLocale', handler: (ctx, cb) => cb(null, ctx.preferredLocale()) },
    'libraryname': { name: 'libraryName', handler: (ctx, cb) => cb(null, ctx.libraryName) }
};
