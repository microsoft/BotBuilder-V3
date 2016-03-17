/*-----------------------------------------------------------------------------
This Bot demonstrates how to use a waterfall to prompt the user with a series
of questions.

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

var controller = Botkit.slackbot();
var bot = controller.spawn({
   token: process.env.token || 'xoxb-27353876818-1JaNnDS8yhLSKuzmX9DncWAI'
});

var slackBot = new builder.SlackBot(controller, bot);
slackBot.add('/', [
    function (session) {
        builder.Prompts.text(session, "Hello... What's your name?");
    },
    function (session, results) {
        session.userData.name = results.response;
        builder.Prompts.number(session, "Hi " + results.response + ", How many years have you been coding?"); 
    },
    function (session, results) {
        session.userData.coding = results.response;
        builder.Prompts.choice(session, "What language do you code Node using?", ["JavaScript", "CoffeeScript", "TypeScript"]);
    },
    function (session, results) {
        session.userData.language = results.response.entity;
        session.send("Got it... " + session.userData.name + 
                     " you've been programming for " + session.userData.coding + 
                     " years and use " + session.userData.language + ".");
    }
]);

slackBot.listen(['direct_message','direct_mention']);

bot.startRTM(function(err,bot,payload) {
  if (err) {
    throw new Error('Could not connect to Slack');
  }
});
