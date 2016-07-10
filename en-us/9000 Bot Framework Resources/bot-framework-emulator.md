---
layout: page
title: Bot Framework Emulator
permalink: /en-us/tools/bot-framework-emulator/
weight: 9250
parent1: Bot Framework Resources
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
* [Install the Windows Emulator](https://aka.ms/bf-bc-emulator)
* Launch the Emulator
* Copy the MicrosoftAppId and MicrosoftAppSecret from the Web.config of your Bot app
![Configure the Bot Framework](/en-us/images/emulator/emulator-configure.png)
* Enter the localhost REST endpoint of your bot
![Enter the localhost REST endpoint of your bot](/en-us/images/emulator/emulator-url.png)

## Using the Emulator to test your bot
Let's use the [Echobot Sample](http://github.com/Microsoft/BotBuilder) for this section. 

* Run the Echobot sample in debug mode
* Enter the localhost REST endpoint in the url field of the Bot framework Emulator
* By default you will be in the Chat tab
* Enter some text e.g. "Hello World!" and hit Send (or the Enter key)
![Send message to bot](/en-us/images/emulator/emulator-helloworld.png)
{% comment %} 
* The Emulator will send the request to your bot, and will display the response below your initial message
![Emulator response](/en-us/images/emulator/emulator-response.png)
* Switch to the Debug tab and inspect the full Json response
![Inspect the Json response](/en-us/images/emulator/emulator-json.png)
{% endcomment %}
 
See an example of use in the [Getting started page](/en-us/csharp/builder/sdkreference/gettingstarted.html).

## Using the Emulator with Ngrok to interact with your bot in the cloud
You can also use the emulator to talk with your bot deployed in the cloud.  Doing this allows you to see JSON back and forth, and also the RAW
error messages that are hidden from the end user in normal chat.

In the V3 version of the Bot Framework API, the authentication model has changed from Basic Auth to Open Id with JWT tokens and Microsoft Account. 
Doing auth in this way introduces the additional requirement of being able to get auth callbacks to the Emulator from the Internet.  Conveniently
ngrok (https://ngrok.com/) provides an easy way to do this for debugging/diagnosis purposes.

Download ngrok from the site; and run it from a command prompt:

{% highlight csharp %}
    ngrok http -host-header=rewrite 9000
{% endhighlight %}

![Getting ngrok running](/en-us/images/emulator/emulator-ngrok-config.png)

This will set up a temporary open port 9000 that will be redirected from an ngrok location out on the web. The forwarding https URL is the 
piece we have to care about for now.

Now open up the Bot Framework Channel Emulator and fill in the fields at the top:

| field | value |
|-------|-------|
| Local port | 9000, or whatever was specified in the ngrok command |
| Emulator URL | the ngrok forwarding URL (with https) |
| Bot ID | Your bot ID from the bot framework portal |
| Microsoft App ID | Your Microsoft App ID, easiest to find in your Bot's web.config |
| Microsoft App Password | Your Microsoft App Password, easiest to find in your Bot's web.config |

Once populated, the emulator will look a little like this:

![Emulator configured for cloud debugging](/en-us/images/emulator/emulator-testbot-cloud-config.png)

In this configuration, ngrok also offers some lovely communication monitoring by opening a browser to the port http://127.0.0.1:4040.

## Mac and Linux support using command line emulator
For folks who are developing on Mac and Linux we have created a console only version which works using mono. 

To install

1. Download [Console Emulator Zip](https://aka.ms/bfemulator)
2. Unzip it
3. Install [Mono](http://www.mono-project.com/download/#download-mac)
4. mono BFEmulator.exe

{% comment %}adding tabs for codeblocks after a list, embeds them in the last list item. adding &nbsp; to break it.{% endcomment %}
&nbsp;

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
