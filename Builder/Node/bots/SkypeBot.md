---
layout: page
title: SkypeBot
permalink: /builder/node/bots/SkypeBot/
weight: 623
parent1: Bot Builder for Node.js
parent2: Bots
---

* TOC
{:toc}

## Overview
The [SkypeBot](/sdkreference/nodejs/classes/_botbuilder_d_.skypebot.html) class makes it easy to build a native bot for [Skype](http://www.skype.com) using the Skype Bot SDK for Node.js.  Bot Builder is a perfect companion for the Skype Bot SDK as it lets you do everything you can do in Skype with the added power of Bot Builders Dialog System.

## Usage
Before you get started, make sure you register your bot. Also have ready your:

* Bot’s ID
* Application ID If you've forgotten it, you can find your Application's ID in Details in My Bots.
* Your Application Secret If you've misplaced your Application Secret, you'll have to generate a new one at Microsoft Applications. 

NOTE: the Skype Bot Kit SDK requires node version 5.6 or greater so add the following to your bots package.json file. 

{% highlight JavaScript %}
{
    "engines": {
        "node": ">=5.6.0"
    }
}
{% endhighlight %}

### Create server.js
The example below shows a Hello World bot built using [restify](http://restify.com/) web service framework. The example would be relatively the same if you were instead using the [Express](http://expressjs.com/) web application framework. Both frameworks are equally supported by Bot Builder and the Skype Bot SDK.

{% highlight JavaScript %}
const restify = require('restify');
const skype = require('skype-sdk');
const builder = require('botbuilder');

// Initialize the BotService
const botService = new skype.BotService({
    messaging: {
        botId: "28:<bot’s id>",
        serverUrl : "https://apis.skype.com",
        requestTimeout : 15000,
        appId: process.env.APP_ID,
        appSecret: process.env.APP_SECRET
    }
});

// Create bot and add dialogs
var bot = new builder.SkypeBot(botService);
bot.add('/', function (session) {
   session.send('Hello World'); 
});

// Setup Restify Server
const server = restify.createServer();
server.post('/v1/chat', skype.messagingHandler(botService));
server.listen(process.env.PORT || 8000, function () {
   console.log('%s listening to %s', server.name, server.url); 
});
{% endhighlight %}

### Start the bot

    node server.js

Make sure you created environment variables:

* APP_ID With your application ID.
* APP_SECRET With your application secret for OAuth.

If you don't, your bot won't authenticate and won't be able to call into Skype services.

You can also create an environment variable PORT to change the default port from 8000. 

### Testing with ngrok
There are tools that can create a public url to your local webserver on your machine, e.g. [ngrok](https://ngrok.com/). We’ll show how you can test your bot running locally over skype.

You’ll need to download ngrok and modify your bot’s registration.

First step is to start ngrok on your machine and map it to a local http port:

    ngrok http 8000

This will create a new tunnel from a public url to http://localhost:8000 on your machine. After you start the command, you can see the status of the tunnel:

{% highlight JavaScript %}
ngrok by @inconshreveable                                           (Ctrl+C to quit)
Tunnel Status       online
Update              update available (version 2.0.24, Ctrl-U to update)
Version             2.0.19/2.0.25
Web Interface       http://127.0.0.1:4040
Forwarding          http://78191649.ngrok.io -> localhost:8000
Forwarding          https://78191649.ngrok.io -> localhost:8000

Connections     ttl     opn     rt1     rt5     p50     p90
                0       0       0.00    0.00    0.00    0.00
{% endhighlight %}

Notice the “Forwarding” lines, in this case you can see that ngrok created two endpoints for us http://78191649.ngrok.io and https://78191649.ngrok.io for http and https traffic.

You will now need to configure your Bot to use these endpoints. Don’t forget to append your route when updating the messaging url, the new url should look like this: 
https://78191649.ngrok.io/v1/chat

Now you can start your server locally

    node server.js

If you are done with testing, you can stop ngrok (Ctrl+C), your agent will stop working as there is nothing to forward the requests to your local server.

NOTE: Free version of ngrok will create a new unique url for you everytime you start it. That means you always need to go back and update the messaging url for your bot.

NOTE: When running locally with ngrok, you need to comment out the call to skype.ensureHttps(true), because your service is not running as https server. 
