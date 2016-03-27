---
layout: page
title: Bot Framework FAQ
permalink: /faq/
weight: 150
parent1: none
---


* TOC
{:toc}

## What is the Microsoft Bot Framework?

The Microsoft Bot Framework provides everything you need to build and connect intelligent bots that interact naturally wherever your users are talking, from text/sms to Skype, Slack, Office 365 mail and other popular services.

![Bot Framework Diagram](/images/bot_framework_wht_bkgrnd.png)

Bots (or conversation agents) are rapidly becoming an integral part of one’s digital experience – they are as vital a way for users to interact with a service or application as is a web site or a mobile experience. Developers writing bots all face the same problems: bots require basic I/O; they must have language and dialog skills; and they must connect to users – preferably in any conversation experience and language the user chooses. The Bot Framework provides tools to easily solve these problems and more for developers e.g., automatic translation to more than 30 languages, user and conversation state management, debugging tools, an embeddable web chat control and a way for users to discover, try, and add bots to the conversation experiences they love.

The Bot Framework has a number of components including the Bot Connector, Bot Builder SDK, and the Bot Directory.

### Bot Connector
{:.no_toc}

The Bot Connector lets you connect your bot(s) seamlessly to text/sms, Office 365 mail, Skype, Slack, and other services. Simply register your bot, configure desired channels and publish in the Bot Directory. 

![Bot Connector Diagram](/images/bot_connector_diagram.png)

### Bot Builder SDK
{:.no_toc}

The Bot Builder SDK is [an open source SDK hosted on GitHub](https://github/Microsoft/BotBuilder) that provides everything you need to build great dialogs within your Node.js- or C#-based bot.

### Bot Directory
{:.no_toc}

The Bot Directory is a public directory of all approved bots registered through the Bot Connector. Users will be able to discover, try, and add bots to their favorite conversation experiences from the Bot Directory. Initially the Bot Directory will feature bots demonstrated at [Microsoft Build 2016](http://build.microsoft.com/).

![Bot Directory (coming soon)](/images/bot_directory_mock_comingsoon.png)

## Why should I write a bot?
The Conversational User Interface, or CUI, has arrived. 

A plethora of chit-chat bots are offering to do things for us in our various communication channels like Skype and Twitter. A series of personal agent services have emerged that leverage machines, humans or both to complete tasks for us (x.ai, Clara Labs, Fancy Hands, Task Rabbit, Facebook “M” to name a few). 

The primary interface for these experiences is email, text or voice. Conversation-driven UI now enables us to do everything from grabbing a taxi, to paying the electric bill or sending money to a friend. Offerings such as Siri, Google Now and Cortana demonstrate value to millions of people every day, particularly on mobile devices where the CUI is often superior to the GUI or complements it. 

Bots and conversation agents are rapidly becoming an integral part of one’s digital experience – they are as vital a way for users to interact with a service or application as is a web site or a mobile experience. 

## Why did Microsoft develop the Bot Framework?

While the “CUI is upon us,” at this point few developers have the expertise and tools needed to create new conversational experiences or enable existing applications and services with a conversational interface their users can enjoy. We have created the Bot Framework to make it easier for developers to build and connect great bots to users, wherever they converse.

## Who are the people behind the Bot Framework?

In the spirit of One Microsoft, the Bot Framework is a collaborative effort across many teams, including Microsoft Technology and Research, Microsoft’s Applications and Services Group and Microsoft’s Developer Experience teams.

## When did work begin on the Bot Framework?

The core Bot Framework work has been underway since the summer of 2015, primarily driven by Fuse Labs within Microsoft Technology and Research.

## Is the Bot Framework publicly available now?

The Bot Framework will be released as a preview in conjunction with Microsoft’s annual developer conference [/build](http://build.microsoft.com/).

## How long will the Bot Framework be in preview? Can I start building/shipping products based on a preview framework?

The Bot Framework is currently in preview; expect to see it become generally available by the end of CY 16.

## Who are the target users for the Bot Framework? How will they benefit?

The Bot Framework is targeted at developers who want to create a new service with a great bot interface or enable an existing service with a great bot interface. 

Developers writing a bot all face the same problems: bots require basic I/O, they must have language and dialog skills, and they must connect to users – preferably in any conversation experience and in any language the user chooses. The Bot Framework provides tools to address these problems while also providing a way for users to discover, try, and add bots to the conversation experiences they love via the Bot Directory. 

Additionally, as a participant in the Bot Framework, your bot will also be enabled with automatic translation, user and conversation state management, a web chat control, and debugging tools including the Bot Framework Emulator.

## What does the Bot Connector provide to developers? How does it work?

Bot Connector is the easiest way to achieve broad reach for your text/speech, image, and/or card-capable bot. In addition to enabling broad reach to the conversation experiences your users love, the Bot Connector also provides automatic translation to more than 30 languages, an embeddable web chat control, user and conversation state management, and debugging through the Bot Framework Emulator.

![Bot Details - Develop0er Portal)](/images/connector_channel_config_skype.png)

#### How it works
{:.no_toc}

Bot Connector lets you connect your bot with many different communication experiences. If you write a bot – sometimes called a conversation agent – and expose a Bot Connector-compatible API on the internet (a REST API), Bot Connector will forward messages to your bot from your user, and will send messages back to the user. 
To connect your bot to your users you must have:


* A bot (if you don’t have one, check out the [Bot Builder SDK](http://github/Microsoft/BotBuilder) on Github
* A Microsoft Account, which you will use to register and manage your bot in the Bot Framework
* An internet-accessible REST endpoint exposing the Bot Connector messages API
* Optionally, accounts on one or more communication services where your bot will converse.

#### Register
{:.no_toc}

To register your bot, sign in to the Bot Framework website and provide the requisite details for your bot, including a bot profile image.

Once registered, use the dashboard to test your bot to ensure it is talking to the Bot Connector service and/or use the web chat control, an auto-configured channel, to experience what your users will experience when conversing with your bot.

#### Connect to Channels
{:.no_toc}

Connect your bot to the conversation channels of your choice using the channel configuration page and your developer credentials associated with that channel.

#### View in Bot Directory (comming soon)
{:.no_toc}

Bots registered through Bot Connector and approved for publishing will appear in the [Bot Directory](http://bots.botframework.com) where users can discover, try, and add bots to their favorite conversation experiences. Public visibility of your bot in the directory is a setting made during registration and can be changed at any time. 

#### Measure
{:.no_toc}

If you host your bot in Azure you can link to [Azure Application Insights](https://www.visualstudio.com/features/application-insights) analytics directly from the Bot Connector dashboard in the Bot Framework website. Naturally, a variety of analytics tools exist in the market to help developers gain insight into bot usage (which is certainly advisable to do).

#### Manage
{:.no_toc}

Once registered and connected to channels you can manage your bot via the Bot Framework website.

## What channels does the Bot Framework currently support?

Supported channels as of March 30, 2016 are:

1. Text/sms
2. Office 365 mail
3. Skype
4. Twitter (coming soon)
5. Slack
6. GroupMe
7. Telegram
8. Web (via the Bot Framework embeddable web chat control).

## I have a communication channel I’d like to be configurable on Bot Connector. Can I work with Microsoft to do that?

We have not provided a general mechanism for developers to add new channels to the Bot Connector, but you can connect your bot to your app via the [Direct Line API](http://docs.botframework.com). If you are a developer of a communication channel and would like to work with us to enable your channel in the Bot Connector [we’d love to hear from you](http://feedback.botframework.com).

## What does the Bot Builder SDK provide to developers? How does it work?

The Bot Builder SDK is an open source SDK hosted on GitHub that provides everything you need to build a great bot using Node.js or C\#. From simple prompt and command dialogs to simple-to-use yet sophisticated “FormFlow” dialogs that help with tricky issues such as multi-turn and disambiguation – the SDK provides the libraries, samples and tools you need to get up and running. Visit the Bot Builder SDK [Documentation](http://docs.botframework.com) to learn more.

## Is it possible for me to build a bot using the Bot Framework/SDK that is a “private or enterprise-only” bot that is only available inside my company?

At this point, we do not have plans to enable a private instance of the Bot Directory, but we are interested in exploring ideas like this with the developer community.

## What does the Bot Directory provide to developers? How does it work?

The [Bot Directory](http://bots.botframework.com) (coming soon) is a publicly accessible list of all the bots registered with Bot Connector that have been approved to appear in the directory. Each Bot has its own contact card which includes the bot name, publisher, description, and the channels on which it is available. Your users can tap in to view details on any bot, try your bot using the web chat control and add the bot to any channels on which it is configured. Bot cards also provide a way for users to report abuse as well.
The Bot Directory includes featured bots and is searchable to aid discovery. Developers can choose whether or not to list their bot in the directory during bot registration.

## You state that the Bot Directory is “coming soon” – when will it be available?
Effective immediately, developers can elect to make their bots public and submit them for approval during bot registration. We cannot provide a specific schedule for when the directory will go live at this time (a broadcast announcement will be made when the directory is made public, likely via a blog post).


## What is the roadmap for Bot Framework?

We are excited to provide initial availability of the Bot Framework at [/build 2016](http://build.microsoft.com/) and plan to continuously improve the framework with additional tools, samples, and channels.  Bot Builder is an open source SDK hosted on GitHub and we look forward to the contributions of the community at large. [Feedback](http://feedback.botframework.com) as to what you’d like to see is welcome.

## How does Microsoft Bot Framework compare with other bot development tools?

There are lots of great tools out there to build bots. In fact, if you implement a simple REST endpoint for your bot, you can build your bot using any tool you like and still access services available in the framework. The Bot Framework provides not only a way to build a great bot, but also an easy way to seamlessly connect your bot to any conversation experience supported by Bot Connector. Additionally, the framework provides an embeddable web chat control, automatic translation for more than 30 languages, user and conversation state management, and debugging tools. Lastly, the framework includes the Bot Directory (coming soon) so your users have a way to discover your bot, say hello, and add it to any channels on which it is configured. Sweet!

## Where can I get more information and/or access the technology?

Simply visit [www.botframework.com](http://botframework.com) to learn more.

## I'm a developer, what do I need to get started?

You can get started by visiting the Bot Framework site. To register a bot in the Bot Connector service, you’ll need a Microsoft account. The [Bot Builder SDK](http://github/Microsoft/BotBuilder) is open source and available to all on Github.

## Do the bots registered with the Bot Connector collect personal information? If yes, how can I be sure the data is safe and secure? What about privacy?

Each bot is its own service, and developers of these services are required to provide Terms of Service and Privacy Statements per their Developer Code of Conduct.  You can access this information from the bot’s card in the Bot Directory.

In order to provide the I/O service, the Bot Connector collects and stores your ID from the service you used to contact the bot. In turn the Bot Connector may additionally store anonymized conversation content for service improvement purposes.

## How do you ban or remove bots from the service?

Users have a way to report a misbehaving bot via the bot’s contact card in the directory. Developers must abide by Microsoft terms of service to participate in the service.

## Bot Connector sounds a little too good to be true. I've heard the promise of write once, run anywhere before. Does Bot Connector really just provide a lowest common denominator solution that won't be satisfying in the end?

For the vast majority of bot interactions (text/sms, image, or card) Bot Connector provides high quality and broad reach to many of the world’s top conversation experiences while also providing a way to configure, manage and make your bot discoverable through the Bot Framework website and Bot Directory. Additionally, by participating in the framework your bot is enabled with additional capabilities via other Microsoft services, such as translation.

## If I want to create a bot for Skype, what tools and services should I use?

Any bot with an internet-accessible REST endpoint can be connected to Skype via the Bot Framework Bot Connector. The Bot Framework provides SDKs designed to create text/sms, image and card-capable bots, which constitute the majority of bot interactions today across conversation experiences. The [Skype Bot SDK and APIs](http://www.skype.com/en/developer/signup) enable those interactions plus bot interactions which are Skype-specific and tuned to leverage the unique capabilities found in Skype, among them rich audio and video experiences. 

## When will Bot Builder SDK provide support for other languages such as Python?

At its core, a bot is simply a piece of code that exposes a REST endpoint that matches the API signature required by the Bot Framework Connector. We have two SDKs that make developing a bot easy, these support C\# and Node.JS (JavaScript). If your bot is capable of providing a REST endpoint that can be called by the Bot Connector then you can write without using these SDKs but we encourage you to check them out as the SDKs provide value in dialogs, common controls, and reach for your bot.

## When will you add more conversation experiences to Bot Connector?

We plan on making continuous improvements to the Bot Framework, including additional channels for the Bot Connector but cannot provide a schedule at this time.

## Why are Facebook Messenger and Google Hangouts missing as channels? Do you intend to support these channels in future?

A variety of factors contribute to channel support, among them SDK availability, user demand and schedule. When there is an available SDK for these channels we intend to make them available in the framework.

## Are you going to add a Minecraft channel?

We think a Minecraft channel would be a great addition to the framework and plan to add support in future. We do not have a schedule for additional channels at this time.

## How does the Bot Framework relate to Cognitive Services?

Both the Bot Framework and [Cognitive Services](http://www.microsoft.com/cognitive) are new capabilities introduced at [//Build 2016](http://build.microsoft.com) that will also be integrated into Cortana Intelligence Suite at GA. Both these services are built from years of research and use in popular Microsoft products. These capabilities combined with ‘Cortana Intelligence’ enable every organization to take advantage of the power of data, the cloud and intelligence to build their own intelligent systems that unlock new opportunities, increase their speed of business and lead the industries in which they serve their customers.

## What is Cortana Intelligence?

Cortana Intelligence is a fully managed Big Data, Advanced Analytics and Intelligence suite that transforms your data into intelligent action.  
It is a comprehensive suite that brings together technologies founded upon years of research and innovation throughout Microsoft (spanning advanced analytics, machine learning, big data storage and processing in the cloud) and
* Allows you to collect, manage and store all your data that can seamlessly and cost effectively grow over time in a scalable and secure way.
* Provides easy and actionable analytics powered by the cloud that allow you to predict, prescribe and automate decision making for the most demanding problems. 
* Enables intelligent systems through cognitive services and agents that allow you to see, hear, interpret and understand the world around you in more contextual and natural ways.

With Cortana Intelligence, we hope to help our enterprise customers unlock new opportunities, increase their speed of business and be leaders in their industries.


