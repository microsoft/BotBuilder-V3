var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var dialog = require('./Dialog');
var actions = require('./DialogAction');
var consts = require('../consts');
var IntentDialog = (function (_super) {
    __extends(IntentDialog, _super);
    function IntentDialog() {
        _super.apply(this, arguments);
        this.groups = {};
        this.intentThreshold = 0.1;
    }
    IntentDialog.prototype.begin = function (session, args) {
        var _this = this;
        if (this.beginDialog) {
            this.beginDialog(session, args, function () {
                _super.prototype.begin.call(_this, session, args);
            });
        }
        else {
            _super.prototype.begin.call(this, session, args);
        }
    };
    IntentDialog.prototype.replyReceived = function (session) {
        var _this = this;
        var msg = session.message;
        this.recognizeIntents(msg.language, msg.text, function (err, intents, entities) {
            if (!err) {
                var topIntent = _this.findTopIntent(intents);
                var score = topIntent ? topIntent.score : 0;
                session.compareConfidence(msg.language, msg.text, score, function (handled) {
                    if (!handled) {
                        _this.invokeIntent(session, intents, entities);
                    }
                });
            }
            else {
                session.endDialog({ error: new Error('Intent recognition error: ' + err.message) });
            }
        });
    };
    IntentDialog.prototype.dialogResumed = function (session, result) {
        if (result.captured) {
            this.invokeIntent(session, result.captured.intents, result.captured.entities);
        }
        else {
            var activeGroup = session.dialogData[consts.Data.Group];
            var activeIntent = session.dialogData[consts.Data.Intent];
            var group = activeGroup ? this.groups[activeGroup] : null;
            var handler = group && activeIntent ? group._intentHandler(activeIntent) : null;
            if (handler) {
                handler(session, result);
            }
            else {
                _super.prototype.dialogResumed.call(this, session, result);
            }
        }
    };
    IntentDialog.prototype.compareConfidence = function (action, language, utterance, score) {
        var _this = this;
        if (score < IntentDialog.CAPTURE_THRESHOLD && this.captureIntent) {
            this.recognizeIntents(language, utterance, function (err, intents, entities) {
                var handled = false;
                if (!err) {
                    var matches;
                    var topIntent = _this.findTopIntent(intents);
                    if (topIntent && topIntent.score > _this.intentThreshold && topIntent.score > score) {
                        matches = _this.findHandler(topIntent);
                    }
                    if (matches) {
                        _this.captureIntent({
                            next: action.next,
                            userData: action.userData,
                            dialogData: action.dialogData,
                            endDialog: function () {
                                action.endDialog({
                                    resumed: dialog.ResumeReason.completed,
                                    captured: {
                                        intents: intents,
                                        entities: entities
                                    }
                                });
                            },
                            send: action.send
                        }, topIntent, entities);
                    }
                    else {
                        action.next();
                    }
                }
                else {
                    console.error('Intent recognition error: ' + err.message);
                    action.next();
                }
            });
        }
        else {
            action.next();
        }
    };
    IntentDialog.prototype.addGroup = function (group) {
        var id = group.getId();
        if (!this.groups.hasOwnProperty(id)) {
            this.groups[id] = group;
        }
        else {
            throw "Group of " + id + " already exists within the dialog.";
        }
        return this;
    };
    IntentDialog.prototype.onBegin = function (handler) {
        this.beginDialog = handler;
        return this;
    };
    IntentDialog.prototype.on = function (intent, dialogId, dialogArgs) {
        this.getDefaultGroup().on(intent, dialogId, dialogArgs);
        return this;
    };
    IntentDialog.prototype.onDefault = function (dialogId, dialogArgs) {
        this.getDefaultGroup().on(consts.Intents.Default, dialogId, dialogArgs);
        return this;
    };
    IntentDialog.prototype.getThreshold = function () {
        return this.intentThreshold;
    };
    IntentDialog.prototype.setThreshold = function (score) {
        this.intentThreshold = score;
        return this;
    };
    IntentDialog.prototype.invokeIntent = function (session, intents, entities) {
        try {
            var match;
            var topIntent = this.findTopIntent(intents);
            if (topIntent && topIntent.score > this.intentThreshold) {
                match = this.findHandler(topIntent);
            }
            if (!match) {
                topIntent = { intent: consts.Intents.Default, score: 1.0 };
                match = {
                    groupId: consts.Id.DefaultGroup,
                    handler: this.getDefaultGroup()._intentHandler(topIntent.intent)
                };
            }
            if (match) {
                session.dialogData[consts.Data.Group] = match.groupId;
                session.dialogData[consts.Data.Intent] = topIntent.intent;
                match.handler(session, { intents: intents, entities: entities });
            }
            else {
                session.send();
            }
        }
        catch (e) {
            session.endDialog({ error: new Error('Exception handling intent: ' + e.message) });
        }
    };
    IntentDialog.prototype.findTopIntent = function (intents) {
        var topIntent;
        if (intents) {
            for (var i = 0; i < intents.length; i++) {
                var intent = intents[i];
                if (!topIntent) {
                    topIntent = intent;
                }
                else if (intent.score > topIntent.score) {
                    topIntent = intent;
                }
            }
        }
        return topIntent;
    };
    IntentDialog.prototype.findHandler = function (intent) {
        for (var groupId in this.groups) {
            var handler = this.groups[groupId]._intentHandler(intent.intent);
            if (handler) {
                return { groupId: groupId, handler: handler };
            }
        }
        return null;
    };
    IntentDialog.prototype.getDefaultGroup = function () {
        var group = this.groups[consts.Id.DefaultGroup];
        if (!group) {
            this.groups[consts.Id.DefaultGroup] = group = new IntentGroup(consts.Id.DefaultGroup);
        }
        return group;
    };
    IntentDialog.CAPTURE_THRESHOLD = 0.6;
    return IntentDialog;
})(dialog.Dialog);
exports.IntentDialog = IntentDialog;
var IntentGroup = (function () {
    function IntentGroup(id) {
        this.id = id;
        this.handlers = {};
    }
    IntentGroup.prototype.getId = function () {
        return this.id;
    };
    IntentGroup.prototype._intentHandler = function (intent) {
        return this.handlers[intent];
    };
    IntentGroup.prototype.on = function (intent, dialogId, dialogArgs) {
        if (!this.handlers.hasOwnProperty(intent)) {
            if (Array.isArray(dialogId)) {
                this.handlers[intent] = actions.DialogAction.waterfall(dialogId);
            }
            else if (typeof dialogId == 'string') {
                this.handlers[intent] = actions.DialogAction.beginDialog(dialogId, dialogArgs);
            }
            else {
                this.handlers[intent] = dialogId;
            }
        }
        else {
            throw new Error('Intent[' + intent + '] already exists.');
        }
        return this;
    };
    return IntentGroup;
})();
exports.IntentGroup = IntentGroup;
