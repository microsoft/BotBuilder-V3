"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var dlg = require('../dialogs/Dialog');
var intent = require('../dialogs/IntentDialog');
var luis = require('../dialogs/LuisRecognizer');
var LuisDialog = (function (_super) {
    __extends(LuisDialog, _super);
    function LuisDialog(serviceUri) {
        _super.call(this);
        console.warn('LuisDialog class is deprecated. Use IntentDialog with a LuisRecognizer instead.');
        var recognizer = new luis.LuisRecognizer(serviceUri);
        this.dialog = new intent.IntentDialog({ recognizers: [recognizer] });
    }
    LuisDialog.prototype.begin = function (session, args) {
        this.dialog.begin(session, args);
    };
    LuisDialog.prototype.replyReceived = function (session, recognizeResult) {
        this.dialog.replyReceived(session, recognizeResult);
    };
    LuisDialog.prototype.dialogResumed = function (session, result) {
        this.dialog.dialogResumed(session, result);
    };
    LuisDialog.prototype.recognize = function (context, cb) {
        this.dialog.recognize(context, cb);
    };
    LuisDialog.prototype.onBegin = function (handler) {
        this.dialog.onBegin(handler);
        return this;
    };
    LuisDialog.prototype.on = function (intent, dialogId, dialogArgs) {
        this.dialog.matches(intent, dialogId, dialogArgs);
        return this;
    };
    LuisDialog.prototype.onDefault = function (dialogId, dialogArgs) {
        this.dialog.onDefault(dialogId, dialogArgs);
        return this;
    };
    return LuisDialog;
}(dlg.Dialog));
exports.LuisDialog = LuisDialog;
