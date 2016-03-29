var session = require('../Session');
var dialog = require('./Dialog');
var consts = require('../consts');
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
            session.Session.prototype.send.apply(s, args);
        };
    };
    DialogAction.beginDialog = function (id, args) {
        return function beginDialogAction(s, a) {
            if (a && a.hasOwnProperty('resumed')) {
                var r = a;
                if (r.error) {
                    s.error(r.error);
                }
                else {
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
    DialogAction.waterfall = function (steps) {
        return function waterfallAction(s, r) {
            var skip = function (result) {
                result = result || {};
                if (!result.resumed) {
                    result.resumed = dialog.ResumeReason.forward;
                }
                waterfallAction(s, result);
            };
            try {
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
                        s.dialogData[consts.Data.WaterfallStep] = step;
                        steps[step](s, r, skip);
                    }
                    else {
                        delete s.dialogData[consts.Data.WaterfallStep];
                        s.send();
                    }
                }
                else if (steps && steps.length > 0) {
                    s.dialogData[consts.Data.WaterfallStep] = 0;
                    steps[0](s, r, skip);
                }
                else {
                    delete s.dialogData[consts.Data.WaterfallStep];
                    s.send();
                }
            }
            catch (e) {
                delete s.dialogData[consts.Data.WaterfallStep];
                s.endDialog({ resumed: dialog.ResumeReason.notCompleted, error: e instanceof Error ? e : new Error(e.toString()) });
            }
        };
    };
    return DialogAction;
})();
exports.DialogAction = DialogAction;
