---
layout: page
title: Bot Framework Emulator
permalink: /connector/tools/bot-framework-emulator/
weight: 240
parent1: Bot Connector
parent2: Tools
---

* TOC
{:toc}

## What is the Bot Framework Emulator?
The Bot Framework provides an emulator that lets you test calls to your Bot as if it were being called by the Bot Framework cloud service. 

Using the Emulator, you can:

* Send requests and receive responses to/from your bot endpoint on localhost
* Inspect the Json response
* Emulate a specific user and/or conversation

## Installation and configuration
* [Install the Emulator](http://aka.ms/bf-bc-emulator)
* Launch the Emulator
* Copy the AppId and AppSecret from the Web.config of your Bot app
![Configure the Bot Framework](/images/emulator-configure.png)
* Enter the localhost REST endpoint of your bot
![Enter the localhost REST endpoint of your bot](/images/emulator-url.png)


## Using the Emulator to test your bot
Let's use the [Echobot Sample](http://github.com/Microsoft/BotBuilder) for this section. 

* Run the Echobot sample in debug mode
* Enter the localhost REST endpoint in the url field of the Bot framework Emulator
* By default you will be in the Chat tab
* Enter some text e.g. "Hello World!" and hit Send (or the Enter key)
![Send message to bot](/images/emulator-helloworld.png)
* The Emulator will send the request to your bot, and will display the response below your initial message
![Emulator response](/images/emulator-response.png)
* Switch to the Debug tab and inspect the full Json response
![Inspect the Json response](/images/emulator-json.png)

See an example of use in the [Getting started page](/connector/getstarted/).