---
layout: page
title: LuisDialog
permalink: /builder/node/dialogs/LuisDialog/
weight: 633
parent1: Bot Builder for Node.js
parent2: Dialogs
---

* TOC
{:toc}

## Overview
The [LuisDialog](/sdkreference/nodejs/classes/_botbuilder_d_.luisdialog.html) class lets you use Microsofts [Language Understanding Intelligent Service (LUIS)](http://luis.ai) to add natural language capabilities to your bot. For a detailed walkthrough of using LUIS and how to add natural language support to your bot, read the [Understanding Natural Language](/builder/node/guides/understanding-natural-language/) guide.

## Intent Handling
You initialize the [LuisDialog](/sdkreference/nodejs/classes/_botbuilder_d_.luisdialog.html) with the Application  Service URL of the model you trained using [LUIS]( http://luis.ai). When the dialog receives a message from the user it will first pass the users utterance to your model for recognition. It will then take the top [intent](/sdkreference/nodejs/interfaces/_botbuilder_d_.iintent.html) predicted by the model and invoke the handler registered for that intent. If there’s not a handler registered for the top intent, or the top intent doesn’t meet a minimum threshold score, the [onDefault](/sdkreference/nodejs/classes/_botbuilder_d_.luisdialog.html#ondefault) handler will be invoked instead.

LUIS ranks intents with a [score](/sdkreference/nodejs/interfaces/_botbuilder_d_.iintent.html#score) from 0.0 to 1.0. This is LUIS’s confidence that an intent matches the users intention of what they’d like to do.  By default, the LuisDialog requires a minimum score of 0.1 for an intent to be triggered. You can adjust this threshold on a per dialog basis using the [LuisDialog.setThreshold()](/sdkreference/nodejs/classes/_botbuilder_d_.luisdialog.html#setthreshold) method.

Intent handlers can be registered for a model using the [LuisDialog.on()](/sdkreference/nodejs/classes/_botbuilder_d_.luisdialog.html#on) method. When a handler is invoked it will be passed the full list of [intents](/sdkreference/nodejs/interfaces/_botbuilder_d_.iintentargs.html#intents) & [entities](/sdkreference/nodejs/interfaces/_botbuilder_d_.iintentargs.html#entities) that LUIS recognized via the handlers args parameter. The handler itself can be implemented in a variety of ways. 


A simple closure. This function will be invoked both when the handler is initially triggered and again when a child dialog started by the handler returns.

{% highlight JavaScript %}
var dialog = new builder.LuisDialog('https://api.projectoxford.ai/luis/...');
bot.add('/', dialog);

dialog.on('AskVersion', function (session, args) {
    session.send('Bot version 1.2');
});
{% endhighlight %}

A [DialogAction](/builder/node/dialogs/Prompts/#dialog-actions). Shortcuts for implementing the above closure.

{% highlight JavaScript %}
var dialog = new builder.LuisDialog('https://api.projectoxford.ai/luis/...');
bot.add('/', dialog);

dialog.on('AskVersion', builder.DialogAction.send('Bot version 1.2'));
{% endhighlight %}

A waterfall when you need to collect input from the user.

{% highlight JavaScript %}
var dialog = new builder.LuisDialog('https://api.projectoxford.ai/luis/...');
bot.add('/', dialog);

dialog.on('EchoUser', [
    function (session, args) {
        builder.Prompts.text(session, "What would you like me to say?");
    },
    function (session, results) {
        if (results.response) {
            session.send("Ok... %s", results.response);
        } else {
            session.send("Ok");
        }
    }
]);
{% endhighlight %}

The ID of a dialog to redirect to and optional arguments to pass to that dialog. It's often easiest to think of the LuisDialog as a switch that simply redirects to another dialog when a given intent is recognized so we support a shorthand way of expressing that. The full list of [intents](/sdkreference/nodejs/interfaces/_botbuilder_d_.iintentargs.html#intents) & [entities](/sdkreference/nodejs/interfaces/_botbuilder_d_.iintentargs.html#entities) that LUIS recognized will be passed to the dialog as part of its arguments.

{% highlight JavaScript %}
bot.add('/', new builder.LuisDialog('https://api.projectoxford.ai/luis/...')
    .on('AddTask', '/addTask')
    .on('ChangeTask', '/changeTask')
    .on('DeleteTask', '/deleteTask')
    .onDefault(builder.DialogAction.send("I'm sorry. I didn't understand."))
);
{% endhighlight %}

## Entity Recognition
LUIS can not only identify a users intention given an utterance, it can extract entities from their utterance as well.  Any entities recognized in the users utterance will be passed to the intent handler via its [args](/sdkreference/nodejs/interfaces/_botbuilder_d_.iintentargs.html) parameter. Bot Builder includes an [EntityRecognizer](/sdkreference/nodejs/classes/_botbuilder_d_.entityrecognizer.html) class to simplify working with these entities. 

### Finding Entities
You can use [EntityRecognizer.findEntity()](/sdkreference/nodejs/classes/_botbuilder_d_.entityrecognizer.html#findentity) and [EntityRecognizer.findAllEntities()](/sdkreference/nodejs/classes/_botbuilder_d_.entityrecognizer.html#findallentities) to search for entities of a specific type by name.

{% highlight JavaScript %}
var dialog = new builder.LuisDialog('https://api.projectoxford.ai/luis/...');
bot.add('/', dialog);

dialog.on('AddTask', [
    function (session, args, next) {
        var task = builder.EntityRecognizer.findEntity(args.entities, 'TaskTitle');
        if (!task) {
            builder.Prompts.text(session, "What would you like to call the task?");
        } else {
            next({ response: task.entity });
        }
    },
    function (session, results) {
        if (results.response) {
            // ... save task
            session.send("Ok... Added the '%s' task.", results.response);
        } else {
            session.send("Ok");
        }
    }
]);
{% endhighlight %}

### Resolving Dates & Times
LUIS has a powerful builtin.datetime entity recognizer that can recognize a wide range of relative & absolute dates expressed using natural language. The issue is that when LUIS returns the dates & times it recognized, it does so by returning their component parts. So if the user says “june 5th at 9am” it will return separate entities for the date & time components of the utterance. These entities need to be combined using [EntityRecognizer.resolveTime()](/sdkreference/nodejs/classes/_botbuilder_d_.entityrecognizer.html#resolvetime) to get the actual resolved date. This method will try to convert an array of entities a valid JavaScript Date object. If it can’t resolve the entities to a valid Date it will return null.

{% highlight JavaScript %}
var dialog = new builder.LuisDialog('https://api.projectoxford.ai/luis/...');
bot.add('/', dialog);

dialog.on('SetAlarm', [
    function (session, args, next) {
        var time = builder.EntityRecognizer.resolveTime(args.entities);
        if (!time) {
            builder.Prompts.time(session, 'What time would you like to set the alarm for?');
        } else {
            // Saving date as a timestamp between turns as session.dialogData could get serialized.
            session.dialogData.timestamp = time.getTime();
            next();
        }
    },
    function (session, results) {
        var time;
        if (results.response) {
            time = builder.EntityRecognizer.resolveTime([results.response]);
        } else if (session.dialogData.timestamp) {
            time = new Date(session.dialogData.timestamp);
        }
        
        // Set the alarm
        if (time) {

            // .... save alarm
            
            // Send confirmation to user
            var isAM = time.getHours() < 12;
            session.send('Setting alarm for %d/%d/%d %d:%02d%s',
                time.getMonth() + 1, time.getDate(), time.getFullYear(),
                isAM ? time.getHours() : time.getHours() - 12, time.getMinutes(), isAM ? 'am' : 'pm');
        } else {
            session.send('Ok... no problem.');
        }
    }
]);
{% endhighlight %}

### Matching List Items
Bot Builder includes a powerful [choice() prompt](/builder/node/dialogs/Prompts/#promptschoice) which lets you present a list of choices to a user for them to pick from. LUIS makes it easy to map a users choice to a named entity but it doesn’t do any validation that the user entered a valid choice. You can use [EntityRecognizer.findBestMatch()](/sdkreference/nodejs/classes/_botbuilder_d_.entityrecognizer.html#findbestmatch) and [EntityRecognizer.findAllMatches()](/sdkreference/nodejs/classes/_botbuilder_d_.entityrecognizer.html#findallmatches) to verify that the user entered a valid choice. These methods are the same methods used by the choice() prompt and offer a lot of flexibility when matching a users utterance to a value in a list.

List items can be matched using a case insensitive exact match so given the list [“Red”,”Green”,”Blue”] the user can say “red” to match the “Red” item. Using a partial match where the user says “blu” to match the “Blue” item. Or a reverse partial match where the user says “the green one” to match the “Green” item.  Internally the match functions calculate a coverage score when evaluating partial matches. For the “blu” utterance that matched the “Blue” item the coverage score would have been 0.75 and for the “the green one” utterance that matched “green” the coverage score would have been 0.88. The minimum score needed to trigger a match is 0.6 but this can be adjusted for each match.

An [IFindMatchResult](/sdkreference/nodejs/interfaces/_botbuilder_d_.ifindmatchresult.html) is returned for each match and contains the [entity](/sdkreference/nodejs/interfaces/_botbuilder_d_.ifindmatchresult.html#entity), [index](/sdkreference/nodejs/interfaces/_botbuilder_d_.ifindmatchresult.html#index), and [score](/sdkreference/nodejs/interfaces/_botbuilder_d_.ifindmatchresult.html#score) for the list item that was watched.

{% highlight JavaScript %}
var dialog = new builder.LuisDialog('https://api.projectoxford.ai/luis/...');
bot.add('/', dialog);

dialog.on('DeleteTask', [
    function (session, args, next) {
        // Process optional entities received from LUIS
        var match;
        var entity = builder.EntityRecognizer.findEntity(args.entities, 'TaskTitle');
        if (entity) {
            match = builder.EntityRecognizer.findBestMatch(tasks, entity.entity);
        }
        
        // Prompt for task name
        if (!match) {
            builder.Prompts.choice(session, "Which task would you like to delete?", tasks);
        } else {
            next({ response: match });
        }
    },
    function (session, results) {
        if (results.response) {
            delete tasks[results.response.entity];
            session.send("Deleted the '%s' task.", results.response.entity);
        } else {
            session.send('Ok... no problem.');
        }
    }
]);
{% endhighlight %}

## onBegin & onDefault Handlers
The LuisDialog lets you register an [onBegin](/sdkreference/nodejs/classes/_botbuilder_d_.luisdialog.html#onbegin) handler that will be notified anytime the dialog is first loaded for a conversation and an [onDefault](/sdkreference/nodejs/classes/_botbuilder_d_.luisdialog.html#ondefault) handler that will be notified anytime the users utterance failed to match one of the registered intents.

The onBegin handler is invoked when [session.beginDialog()](/sdkreference/nodejs/classes/_botbuilder_d_.session.html#begindialog) has been called for the dialog and gives the dialog an opportunity to process optional arguments passed in the call to beginDialog().  The handler is passed a next() function which can be invoked to continue executing the dialogs default logic. You can intercept the call all together by not calling next(). This would let you potentially redirect to another dialog with the caveat that you won’t be able to process any results returned from that dialog when it calls [session.endDialog()](/sdkreference/nodejs/classes/_botbuilder_d_.session.html#enddialog).

{% highlight JavaScript %}
dialog.onBegin(function (session, args, next) {
    if (!session.userData.firstRun) {
        // Send the user through the first run experience
        session.userData.firstRun = true;
        session.beginDialog('/firstRun');
    } else {
        next();
    }
});
{% endhighlight %}

The onDefault handler is invoked anytime the users utterance doesn’t match one of the registered intents. The handler can be a closure, DialogAction, waterfall, or dialog redirect.

{% highlight JavaScript %}
dialog.onDefault(builder.DialogAction.send("I'm sorry. I didn't understand."));
{% endhighlight %}
