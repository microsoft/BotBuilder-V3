"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const ActionSet_1 = require("./ActionSet");
var ResumeReason;
(function (ResumeReason) {
    ResumeReason[ResumeReason["completed"] = 0] = "completed";
    ResumeReason[ResumeReason["notCompleted"] = 1] = "notCompleted";
    ResumeReason[ResumeReason["canceled"] = 2] = "canceled";
    ResumeReason[ResumeReason["back"] = 3] = "back";
    ResumeReason[ResumeReason["forward"] = 4] = "forward";
    ResumeReason[ResumeReason["reprompt"] = 5] = "reprompt";
})(ResumeReason = exports.ResumeReason || (exports.ResumeReason = {}));
class Dialog extends ActionSet_1.ActionSet {
    begin(session, args) {
        this.replyReceived(session);
    }
    dialogResumed(session, result) {
        if (result.error) {
            session.error(result.error);
        }
    }
    recognize(context, cb) {
        cb(null, { score: 0.1 });
    }
}
exports.Dialog = Dialog;
