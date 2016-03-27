---
layout: page
title: Dialogs
permalink: /builder/node/dialogs/overview/
weight: 630
parent1: Bot Builder for Node.js
parent2: Dialogs
---

* TOC
{:toc}

## Overview
Bot Builder uses dialogs to manage a bots conversations with a user. To understand dialogs its easiest to think of them as the equivalent of routes for a website. All bots will have at least one root ‘/’ dialog just like all websites typically have at least one root ‘/’ route. When the framework receives a message from the user it will be routed to this root ‘/’ dialog for processing. For many bots this single root ‘/’ dialog is all that’s needed but just like websites often have multiple routes, bots will often have multiple dialogs.

{% highlight JavaScript %}
var builder = require('botbuilder');

var helloBot = new builder.TextBot();
helloBot.add('/', function (session) {
    if (!session.userData.name) {
        session.beginDialog('/profile');
    } else {
        session.send('Hello %s!', session.userData.name);
    }
});
helloBot.add('/profile', [
    function (session) {
        builder.Prompts.text(session, 'Hi! What is your name?');
    },
    function (session, results) {
        session.userData.name = results.response;
        session.endDialog();
    }
]);

helloBot.listenStdin();
{% endhighlight %}

The example above shows a bot with 2 dialogs. The first message from a user will be routed to the Dialog Handler for the root ‘/’ dialog. This function gets passed a [session]( /sdkreference/nodejs/classes/_botbuilder_d_.session.html) object which can be used to inspect the users message, send a reply to the user, save state on behalf of the user, or redirect to another dialog. 

When a user starts a conversation with our bot we'll first look to see if we know the users name by checking a property off the [session.userData]( /sdkreference/nodejs/classes/_botbuilder_d_.session.html#userdata) object. This data will be persisted across all of the users interactions with the bot and can be used to store things like profile information. If we don’t know the users name we’re going to redirect them to the ‘/profile’ dialog to ask them their name using a call to [session.beginDialog()]( /sdkreference/nodejs/classes/_botbuilder_d_.session.html#begindialog).

The ‘/profile’ dialog is implemented as a [waterfall](#waterfall) and when beginDialog() is called the first step of the waterfall will be immediately executed. This step simply calls [Prompts.text()](/builder/node/dialogs/Prompts/#promptstext) to ask the user their name. This built-in prompt is just another dialog that gets redirected to. The framework maintains a stack of dialogs for each conversation so if we were to inspect our bots dialog stack at this point (which you can do using the [Bot Framework Emulator]( /connector/tools/bot-framework-emulator/)) it would look something like [‘/’, ‘/profile’, ‘BotConnector.Dialogs.Prompts’]. The conversations dialog stack helps the framework know where to route the users reply to.

When the user replies with their name the prompt will call [session.endDialog()](/sdkreference/nodejs/classes/_botbuilder_d_.session.html#enddialog) with the users response. This response will be passed as an argument to the second step of the ‘/profile’ dialogs waterfall. In this step we'll save the users name to session.userData.name property and return control back to the root ‘/’ dialog through another call to endDialog().  At that point the root ‘/’ dialogs closure will be re-run but this time we know the users name so we’ll just present the user with a personalized greeting.

It’s worth noting that the built-in prompts will let the user cancel an action by saying something like ‘nevermind’ or ‘cancel’.  It’s up to the dialog that called the prompt to determine what cancel means so to detect that a prompt was canceled you can either check the [ResumeReason]( /sdkreference/nodejs/enums/_botbuilder_d_.resumereason.html) code returned in [result.resumed]( /sdkreference/nodejs/interfaces/_botbuilder_d_.ipromptresult.html#resumed) or simply check that [result.response]( /sdkreference/nodejs/interfaces/_botbuilder_d_.ipromptresult.html#response) isn't null. There are actually a number of reasons that can cause the prompt to return without a response so checking for a null response tends to be the best approach.  In our example bot, should the user say ‘nevermind’ when prompted for their name, the bot would simply ask them for their name again.   

## Dialog Handlers
Bots derive from a [DialogCollection]( /sdkreference/nodejs/classes/_botbuilder_d_.dialogcollection.html) class that can be used to register dialogs with the bot.  The [DialogCollection.add()](/sdkreference/nodejs/classes/_botbuilder_d_.dialogcollection.html#add) method lets you express a dialog in a variety of forms.

### Closure
The simplest form of a dialog is a function that will process all messages received from the user as well as results returned from other dialogs when they call [session.endDialog()](/sdkreference/nodejs/classes/_botbuilder_d_.session.html#enddialog).

{% highlight JavaScript %}
bot.add('/', function (session) {
    session.send('Hello World');
});
{% endhighlight %}

### Waterfall
Waterfalls simplify the processing of input received from the user.  If you redirect to another dialog or call one of the built-in prompts any results returned from that dialog will be passed as input to the next step of the waterfall. To create a waterfall just pass an array of functions in your call to add().

{% highlight JavaScript %}
bot.add('/', [
    function (session) {
        builder.Prompts.text(session, 'Hi! What is your name?');
    },
    function (session, results) {
        session.send('Hello %s!', results.response);
    }
]);
{% endhighlight %}

### Dialog Object
For more specialized dialogs you can add an instance of a class that derives from [Dialog]( /sdkreference/nodejs/classes/_botbuilder_d_.dialog.html). Typically, you’ll add either a [CommandDialog]( /builder/node/dialogs/CommandDialog/) or a [LuisDialog]( /builder/node/dialogs/LuisDialog/) to listen for the user to say something specific.

{% highlight JavaScript %}
bot.add('/', new builder.CommandDialog()
    .matches('^hello', function (session) {
        session.send("Hi there!");
    })
    .onDefault(function (session) {
        session.send("I didn't understand. Say hello to me!");
    }));
{% endhighlight %}
