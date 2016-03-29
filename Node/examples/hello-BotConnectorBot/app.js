/*-----------------------------------------------------------------------------
A simple "Hello World" bot for the Microsoft Bot Connector Service. A detailed 
walkthrough of creating and running this bot can be found at the link below.

    http://docs.botframework.com/builder/node/bots/BotConnectorBot

-----------------------------------------------------------------------------*/

var restify = require('restify');
var builder = require('../../');

// Create bot and add dialogs
var bot = new builder.BotConnectorBot({ appId: '<your appId>', appSecret: '<your appSecret>' });
bot.add('/', function (session) {
   session.send('Hello World'); 
});

// Setup Restify Server
var server = restify.createServer();
server.post('/v1/messages', bot.verifyBotFramework(), bot.listen());
server.listen(8080, function () {
   console.log('%s listening to %s', server.name, server.url); 
});