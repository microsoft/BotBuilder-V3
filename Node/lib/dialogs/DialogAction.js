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
            // Send a message to the user.
            session.Session.prototype.send.apply(s, args);
        };
    };
    DialogAction.beginDialog = function (id, args) {
        return function beginDialogAction(s, a) {
            // Handle calls where we're being resumed.
            if (a && a.hasOwnProperty('resumed')) {
                // We have to implement logic to ensure our callstack gets persisted.
                var r = a;
                if (r.error) {
                    s.error(r.error);
                }
                else {
                    s.send();
                }
            }
            else {
                // Merge args
                if (args) {
                    a = a || {};
                    for (var key in args) {
                        if (args.hasOwnProperty(key)) {
                            a[key] = args[key];
                        }
                    }
                }
                // Begin a new dialog
                s.beginDialog(id, a);
            }
        };
    };
    DialogAction.endDialog = function (result) {
        return function endDialogAction(s) {
            // End dialog
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
                // Check for continuation of waterfall
                if (r && r.hasOwnProperty('resumed')) {
                    // Adjust step based on users utterance
                    var step = s.dialogData[consts.Data.WaterfallStep];
                    switch (r.resumed) {
                        case dialog.ResumeReason.back:
                            step -= 1;
                            break;
                        default:
                            step++;
                    }
                    // Handle result
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
                    // Start waterfall
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
