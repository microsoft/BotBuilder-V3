var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var dialog = require('./Dialog');
var SimpleDialog = (function (_super) {
    __extends(SimpleDialog, _super);
    function SimpleDialog(fn) {
        _super.call(this);
        this.fn = fn;
    }
    SimpleDialog.prototype.begin = function (session, args) {
        this.fn(session, args);
    };
    SimpleDialog.prototype.replyReceived = function (session) {
        var _this = this;
        session.compareConfidence(session.message.language, session.message.text, 0.0, function (handled) {
            if (!handled) {
                _this.fn(session);
            }
        });
    };
    SimpleDialog.prototype.dialogResumed = function (session, result) {
        this.fn(session, result);
    };
    return SimpleDialog;
})(dialog.Dialog);
exports.SimpleDialog = SimpleDialog;
