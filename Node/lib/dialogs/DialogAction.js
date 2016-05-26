var ses = require('../Session');
var consts = require('../consts');
var utils = require('../utils');
var dialog = require('./Dialog');
var simple = require('./SimpleDialog');
var DialogAction = (function () {
    function DialogAction() {
    }
    DialogAction.send = function (msg) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        args.splice(0, 0, msg);
        return function sendAction(s) {
            ses.Session.prototype.send.apply(s, args);
        };
    };
    DialogAction.beginDialog = function (id, args) {
        return function beginDialogAction(s, a) {
            if (a && a.hasOwnProperty('resumed')) {
                var r = a;
                if (r.error) {
                    s.error(r.error);
                }
                else if (!s.messageSent()) {
                    s.send();
                }
            }
            else {
                if (args) {
                    a = a || {};
                    for (var key in args) {
                        if (args.hasOwnProperty(key)) {
                            a[key] = args[key];
                        }
                    }
                }
                s.beginDialog(id, a);
            }
        };
    };
    DialogAction.endDialog = function (result) {
        return function endDialogAction(s) {
            s.endDialog(result);
        };
    };
    DialogAction.validatedPrompt = function (promptType, validator) {
        return new simple.SimpleDialog(function (s, r) {
            r = r || {};
            var valid = false;
            if (r.response) {
                try {
                    valid = validator(r.response);
                }
                catch (e) {
                    s.endDialog({ resumed: dialog.ResumeReason.notCompleted, error: e instanceof Error ? e : new Error(e.toString()) });
                }
            }
            var canceled = false;
            switch (r.resumed) {
                case dialog.ResumeReason.canceled:
                case dialog.ResumeReason.forward:
                case dialog.ResumeReason.back:
                    canceled = true;
                    break;
            }
            if (valid || canceled) {
                s.endDialog(r);
            }
            else if (!s.dialogData.hasOwnProperty('prompt')) {
                s.dialogData = utils.clone(r);
                s.dialogData.promptType = promptType;
                if (!s.dialogData.hasOwnProperty('maxRetries')) {
                    s.dialogData.maxRetries = 2;
                }
                var a = utils.clone(s.dialogData);
                a.maxRetries = 0;
                s.beginDialog(consts.DialogId.Prompts, a);
            }
            else if (s.dialogData.maxRetries > 0) {
                s.dialogData.maxRetries--;
                var a = utils.clone(s.dialogData);
                a.maxRetries = 0;
                a.prompt = s.dialogData.retryPrompt || "I didn't understand. " + s.dialogData.prompt;
                s.beginDialog(consts.DialogId.Prompts, a);
            }
            else {
                s.endDialog({ resumed: dialog.ResumeReason.notCompleted });
            }
        });
    };
    return DialogAction;
})();
exports.DialogAction = DialogAction;
function waterfall(steps) {
    return function waterfallAction(s, r) {
        var skip = function (result) {
            result = result || {};
            if (!result.resumed) {
                result.resumed = dialog.ResumeReason.forward;
            }
            waterfallAction(s, result);
        };
        if (r && r.hasOwnProperty('resumed')) {
            var step = s.dialogData[consts.Data.WaterfallStep];
            switch (r.resumed) {
                case dialog.ResumeReason.back:
                    step -= 1;
                    break;
                default:
                    step++;
            }
            if (step >= 0 && step < steps.length) {
                try {
                    s.dialogData[consts.Data.WaterfallStep] = step;
                    steps[step](s, r, skip);
                }
                catch (e) {
                    delete s.dialogData[consts.Data.WaterfallStep];
                    s.endDialog({ resumed: dialog.ResumeReason.notCompleted, error: e instanceof Error ? e : new Error(e.toString()) });
                }
            }
            else {
                s.endDialog(r);
            }
        }
        else if (steps && steps.length > 0) {
            try {
                s.dialogData[consts.Data.WaterfallStep] = 0;
                steps[0](s, r, skip);
            }
            catch (e) {
                delete s.dialogData[consts.Data.WaterfallStep];
                s.endDialog({ resumed: dialog.ResumeReason.notCompleted, error: e instanceof Error ? e : new Error(e.toString()) });
            }
        }
        else {
            s.endDialog({ resumed: dialog.ResumeReason.notCompleted });
        }
    };
}
exports.waterfall = waterfall;
