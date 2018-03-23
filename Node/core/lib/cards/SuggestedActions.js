"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class SuggestedActions {
    constructor(session) {
        this.session = session;
        this.data = {};
    }
    to(to) {
        this.data.to = [];
        if (to) {
            if (Array.isArray(to)) {
                for (let i = 0; i < to.length; i++) {
                    this.data.to.push(to[i]);
                }
            }
            else {
                this.data.to.push(to);
            }
        }
        return this;
    }
    actions(list) {
        this.data.actions = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                this.addAction(list[i]);
            }
        }
        return this;
    }
    addAction(action) {
        if (action) {
            var cardAction = action.toAction ? action.toAction() : action;
            if (!this.data.actions) {
                this.data.actions = [cardAction];
            }
            else {
                this.data.actions.push(cardAction);
            }
        }
        return this;
    }
    toSuggestedActions() {
        return this.data;
    }
    static create(session, actions, to) {
        return new SuggestedActions(session)
            .to(to)
            .actions(actions);
    }
}
exports.SuggestedActions = SuggestedActions;
