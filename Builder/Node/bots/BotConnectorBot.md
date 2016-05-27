---
layout: page
title: BotConnectorBot
permalink: /builder/node/bots/BotConnectorBot/
weight: 622
parent1: Bot Builder for Node.js
parent2: Bots
---

* TOC
{:toc}

## Overview
The [BotConnectorBot](/sdkreference/nodejs/classes/_botbuilder_d_.botconnectorbot.html) class makes it easy to build a bot that’s compatible with the Bot Frameworks [Bot Connector Service](/connector/getstarted/). Using the Bot Connector Service to host your bot lets you maximize your bots visibility across a wide range of communication channels. It also lets your bot take advantage of features provided by the Bot Connector Service like [Automatic Language Translation](/connector/bot-options/#translation). 

## Usage
The example below shows a Hello World bot built using [restify](http://restify.com/) web service framework. The example would be relatively the same if you were instead using the [Express](http://expressjs.com/) web application framework. Both frameworks are equally supported by Bot Builder.

{% highlight JavaScript %}
var restify = require('restify');
var builder = require('botbuilder');

// Create bot and add dialogs
var bot = new builder.BotConnectorBot({ appId: 'YourAppId', appSecret: 'YourAppSecret' });
bot.add('/', function (session) {
   session.send('Hello World'); 
});

// Setup Restify Server
var server = restify.createServer();
server.post('/api/messages', bot.verifyBotFramework(), bot.listen());
server.listen(process.env.port || 3978, function () {
    console.log('%s listening to %s', server.name, server.url); 
});
{% endhighlight %}

When you create your [BotConnectorBot](/sdkreference/nodejs/classes/_botbuilder_d_.botconnectorbot.html#constructor) you’ll want to pass in the [appId](/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotconnectoroptions.html#appid) and [appSecret]( /sdkreference/nodejs/interfaces/_botbuilder_d_.ibotconnectoroptions.html#appsecret) assigned to your bot when you [registered it](/connector/getstarted/#registering-your-bot-with-the-microsoft-bot-framework) with the Bot Connector Service. You won’t have this information when you’re first building your bot so it’s ok to leave them blank during your initial development and testing. These credentials are used for two purposes. They’re used by the [BotConnectorBot.verifyBotFramework()](/sdkreference/nodejs/classes/_botbuilder_d_.botconnectorbot.html#verifybotframework) middleware to ensure that your bot is only called by the Bot Connector Service, and they used when your bot initiates an outgoing conversation through the  [BotConnectorBot.beginDialog()](/sdkreference/nodejs/classes/_botbuilder_d_.botconnectorbot.html#begindialog) method.

## Testing
If you're using windows for your development environment you can test your bot locally using the [Bot Framework Emulator](/connector/tools/bot-framework-emulator/). For non-windows environments you should consider adding support for running your bot locally in a console window using [TextBot](/builder/node/bots/TextBot/). Bot Builder includes several [Examples](/builder/node/guides/examples/) of how to create a bot that supports multiple platforms.

## Publishing
To publish your bot first deploy it to the cloud and then register it with the Microsoft Bot Framework.

* [Publishing a bot to Microsoft Azure](/connector/getstarted/#publishing-your-bot-application-to-microsoft-azure)
* [Registering a bot with the Microsoft Bot Framework](/connector/getstarted/#registering-your-bot-with-the-microsoft-bot-framework)

## Connector Message Types
The Bot Connector Service actually sends a variety of [message types](/connector/message-types/) to your bot. Most of the messages your bot will receive will be utterances sent by the user which the framework will automatically route to the appropriate dialog. The Bot Connector Service can also send several control messages to your bot which the framework will also automatically handle for you but they’re worth you being aware of as in some cases you’ll want to customize the way they’re handled.

### Delete Data Event
The primary event you’ll likely want to handle yourself is the user requesting to have their data deleted.  Your bot is notified of this via a ‘DeleteUserData’ event which you can listen for using the [BotConnectorBot.on()](/sdkreference/nodejs/classes/_botbuilder_d_.botconnectorbot.html#on) method.  If you store users data exclusively using [Session.userData](/sdkreference/nodejs/classes/_botbuilder_d_.session.html#userdata) then there’s nothing for you to do, the framework will take care of deleting all of the users data for you. However, if you store user data using your own storage system it’s recommended that you use this event to delete that data.

{% highlight JavaScript %}
bot.on('DeleteUserData', function (message) {
    // ... delete users data
});
{% endhighlight %}

### Join & Leave Events
The Bot Connector Service will also send several events related to users joining & leaving conversations and groups. You can get notified of these events via the [BotConnectorBot.on()](/sdkreference/nodejs/classes/_botbuilder_d_.botconnectorbot.html#on) method but the BotConnectorBot will also let you [configure](/sdkreference/nodejs/interfaces/_botbuilder_d_.ibotconnectoroptions.html) several static messages which it will automatically reply with anytime one of these events is received.

{% highlight JavaScript %}
bot.configure({
    userWelcomeMessage: "Hello... Welcome to the group.",
    goodbyeMessage: "Goodbye..."
});
{% endhighlight %}

