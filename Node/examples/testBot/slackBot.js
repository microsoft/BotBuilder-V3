/*-----------------------------------------------------------------------------
A bot for testing various features of the framework.  See the README.md file 
for usage instructions.
-----------------------------------------------------------------------------*/

var Botkit = require('botkit');
var builder = require('../../');
var index = require('./dialogs/index')

var controller = Botkit.slackbot();
var bot = controller.spawn({
   token: process.env.token
});

var slackBot = new builder.SlackBot(controller, bot);
index.addDialogs(slackBot, function (message) {
    return {
        user: message.channelData.user,
        channel: message.channelData.channel    
    };
});

slackBot.listenForMentions();

bot.startRTM(function(err,bot,payload) {
  if (err) {
    throw new Error('Could not connect to Slack');
  }
});