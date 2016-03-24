---
layout: page
title: Understanding Natural Language
permalink: /builder/node/understanding-natural-language/
weight: 618
parent1: Bot Builder for Node.js
---

* TOC
{:toc}

## LUIS
Microsofts [Language Understanding Intelligent Service (LUIS)](http://luis.ai) offers a fast and effective way of adding language understanding to applications. With LUIS, you can use pre-existing, world-class, pre-built models from Bing and Cortana whenever they suit your purposes -- and when you need specialized models, LUIS guides you through the process of quickly building them. 

LUIS draws on technology for interactive machine learning and language understanding from [Microsoft Research](http://research.microsoft.com/en-us/) and Bing, including Microsoft Research's Platform for Interactive Concept Learning (PICL). LUIS is a part of project of Microsoft [Project Oxford](https://www.projectoxford.ai/). 

Bot Builder lets you use LUIS to add natural language understanding to your bot via the LuisDialog class. You can add an instance of a LuisDialog that references your published language model and then add intent handlers to take actions in response to users utterances.  To see LUIS in action watch the 10 minute tutorial below.

* [Microsoft LUIS Tutorial](https://vimeo.com/145499419) (video)

## Intents, Entities, and Model Training
One of the key problems in human-computer interactions is the ability of the computer to understand what a person wants, and to find the pieces of information that are relevant to their intent. For example, in a news-browsing app, you might say "Get news about virtual reality companies," in which case there is the intention to FindNews, and "virtual reality companies" is the topic. LUIS is designed to enable you to very quickly deploy an http endpoint that will take the sentences you send it, and interpret them in terms of the intention they convey, and the key entities like "virtual reality companies" that are present. LUIS lets you custom design the set of intentions and entities that are relevant to the application, and then guides you through the process of building a language understanding system. 

Once your application is deployed and traffic starts to flow into the system, LUIS uses active learning to improve itself. In the active learning process, LUIS identifies the interactions that it is relatively unsure of, and asks you to label them according to intent and entities. This has tremendous advantages: LUIS knows what it is unsure of, and asks you to help where you will provide the maximum improvement in system performance. Secondly, by focusing on the important cases, LUIS learns as quickly as possible, and takes the minimum amount of your time. 

## Create Your Model
The first step of adding natural language support to your bot is to create your LUIS Model. You do this by logging into [LUIS](http://luis.ai) and creating a new LUIS Application for your bot. This application is what you’ll use to add the Intents & Entities that LUIS will use to train your bots Model.

![Create LUIS Application](/images/builder-luis-create-app.png)

In addition to creating a new app you have the option of either importing an existing model (this is what you'll do when working with the Bot Builder examples that use LUIS) or using the prebuilt Cortana app.  For the purposes of this tutorial we'll create a bot based on the prebuilt Cortana app. 
When you select the prebuilt Cortana app for English you’ll see a dialog like below. 

You’ll want to copy the URL listed on the dialog as this is what you’ll bind your LuisDialog class to.  This URL points to the Model that LUIS published for your bots LUIS app and will be stable for the lifetime of the app. So once you’ve trained and published a model for a LUIS app you can update and re-train the model all you want without having to even redeploy your bot.  This is very handy in the early stages of building a bot as you’ll be re-training your model a lot.

![Prebuilt Cortana Application](/images/builder-luis-default-app.png)

## Handle Intents
Once you've deployed a model for your LUIS app we can create a bot that consumes that model. To keep things simple we'll create a TextBot that we can interact with from a console window.

{% highlight JavaScript %}
var builder = require('botbuilder');

// Create LUIS Dialog that points at our model and add it as the root '/' dialog for our Cortana Bot.
var dialog = new builder.LuisDialog('https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=fe054e042fd14754a83f0a205f6552a5&q= ');
var cortanaBot = new builder.TextBot();
cortanaBot.add('/', dialog);

// Add intent handlers
dialog.on('builtin.intent.alarm.alarm_other', builder.DialogAction.send('Changing Alarm');
dialog.on('builtin.intent.alarm.delete_alarm', builder.DialogAction.send('Deleting Alarm');
dialog.on('builtin.intent.alarm.set_alarm', builder.DialogAction.send('Creating Alarm');
dialog.on('builtin.intent.alarm.snooze', builder.DialogAction.send('Snoozing Alarm');
dialog.onDefault(builder.DialogAction.send("I'm sorry I didn't understand. I only support alarm operations");

cortanaBot.listenStdin();

{% endhighlight %}