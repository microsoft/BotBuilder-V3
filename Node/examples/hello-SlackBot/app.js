/*-----------------------------------------------------------------------------
A simple "Hello World" bot for slack. SlackBot is powered by Botkit and you can
do pretty much anything you can do in Botkit.

More details about setting up Botkit can be found at:

    http://howdy.ai/botkit
 
A detailed walkthrough of creating and running this bot can be found at the 
link below.

    http://docs.botframework.com/builder/node/bots/SlackBot

-----------------------------------------------------------------------------*/

var Botkit = require('botkit');
var builder = require('../../');

var controller = Botkit.slackbot();
var bot = controller.spawn({
   token: process.env.token
});

var slackBot = new builder.SlackBot(controller, bot);
slackBot.add('/', function (session) {
   session.send('Hello World'); 
});

slackBot.listenForMentions();

bot.startRTM(function(err,bot,payload) {
  if (err) {
    throw new Error('Could not connect to Slack');
  }
});