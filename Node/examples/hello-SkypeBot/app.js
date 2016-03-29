/*-----------------------------------------------------------------------------
A simple "Hello World" bot for the Skype. SkypeBot is powered by the Skype SDK
for Node.js. A detailed walkthrough of creating and running this bot can be 
found at the link below.
 
    http://docs.botframework.com/builder/node/bots/SkypeBot

-----------------------------------------------------------------------------*/

const restify = require('restify');
const skype = require('skype-sdk');
const builder = require('../../');

// Initialize the BotService
const botService = new skype.BotService({
    messaging: {
        botId: "28:<bot’s id>",
        serverUrl : "https://apis.skype.com",
        requestTimeout : 15000,
        appId: process.env.APP_ID,
        appSecret: process.env.APP_SECRET
    }
});

// Create bot and add dialogs
var bot = new builder.SkypeBot(botService);
bot.add('/', function (session) {
   session.send('Hello World'); 
});

// Setup Restify Server
const server = restify.createServer();
server.post('/v1/chat', skype.messagingHandler(botService));
server.listen(process.env.PORT || 8080, function () {
   console.log('%s listening to %s', server.name, server.url); 
});
