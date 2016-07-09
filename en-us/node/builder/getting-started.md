---
layout: page
title: Getting Started
permalink: /en-us/node/builder/overview/
weight: 2600
parent1: Bot Builder for Node.js
---


## What is Bot Builder for Node.js and why should I use it?
Bot Builder for Node.js is a powerful framework for constructing bots that can handle both freeform interactions and more guided ones where the possibilities are explicitly shown to the user. It is easy to use and models frameworks like Express & Restify to provide developers with a familiar way to write Bots.

High Level Features:

* Powerful dialog system with dialogs that are isolated and composable.
* Built-in prompts for simple things like Yes/No, strings, numbers, enumerations.
* Built-in dialogs that utilize powerful AI frameworks like [LUIS](http://luis.ai).
* Bots are stateless which helps them scale.
* Bots can run on almost any bot platform like the [Microsoft Bot Framework](http://botframework.com), [Skype](http://skype.com), and [Slack](http://slack.com).

## Build a bot
Create a folder for your bot, cd into it, and run npm init.

    npm init
    
Get the Bot Builder and Restify modules using npm.

    npm install --save botbuilder
    npm install --save restify

       
Make a file named app.js and say hello in a few lines of code.
 
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

## Test your bot (Windows Only)
Use the [Bot Framework Emulator](/en-us/tools/bot-framework-emulator/) to test your bot on localhost. 

Install the emulator from [here](https://aka.ms/bf-bc-emulator) and then start your bot in a console window.

    node app.js
    
Start the emulator and say "hello" to your bot.

## Publish your bot
Deploy your bot to the cloud and then [register it](/en-us/csharp/builder/sdkreference/gettingstarted.html#registering) with the Microsoft Bot Framework. If you're deploying your bot to Microsoft Azure you can use this great guide for [Publishing a Node.js app to Azure using Continuous Integration](https://blogs.msdn.microsoft.com/sarahsays/2015/08/31/building-your-first-node-js-app-and-publishing-to-azure/).

NOTE: When you register your bot with the Bot Framework you'll want to update the appId & appSecret for both your bot and the emulator with the values assigned to you by the portal.

## Dive deeper
Learn how to build great bots.

* [Core Concepts Guide](/en-us/node/builder/guides/core-concepts/)
* [Chat SDK Reference](/en-us/node/builder/chat-reference/modules/_botbuilder_d_.html)
* [Calling SDK Reference](/en-us/node/builder/calling-reference/modules/_botbuilder_d_.html)
* [Bot Builder on GitHub](https://github.com/Microsoft/BotBuilder)
