/*-----------------------------------------------------------------------------
A bot for managing a users to-do list.  See the README.md file for usage 
instructions.
-----------------------------------------------------------------------------*/

var restify = require('restify');
var builder = require('../../');
var index = require('./dialogs/index')

// Create bot and add dialogs
var bot = new builder.BotConnectorBot({ 
    appId: process.env.appId, 
    appSecret: process.env.appSecret 
});
bot.add('/', index);

// Setup Restify Server
var server = restify.createServer();
server.post('/v1/messages', bot.verifyBotFramework(), bot.listen());
server.listen(8080, function () {
   console.log('%s listening to %s', server.name, server.url); 
});
