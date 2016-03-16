import dialog = require('./Dialog');
import actions = require('./DialogAction');
import simpleDialog = require('./SimpleDialog');
import events = require('events');

interface IDialogMap {
    [id: string]: dialog.IDialog;
}

export class DialogCollection extends events.EventEmitter {
    private middleware: { (session: ISession, next: Function): void; }[] = []; 
    private dialogs: IDialogMap = {};

    constructor() {
        super();
    }

    public add(dialogs: { [id: string]: dialog.IDialog; }): DialogCollection;
    public add(id: string, fn: IDialogHandler<any>): DialogCollection;
    public add(id: string, waterfall: actions.IDialogWaterfallStep[]): DialogCollection;
    public add(id: string, dialog: dialog.IDialog): DialogCollection;
    public add(id: any, dialog?: any): DialogCollection {
        // Fixup params
        var dialogs: { [id: string]: dialog.IDialog; };
        if (typeof id == 'string') {
            if (Array.isArray(dialog)) {
                dialog = new simpleDialog.SimpleDialog(actions.DialogAction.waterfall(dialog));
            } else if (typeof dialog == 'function') {
                dialog = new simpleDialog.SimpleDialog(dialog);
            }
            dialogs = { [id]: dialog };
        } else {
            dialogs = id;
        }

        // Add dialogs
        for (var key in dialogs) {
            if (!this.dialogs.hasOwnProperty(key)) {
                this.dialogs[key] = dialogs[key];
            } else {
                throw new Error('Dialog[' + key + '] already exists.');
            }
        }
        return this;
    }

    public getDialog(id: string): dialog.IDialog {
        return this.dialogs[id];
    }

    public getMiddleware(): { (session: ISession, next: Function): void; }[] {
        return this.middleware;
    }

    public hasDialog(id: string): boolean {
        return this.dialogs.hasOwnProperty(id);
    }

    public use(fn: (session: ISession, next: Function) => void): DialogCollection {
        this.middleware.push(fn);
        return this;
    }
}