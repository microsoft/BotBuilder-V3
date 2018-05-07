"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var Message_1 = require("../Message");
var SigninCard_1 = require("./SigninCard");
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
    OAuthCard.create = function (connector, session, connectionName, text, buttonTitle, done) {
        var msg = new Message_1.Message(session);
        var asSignInCard = false;
        switch (session.message.address.channelId) {
            case 'msteams':
            case 'cortana':
            case 'skype':
            case 'skypeforbusiness':
                asSignInCard = true;
                break;
        }
        if (asSignInCard) {
            connector.getSignInLink(session.message.address, connectionName, function (getSignInLinkErr, link) {
                if (getSignInLinkErr) {
                    done(getSignInLinkErr, undefined);
                }
                else {
                    msg.attachments([
                        new SigninCard_1.SigninCard(session)
                            .text(text)
                            .button(buttonTitle, link)
                    ]);
                    done(undefined, msg);
                }
            });
        }
        else {
            msg.attachments([
                new OAuthCard(session)
                    .text(text)
                    .connectionName(connectionName)
                    .button(buttonTitle)
            ]);
            done(undefined, msg);
        }
    };
    return OAuthCard;
}());
exports.OAuthCard = OAuthCard;
