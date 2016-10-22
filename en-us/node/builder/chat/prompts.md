---
layout: page
title: Prompts
permalink: /en-us/node/builder/chat/prompts/
weight: 2623
parent1: Bot Builder for Node.js
parent2: Chat Bots
---

* TOC
{:toc}

## Collecting Input
Bot Builder comes with a number of built-in prompts that can be used to collect input from a user.  

|**Prompt Type**     | **Description**                                   
| -------------------| ---------------------------------------------
|[Prompts.text](#promptstext) | Asks the user to enter a string of text.      
|[Prompts.confirm](#promptsconfirm) | Asks the user to confirm an action.  
|[Prompts.number](#promptsnumber) | Asks the user to enter a number.
|[Prompts.time](#promptstime) | Asks the user for a time or date.
|[Prompts.choice](#promptschoice) | Asks the user to choose from a list of choices.       
|[Prompts.attachment](#promptsattachment) | Asks the user to upload a picture or video.       

These built-in prompts are implemented as a [Dialog](/en-us/node/builder/chat/dialogs/) so they’ll return the users response through a call to [session.endDialogWithresult()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session.html#enddialogwithresult). Any [DialogHandler](/en-us/node/builder/chat/dialogs/#dialog-handlers) can receive the result of a dialog but [waterfalls](/en-us/node/builder/chat/dialogs/#waterfall) tend to be the simplest way to handle a prompt result.  

Prompts return to the caller an [IPromptResult](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ipromptresult.html). The users response will be contained in the [results.response](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ipromptresult.html#reponse) field and may be null. There are a number of reasons for the response to be null. The built-in prompts let the user cancel an action by saying something like ‘cancel’ or ‘nevermind’ which will result in a null response. Or the user may fail to enter a properly formatted response which can also result in a null response. The exact reason can be determined by examining the [ResumeReason](/en-us/node/builder/chat-reference/enums/_botbuilder_d_.resumereason.html) returned in [result.resumed](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ipromptresult.html#resumed).

### Prompts.text()
The [Prompts.text()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.prompts.html#text) method asks the user for a string of text. The users response will be returned as an [IPromptTextResult](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.iprompttextresult.html).

{% highlight JavaScript %}
builder.Prompts.text(session, "What is your name?");
{% endhighlight %}

### Prompts.confirm()
The [Prompts.confirm()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.prompts.html#confirm) method will ask the user to confirm an action with yes/no response. The users response will be returned as an [IPromptConfirmResult](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ipromptconfirmresult.html).

{% highlight JavaScript %}
builder.Prompts.confirm(session, "Are you sure you wish to cancel your order?");
{% endhighlight %}

### Prompts.number()
The [Prompts.number()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.prompts.html#number) method will ask the user to reply with a number. The users response will be returned as an [IPromptNumberResult](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ipromptnumberresult.html).

{% highlight JavaScript %}
builder.Prompts.number(session, "How many would you like to order?");
{% endhighlight %}

### Prompts.time()
The [Prompts.time()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.prompts.html#time) method will ask the user to reply with a time. The users response will be returned as an [IPromptTimeResult](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.iprompttimeresult.html). The framework uses a library called [Chrono](http://wanasit.github.io/pages/chrono/) to parse the users response and supports both relative “in 5 minutes” and non-relative “june 6th at 2pm” type responses.

The [results.response](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.iprompttimeresult.html#response) returned is an [entity](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ientity.html) that can be resolved into a JavaScript Date object using [EntityRecognizer.resolveTime()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.entityrecognizer.html#resolvetime).

{% highlight JavaScript %}
bot.dialog('/createAlarm', [
    function (session) {
        session.dialogData.alarm = {};
        builder.Prompts.text(session, "What would you like to name this alarm?");
    },
    function (session, results, next) {
        if (results.response) {
            session.dialogData.name = results.response;
            builder.Prompts.time(session, "What time would you like to set an alarm for?");
        } else {
            next();
        }
    },
    function (session, results) {
        if (results.response) {
            session.dialogData.time = builder.EntityRecognizer.resolveTime([results.response]);
        }
        
        // Return alarm to caller  
        if (session.dialogData.name && session.dialogData.time) {
            session.endDialogWithResult({ 
                response: { name: session.dialogData.name, time: session.dialogData.time } 
            }); 
        } else {
            session.endDialogWithResult({
                resumed: builder.ResumeReason.notCompleted
            });
        }
    }
]);
{% endhighlight %}

### Prompts.choice()
The [Prompts.choice()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.prompts.html#choice) method asks the user to pick an option from a list. The users response will be returned as an [IPromptChoiceResult](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ipromptchoiceresult.html). The list of choices can be presented to the user in a variety of styles via the [IPromptOptions.listStyle](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ipromptoptions.html#liststyle) property. The user can express their choice by either entering the number of the option or its name. Both full and partial matches of the options name are supported.

The list of choices can be passed to Prompts.choice() in a variety of ways. As a pipe '\|' delimited string.

{% highlight JavaScript %}
builder.Prompts.choice(session, "Which color?", "red|green|blue");
{% endhighlight %}

As an array of strings.

{% highlight JavaScript %}
builder.Prompts.choice(session, "Which color?", ["red","green","blue"]);
{% endhighlight %}

Or as an Object map. When an Object is passed in Objects keys will be used to determine the choices.

{% highlight JavaScript %}
var salesData = {
    "west": {
        units: 200,
        total: "$6,000"
    },
    "central": {
        units: 100,
        total: "$3,000"
    },
    "east": {
        units: 300,
        total: "$9,000"
    }
};

bot.dialog('/', [
    function (session) {
        builder.Prompts.choice(session, "Which region would you like sales for?", salesData); 
    },
    function (session, results) {
        if (results.response) {
            var region = salesData[results.response.entity];
            session.send("We sold %(units)d units for a total of %(total)s.", region); 
        } else {
            session.send("ok");
        }
    }
]);
{% endhighlight %}

### Prompts.attachment()
The [Prompts.attachment()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.prompts.html#attachment) method will ask the user to upload a file attachment like an image or video. The users response will be returned as an [IPromptAttachmentResult](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ipromptattachmentresult.html).

{% highlight JavaScript %}
builder.Prompts.attachment(session, "Upload a picture for me to transform.");
{% endhighlight %}

## Dialog Actions
Dialog actions offer shortcuts to implementing common actions. The [DialogAction](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.dialogaction.html) class provides a set of static methods that return a closure which can be passed to anything that accepts a dialog handler. This includes but is not limited to [UniversalBot.dialog()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.universalbot.html#dialog), [Library.dialog()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.library.html#dialog), [IntentDialog.matches()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.intentdialog.html#matches), and [IntentDialog.onDefault()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.intentdialog.html#ondefault).


|**Action Type**     | **Description**                                   
| -------------------| ---------------------------------------------
|[DialogAction.send](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.dialogaction.html#send) | Sends a static message to the user.      
|[DialogAction.beginDialog](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.dialogaction.html#begindialog) | Passes control of the conversation to a new dialog.  
|[DialogAction.endDialog](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.dialogaction.html#enddialog) | Ends the current dialog.
|[DialogAction.validatedPrompt](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.dialogaction.html#validatedprompt) | Creates a custom prompt by wrapping one of the built-in prompts with a validation routine.
