var Botkit = require('botkit');
var builder = require('../../');

var controller = Botkit.slackbot();
var bot = controller.spawn({
   token: 'xoxb-27353876818-1JaNnDS8yhLSKuzmX9DncWAI' 
});

var slackBot = new builder.SlackBot(controller, bot);
slackBot.add('/', function (session) {
   session.send('Hello World'); 
});

slackBot.listen(['direct_message','direct_mention']);

bot.startRTM(function(err,bot,payload) {
  if (err) {
    throw new Error('Could not connect to Slack');
  }
});