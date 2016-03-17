import dialog = require('./Dialog');

export class SimpleDialog extends dialog.Dialog {
    constructor(private fn: (session: ISession, arg?: any) => void) {
        super();
    }

    public begin<T>(session: ISession, args?: T): void {
        this.fn(session, args);
    }

    public replyReceived(session: ISession): void {
        session.compareConfidence(session.message.language, session.message.text, 0.0, (handled) => {
            if (!handled) {
                this.fn(session);
            }
        });
    }

    public dialogResumed(session: ISession, result: any): void {
        this.fn(session, result);
    }
}

