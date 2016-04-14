var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var dialog = require('./Dialog');
var actions = require('./DialogAction');
var consts = require('../consts');
var util = require('util');
var CommandDialog = (function (_super) {
    __extends(CommandDialog, _super);
    function CommandDialog() {
        _super.apply(this, arguments);
        this.commands = [];
    }
    CommandDialog.prototype.begin = function (session, args) {
        var _this = this;
        if (this.beginDialog) {
            session.dialogData[consts.Data.Handler] = -1;
            this.beginDialog(session, args, function () {
                _super.prototype.begin.call(_this, session, args);
            });
        }
        else {
            _super.prototype.begin.call(this, session, args);
        }
    };
    CommandDialog.prototype.replyReceived = function (session) {
        var score = 0.0;
        var expression;
        var matches;
        var text = session.message.text;
        var matched;
        for (var i = 0; i < this.commands.length; i++) {
            var cmd = this.commands[i];
            for (var j = 0; j < cmd.expressions.length; j++) {
                expression = cmd.expressions[j];
                if (expression.test(text)) {
                    matched = cmd;
                    session.dialogData[consts.Data.Handler] = i;
                    matches = expression.exec(text);
                    if (matches) {
                        var length = 0;
                        matches.forEach(function (value) {
                            if (value) {
                                length += value.length;
                            }
                        });
                        score = length / text.length;
                    }
                    break;
                }
            }
            if (matched)
                break;
        }
        if (!matched && this.default) {
            expression = null;
            matched = this.default;
            session.dialogData[consts.Data.Handler] = this.commands.length;
        }
        if (matched) {
            session.compareConfidence(session.message.language, text, score, function (handled) {
                if (!handled) {
                    matched.fn(session, { expression: expression, matches: matches });
                }
            });
        }
        else {
            session.send();
        }
    };
    CommandDialog.prototype.dialogResumed = function (session, result) {
        var cur;
        var handler = session.dialogData[consts.Data.Handler];
        if (handler >= 0 && handler < this.commands.length) {
            cur = this.commands[handler];
        }
        else if (handler > this.commands.length && this.default) {
            cur = this.default;
        }
        if (cur) {
            cur.fn(session, result);
        }
        else {
            _super.prototype.dialogResumed.call(this, session, result);
        }
    };
    CommandDialog.prototype.onBegin = function (handler) {
        this.beginDialog = handler;
        return this;
    };
    CommandDialog.prototype.matches = function (patterns, dialogId, dialogArgs) {
        var fn;
        var patterns = !util.isArray(patterns) ? [patterns] : patterns;
        if (Array.isArray(dialogId)) {
            fn = actions.DialogAction.waterfall(dialogId);
        }
        else if (typeof dialogId == 'string') {
            fn = actions.DialogAction.beginDialog(dialogId, dialogArgs);
        }
        else {
            fn = dialogId;
        }
        var expressions = [];
        for (var i = 0; i < patterns.length; i++) {
            expressions.push(new RegExp(patterns[i], 'i'));
        }
        this.commands.push({ expressions: expressions, fn: fn });
        return this;
    };
    CommandDialog.prototype.onDefault = function (dialogId, dialogArgs) {
        var fn;
        if (Array.isArray(dialogId)) {
            fn = actions.DialogAction.waterfall(dialogId);
        }
        else if (typeof dialogId == 'string') {
            fn = actions.DialogAction.beginDialog(dialogId, dialogArgs);
        }
        else {
            fn = dialogId;
        }
        this.default = { fn: fn };
        return this;
    };
    return CommandDialog;
})(dialog.Dialog);
exports.CommandDialog = CommandDialog;
