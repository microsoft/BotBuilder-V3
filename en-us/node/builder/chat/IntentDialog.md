---
layout: page
title: IntentDialog
permalink: /en-us/node/builder/chat/IntentDialog/
weight: 2624
parent1: Bot Builder for Node.js
parent2: Chat Bots
---

* TOC
{:toc}

## Overview
The [IntentDialog](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.intentdialog.html) class lets you listen for the user to say a specific keyword or phrase. We call this intent detection because you are attempting to determine what the user is intending to do. IntentDialogs are useful for building more open ended bots that support natural language style understanding. For an in depth walk through of using IntentDialogs to add natural language support to a bot see the [Understanding Natural Language](/en-us/node/builder/guides/understanding-natural-language/) guide.

> __NOTE:__ For users of Bot Builder v1.x the [CommandDialog](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.commanddialog) and [LuisDialog](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.luisdialog) classes have been deprecated.  These classes will continue to function but developers are encouraged to upgrade to the more flexible IntentDialog class at their earliest convenience.

## Matching Regular Expressions
The [IntentDialog.matches()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.intentdialog.html#matches) method lets you trigger a handler based on the users utterance matching a regular expressions. The handler itself can take a variety of forms.

A waterfall when you need to collect input from the user:

{% highlight JavaScript %}
var intents = new builder.IntentDialog();
bot.dialog('/', intents);

intents.matches(/^echo/i, [
    function (session) {
        builder.Prompts.text(session, "What would you like me to say?");
    },
    function (session, results) {
        session.send("Ok... %s", results.response);
    }
]);
{% endhighlight %}

A simple closure that behaves as 1 step waterfall: 

{% highlight JavaScript %}
var intents = new builder.IntentDialog();
bot.dialog('/', intents);

intents.matches(/^version/i, function (session) {
    session.send('Bot version 1.2');
});
{% endhighlight %}

A [DialogAction](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.#dialogaction) that can provide a shortcut for implementing simpler closures:

{% highlight JavaScript %}
var intents = new builder.IntentDialog();
bot.dialog('/', intents);

intents.matches(/^version/i, builder.DialogAction.send('Bot version 1.2'));
{% endhighlight %}

Or the ID of a dialog to redirect to:

{% highlight JavaScript %}
bot.dialog('/', new builder.IntentDialog()
    .matches(/^add/i, '/addTask')
    .matches(/^change/i, '/changeTask')
    .matches(/^delete/i, '/deleteTask')
    .onDefault(builder.DialogAction.send("I'm sorry. I didn't understand."))
);
{% endhighlight %}

## Intent Recognizers
The IntentDialog class can be configured to use cloud based intent recognition services like [LUIS](http://luis.ai) through an extensible set of [recognizer](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.iintnetrecognizer) plugins.  Out of the box, Bot Builder comes with a [LuisRecognizer](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.luisrecognizer) that can be used to call a machine learning model you’ve trained via their web site.  You can create a LuisRecognizer that’s pointed at your model and then pass that recognizer into your IntentDialog at creation time using the [options](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.iintentdialogoptions) structure. 

{% highlight JavaScript %}
var recognizer = new builder.LuisRecognizer('<your models url>');
var intents = new builder.IntentDialog({ recognizers: [recognizer] });
bot.dialog('/', intents);

intents.matches('Help', '/help');
{% endhighlight %}

Intent recognizers return matches as named intents. To match against an intent from a recognizer you pass the name of the intent you want to handle to [IntentDialog.matches()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.intentdialog#matches) as a _string_ instead of a _RegExp_. This lets you mix in the matching of regular expressions alongside your cloud based recognition model. To improve performance, regular expressions are always evaluated before cloud based recognizer(s) and an exact match regular expression will avoid calling the cloud based recognizer(s) all together.

You can together multiple LUIS models by passing in an array of recognizers.  You can control the order in which the recognizers are evaluated using the [recognizeOrder](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.iintentdialogoptions#recognizeorder) option.  By default the recognizers will be evaluated in parallel and the recognizer returning the intent with the highest [score](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.iintent#score) will be matched.  You can change the recognize order to series and the recognizers will be evaluated in series. Any recognizer that returns an intent with a score of 1.0 will prevent the recognizers after it from being evaluated.

> __NOTE:__ you should avoid adding a matches() handler for LUIS’s “None” intent. Add a [onDefault()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.intentdialog#ondefault) handler instead.  The reason for this is that a LUIS model will often return a very high score for the None intent if it doesn’t understand the users utterance. In the scenario where you’ve configured the IntentDialog with multiple recognizers that could cause the None intent to win out over a non-None intent from a different model that had a slightly lower score. Because of this the LuisRecognizer class suppresses the None intent all together. If you explicitly register a handler for “None” it will never be matched. The onDefault() handler, however can achieve the same effect because it essentially gets triggered when all of the models reported a top intent of “None”.

## Entity Recognition
LUIS can not only identify a users intention given an utterance, it can extract entities from their utterance as well.  Any entities recognized in the users utterance will be passed to the intent handler via its [args](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.iintentrecognizerresult) parameter. Bot Builder includes an [EntityRecognizer](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.entityrecognizer.html) class to simplify working with these entities. 

### Finding Entities
You can use [EntityRecognizer.findEntity()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.entityrecognizer.html#findentity) and [EntityRecognizer.findAllEntities()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.entityrecognizer.html#findallentities) to search for entities of a specific type by name.

{% highlight JavaScript %}
var recognizer = new builder.LuisRecognizer('<your models url>');
var intents = new builder.IntentDialog({ recognizers: [recognizer] });
bot.dialog('/', intents);

intents.matches('AddTask', [
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
LUIS has a powerful builtin.datetime entity recognizer that can recognize a wide range of relative & absolute dates expressed using natural language. The issue is that when LUIS returns the dates & times it recognized, it does so by returning their component parts. So if the user says “june 5th at 9am” it will return separate entities for the date & time components of the utterance. These entities need to be combined using [EntityRecognizer.resolveTime()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.entityrecognizer.html#resolvetime) to get the actual resolved date. This method will try to convert an array of entities a valid JavaScript Date object. If it can’t resolve the entities to a valid Date it will return null.

{% highlight JavaScript %}
var recognizer = new builder.LuisRecognizer('<your models url>');
var intents = new builder.IntentDialog({ recognizers: [recognizer] });
bot.dialog('/', intents);

intents.matches('SetAlarm', [
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
Bot Builder includes a powerful [choice() prompt](/en-us/node/builder/dialogs/Prompts/#promptschoice) which lets you present a list of choices to a user for them to pick from. LUIS makes it easy to map a users choice to a named entity but it doesn’t do any validation that the user entered a valid choice. You can use [EntityRecognizer.findBestMatch()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.entityrecognizer.html#findbestmatch) and [EntityRecognizer.findAllMatches()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.entityrecognizer.html#findallmatches) to verify that the user entered a valid choice. These methods are the same methods used by the choice() prompt and offer a lot of flexibility when matching a users utterance to a value in a list.

List items can be matched using a case insensitive exact match so given the list [“Red”,”Green”,”Blue”] the user can say “red” to match the “Red” item. Using a partial match where the user says “blu” to match the “Blue” item. Or a reverse partial match where the user says “the green one” to match the “Green” item.  Internally the match functions calculate a coverage score when evaluating partial matches. For the “blu” utterance that matched the “Blue” item the coverage score would have been 0.75 and for the “the green one” utterance that matched “green” the coverage score would have been 0.88. The minimum score needed to trigger a match is 0.6 but this can be adjusted for each match.

An [IFindMatchResult](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ifindmatchresult.html) is returned for each match and contains the [entity](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ifindmatchresult.html#entity), [index](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ifindmatchresult.html#index), and [score](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ifindmatchresult.html#score) for the list item that was watched.

{% highlight JavaScript %}
var recognizer = new builder.LuisRecognizer('<your models url>');
var intents = new builder.IntentDialog({ recognizers: [recognizer] });
bot.dialog('/', intents);

intents.matches('DeleteTask', [
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
The IntentDialog lets you register an [onBegin](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.intentdialog.html#onbegin) handler that will be notified anytime the dialog is first loaded for a conversation and an [onDefault](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.intentdialog.html#ondefault) handler that will be notified anytime the users utterance failed to match one of the registered patterns.

The onBegin handler is invoked when [session.beginDialog()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session.html#begindialog) has been called for the dialog and gives the dialog an opportunity to process optional arguments passed in the call to beginDialog().  The handler is passed a `next()` function which should be invoked to continue executing the dialogs default logic. 

{% highlight JavaScript %}
intents.onBegin(function (session, args, next) {
    session.dialogData.name = args.name;
    session.send("Hi %s...", args.name);
    next();
});
{% endhighlight %}

The onDefault handler is invoked anytime the users utterance doesn’t match one of the registered patterns. The handler can be a waterfall, closure, DialogAction, or the ID of a dialog to redirect to.

{% highlight JavaScript %}
intents.onDefault(builder.DialogAction.send("I'm sorry. I didn't understand."));
{% endhighlight %}

The onDefault handler can also be used to manually process intents as it gets passed all of the raw recognizer results via its [args]( /en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.iintentrecognizerresult) parameter.
