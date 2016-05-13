/*-----------------------------------------------------------------------------
A bot for testing various features of the framework.  See the README.md file 
for usage instructions.
-----------------------------------------------------------------------------*/

var restify = require('restify');
var builder = require('../../');
var index = require('./dialogs/index')

// Create bot and add dialogs
var bot = new builder.BotConnectorBot({ 
    appId: process.env.BOTFRAMEWORK_APPID, 
    appSecret: process.env.BOTFRAMEWORK_APPSECRET,
    groupWelcomeMessage: 'Group Welcome Message Works!',
    userWelcomeMessage: 'User Welcome Message Works!',
    goodbyeMessage: 'Goodbye Message Works!' 
});
index.addDialogs(bot, function (message, newConvo) {
    // Compose a return address that's the sender of the message
    if (newConvo) {
        return {
            to: message.from,
            from: message.to
        };
    } else {
        // - Normally you'd reverse the 'from' and 'to' fields and you wouldn't
        //   need the other fields but to call BotConnectorBot.beginDialog() and
        //   have it send a reply instead you need to leave the from & to the 
        //   same and return the additional conversationId related field.
        return {
            to: message.to,
            from: message.from,
            conversationId: message.conversationId,
            channelConversationId: message.channelConversationId,
            channelMessageId: message.channelMessageId
        };
    }
});


// Setup Restify Server
var server = restify.createServer();
server.post('/api/messages', bot.verifyBotFramework(), bot.listen());
server.listen(process.env.port || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});
