(function (ResumeReason) {
    ResumeReason[ResumeReason["completed"] = 0] = "completed";
    ResumeReason[ResumeReason["notCompleted"] = 1] = "notCompleted";
    ResumeReason[ResumeReason["canceled"] = 2] = "canceled";
    ResumeReason[ResumeReason["back"] = 3] = "back";
    ResumeReason[ResumeReason["forward"] = 4] = "forward";
})(exports.ResumeReason || (exports.ResumeReason = {}));
var ResumeReason = exports.ResumeReason;
var Dialog = (function () {
    function Dialog() {
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
        cb(null, { score: 0.0 });
    };
    return Dialog;
})();
exports.Dialog = Dialog;
