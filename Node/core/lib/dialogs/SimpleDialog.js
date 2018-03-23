"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Dialog_1 = require("./Dialog");
class SimpleDialog extends Dialog_1.Dialog {
    constructor(fn) {
        super();
        this.fn = fn;
    }
    begin(session, args) {
        this.fn(session, args);
    }
    replyReceived(session) {
        this.fn(session);
    }
    dialogResumed(session, result) {
        this.fn(session, result);
    }
}
exports.SimpleDialog = SimpleDialog;
