---
layout: page
title: Understanding Natural Language
permalink: /builder/node/guides/understanding-natural-language/
weight: 611
parent1: Bot Builder for Node.js
parent2: Guides
---

* TOC
{:toc}

## LUIS
Microsofts [Language Understanding Intelligent Service (LUIS)](http://luis.ai) offers a fast and effective way of adding language understanding to applications. With LUIS, you can use pre-existing, world-class, pre-built models from Bing and Cortana whenever they suit your purposes -- and when you need specialized models, LUIS guides you through the process of quickly building them. 

LUIS draws on technology for interactive machine learning and language understanding from [Microsoft Research](http://research.microsoft.com/en-us/) and Bing, including Microsoft Research's Platform for Interactive Concept Learning (PICL). LUIS is a part of project of Microsoft [Project Oxford](https://www.projectoxford.ai/). 

Bot Builder lets you use LUIS to add natural language understanding to your bot via the [LuisDialog](/builder/node/dialogs/LuisDialog/) class. You can add an instance of a LuisDialog that references your published language model and then add intent handlers to take actions in response to users utterances.  To see LUIS in action watch the 10 minute tutorial below.

* [Microsoft LUIS Tutorial](https://vimeo.com/145499419) (video)

## Intents, Entities, and Model Training
One of the key problems in human-computer interactions is the ability of the computer to understand what a person wants, and to find the pieces of information that are relevant to their intent. For example, in a news-browsing app, you might say "Get news about virtual reality companies," in which case there is the intention to FindNews, and "virtual reality companies" is the topic. LUIS is designed to enable you to very quickly deploy an http endpoint that will take the sentences you send it, and interpret them in terms of the intention they convey, and the key entities like "virtual reality companies" that are present. LUIS lets you custom design the set of intentions and entities that are relevant to the application, and then guides you through the process of building a language understanding system. 

Once your application is deployed and traffic starts to flow into the system, LUIS uses active learning to improve itself. In the active learning process, LUIS identifies the interactions that it is relatively unsure of, and asks you to label them according to intent and entities. This has tremendous advantages: LUIS knows what it is unsure of, and asks you to help where you will provide the maximum improvement in system performance. Secondly, by focusing on the important cases, LUIS learns as quickly as possible, and takes the minimum amount of your time. 

## Create Your Model
The first step of adding natural language support to your bot is to create your LUIS Model. You do this by logging into [LUIS](http://luis.ai) and creating a new LUIS Application for your bot. This application is what you’ll use to add the Intents & Entities that LUIS will use to train your bots Model.

![Create LUIS Application](/images/builder-luis-create-app.png)

In addition to creating a new app you have the option of either importing an existing model (this is what you'll do when working with the Bot Builder examples that use LUIS) or using the prebuilt Cortana app.  For the purposes of this tutorial we'll create a bot based on the prebuilt Cortana app. 
When you select the prebuilt Cortana app for English you’ll see a dialog like below. 

You’ll want to copy the URL listed on the dialog as this is what you’ll bind your [LuisDialog](/builder/node/dialogs/LuisDialog/) class to.  This URL points to the Model that LUIS published for your bots LUIS app and will be stable for the lifetime of the app. So once you’ve trained and published a model for a LUIS app you can update and re-train the model all you want without having to even redeploy your bot.  This is very handy in the early stages of building a bot as you’ll be re-training your model a lot.

![Prebuilt Cortana Application](/images/builder-luis-default-app.png)

## Handle Intents
Once you've deployed a model for your LUIS app we can create a bot that consumes that model. To keep things simple we'll create a [TextBot](/builder/node/bots/TextBot/) that we can interact with from a console window.

Create a folder for yoru bot, cd into it, and run npm init.

    npm init

Install the BotBuilder module from npm.

    npm install --save botbuilder

Create a file named app.js with the code below. You'll need to update the model in the sample code below to use the URL you got from LUIS for your copy of the prebuilt Cortana App.

{% highlight JavaScript %}
var builder = require('botbuilder');

// Create LUIS Dialog that points at our model and add it as the root '/' dialog for our Cortana Bot.
var model = '<your models url>';
var dialog = new builder.LuisDialog(model);
var cortanaBot = new builder.TextBot();
cortanaBot.add('/', dialog);

// Add intent handlers
dialog.on('builtin.intent.alarm.set_alarm', builder.DialogAction.send('Creating Alarm'));
dialog.on('builtin.intent.alarm.delete_alarm', builder.DialogAction.send('Deleting Alarm'));
dialog.onDefault(builder.DialogAction.send("I'm sorry I didn't understand. I can only create & delete alarms."));

cortanaBot.listenStdin();
{% endhighlight %}

This sample pulls in our Cortana Model and implements two intent handlers, one for setting a new alarm and one for deleting an alarm. Both handlers for now just use a [DialogAction](/builder/node/dialogs/Prompts/#dialog-actions) to send a static message when triggered. The Cortana Model can actually trigger a number of intents so we'll also add an [onDefault()](/sdkreference/nodejs/classes/_botbuilder_d_.luisdialog.html#ondefault) handler to catch any intents we don't currently support.  Running this sample from the command line we get something like this:

    node app.js
    set an alarm in 5 minutes called wakeup
    Creating Alarm
    snooze the wakeup alarm
    I'm sorry I didn't understand. I can only create & delete alarms.
    delete the wakeup alarm
    Deleting Alarm

## Process Entities
Now that we have our bot understanding what the users intended action is we can do the work of actually creating and deleting alarms. We’ll extend our sample to include logic to handle each [intent](/sdkreference/nodejs/interfaces/_botbuilder_d_.iintent.html) and a very simple in-memory alarm scheduler. We’ll also need to add a new ‘/notify’ dialog to do the work of actually telling the user when an alarm has triggered.    
  
{% highlight JavaScript %}
var builder = require('../../');

// Create LUIS Dialog that points at our model and add it as the root '/' dialog for our Cortana Bot.
var model = '<your models url>';
var dialog = new builder.LuisDialog(model);
var cortanaBot = new builder.TextBot();
cortanaBot.add('/', dialog);

// Add intent handlers
dialog.on('builtin.intent.alarm.set_alarm', [
    function (session, args, next) {
        // Resolve and store any entities passed from LUIS.
        var title = builder.EntityRecognizer.findEntity(args.entities, 'builtin.alarm.title');
        var time = builder.EntityRecognizer.resolveTime(args.entities);
        var alarm = session.dialogData.alarm = {
          title: title ? title.entity : null,
          timestamp: time ? time.getTime() : null  
        };
        
        // Prompt for title
        if (!alarm.title) {
            builder.Prompts.text(session, 'What would you like to call your alarm?');
        } else {
            next();
        }
    },
    function (session, results, next) {
        var alarm = session.dialogData.alarm;
        if (results.response) {
            alarm.title = results.response;
        }
        
        // Prompt for time (title will be blank if the user said cancel)
        if (alarm.title && !alarm.timestamp) {
            builder.Prompts.time(session, 'What time would you like to set the alarm for?');
        } else {
            next();
        }
    },
    function (session, results) {
        var alarm = session.dialogData.alarm;
        if (results.response) {
            var time = builder.EntityRecognizer.resolveTime([results.response]);
            alarm.timestamp = time ? time.getTime() : null;
        }
        
        // Set the alarm (if title or timestamp is blank the user said cancel)
        if (alarm.title && alarm.timestamp) {
            // Save address of who to notify and write to scheduler.
            alarm.to = session.message.from;
            alarm.from = session.message.to;
            alarms[alarm.title] = alarm;
            
            // Send confirmation to user
            var date = new Date(alarm.timestamp);
            var isAM = date.getHours() < 12;
            session.send('Creating alarm named "%s" for %d/%d/%d %d:%02d%s',
                alarm.title,
                date.getMonth() + 1, date.getDate(), date.getFullYear(),
                isAM ? date.getHours() : date.getHours() - 12, date.getMinutes(), isAM ? 'am' : 'pm');
        } else {
            session.send('Ok... no problem.');
        }
    }
]);

dialog.on('builtin.intent.alarm.delete_alarm', [
    function (session, args, next) {
        // Resolve entities passed from LUIS.
        var title;
        var entity = builder.EntityRecognizer.findEntity(args.entities, 'builtin.alarm.title');
        if (entity) {
            // Verify its in our set of alarms.
            title = builder.EntityRecognizer.findBestMatch(alarms, entity.entity);
        }
        
        // Prompt for alarm name
        if (!title) {
            builder.Prompts.choice(session, 'Which alarm would you like to delete?', alarms);
        } else {
            next({ response: title });
        }
    },
    function (session, results) {
        // If response is null the user canceled the task
        if (results.response) {
            delete alarms[results.response.entity];
            session.send("Deleted the '%s' alarm.", results.response.entity);
        } else {
            session.send('Ok... no problem.');
        }
    }
]);

dialog.onDefault(builder.DialogAction.send("I'm sorry I didn't understand. I can only create & delete alarms."));

// Add notification dialog
cortanaBot.add('/notify', function (session, alarm) {
   session.send("Here's your '%s' alarm.", alarm.title); 
   session.endDialog(); // <= we don't want replies coming to us 
});

cortanaBot.listenStdin();

// Very simple alarm scheduler
var alarms = {};
setInterval(function () {
    var now = new Date().getTime();
    for (var key in alarms) {
        var alarm = alarms[key];
        if (now >= alarm.timestamp) {
            cortanaBot.beginDialog({ from: alarm.from, to: alarm.to }, '/notify', alarm);
            delete alarms[key];
        }
    }
}, 15000);
{% endhighlight %}

We’re using [waterfalls](/builder/node/dialogs/overview/#waterfall) for our set_alarm & delete_alarm [intent handlers](/builder/node/dialogs/LuisDialog/#intent-handling). This is a common pattern that you’ll likely use for most of your intent handlers. The way waterfalls work in Bot Builder is the very first step of the waterfall is called when a dialog (or in this case intent handler) is triggered. The step then does some work and continue execution of the waterfall by either calling another dialog (like a built-in prompt) or calling the optional next() function passed in.  When a dialog is called in a step, any result returned from the dialog will be passed as input to the results parameter for the next step.
   
In the case of intent handlers any [entities](/sdkreference/nodejs/interfaces/_botbuilder_d_.ientity.html) that LUIS recognized will be passed along in the args parameter of that first step of the waterfall. More often than not you need some additional pieces of information before you can fully process the users request.  LUIS uses entities to pass that extra data along but you really don’t want to require that the user enter every single piece of information up front. So in the set_alarm case we want to support “set alarm in 5 minutes called wakeup”, “create an alarm called wakeup”, and just “create an alarm”.  That means we may not always get all of the entities we expect from LUIS and even when we get them we should expect them to be invalid at times.  So in all cases we’re going to want to be prepared to prompt the user for missing or invalid entities. Bot Builder makes it relatively easy to build that flexibility into your bot by using a combination of [waterfalls](/builder/node/dialogs/overview/#waterfall) & built-in [prompts](/builder/node/dialogs/Prompts/). 

Looking at the waterfall for the set_alarm intent.  We’re first going to try and validate & store any entities we received from LUIS. Bot Builder includes an [EntityRecognizer](/builder/node/dialogs/LuisDialog/#entity-recognition) class which has useful functions for working with entities. Times, for instance, can come in fairly decomposed and often spanning multiple entities. You can use the [EntityRecognizer.resolveTime()](/builder/node/dialogs/LuisDialog/#resolving-dates--times) function to return you an actual JavaScript Date object based upon the passed in time entities if it’s able to calculate one. 

Once we’ve validated and stored our entities we’re going to then decide if we need to prompt the user for the name of the alarm. If we got the title passed to us from LUIS we can skip the prompt and proceed to the next step in the waterfall using the next() function. If not we can ask the user for the title using the [Prompts.text()](/builder/node/dialogs/Prompts/#promptstext) built-in prompt.  If the user enters a title it will be passed to the next step via the results.response field so in the next step of the waterfall we can store the users response, and then figure out do we have the next missing piece of data and either skip to the next step or prompt. This sequence continues until we’ve either collected all of the needed entities or the user cancels the task. 

The built-in prompts all support letting the user cancel a prompt by saying ‘cancel’ or ‘nevermind’.  It’s up to you to decide whether that means cancel just the current step or cancel the whole task.

For the delete_alarm intent handler we have a similar waterfall. It’s a little simpler because it only needs the title but this waterfall illustrates using two very power features of Bot Builder. You can use [EntityRecognizer.findBestMatch()](/builder/node/dialogs/LuisDialog/#matching-list-items) to compare a users utterance against a list of choices and [Prompts.choice()](/builder/node/dialogs/Prompts/#promptschoice) to present the user with a list of choices to choose from. Both are very flexible and support fuzzy matching of choices.    

Finally, we added a ‘/notify’ dialog to notify the user when their alarm fires. Our simple alarm scheduler triggers this push notification to the user via a call [cortanaBot.beginDialog()](/sdkreference/nodejs/classes/_botbuilder_d_.textbot.html#begindialog) specifying the address of the user to contact and the name of dialog to start.  It can also pass additional arguments to the dialog so in this example we’re passing the triggered alarm.  The alarm.from & alarm.to aren’t that relevant for our simple [TextBot](/builder/node/bots/TextBot/) based bot but in a real bot you’d need to address the outgoing message with the user you're starting a conversation with so those fields are included here for completeness.

The important thing to note with bot originated dialogs is that they’re full dialogs meaning the user can reply to the bots message and that response will get routed to the dialog. This is really powerful because it means that you could notify a user that their alarm trigged and they could reply asking your bot to “snooze it”.

If we now run our bot again, we’ll get an output similar to:
 
    node app.js
    set an alarm in 5 minutes called wakeup
    Creating alarm named 'wakeup' for 3/24/2016 9:05am
    snooze the wakeup alarm
    I'm sorry I didn't understand. I can only create & delete alarms.
    delete the wakeup alarm
    Deleting Alarm