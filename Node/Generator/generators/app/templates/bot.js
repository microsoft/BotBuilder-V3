"use strict";

const builder = require("botbuilder");
const dialog = require("./dialogs/<%= defaultDialog %>");
const bot = new builder.UniversalBot(
    new builder.ChatConnector({
        appId: process.env.MICROSOFT_APP_ID,
        appPassword: process.env.MICROSOFT_APP_PASSWORD
    }), 
    dialog.waterfall
);
<%= luisRegistration %>
module.exports = bot;
