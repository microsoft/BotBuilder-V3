/*-----------------------------------------------------------------------------
A bot for managing a teams to-do list. Created tasks are saved to the sessions
channelData object which means that private task lists can be created by sending
the bot a Direct Message and group task lists can be managed by @mentioning 
the bot in a channel.
 

# RUN THE BOT:

  Get a Bot token from Slack:

    http://my.slack.com/services/new/bot

  Run your bot from the command line:

    token=YOUR_TOKEN node app.js

  Run your bot from the command line (WINDOWS):
    
    set token=YOUR_TOKEN
    node app.js

# USE THE BOT:

  To manage your private tasks find the bot in slack and send it a Direct 
  Message saying "Hello". The bot will reply with instructions for how to
  manage tasks.
  
  To manage group tasks invite the bot to a channel by saying "/invite @bot"
  then say "@bot hello" for a list of instructions.

-----------------------------------------------------------------------------*/

var Botkit = require('botkit');
var builder = require('../../');
var index = require('./dialogs/index')

var controller = Botkit.slackbot();
var bot = controller.spawn({
   token: process.env.token || 'xoxb-27353876818-1JaNnDS8yhLSKuzmX9DncWAI'
});

var slackBot = new builder.SlackBot(controller, bot);
slackBot.add('/', index);

slackBot.listenForMentions();

bot.startRTM(function(err,bot,payload) {
  if (err) {
    throw new Error('Could not connect to Slack');
  }
});