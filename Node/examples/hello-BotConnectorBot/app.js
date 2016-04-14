/*-----------------------------------------------------------------------------
A simple "Hello World" bot for the Microsoft Bot Connector Service. A detailed 
walkthrough of creating and running this bot can be found at the link below.

    http://docs.botframework.com/builder/node/bots/BotConnectorBot

-----------------------------------------------------------------------------*/

var restify = require('restify');
var builder = require('../../');

// Create bot and add dialogs
var bot = new builder.BotConnectorBot({ appId: 'YourAppId', appSecret: 'YourAppSecret' });
bot.add('/', function (session) {
   session.send('Hello World'); 
});

// Setup Restify Server
var server = restify.createServer();
server.post('/api/messages', bot.verifyBotFramework(), bot.listen());
server.listen(process.env.port || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});