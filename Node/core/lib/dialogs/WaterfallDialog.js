"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Dialog_1 = require("./Dialog");
const consts = require("../consts");
class WaterfallDialog extends Dialog_1.Dialog {
    constructor(steps) {
        super();
        this._onBeforeStep = [];
        if (steps) {
            this.steps = Array.isArray(steps) ? steps : [steps];
        }
        else {
            this.steps = [];
        }
    }
    begin(session, args) {
        this.doStep(session, 0, args);
    }
    replyReceived(session, recognizeResult) {
        this.doStep(session, 0, recognizeResult.args);
    }
    dialogResumed(session, result) {
        let step = session.dialogData[consts.Data.WaterfallStep];
        switch (result.resumed) {
            case Dialog_1.ResumeReason.reprompt:
                return;
            case Dialog_1.ResumeReason.back:
                step--;
                break;
            default:
                step++;
                break;
        }
        this.doStep(session, step, result);
    }
    onBeforeStep(handler) {
        this._onBeforeStep.unshift(handler);
        return this;
    }
    doStep(session, step, args) {
        var skip = (result) => {
            result = result || {};
            if (result.resumed == null) {
                result.resumed = Dialog_1.ResumeReason.forward;
            }
            this.dialogResumed(session, result);
        };
        this.beforeStep(session, step, args, (s, a) => {
            if (s >= 0) {
                if (s < this.steps.length) {
                    try {
                        session.logger.log(session.dialogStack(), 'waterfall() step ' + (s + 1) + ' of ' + this.steps.length);
                        session.dialogData[consts.Data.WaterfallStep] = s;
                        this.steps[s](session, a, skip);
                    }
                    catch (e) {
                        session.error(e);
                    }
                }
                else if (a && a.hasOwnProperty('resumed')) {
                    session.endDialogWithResult(a);
                }
                else {
                    session.logger.warn(session.dialogStack(), 'waterfall() empty waterfall detected');
                    session.endDialogWithResult({ resumed: Dialog_1.ResumeReason.notCompleted });
                }
            }
        });
    }
    beforeStep(session, step, args, final) {
        let index = 0;
        let handlers = this._onBeforeStep;
        function next(s, a) {
            try {
                if (index < handlers.length) {
                    handlers[index++](session, s, a, next);
                }
                else {
                    final(s, a);
                }
            }
            catch (e) {
                session.error(e);
            }
        }
        next(step, args);
    }
    static createHandler(steps) {
        return function waterfallHandler(s, r) {
            var skip = (result) => {
                result = result || {};
                if (result.resumed == null) {
                    result.resumed = Dialog_1.ResumeReason.forward;
                }
                waterfallHandler(s, result);
            };
            if (r && r.hasOwnProperty('resumed')) {
                if (r.resumed !== Dialog_1.ResumeReason.reprompt) {
                    var step = s.dialogData[consts.Data.WaterfallStep];
                    switch (r.resumed) {
                        case Dialog_1.ResumeReason.back:
                            step -= 1;
                            break;
                        default:
                            step++;
                    }
                    if (step >= 0 && step < steps.length) {
                        try {
                            s.logger.log(s.dialogStack(), 'waterfall() step ' + step + 1 + ' of ' + steps.length);
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
            }
            else if (steps && steps.length > 0) {
                try {
                    s.logger.log(s.dialogStack(), 'waterfall() step 1 of ' + steps.length);
                    s.dialogData[consts.Data.WaterfallStep] = 0;
                    steps[0](s, r, skip);
                }
                catch (e) {
                    s.error(e);
                }
            }
            else {
                s.logger.warn(s.dialogStack(), 'waterfall() empty waterfall detected');
                s.endDialogWithResult({ resumed: Dialog_1.ResumeReason.notCompleted });
            }
        };
    }
}
exports.WaterfallDialog = WaterfallDialog;
