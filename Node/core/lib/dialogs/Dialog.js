"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var actions = require('./ActionSet');
(function (ResumeReason) {
    ResumeReason[ResumeReason["completed"] = 0] = "completed";
    ResumeReason[ResumeReason["notCompleted"] = 1] = "notCompleted";
    ResumeReason[ResumeReason["canceled"] = 2] = "canceled";
    ResumeReason[ResumeReason["back"] = 3] = "back";
    ResumeReason[ResumeReason["forward"] = 4] = "forward";
})(exports.ResumeReason || (exports.ResumeReason = {}));
var ResumeReason = exports.ResumeReason;
var Dialog = (function (_super) {
    __extends(Dialog, _super);
    function Dialog() {
        _super.apply(this, arguments);
    }
    Dialog.prototype.begin = function (session, args) {
        this.replyReceived(session);
    };
    Dialog.prototype.dialogResumed = function (session, result) {
        if (result.error) {
            session.error(result.error);
        }
    };
    Dialog.prototype.recognize = function (context, cb) {
        cb(null, { score: 0.5 });
    };
    return Dialog;
}(actions.ActionSet));
exports.Dialog = Dialog;
