---
layout: page
title: What's new or changed in v1.x
permalink: /builder/node/whats-new/
weight: 601
parent1: Bot Builder for Node.js
---

* TOC
{:toc}

## Overview
With the 1.x release of Bot Builder for Node.js we're providing developers with what we believe is a reasonably stable version of the library to use for building bots across a range of channels. Over the last few months we’ve addressed several bugs identified by the community and have continued to add new features including:

* Support for showing buttons on channels that support buttons. The built-in `choice()` and `confirm()` prompts will show buttons by default  when appropriate.
* A new `attachment()` prompt for asking the user to upload a file.
* A new `Message` builder class to simplify building replies with attachments and random prompts.
* A new `validatedPrompt()` dialog action to simplify creating custom prompts with validation logic.

Along with all of the features we’ve added, version 1.x does include a breaking change which may impact a small number of bots. I would encourage all developers to read the [Waterfall Changes](#waterfallchanges) to see if their bot is impacted by these changes.

Moving forward we will continue to address bugs identified by the community and listen to your feedback as we work towards a 2.x release of the library.  We will also be adding new features that we believe will simplify bot development even further.  Here’s a couple of things we’re thinking about for future releases:

* A new __UniversalBot__ class that simplifies creating a single bot that runs across multiple channels. This bot will likey replace all of the various channel specific bots (i.e. `TextBot` and `BotConnectorBot`) the library uses today so developer should prepare for another breaking change in v2.x.
* __Form Flow__ from the C# version of the library will be coming to the Node version of the library very soon. I've already begun work on this but based on internal feedback will be re-designing things a bit before releasing it.
* A new __Custom Prompts__ system to replace the current validated prompts. This new prompt system will form the under pinning’s of Form Flow and will let you create custom prompts that are more powerful then what you can create with validated prompts today.  It will also let you replace the built-in prompts with custom implementations making it easier to adapt the system to support other languages or to switch to using a cloud based recognizer like LUIS.
* Re-write of the __LuisDialog__ class to support the latest features of LUIS and add new features like the ability to chain across multiple LUIS models.
* A whole bunch of new dialog types and higher level constructs to simplify building rich bots. :)

## Waterfall Changes
Over the past several months we've observed how developers use waterfalls to build bots and while they generally get the model there are a couple of things that are continually tripping people up. The 1.0 version of the library makes two key changes to waterfalls that we hope will address some of these issues. In short the changes make waterfalls work the way you probably already assumed they work but they are breaking changes so it’s probably worth reviewing the changes to see if they impact your bot.

### Simple closure based handlers are now waterfalls
When you register a new dialog with the system or add an intent handler you have a choice of either providing a simple closure or specifying a waterfall by providing an array of closures.  Some developers have been making the mistake of assuming that when they provide a simple closure for their handler it works pretty much the same way as a waterfall but prior to 1.x that wasn’t cases.  

The difference is subtle but if you call another dialog like a built-in prompt from within a waterfall any result returned from that dialog/prompt is passed to the next step of the waterfall for processing. When you call another dialog or prompt from a simple closure there is no next waterfall step so the result is passed back to your closure by calling it a second time with different parameters.  This behavior was tripping a lot of people up because if you don’t protect against that second call you can easily get into an infinite loop where you just keep re-starting the child dialog when it returns.

In version 1.0 we’ve changed things such that when you specify a simple closure for your handler its essentially the same as specifying a waterfall with only one step.  This avoids the infinite loop problem because when the child dialog/prompt returns control will pass to the next step of the waterfall. In this case since there isn’t a next step the dialog will simply end thanks to the other change we’ve made to waterfalls. 

In most cases the changes needed to fix a bot which did protect against the double callback issue are minor. A bot with a handler like this:

{% highlight JavaScript %}
bot.add('/', function (session) {
    if (!session.userData.name) {
        session.beginDialog('/profile');
    } else {
        session.send('Hello %s!', session.userData.name);
    }
});
{% endhighlight %}

becomes:

{% highlight JavaScript %}
bot.add('/', [
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
{% endhighlight %}

There are times where you may still want the old behavior so to cover those cases we’ve exposed a new `SimpleDialog` class which we were using internally to implement the old behavior. You can simply wrap your handler with this class to keep the old behavior:

{% highlight JavaScript %}
bot.add('/', new builder.SimpleDialog(function (session) {
    if (!session.userData.name) {
        session.beginDialog('/profile');
    } else {
        session.send('Hello %s!', session.userData.name);
    }
}));
{% endhighlight %}

### Waterfalls now a have a phantom last step that ends the dialog
The other issue we have been seeing a lot is that developers call a nested series of dialogs and end up getting stuck somewhere within a dialog that's in the middle of the stack.  The reason for that seems to be a built in assumption that child dialogs will automatically end themselves which before 1.0 they did not. Now they do. So another example:

{% highlight JavaScript %}
bot.add('/', [
    function (session) {
        session.beginDialog('/theGuide');
    },
    function (session, results) {
        session.send("The meaning of life is %d", results.response); 
    }
]);
bot.add('/theGuide', [
    function (session) {
        session.beginDialog('/meaningOfLife');
    }
]);
bot.add('/meaningOfLife', [
    function (session) {
        session.endDialog({ response: 42 });
    }
]);
{% endhighlight %}

Most developers expectations from this sequence of dialog calls is that the bot should print out "The meaning of life is 42" but prior to 1.0 it wouldn't have printed anything. The reason for that is the calls to `beginDialog()` work as expected and when we make it to the '/meaningOfLife' dialog we return 42 as the answer. But since '/theGuide' dialog doesn't contain an explicit next step to return the answer recieved from '/meaningOfLife' you're stuck. 

With 1.0 there's now a phantom next step which calls `endDialog()` automatically so this flow now works as expected and prints "The meaning of life is 42" as expected.
    