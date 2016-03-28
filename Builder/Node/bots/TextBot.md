---
layout: page
title: TextBot
permalink: /builder/node/bots/TextBot/
weight: 621
parent1: Bot Builder for Node.js
parent2: Bots
---

* TOC
{:toc}

## Overview
The [TextBot](/sdkreference/nodejs/classes/_botbuilder_d_.textbot.html) class can be used to build a bot that runs from a console window and also lets you easily adapt your bot to run on other bot platforms.

## Console Bot
The [TextBot.listenStdin()](/sdkreference/nodejs/classes/_botbuilder_d_.textbot.html#listenstdin) method can be used to start the TextBot listening to the console for input. 

{% highlight JavaScript %}
var builder = require('botbuilder');

var bot = new builder.TextBot();
bot.add('/', function (session) {
   session.send('Hello World'); 
});

bot.listenStdin();
{% endhighlight %}


## Platform Adaptor
To use the TextBot as an adaptor for other platforms you’ll want to use the [TextBot.processMessage()](/sdkreference/nodejs/classes/_botbuilder_d_.textbot.html#processmessage) method to pass your bot messages received from the bot platform. There are then 2 ways of returning your bots replies to the user, you can use the TextBot in either a purely event driven mode (preferred) or in mixed mode where you pass a callback to the TextBot.processMessage() method and also listen for events. In this second mode the first reply or error will be returned via the callback and any additional replies will be delivered as events. Should you decide to ignore the events just be aware that any additional replies from the bot will be lost.

The [TextBot.listenStdin()](/sdkreference/nodejs/classes/_botbuilder_d_.textbot.html#listenstdin) method provides a good example of the preferred way to use the TextBot. 

{% highlight JavaScript %}
function listenStdin() {
    var _this = this;
    function onMessage(message) {
        console.log(message.text);
    }
    this.on('reply', onMessage);
    this.on('send', onMessage);
    this.on('quit', function () {
        rl.close();
        process.exit();
    });
    var rl = readline.createInterface({ input: process.stdin, output: process.stdout, terminal: false });
    rl.on('line', function (line) {
        _this.processMessage({ text: line || '' });
    });
}
{% endhighlight %}
