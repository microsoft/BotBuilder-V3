---
layout: page
title: CommandDialog
permalink: /builder/node/dialogs/CommandDialog/
weight: 632
parent1: Bot Builder for Node.js
parent2: Dialogs
---

* TOC
{:toc}

## Overview
The [CommandDialog](/sdkreference/nodejs/classes/_botbuilder_d_.commanddialog.html) class lets you listen for the user to say a specific keyword or phrase. It’s particular useful for building /command style bots.

## Matching Patterns
The [CommandDialog.matches()](/sdkreference/nodejs/classes/_botbuilder_d_.commanddialog.html#matches) method lets you trigger a handler based on the users utterance matching one or more regular expressions. The handler itself can take a variety of forms.

A simple closure. This function will be invoked both when the handler is initially triggered and again when a child dialog started by the handler returns.

{% highlight JavaScript %}
var dialog = new builder.CommandDialog();
bot.add('/', dialog);

dialog.matches('^version', function (session) {
    session.send('Bot version 1.2');
});
{% endhighlight %}

A [DialogAction](/builder/node/dialogs/Prompts/#dialog-actions). Shortcuts for implementing the above closure.

{% highlight JavaScript %}
var dialog = new builder.CommandDialog();
bot.add('/', dialog);

dialog.matches('^version', builder.DialogAction.send('Bot version 1.2'));
{% endhighlight %}

A waterfall when you need to collect input from the user.

{% highlight JavaScript %}
var dialog = new builder.CommandDialog();
bot.add('/', dialog);

dialog.matches('^echo', [
    function (session) {
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

The ID of a dialog to redirect to and optional arguments to pass to that dialog. It's often easiest to think of the CommandDialog as a switch that simply redirects to another dialog when a pattern is matched so we support a shorthand way of expressing that.

{% highlight JavaScript %}
bot.add('/', new builder.CommandDialog()
    .matches('^add', '/addTask')
    .matches('^change', '/changeTask')
    .matches('^delete', '/deleteTask')
    .onDefault(builder.DialogAction.send("I'm sorry. I didn't understand."))
);
{% endhighlight %}

## onBegin & onDefault Handlers
The CommandDialog lets you register an [onBegin](/sdkreference/nodejs/classes/_botbuilder_d_.commanddialog.html#onbegin) handler that will be notified anytime the dialog is first loaded for a conversation and an [onDefault](/sdkreference/nodejs/classes/_botbuilder_d_.commanddialog.html#ondefault) handler that will be notified anytime the users utterance failed to match one of the registered patterns.

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

The onDefault handler is invoked anytime the users utterance doesn’t match one of the registered patterns. The handler can be a closure, DialogAction, waterfall, or dialog redirect.

{% highlight JavaScript %}
dialog.onDefault(builder.DialogAction.send("I'm sorry. I didn't understand."));
{% endhighlight %}
