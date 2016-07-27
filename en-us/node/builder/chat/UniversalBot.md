---
layout: page
title: UniversalBot
permalink: /en-us/node/builder/chat/UniversalBot/
weight: 2620
parent1: Bot Builder for Node.js
parent2: Chat Bots
---

* TOC
{:toc}

## Overview
The [UniversalBot](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.universalbot.html) class forms the brains of your bot. It's responsible for managing all of the conversations your bot has with a user.  You first initialize your bot with a [connector](#connectors) that connects your bot to either the [Bot Framework](botframework.com) or the console.  Next you can configure your bot with [dialogs]( /en-us/node/builder/chat/dialogs/) that implement the actual conversation logic for your bot.

> __NOTE:__ For users of Bot Builder v1.x the [BotConnectorBot](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.botconnectorbot), [TextBot](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.textbot), and [SkypeBot](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.skypebot) class have been deprecated. They will continue to function in most cases but developers are encouraged to migrate to the new UniversalBot class at their earliest convenience.  The old [SlackBot](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.slackbot) class has been removed from the SDK and unfortunately, at this time the only option is for developers to migrate their native Slack bots to the [Bot Framework](botframework.com).

## Connectors
The UniversalBot class supports an extensible connector system the lets you configure the bot to receive messages & events and sources. Out of the box, Bot Builder includes a [ChatConnector](#chatconnector) class for connecting to the [Bot Framework](botframework.com) and a [ConsoleConnector](#consoleconnector) class for interacting with a bot from a console window. 

### ChatConnector
The [ChatConnector](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.chatconnector) class configures the UniversalBot to communicate with either the [emulator](/en-us/tools/bot-framework-emulator/) or any of the channels supported by the [Bot Framework](botframework.com). Below is an example of a "hello world" bot that's configured to use the ChatConnector:

{% highlight JavaScript %}
var restify = require('restify');
var builder = require('botbuilder');

//=========================================================
// Bot Setup
//=========================================================

// Setup Restify Server
var server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});
  
// Create chat bot
var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});
var bot = new builder.UniversalBot(connector);
server.post('/api/messages', connector.listen());

//=========================================================
// Bots Dialogs
//=========================================================

bot.dialog('/', function (session) {
    session.send("Hello World");
});
{% endhighlight %}

The [appId](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ichatconnectorsettings#appid) & [appPassword](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ichatconnectorsettings#apppassword) settings are generally required and will be generated when registering your bot in the [developer portal](botframework.com). The one exception to that rule is when running locally against the emulator. When you're first developing your bot you can leave the “Microsoft App Id” & “Microsoft App Password” blank in the emulator and no security will be enforced between the bot and emulator.  When deployed, however, these values are required for proper operation.

The example is coded to retrieve its appId & appPassword settings from environment variables. This sets up the bot to support storing these values in a config file when deployed to a hosting service like [Microsoft Azure](https://azure.microsoft.com). When testing your bots security settings locally you'll need to either manually set the environment variables in the console window you're using to run your bot or if you're using VSCode you can add them to the “env” section of your [launch.json](https://code.visualstudio.com/Docs/editor/debugging#_launch-configurations) file.

### ConsoleConnector
The [ConsoleConnector](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.consoleconnector) class configures the UniversalBot to interact with the user via the console window.  This connector is primarily useful for quick testing of a bot or for testing on a Mac where you can’t easily run the emulator.  Below is an example of a “hello world” bot that’s configured to use the ConsoleConnector:

{% highlight JavaScript %}
var builder = require('botbuilder');

var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector);
bot.dialog('/', function (session) {
   session.send('Hello World'); 
});
{% endhighlight %}

If you’re debugging your bot using VSCode you’ll want to start your bot using a command similar to node `--debug-brk app.js` and then you’ll want to start the debugger using [attach mode](https://code.visualstudio.com/docs/editor/debugging#_node-debugging).


