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
* [Install the Windows Emulator](http://aka.ms/bf-bc-emulator)
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
{% comment %} 
* The Emulator will send the request to your bot, and will display the response below your initial message
![Emulator response](/images/emulator-response.png)
* Switch to the Debug tab and inspect the full Json response
![Inspect the Json response](/images/emulator-json.png)
{% endcomment %}
 
See an example of use in the [Getting started page](/connector/getstarted/).

## Using ConnectorClient library with emulator
If you need access to the SendMessageAsync API from the connector client you can use localhost:9000 

{% highlight csharp %}
    var connector = new ConnectorClient(new Uri("http://localhost:9000"), new ConnectorClientCredentials());
{% endhighlight %}


## Mac and Linux support using command line emulator
For folks who are developing on Mac and Linux we have created a console only version which works using mono. 

To install

1. Download [Console Emulator Zip](http://aka.ms/bfemulator)
2. Unzip it
3. Install [Mono](http://www.mono-project.com/download/#download-mac)
4. mono BFEmulator.exe 

{% highlight vctreestatus %}
Microsoft Framework Emulator

/exit or /quit to exit
/settings to change endpoint, appId and appSecret settings
/dump [#] to show contents of last # messages (default: 1)
/attachment [path] <- to add a file to your message
Current settings:
Endpoint: http://localhost:8002/api/messages
AppId: TestBot
AppSecret: 12345678901234567890123456789012
           

> hello
Cookie:1 User:1 Conversation:1 PerUser:1 You said:hello


> /dump

TestBot said:
Cookie:1 User:1 Conversation:1 PerUser:1 You said:hello
==== raw BOT Content ====
{
  "type": "Message",
  "conversationId": "8a684db8",
  "language": "en",
  "text": "Cookie:1 User:1 Conversation:1 PerUser:1 You said:hello",
  "from": {
    "name": "TestBot",
    "channelId": "emulator",
    "address": "TestBot",
    "isBot": true
  },
  "to": {
    "name": "User1",
    "channelId": "emulator",
    "address": "User1",
    "isBot": false
  },
  "replyToMessageId": "8c0aa5205b374a6d8f58145e4dec041b",
  "participants": [
    {
      "name": "User1",
      "channelId": "emulator",
      "address": "User1"
    },
    {
      "name": "TestBot",
      "channelId": "emulator",
      "address": "TestBot"
    }
  ],
  "totalParticipants": 2,
  "channelMessageId": "cc693595f3e046ecb9c02bda9de603c0",
  "channelConversationId": "Conv1",
  "botUserData": {
    "counter": 1
  },
  "botConversationData": {
    "counter": 1
  },
  "botPerUserInConversationData": {
    "counter": 1
  }
}
{% endhighlight %}




