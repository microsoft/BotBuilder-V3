---
layout: page
title: Dialogs
permalink: /en-us/node/builder/chat/dialogs/
weight: 2621
parent1: Bot Builder for Node.js
parent2: Chat Bots
---

* TOC
{:toc}

## Overview
Bot Builder uses dialogs to manage a bots conversations with a user. To understand dialogs its easiest to think of them as the equivalent of routes for a website. All bots will have at least one root ‘/’ dialog just like all websites typically have at least one root ‘/’ route. 
When the framework receives a message from the user it will be routed to this root ‘/’ dialog for processing. For many bots this single root ‘/’ dialog is all that’s needed but just like websites often have multiple routes, bots will often have multiple dialogs.

{% highlight JavaScript %}
var builder = require('botbuilder');

var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector);
bot.dialog('/', [
    function (session, args, next) {
        if (!session.userData.name) {
            session.beginDialog('/profile');
        } else {
            next();
        }
    },
    function (session, results) {
        session.send('Hello %s!', session.userData.name);
    }
]);

bot.dialog('/profile', [
    function (session) {
        builder.Prompts.text(session, 'Hi! What is your name?');
    },
    function (session, results) {
        session.userData.name = results.response;
        session.endDialog();
    }
]);
{% endhighlight %}

The example above shows a bot with 2 dialogs. The first message from a user will be routed to the Dialog Handler for the root ‘/’ dialog. This function gets passed a [session](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session.html) object which can be used to inspect the users message, send a reply to the user, save state on behalf of the user, or redirect to another dialog. 

When a user starts a conversation with our bot we'll first look to see if we know the users name by checking a property off the [session.userData](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session.html#userdata) object. This data will be persisted across all of the users interactions with the bot and can be used to store things like profile information. If we don’t know the users name we’re going to redirect them to the ‘/profile’ dialog to ask them their name using a call to [session.beginDialog()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session.html#begindialog).

The ‘/profile’ dialog is implemented as a [waterfall](#waterfall) and when beginDialog() is called the first step of the waterfall will be immediately executed. This step simply calls [Prompts.text()](/en-us/node/builder/chat/prompts/#promptstext) to ask the user their name. This built-in prompt is just another dialog that gets redirected to. The framework maintains a stack of dialogs for each conversation so if we were to inspect our bots dialog stack at this point it would look something like [‘/’, ‘/profile’, ‘BotBuilder:Prompts’]. The conversations dialog stack helps the framework know where to route the users reply to.

When the user replies with their name, the prompt will call [session.endDialogWithResult()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session.html#enddialogwithresult) with the users response. This response will be passed as an argument to the second step of the ‘/profile’ dialogs waterfall. In this step we'll save the users name to session.userData.name property and return control back to the root ‘/’ dialog through a call to [endDialog()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#enddialog).  At that point the next step of the root ‘/’ dialogs waterfall will be executed ad a custom greeting will be sent to the user.

It’s worth noting that the built-in prompts will let the user cancel an action by saying something like ‘nevermind’ or ‘cancel’.  It’s up to the dialog that called the prompt to determine what cancel means so to detect that a prompt was canceled you can either check the [ResumeReason](/en-us/node/builder/chat-reference/enums/_botbuilder_d_.resumereason.html) code returned in [result.resumed](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ipromptresult.html#resumed) or simply check that [result.response](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ipromptresult.html#response) isn't null. There are actually a number of reasons that can cause the prompt to return without a response so checking for a null response tends to be the best approach.  In our example bot, should the user say ‘nevermind’ when prompted for their name, the bot would simply ask them for their name again.   

## Dialog Handlers
A bots dialogs can be expressed using a variety of forms.

### Waterfall
Waterfalls will likely be the most common form of dialog you use so understanding how they work is a fundamental skill in bot development. Waterfalls let you collect input from a user using a sequence of steps. A bot is always in a state of providing a user with information or asking a question and then waiting for input. In the Node version of Bot Builder its waterfalls that drive this back-n-forth flow.

Paired with the built-in [Prompts](/en-us/node/builder/chat/prompts/) you can easily prompt the user with a series of questions:

{% highlight JavaScript %}
bot.dialog('/', [
    function (session) {
        builder.Prompts.text(session, 'Hi! What is your name?');
    },
    function (session, results) {
        session.send('Hello %s!', results.response);
    }
]);
{% endhighlight %}

Bots based on Bot Builder implement something we call “Guided Dialog” meaning that the bot is generally driving (or guiding) the conversation with the user.  With waterfalls you drive the conversation by taking an action that moves the waterfall from one step to the next.  Calling a built-in prompt like [Prompts.text()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.prompts#text) moves the conversation along because the users response to the prompt is passed to the input of the next waterfall step.  You can also call [session.beginDialog())] to start one of your own dialogs to move the conversation to the next step:

{% highlight JavaScript %}
bot.dialog('/', [
    function (session) {
        session.beginDialog('/askName');
    },
    function (session, results) {
        session.send('Hello %s!', results.response);
    }
]);
bot.dialog('/askName', [
    function (session) {
        builder.Prompts.text(session, 'Hi! What is your name?');
    },
    function (session, results) {
        session.endDialogWithResult(results);
    }
]);
{% endhighlight %}

This achieves the same basic behavior as before but calls a child dialog to prompt for the users name. That’s somewhat pointless in this example but could be a useful way of partitioning the conversation if you had multiple profile fields you wanted to populate.  

This example can actually be simplified some.  All waterfalls contain a phantom last step which automatically returns the result from the last step so we could actually simplify this to:

{% highlight JavaScript %}
bot.dialog('/', [
    function (session) {
        session.beginDialog('/askName');
    },
    function (session, results) {
        session.send('Hello %s!', results.response);
    }
]);
bot.dialog('/askName', [
    function (session) {
        builder.Prompts.text(session, 'Hi! What is your name?');
    }
]);
{% endhighlight %}

The first step of a waterfall can receive arguments passed to the dialog and every step receives a `next()` function that can be used to advance the waterfall forward manually.  In the example below we’ve paired these two features together to create an ‘/ensureProfile’ dialog that will verify that a users profile is filled in and prompt the user for any missing fields. This pattern would let us add fields to the profile later that would be automatically filled in as users message the bot:

{% highlight JavaScript %}
bot.dialog('/', [
    function (session) {
        session.beginDialog('/ensureProfile', session.userData.profile);
    },
    function (session, results) {
        session.userData.profile = results.response;
        session.send('Hello %(name)s! I love %(company)s!', session.userData.profile);
    }
]);
bot.dialog('/ensureProfile', [
    function (session, args, next) {
        session.dialogData = args;
        if (!session.dialogData.name) {
            builder.Prompts.text(session, "What's your name?");
        } else {
            next();
        }
    },
    function (session, results, next) {
        if (results.repsonse) {
            session.dialogData.name = results.response;
        }
        if (!session.dialogData.company) {
            builder.Prompts.text(session, "What company do you work for?");
        } else {
            next();
        }
    },
    function (session, results) {
        if (results.response) {
            session.dialogData.company = results.response;
        }
        session.endDialogWithResults({ response: session.dialogData });
    }
]);
{% endhighlight %}

In the ‘/ensureProfile’ dialog we’re using [session.dialogData](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#dialogdata) to temporarily hold the users profile. We do this because when our bot is distributed across multiple compute nodes, every step of the waterfall could be processed by a different compute node. The dialogData field ensures that the dialogs state is properly maintained between each turn of the conversation.  You can store anything you want into this field but should limit yourself to JavaScript primitives that can be properly serialized. 

It’s worth noting that the `next()` function can be passed an [IDialogResult](<iface/>idialogresult) so it can mimic any results returned from a built-in prompt or other dialog which sometimes simplifies your bots control logic.

### Closure
You can also pass a single function for your dialog handler which simply results in creating a 1 step waterfall: 

{% highlight JavaScript %}
bot.dialog('/', function (session) {
    session.send("Hello World");
});
{% endhighlight %}

### Dialog Object
For more specialized dialogs you can add an instance of a class that derives from the [Dialog](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.dialog.html) base class. This gives maximum flexibility for how your bot behaves as the built-in prompts and even waterfalls are implemented internally as dialogs.

{% highlight JavaScript %}
bot.dialog('/', new builder.IntentDialog()
    .matches(/^hello/i, function (session) {
        session.send("Hi there!");
    })
    .onDefault(function (session) {
        session.send("I didn't understand. Say hello to me!");
    }));
{% endhighlight %}

### SimpleDialog
Implementing a new Dialog from scratch can be tricky as there are a lot of things to consider. To try and cover the bulk of the scenarios not covered by waterfalls, we include a [SimpleDialog](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.simpledialog) class. The closure passed to this class works very similar to a waterfall step with the exception that the results from calling a built-in prompt or other dialog will be passed back to one closure.  Unlike a waterfall there’s no phantom step that the conversation is advanced to.  This is powerful but also dangerous as your bot can easily get stuck in a loop so care should be used:
 
{% highlight JavaScript %}
bot.dialog('/', new builder.SimpleDialog(function (session, results) {
    if (results && results.response) {
        session.send(results.response.toString('base64'));
    }
    builder.Prompts.text(session, "What would you like to base64 encode?");
}));
{% endhighlight %}

The above example is a base64 bot that all it does is convert a users input to base64.  It sits in a tight loop prompting the user for string which it then encodes. 

