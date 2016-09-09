"use strict";
var consts = require('../consts');
var utils = require('../utils');
var ActionSet = (function () {
    function ActionSet() {
        this.actions = {};
    }
    ActionSet.prototype.recognizeAction = function (message, cb) {
        var result = { score: 0.0 };
        if (message && message.text) {
            if (message.text.indexOf('action?') == 0) {
                var parts = message.text.split('?')[1].split('=');
                if (this.actions.hasOwnProperty(parts[0])) {
                    result.score = 1.0;
                    result.action = parts[0];
                    if (parts.length > 1) {
                        result.data = parts[1];
                    }
                }
            }
            else {
                for (var name in this.actions) {
                    var entry = this.actions[name];
                    if (message.text && entry.options.matches) {
                        var exp = entry.options.matches;
                        var matches = exp.exec(message.text);
                        if (matches && matches.length) {
                            var matched = matches[0];
                            var score = matched.length / message.text.length;
                            if (score > result.score && score >= (entry.options.intentThreshold || 0.1)) {
                                result.score = score;
                                result.action = name;
                                result.expression = exp;
                                result.matched = matches;
                                if (score == 1.0) {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        cb(null, result);
    };
    ActionSet.prototype.invokeAction = function (session, recognizeResult) {
        this.actions[recognizeResult.action].handler(session, recognizeResult);
    };
    ActionSet.prototype.cancelAction = function (name, msg, options) {
        return this.action(name, function (session, args) {
            if (args && typeof args.dialogIndex === 'number') {
                if (msg) {
                    session.send(msg);
                }
                session.cancelDialog(args.dialogIndex);
            }
        }, options);
    };
    ActionSet.prototype.reloadAction = function (name, msg, options) {
        if (options === void 0) { options = {}; }
        return this.action(name, function (session, args) {
            if (msg) {
                session.send(msg);
            }
            session.cancelDialog(args.dialogIndex, args.dialogId, options.dialogArgs);
        }, options);
    };
    ActionSet.prototype.beginDialogAction = function (name, id, options) {
        if (options === void 0) { options = {}; }
        return this.action(name, function (session, args) {
            if (options.dialogArgs) {
                utils.copyTo(options.dialogArgs, args);
            }
            if (id.indexOf(':') < 0) {
                var lib = args.dialogId ? args.dialogId.split(':')[0] : consts.Library.default;
                id = lib + ':' + id;
            }
            session.beginDialog(id, args);
        }, options);
    };
    ActionSet.prototype.endConversationAction = function (name, msg, options) {
        return this.action(name, function (session, args) {
            session.endConversation(msg);
        }, options);
    };
    ActionSet.prototype.action = function (name, handler, options) {
        if (options === void 0) { options = {}; }
        if (this.actions.hasOwnProperty(name)) {
            throw new Error("DialogAction[" + name + "] already exists.");
        }
        this.actions[name] = { handler: handler, options: options };
        return this;
    };
    return ActionSet;
}());
exports.ActionSet = ActionSet;
