import session = require('../Session');
import dialog = require('./Dialog');
import consts = require('../consts');

export interface IDialogWaterfallStep {
    (session: ISession, result?: any, skip?: IDialogWaterfallCursor): void;
}

export interface IDialogWaterfallCursor {
    (count?: number, results?: dialog.IDialogResult<any>): void;
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
                            a[key] = (<any>args)[key];
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
        return function waterfallAction(s: ISession, r: dialog.IDialogResult<any>) {
            var skip = (count = 1, result?: dialog.IDialogResult<any>) => {
                result = result || { resumed: dialog.ResumeReason.forward };
                s.dialogData[consts.Data.WaterfallStep] += count;
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
                        case dialog.ResumeReason.forward:
                            step += 2;
                            break;
                        default:
                            step++;
                    }

                    // Handle result
                    if (step >= 0 && step < steps.length) {
                        s.dialogData[consts.Data.WaterfallStep] = step;
                        steps[step](s, r, skip);
                    } else {
                        delete s.dialogData[consts.Data.WaterfallStep];
                        s.send();
                    }
                } else if (steps && steps.length > 0) {
                    // Start waterfall
                    s.dialogData[consts.Data.WaterfallStep] = 0;
                    steps[0](s, r, skip);
                } else {
                    delete s.dialogData[consts.Data.WaterfallStep];
                    s.send();
                }
            } catch (e) {
                delete s.dialogData[consts.Data.WaterfallStep];
                s.endDialog({ resumed: dialog.ResumeReason.notCompleted, error: e instanceof Error ? e : new Error(e.toString()) });
            }
        }; 
    }
}