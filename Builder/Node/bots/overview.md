---
layout: page
title: Overview
permalink: /builder/node/bots/overview/
weight: 620
parent1: Bot Builder for Node.js
parent2: Bots
---

* TOC
{:toc}

## Bots
Bot Builder includes several adapter classes (Bots) to let you build bots that run on a variety of platforms and using the [TextBot](/builder/node/bots/TextBot/) you can easily adapt Bot Builder to work with virtually any platform.

## Dialogs
All of the built-in Bot classes derive from the [DialogCollection](/sdkreference/nodejs/classes/_botbuilder_d_.dialogcollection.html) class which contains methods for adding [dialogs](/builder/node/dialogs/overview/) and [middleware](#middleware) to your bot. The dialogs you add to your bot should generally be portable across all types of bots so a common practice is start building as a [TextBot](/builder/node/bots/TextBot/) so that you can easily test it from a console window and then port it to one or more other platforms using the other Bot class.  Bot Builder provides several [examples](/builder/node/guides/examples/) of bots that support multiple platforms.

## Middleware
Bot Builder supports a [Connect](https://github.com/senchalabs/connect) style middleware layer that lets developer easily extend the framework with new functionality.  Middleware gets a chance to process every message coming into the bot so using middleware you can do things like redirect new users to a first run experience, perform sentiment analysis for incoming messages, or add new functions to the [Session](/sdkreference/nodejs/classes/_botbuilder_d_.session.html) object  that do things like print a rich table using markdown. 

Call the [DialogCollection.use()](/sdkreference/nodejs/classes/_botbuilder_d_.dialogcollection.html#use) method to install middleware. The middleware will be executed in the order in which its installed. Below is an example of using middleware to redirect new users to a first run experience.

{% highlight JavaScript %}
var builder = require('../../');

var bot = new builder.TextBot();
bot.add('/', function (session) {
    session.send("Hi %s, what can I help you with?", session.userData.name);
});

// Install First Run middleware and dialog
bot.use(function (session, next) {
   if (!session.userData.firstRun) {
       session.userData.firstRun = true;
       session.beginDialog('/firstRun');
   } else {
       next();
   }
});
bot.add('/firstRun', [
    function (session) {
        builder.Prompts.text(session, "Hello... What's your name?");
    },
    function (session, results) {
        // We'll save the prompts result and return control to main through
        // a call to replaceDialog(). We need to use replaceDialog() because
        // we intercepted the original call to main and we want to remove the
        // /firstRun dialog from the callstack. If we called endDialog() here
        // the conversation would end since the /firstRun dialog is the only 
        // dialog on the stack.
        session.userData.name = results.response;
        session.replaceDialog('/'); 
    }
]);

bot.listenStdin();
{% endhighlight %}
