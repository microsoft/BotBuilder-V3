"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Dialog_1 = require("../dialogs/Dialog");
const IntentDialog_1 = require("../dialogs/IntentDialog");
const LuisRecognizer_1 = require("../dialogs/LuisRecognizer");
class LuisDialog extends Dialog_1.Dialog {
    constructor(serviceUri) {
        super();
        console.warn('LuisDialog class is deprecated. Use IntentDialog with a LuisRecognizer instead.');
        var recognizer = new LuisRecognizer_1.LuisRecognizer(serviceUri);
        this.dialog = new IntentDialog_1.IntentDialog({ recognizers: [recognizer] });
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
    on(intent, dialogId, dialogArgs) {
        this.dialog.matches(intent, dialogId, dialogArgs);
        return this;
    }
    onDefault(dialogId, dialogArgs) {
        this.dialog.onDefault(dialogId, dialogArgs);
        return this;
    }
}
exports.LuisDialog = LuisDialog;
