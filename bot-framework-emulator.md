---
layout: page
title: Bot Framework Emulator
permalink: /bot-framework-emulator/
weight: 40
parent: Bot Connector SDK
---

* TOC
{:toc}

## What is the Bot Framework Emulator?
The Bot Framework Emulator is a tool that allows you to test your bot on localhost while you are developing it.

The tool allows you to:

* Send requests and receive responses to/from your bot endpoint on localhost
* Inspect the Json response
* Emulate a specific user

## Installation and configuration
* Install the Emulator from [github or ?]()
* Launch the Emulator
* Copy the AppId and AppSecret from the Web.config of your Bot app
![Configure the Bot Framework](../emulator-configure.jpg)
* Enter the localhost REST endpoint of your bot
![Enter the localhost REST endpoint of your bot](../emulator-url.jpg)


## Using the Emulator to test your bot
Let's use the [Echobot Sample](http://github.com/Microsoft/BotBuilder) for this section. 

* Run the Echobot sample in debug mode
* Enter the localhost REST endpoint in the url field of the Bot framework Emulator
* By default you will be in the Chat tab
* Enter some text e.g. "Hello World!" and hit Send (or the Enter key)
![Send message to bot](../emulator-helloworld.jpg)
* The Emulator will send the request to your bot, and will display the response below your initial message
![Emulator response](../emulator-response.jpg)
* Switch to the Debug tab and inspect the full Json response
![Inspect the Json response](../emulator-json.jpg)


Happy coding!
