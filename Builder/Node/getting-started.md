---
layout: page
title: Getting Started
permalink: /builder/node/overview/
weight: 600
parent1: Bot Builder for Node.js
---

## What is Bot Builder for Node.js and why should I use it?
Bot Builder for Node.js is targeted at Node.js developers creating new bots from scratch. By building your bot using the Bot Builder framework you can easily adapt it to run on nearly any communication platform. This gives your bot the flexibility to be wherever your users are.

* [Bot Builder for Node.js Reference](/sdkreference/nodejs/modules/_botbuilder_d_.html)
* [Bot Builder on GitHub](https://github.com/Microsoft/BotBuilder)

## Install
Get the BotBuilder module using npm.

    npm install --save botbuilder

## Build a bot
Say hello in a few lines of code.
 
{% highlight JavaScript %}
var restify = require('restify');
var builder = require('botbuilder');

var server = restify.createServer();

var helloBot = new builder.TextBot();
helloBot.add('/', function (session) {
    session.send('Hello World');
});

server.post('/v1/messages', helloBot.listen());

server.listen(8080, function () {
    console.log('%s listening to %s', server.name, server.url); 
});
{% endhighlight %}

## Test your bot
Use the Bot Framework Emulator to test your bot on localhost (Windows Only)

* Download it [here](http://aka.ms/bf-bc-emulator)
* Learn how to use it [here](/botframework/bot-framework-emulator/)

## Publish your bot
Deploy your bot to the cloud and then register it with the Microsoft Bot Framework.

* [Publishing a bot to Microsoft Azure](/connector/getstarted/#publishing-your-bot-application-to-microsoft-azure)
* [Registering a bot with the Microsoft Bot Framework](/connector/getstarted/#registering-your-bot-with-the-microsoft-bot-framework)

## Dive deeper
Learn how to build great bots.

* [Core Concepts](/builder/node/guides/core-concepts/)
