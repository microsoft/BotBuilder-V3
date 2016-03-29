---
layout: page
title: SlackBot
permalink: /builder/node/bots/SlackBot/
weight: 624
parent1: Bot Builder for Node.js
parent2: Bots
---

* TOC
{:toc}

## Overview
The [SlackBot](/sdkreference/nodejs/classes/_botbuilder_d_.slackbot.html) class makes it easy to build a native bot for [slack](https://slack.com/) using [Botkit](http://howdy.ai/botkit/).  Bot Builder is a perfect companion for Botkit as it lets you do everything you can do in Botkit with the added power of Bot Builders Dialog System.

## Usage
The example below shows a Hello World bot built using [Botkit](http://howdy.ai/botkit/) and the [SlackBot](/sdkreference/nodejs/classes/_botbuilder_d_.slackbot.html) class. You simply create you Botkit controller and spawn your bot the way you normally would, then pass your controller & bot into the constructor of a SlackBot instance.  Next you add your dialogs and call [SlackBot.listenForMentions()](/sdkreference/nodejs/classes/_botbuilder_d_.slackbot.html#listenformentions) to have your bot listen for messages directed at the bot. Once you call startRtm() on the Botkit side of things the bot will login to slack and start processing messages. You'll need to provide an integration token for your bot which the example pulls from an environment variable.
 
{% highlight JavaScript %}
var Botkit = require('botkit');
var builder = require('botbuilder');

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
{% endhighlight %}

The [SlackBot.listenForMentions()](/sdkreference/nodejs/classes/_botbuilder_d_.slackbot.html#listenformentions) method registers for Direct Messages, Direct Mentions, and Mentions. When a bot is mentioned in a channel it will continue to listen for ambient messages from the user for a few minutes to prevent them from having to continue to use @mentions to interact with them. If you’d like a tighter scope on the Botkit events you listen for you can call [SlackBot.listen()](/sdkreference/nodejs/classes/_botbuilder_d_.slackbot.html#listen) instead with the exact list of events you’d like your bot to listen to.

## Testing
Slack bots are pretty easy to test locally by just running them locally, finding them in slack, and direct messaging them.  When your bot is published and running live, however, you might find it easier if you test locally using a [TextBot](/builder/node/bots/TextBot/) so that you can make changes to your bot in isolation from your published bot. Bot Builder provides several [examples](/builder/node/guides/examples/) showing how to run a bot on multiple platforms. 

Another option is just to use multiple slack integrations and have separate integrations for production and test. The example Hello World bot above is already setup to take the integration token from an environment variable.

## Publishing
Follow Botkits [Getting Started](http://howdy.ai/botkit/docs/#getting-started) guide for step-by-step instructions on publishing a bot to slack.