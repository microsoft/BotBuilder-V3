/*-----------------------------------------------------------------------------
A bot for testing various features of the framework.  See the README.md file 
for usage instructions.
-----------------------------------------------------------------------------*/

var restify = require('restify');
var builder = require('../../');
var index = require('./dialogs/index')

// Create bot and add dialogs
var bot = new builder.BotConnectorBot({ 
    appId: process.env.appId, 
    appSecret: process.env.appSecret 
});
index.addDialogs(bot, function (message) {
    return {
        to: message.from,
        from: message.to    
    };
});

// Setup Restify Server
var server = restify.createServer();
server.post('/api/messages', bot.verifyBotFramework(), bot.listen());
server.listen(process.env.port || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});
