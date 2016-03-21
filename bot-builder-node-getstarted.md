---
layout: page
title: Getting started with the Bot Connector
permalink: /bot-connector-sdk-getstarted/
weight: 250
parent1: Bot Builder SDK
---


* TOC
{:toc}

## Overview
Bot Builder is a dialog system for building voice applications (“Bots”) using Node.js. The framework provides all of the components needed to manage the conversational aspects of a bot. Integrated support for the Microsoft Language Understanding Intelligent Service (LUIS) lets you use machine learning to create a rich natural language interface for your bot. And once your bot is completed you can easily connect it to a variety of platforms like the Microsoft Bot Framework, Skype, and Slack.
## Installation
To get started either install the framework via NPM:

    npm install --save botbuilder

Or clone our GitHub repository using Git. This may be preferable over NPM as it will provide you with numerous example code fragments and bots:

    git clone git@github.com:Microsoft/BotBuilder.git

Examples can then be found under the “Node/examples” directory of the cloned repository. 
## Hello World
Once the framework is installed we can get things started by building our first “Hello World” bot called HelloBot. The first decision we need to make is what kind of bot do we want to build? Bot Builder lets you build bots for a variety of platforms but for our HelloBot we're just going to interact with it though the command line so we're going to create an instance of the frameworks TextBot class. 

    var builder = require('botbuilder');

    var helloBot = new builder.TextBot();

We then need to add a dialog to our newly created bot object. Bot Builder breaks voice applications up into components called dialogs. If you think about building a voice application in the way you'd think about building a web application, each dialog can be thought of as route within the voice application. As users send messages to your bot the framework tracks which dialog is currently active and will automatically route the incoming message to the active dialog. For our HelloBot we'll just add single root dialog that responds to any message with “Hello World” and then we'll start the bot listening with a call listenStdin().

    var builder = require('botbuilder');

    var helloBot = new builder.TextBot();
    helloBot.add('/', function (session) {
        session.send('Hello World');
    });

    helloBot.listenStdin();

We can now run our bot and interact with it from the command line. So run the bot and type 'hello':

    node app.js
    hello
    Hello World

## Collecting Input
It's likely that you're going to want your bot to be a little smarter than HelloBot currently is so let's give HelloBot the ability to ask the user their name and then provide them with a personalized greeting. First let's add a new route called '/profile' and for the handler we're going to use something called a waterfall to prompt the user for their name and then save their response:

    helloBot.add('/profile',  [
        function (session) {
            builder.Prompts.text(session, 'Hi! What is your name?');
        },
        function (session, results) {
            session.userData.name = results.response;
            session.endDialog();
        }
    ]);

By passing an array of functions for our dialog handler a waterfall is setup where the results of the first function are passed to input of the second function. We can chain together a series of these functions into steps that create waterfalls of any length. 

In the first step of the '/profile' dialogs waterfall we're going to call the built-in Prompts.text() prompt to greet the user and ask them their name. The framework will route the users' response to that question to the results value of the second step where we'll save it off and then end the dialog. To save their response we're leveraging the frameworks built in storage constructs. You can persist data for a user globally by assigning values to the session.userData object and you can also leverage more temporary per/dialog storage using session.dialogData.

To make use of our new '/profile' dialog we'll need to modify our root '/' dialog to conditionally start the '/profile' dialog. Here's the complete code for our modified HelloBot:

    var builder = require('botbuilder');

    var helloBot = new builder.TextBot();
    helloBot.add('/', function (session) {
        if (!session.userData.name) {
            session.beginDialog('/profile');
        } else {
            session.send('Hello %s!', session.userData.name);
        }
    });
    helloBot.add('/profile',  [
        function (session) {
            builder.Prompts.text(session, 'Hi! What is your name?');
        },
        function (session, results) {
            session.userData.name = results.response;
            session.endDialog();
        }
    ]);

    helloBot.listenStdin();

We've modified the root '/' dialog to now check to see if we know the users name and if not redirect to the profile dialog using a call to beginDialog(). This will execute our waterfall and then control will be returned back to the root dialog with a call to endDialog(). We can now run HelloBot again to see the results of these improved smarts:

    node app.js
    hello
    Hi! What is your name?
    John
    Hello John!

The framework manages the flow of the conversation by mainting a stack active dialogs for each session, we call this the Dialog Stack. The built-in Prompts themselves are just other dialogs so if we were to inspect our bots dialog stack after the “Hi! What is your name?” prompt we'd see that it looks something like ['/', '/profile', 'BotBuilder.Dialogs.Prompts']. Using this stack plus other bookkeeping information persisted for each dialog, the framework is able to route the users' responses to the appropriate dialog handler.
## Handling Commands
So far we've shown the creation of dialogs based on closures but the framework comes with a number of classes that can be used to create more sophisticated dialogs. Let's use the CommandDialog class to add a couple of commands that make our bot a little more useful.  The CommandDialog lets you add a RegEx that when matched will invoke a Dialog Handler similar to the ones we've been creating so far. We'll add a command for changing the name set for our profile and then a second command to let us quit the conversation.

    var builder = require('botbuilder');

    var helloBot = new builder.TextBot();
    helloBot.add('/', new builder.CommandDialog()
        .matches('^set name', builder.DialogAction.beginDialog('/profile'))
        .matches('^quit', builder.DialogAction.endDialog())
        .onDefault(function (session) {
            if (!session.userData.name) {
                session.beginDialog('/profile');
            } else {
                session.send('Hello %s!', session.userData.name);
            }
        }));
    helloBot.add('/profile',  [
        function (session) {
            if (session.userData.name) {
                builder.Prompts.text(session, 'What would you like to change it to?');
            } else {
                builder.Prompts.text(session, 'Hi! What is your name?');
            }
        },
        function (session, results) {
            session.userData.name = results.response;
            session.endDialog();
        }
    ]);

    helloBot.listenStdin();

To add commands we changed our root '/' dialog to use an instance of a CommandDialog which you can see uses a fluent style interface to configure. We moved our existing dialog handler to become the onDefault() behavior of the dialog and we add our two commands. We're using DialogActions to implement the commands which are simple shortcuts that create a closure for a common action. The beginDialog() Dialog Action is going to begin the '/profile' dialog anytime the user says “set name” and the endDialog() action will exit the conversation when the user says “quit”. We also tweaked our '/profile' prompt to say something slightly different when changing the users name.  If we now run our updated HelloBot we get:

    hello
    Hi! What is your name?
    John
    Hello John!
    set name
    What would you like to change it to?
    John Smith
    Hello John Smith!
 
 