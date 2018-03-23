"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class Keyboard {
    constructor(session) {
        this.session = session;
        this.data = {
            contentType: 'application/vnd.microsoft.keyboard',
            content: {}
        };
    }
    buttons(list) {
        this.data.content.buttons = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                var action = list[i];
                this.data.content.buttons.push(action.toAction ? action.toAction() : action);
            }
        }
        return this;
    }
    toAttachment() {
        return this.data;
    }
}
exports.Keyboard = Keyboard;
