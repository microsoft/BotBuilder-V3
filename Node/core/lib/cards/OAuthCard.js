"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var Message_1 = require("../Message");
var OAuthCard = (function () {
    function OAuthCard(session) {
        this.session = session;
        this.data = {
            contentType: 'application/vnd.microsoft.card.oauth',
            content: {}
        };
    }
    OAuthCard.prototype.connectionName = function (name) {
        if (name) {
            this.data.content.connectionName = name;
        }
        return this;
    };
    OAuthCard.prototype.text = function (prompts) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }
        if (prompts) {
            this.data.content.text = Message_1.fmtText(this.session, prompts, args);
        }
        return this;
    };
    OAuthCard.prototype.button = function (title) {
        if (title) {
            this.data.content.buttons = [{
                    type: 'signin',
                    title: Message_1.fmtText(this.session, title),
                    value: undefined
                }];
        }
        return this;
    };
    OAuthCard.prototype.toAttachment = function () {
        return this.data;
    };
    return OAuthCard;
}());
exports.OAuthCard = OAuthCard;
