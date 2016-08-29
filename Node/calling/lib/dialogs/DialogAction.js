"use strict";
var ses = require('../CallSession');
var consts = require('../consts');
var dlg = require('./Dialog');
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
            ses.CallSession.prototype.send.apply(s, args);
        };
    };
    DialogAction.beginDialog = function (id, args) {
        return function beginDialogAction(s, a) {
            if (a && a.hasOwnProperty('resumed')) {
                var r = a;
                if (r.error) {
                    s.error(r.error);
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
    return DialogAction;
}());
exports.DialogAction = DialogAction;
function waterfall(steps) {
    return function waterfallAction(s, r) {
        var skip = function (result) {
            result = result || {};
            if (!result.resumed) {
                result.resumed = dlg.ResumeReason.forward;
            }
            waterfallAction(s, result);
        };
        if (!r && s.dialogData.hasOwnProperty(consts.Data.WaterfallStep)) {
            r = { resumed: dlg.ResumeReason.completed };
        }
        if (r && r.hasOwnProperty('resumed')) {
            var step = s.dialogData[consts.Data.WaterfallStep];
            switch (r.resumed) {
                case dlg.ResumeReason.back:
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
                    s.error(e);
                }
            }
            else {
                s.endDialogWithResult(r);
            }
        }
        else if (steps && steps.length > 0) {
            try {
                s.dialogData[consts.Data.WaterfallStep] = 0;
                steps[0](s, r, skip);
            }
            catch (e) {
                s.error(e);
            }
        }
        else {
            s.endDialogWithResult({ resumed: dlg.ResumeReason.notCompleted });
        }
    };
}
exports.waterfall = waterfall;
