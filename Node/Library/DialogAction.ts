import session = require('./Session');
import dialog = require('./Dialog');
import consts = require('./Consts');

export interface IDialogWaterfallStep {
    (session: ISession, result?: any): any;
}

export class DialogAction {
    static send(msg: string, ...args: any[]): IDialogHandler<any> {
        args.splice(0, 0, msg);
        return function sendAction(s: ISession) {
            // Send a message to the user.
            session.Session.prototype.send.apply(s, args);
        };
    }

    static beginDialog<T>(id: string, args?: T): IDialogHandler<T> {
        return function beginDialogAction(s: ISession, a: any) {
            // Ignore calls where we're being resumed.
            if (!a || !a.hasOwnProperty('resumed')) {
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
    }

    static endDialog(result?: any): IDialogHandler<any> {
        return function endDialogAction(s: ISession) {
            // End dialog
            s.endDialog(result);
        };
    }

    static waterfall(steps: IDialogWaterfallStep[]): IDialogHandler<any> {
        return function waterfallAction(s: ISession, r: dialog.IDialogResult) {
            try {
                // Check for continuation of waterfall
                var action: dialog.ResumeReason;
                if (r && r.hasOwnProperty('resumed')) {
                    // Adjust step
                    var step = s.dialogData[consts.Data.WaterfallStep];
                    switch (r.resumed) {
                        case dialog.ResumeReason.back:
                            step -= 1;
                            break;
                        case dialog.ResumeReason.formard:
                            step += 2;
                            break;
                        default:
                            step++;
                    }

                    // Handle result
                    if (step >= 0 && step < steps.length) {
                        s.dialogData[consts.Data.WaterfallStep] = step;
                        action = steps[step](s, r);
                    } else {
                        action = dialog.ResumeReason.canceled;
                    }
                } else if (steps && steps.length > 0) {
                    // Start waterfall
                    s.dialogData[consts.Data.WaterfallStep] = 0;
                    action = steps[0](s, r);
                } else {
                    action = dialog.ResumeReason.canceled;
                }

                // Handle step action
                if (action) {
                    var cancel = false;
                    var step = s.dialogData[consts.Data.WaterfallStep];
                    switch (action) {
                        case dialog.ResumeReason.canceled:
                            cancel = true;
                            break;
                        case dialog.ResumeReason.back:
                            if (step > 0) {
                                waterfallAction(s, { resumed: action });
                            } else {
                                cancel = true;
                            }
                            break;
                        case dialog.ResumeReason.formard:
                            if (step < steps.length - 1) {
                                waterfallAction(s, { resumed: action });
                            } else {
                                cancel = true;
                            }
                            break;
                    }
                    if (cancel) {
                        delete s.dialogData[consts.Data.WaterfallStep];
                        s.send();
                    }
                }
            } catch (e) {
                delete s.dialogData[consts.Data.WaterfallStep];
                s.endDialog({ resumed: dialog.ResumeReason.notCompleted, error: e instanceof Error ? e : new Error(e.toString()) });
            }
        }; 
    }
}