
export interface IDialog {
    begin<T>(session: ISession, args?: T): void;
    replyReceived(session: ISession): void;
    dialogResumed(session: ISession, result: any): void;
    compareConfidence(action: ISessionAction, language: string, utterance: string, score: number): void;
}

export enum ResumeReason { completed, notCompleted, canceled, back, forward, captureCompleted, childEnded }

export interface IDialogResult<T> {
    resumed: ResumeReason;
    childId?: string;
    error?: Error;
    response?: T;
}

export abstract class Dialog implements IDialog {
    public begin<T>(session: ISession, args?: T): void {
        this.replyReceived(session);
    }

    abstract replyReceived(session: ISession): void;

    public dialogResumed<T>(session: ISession, result: IDialogResult<T>): void {
        if (result.error) {
            session.error(result.error);
        } else {
            session.send();
        }
    }

    public compareConfidence(action: ISessionAction, language: string, utterance: string, score: number): void {
        action.next();
    }
}
