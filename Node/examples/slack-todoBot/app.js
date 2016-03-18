/*-----------------------------------------------------------------------------
A simple "Hello World" bot for slack. SlackBot is powered by Botkit and you can
do pretty much anything you can do in Botkit.

More details about setting up Botkit can be found at:

    http://howdy.ai/botkit
 

# RUN THE BOT:

  Get a Bot token from Slack:

    http://my.slack.com/services/new/bot

  Run your bot from the command line:

    token=YOUR_TOKEN node app.js

  Run your bot from the command line (WINDOWS):
    
    set token=YOUR_TOKEN
    node app.js

# USE THE BOT:

  Find your bot inside Slack

  Say: "hello"

-----------------------------------------------------------------------------*/

var Botkit = require('botkit');
var builder = require('../../');
var index = require('./dialogs/index')

var controller = Botkit.slackbot();
var bot = controller.spawn({
   token: process.env.token
});

var slackBot = new builder.SlackBot(controller, bot);
slackBot.add('/', index);

slackBot.listen(['direct_message','direct_mention']);

bot.startRTM(function(err,bot,payload) {
  if (err) {
    throw new Error('Could not connect to Slack');
  }
});