"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const UniversalBot_1 = require("../bots/UniversalBot");
const ChatConnector_1 = require("../bots/ChatConnector");
class BotConnectorBot {
    constructor(options) {
        console.warn('BotConnectorBot class is deprecated. Use UniversalBot with a ChatConnector class.');
        var oConnector = {};
        var oBot = {};
        for (var key in options) {
            switch (key) {
                case 'appId':
                    oConnector.appId = options.appId;
                    break;
                case 'appSecret':
                    oConnector.appPassword = options.appSecret;
                    break;
                case 'defaultDialogId':
                    oBot.defaultDialogId = options.defaultDialogId;
                    break;
                case 'defaultDialogArgs':
                    oBot.defaultDialogArgs = options.defaultDialogArgs;
                    break;
                case 'groupWelcomeMessage':
                    this.groupWelcomeMessage = options.groupWelcomeMessage;
                    break;
                case 'userWelcomeMessage':
                    this.userWelcomeMessage = options.userWelcomeMessage;
                    break;
                case 'goodbyeMessage':
                    this.goodbyeMessage = options.goodbyeMessage;
                    break;
                case 'userStore':
                case 'conversationStore':
                case 'perUserInConversationStore':
                    console.error('BotConnectorBot custom stores no longer supported. Use UniversalBot with a custom IBotStorage implementation instead.');
                    throw new Error('BotConnectorBot custom stores no longer supported.');
            }
        }
        this.connector = new ChatConnector_1.ChatConnector(oConnector);
        this.bot = new UniversalBot_1.UniversalBot(this.connector, oBot);
    }
    on(event, listener) {
        this.bot.on(event, listener);
        return this;
    }
    add(id, dialog) {
        this.bot.dialog(id, dialog);
        return this;
    }
    configure(options) {
        console.error("BotConnectorBot.configure() is no longer supported. You should either pass all options into the constructor or update code to use the new UniversalBot class.");
        throw new Error("BotConnectorBot.configure() is no longer supported.");
    }
    verifyBotFramework(options) {
        if (options) {
            console.error("Calling BotConnectorBot.verifyBotFramework() with options is no longer supported. You should either pass all options into the constructor or update code to use the new UniversalBot class.");
            throw new Error("Calling BotConnectorBot.verifyBotFramework() with options is no longer supported.");
        }
        return (req, res, next) => next();
    }
    listen(dialogId, dialogArgs) {
        if (dialogId) {
            console.error("Calling BotConnectorBot.listen() with a custom dialogId is no longer supported. You should either pass as defaultDialogId into the constructor or update code to use the new UniversalBot class.");
            throw new Error("Calling BotConnectorBot.listen() with a custom dialogId is no longer supported.");
        }
        return this.connector.listen();
    }
    beginDialog(address, dialogId, dialogArgs) {
        console.error("BotConnectorBot.beginDialog() is no longer supported. The schema for sending proactive messages has changed and you should update your code to use the new UniversalBot class.");
        throw new Error("BotConnectorBot.beginDialog() is no longer supported.");
    }
}
exports.BotConnectorBot = BotConnectorBot;
