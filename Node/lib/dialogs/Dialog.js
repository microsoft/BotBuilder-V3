(function (ResumeReason) {
    ResumeReason[ResumeReason["completed"] = 0] = "completed";
    ResumeReason[ResumeReason["notCompleted"] = 1] = "notCompleted";
    ResumeReason[ResumeReason["canceled"] = 2] = "canceled";
    ResumeReason[ResumeReason["back"] = 3] = "back";
    ResumeReason[ResumeReason["forward"] = 4] = "forward";
    ResumeReason[ResumeReason["captureCompleted"] = 5] = "captureCompleted";
    ResumeReason[ResumeReason["childEnded"] = 6] = "childEnded";
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
        else {
            session.send();
        }
    };
    Dialog.prototype.compareConfidence = function (action, language, utterance, score) {
        action.next();
    };
    return Dialog;
})();
exports.Dialog = Dialog;
