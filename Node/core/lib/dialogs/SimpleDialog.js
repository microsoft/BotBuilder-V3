"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var Dialog_1 = require("./Dialog");
var SimpleDialog = (function (_super) {
    __extends(SimpleDialog, _super);
    function SimpleDialog(fn) {
        var _this = _super.call(this) || this;
        _this.fn = fn;
        return _this;
    }
    SimpleDialog.prototype.begin = function (session, args) {
        this.fn(session, args);
    };
    SimpleDialog.prototype.replyReceived = function (session) {
        this.fn(session);
    };
    SimpleDialog.prototype.dialogResumed = function (session, result) {
        this.fn(session, result);
    };
    return SimpleDialog;
}(Dialog_1.Dialog));
exports.SimpleDialog = SimpleDialog;
