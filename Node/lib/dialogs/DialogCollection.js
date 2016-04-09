var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var actions = require('./DialogAction');
var simpleDialog = require('./SimpleDialog');
var events = require('events');
var prompts = require('./Prompts');
var consts = require('../consts');
var DialogCollection = (function (_super) {
    __extends(DialogCollection, _super);
    function DialogCollection() {
        _super.call(this);
        this.middleware = [];
        this.dialogs = {};
        this.add(consts.DialogId.Prompts, new prompts.Prompts());
    }
    DialogCollection.prototype.add = function (id, dialog) {
        var dialogs;
        if (typeof id == 'string') {
            if (Array.isArray(dialog)) {
                dialog = new simpleDialog.SimpleDialog(actions.DialogAction.waterfall(dialog));
            }
            else if (typeof dialog == 'function') {
                dialog = new simpleDialog.SimpleDialog(dialog);
            }
            dialogs = (_a = {}, _a[id] = dialog, _a);
        }
        else {
            dialogs = id;
        }
        for (var key in dialogs) {
            if (!this.dialogs.hasOwnProperty(key)) {
                this.dialogs[key] = dialogs[key];
            }
            else {
                throw new Error('Dialog[' + key + '] already exists.');
            }
        }
        return this;
        var _a;
    };
    DialogCollection.prototype.getDialog = function (id) {
        return this.dialogs[id];
    };
    DialogCollection.prototype.getMiddleware = function () {
        return this.middleware;
    };
    DialogCollection.prototype.hasDialog = function (id) {
        return this.dialogs.hasOwnProperty(id);
    };
    DialogCollection.prototype.use = function (fn) {
        this.middleware.push(fn);
        return this;
    };
    return DialogCollection;
})(events.EventEmitter);
exports.DialogCollection = DialogCollection;
