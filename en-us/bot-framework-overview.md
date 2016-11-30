---
layout: page
title: Bot Framework Overview
permalink: /en-us/
weight: 100
parent1: none
---



Microsoft Bot Framework is a comprehensive offering that you use to build and deploy high quality bots for your users to enjoy wherever they are talking. The framework consists of the Bot Builder SDK, Bot Connector, Developer Portal, and Bot Directory. There's also an emulator that you can use to test your bot.

A bot is a web service that interacts with users in a conversational format. Users start conversations with your bot from any channel that you've configured your bot to work on (for example, Text/SMS, Skype, Slack, Facebook Messenger, and other popular services). You can design conversations to be freeform, natural language interactions or more guided ones where you provide the user choices or actions. The conversation can utilize simple text strings or something more complex such as rich cards that contain text, images, and action buttons.
 
The following conversation shows a bot that schedules salon appointments. The bot understands the user's intent, presents appointment options using action buttons, displays the user's selection when they tap an appointment, and then sends a thumbnail card that contains the appointment's specifics. 
 
![salon bot example](/en-us/images/connector/salon_bot_example.png)

To build your bot, the Framework provides a [.NET SDK](/en-us/csharp/builder/sdkreference/) and [Node.js SDK](/en-us/node/builder/overview/). These SDKs provide features such as dialogs and built-in prompts that make interacting with users much simpler. For those using other languages, see the framework’s [REST API](/en-us/connector/overview/). The Bot Builder SDK is provided as open source on GitHub (see [BotBuilder](https://github.com/Microsoft/BotBuilder)).

To give your bot more human-like senses, you can incorporate LUIS for natural language understanding, Cortana for voice, and the Bing APIs for search. For more information about adding intelligence to your bot, see [Bot Intelligence](/en-us/bot-intelligence/getting-started/).

When you finish writing your bot, you need to register it, connect it to channels, and finally publish it. [Registering your bot](https://dev.botframework.com/bots/new){:target="_blank"} describes it to the framework, and it's how you get the bot's app ID and password that's used for authentication. Bots that you register are located at [My bots]( https://dev.botframework.com/bots){:target="_blank"} in the portal. 

After registering your bot, you need to configure it to work on channels that your users use. The configuration process is unique per channel, and some channels are preconfigured for you (for example, Skype and Web Chat). For information about configuring channels, see [Configuring Channels](/en-us/csharp/builder/sdkreference/gettingstarted.html). The framework also provides the [Direct Line](/en-us/restapi/directline/) REST API, which you can use to host your bot in an app or website.

For most channels, you can share your bot with users as soon as you configure the channel. If you configured your bot to work with Skype, you must publish your bot to the Bot Directory and Skype apps (see [Publishing your bot](/en-us/directory/publishing/)) before users can start using it. Although Skype is the only channel that requires you to publish your bot to the directory, you are encouraged to always publish your bot because it makes it more discoverable. Publishing the bot submits it for review. For information about the review process, see [Bot review guidelines](/en-us/directory/review-guidelines/). If your bot passes review, it’s added to the [Bot Directory](https://bots.botframework.com/){:target="_blank"}. The directory is a public directory of all bots that were registered and published with Microsoft Bot Framework. Users can select your bot in the directory and add it to one or more of the configured channels that they use.

|**Section**|**Description**
|[General FAQ](/en-us/faq/)<br/><br/>[Technical FAQ](/en-us/technical-faq/)|Contains frequently asked questions about the framework.
|[Support](/en-us/support/)|Provides a list of resources where you can get help resolving issues that you have with using the framework.
|[Downloads](/en-us/downloads/)|Provides the locations where you can download the .NET and Node.js SDKs from.
|[Samples](https://github.com/Microsoft/BotBuilder-Samples){:target="_blank"}|Provides the locations where you can get the C# and Node.js code samples from.
|[Emulator](https://github.com/Microsoft/BotFramework-Emulator){:target="_blank"}|Provides details about getting and using the framework’s emulator, which you can use to test your bot.
|[User Experience Guidelines](/en-us/directory/best-practices/)|Provides guidelines and best practices for crafting a bot that provides the best experience for the user.
|[Bot Intelligence](/en-us/bot-intelligence/getting-started/)|Identifies the APIs that you can incorporate into your bot to provide a better user experience. Adding intelligence to your bot can make it seem more human-like and provide a better user experience. For example, the Vision APIs can identify emotions in peoples’ faces or extract information about objects and people in images; the Speech APIs can covert text-to-speech or speech-to-text; the Language APIs can process natural language and detect sentiment; and you can use the Search APIs to find information that may be of interest to the user.
|[Bot Builder for Node.js](/en-us/node/builder/overview/)|Provides details for building bots using the Node.js SDK.
|[Bot Builder for .NET](/en-us/csharp/builder/sdkreference/)|Provides details for building bots using the .NET SDK.

{%comment%}
Add after User Experience Guidelines
|[Channel and Feature Matrix](/en-us/matrix/)|Provides the list of supported framework features by channel.
{%endcomment%}


{% comment %}
**************************************************

Microsoft Bot Framework is a comprehensive offering to build and deploy high quality bots for your users to enjoy in their favorite conversation experiences. Developers writing bots all face the same problems: bots require basic I/O; they must have language and dialog skills; they must be performant, responsive and scalable; and they must connect to users – ideally in any conversation experience and language the user chooses. Bot Framework provides just what you need to build, connect, manage and publish intelligent bots that interact naturally wherever your users are talking – from text/sms to Skype, Slack, Facebook Messenger, Kik, Office 365 mail and other popular services. 

![Bot Framework Overview](/en-us/images/faq-overview/botframework_overview_july.png)

The Bot Framework consists of a number of components including the Bot Builder SDK, Developer Portal and the Bot Directory.

### Bot Builder SDK
{:.no_toc}

The Bot Builder SDK is [an open source SDK hosted on GitHub](https://github.com/Microsoft/BotBuilder) that provides everything you need to build great dialogs within your Node.js-, .NET- or REST API-based bot.

![Bot Builder SDK](/en-us/images/faq-overview/bot_builder_sdk_july.png)

### Bot Framework Developer Portal
{:.no_toc}

The Bot Framework Developer Portal lets you connect your bot(s) seamlessly text/sms to Skype, Slack, Facebook Messenger, Kik, Office 365 mail and other popular services. Simply register your bot, configure desired channels and publish in the Bot Directory. All bots registered with Bot Framework are auto-configured to work with Skype and the Web.

![Developer Portal](/en-us/images/faq-overview/developer_portal_july.png)

### Bot Directory (coming soon)
{:.no_toc}

The Bot Directory is a public directory of all reviewed bots registered through the Developer Portal. Users will be able to discover, try, and add bots to their favorite conversation experiences from the Bot Directory. Initially the Bot Directory will feature bots demonstrated at [Microsoft Build 2016](http://build.microsoft.com/).

![Bot Directory (coming soon)](/en-us/images/faq-overview/bot_directory_july.png)

To learn more about Bot Framework, view [the FAQ or dive in to the rest of the documentation](/en-us/faq/).
{% endcomment %}

