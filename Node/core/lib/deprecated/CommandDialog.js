"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Dialog_1 = require("../dialogs/Dialog");
const IntentDialog_1 = require("../dialogs/IntentDialog");
class CommandDialog extends Dialog_1.Dialog {
    constructor(serviceUri) {
        super();
        console.warn('CommandDialog class is deprecated. Use IntentDialog class instead.');
        this.dialog = new IntentDialog_1.IntentDialog();
    }
    begin(session, args) {
        this.dialog.begin(session, args);
    }
    replyReceived(session, recognizeResult) {
    }
    dialogResumed(session, result) {
        this.dialog.dialogResumed(session, result);
    }
    recognize(context, cb) {
        this.dialog.recognize(context, cb);
    }
    onBegin(handler) {
        this.dialog.onBegin(handler);
        return this;
    }
    matches(patterns, dialogId, dialogArgs) {
        var list = (!Array.isArray(patterns) ? [patterns] : patterns);
        list.forEach((p) => {
            this.dialog.matches(new RegExp(p, 'i'), dialogId, dialogArgs);
        });
        return this;
    }
    onDefault(dialogId, dialogArgs) {
        this.dialog.onDefault(dialogId, dialogArgs);
        return this;
    }
}
exports.CommandDialog = CommandDialog;
