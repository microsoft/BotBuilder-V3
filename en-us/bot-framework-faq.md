---
layout: page
title: Bot Framework FAQ
permalink: /en-us/faq/
weight: 150
parent1: none
---


* TOC
{:toc}

## What is the Microsoft Bot Framework?

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

## Why should I write a bot?
The Conversational User Interface, or CUI, has arrived. 

A plethora of chit-chat bots are offering to do things for us in our various communication channels like Skype and Facebook Messenger. A series of personal agent services have emerged that leverage machines, humans or both to complete tasks for us (x.ai, Clara Labs, Fancy Hands, Task Rabbit, Facebook “M” to name a few). 

The primary interface for these experiences is email, text, cards, buttons or voice. Conversation-driven UI now enables us to do everything from grabbing a taxi, to paying the electric bill or sending money to a friend. Offerings such as Siri, Google Now and Cortana demonstrate value to millions of people every day, particularly on mobile devices where the CUI can be superior to the GUI or complements it. 

Bots and conversation agents are rapidly becoming an integral part of one’s digital experience – they are as vital a way for users to interact with a service or application as is a web site or a mobile experience.

## Who are the target users for the Bot Framework? How will they benefit?

The Bot Framework is targeted at developers who want to create a new service with a great bot interface or enable an existing service with a great bot interface. 

Developers writing bots all face the same problems: bots require basic I/O; they must have language and dialog skills; they must be performant, responsive and scalable; and they must connect to users – ideally in any conversation experience and language the user chooses. The Bot Framework provides tools to address these problems while also providing a way for users to discover, try, and add bots to the conversation experiences they love via the Bot Directory. 

As a participant in the Bot Framework, you may also take advantage of:

* the auto-configured Skype channel and Web channel,
* the embeddable Web chat control,
* automatic card normalization so your bot is responsive across channels,
* the Direct Line API which can be used to host your bot in your app, 
* debugging tools including the Bot Framework Emulator (online/offline), and
* powerful service extenstions to make your bot smarter through [Cognitive Services](http://www.microsoft.com/cognitive) such as LUIS for language understanding, Translation for automatic translation to more than 30 languages, and FormFlow for reflection generated bots.

## I'm a developer, what do I need to get started?

You can get started by visiting the [Bot Framework site](http://botframework.com). To register a bot in the Developer Portal, you’ll need a Microsoft account. The [Bot Builder SDK](http://github.com/Microsoft/BotBuilder) is open source and available to all on Github.  We also have guides to get started building a bot using Node.js, .NET or REST API:

* [Get started with the Bot Builder - Node.js](/en-us/node/builder/overview/).
* [Get started with the Bot Builder - .NET](/en-us/csharp/builder/sdkreference/).
* [Get started with the Bot Builder - REST API](/en-us/csharp/builder/sdkreference/gettingstarted.html).

## What does the Developer Portal provide to developers? How does it work?

![Bot Dashboard)](/en-us/images/faq-overview/dashboard.png)

The Bot Framework Developer Portal lets you register and connect your bot to many different conversation experiences (Skype and Web are auto-configured), providing broad reach for your text/speech, image, button, card-capable and audio/video-capable bot.
To make your bot available to your users through the Bot Framework you must have:

* A bot (if you don’t have one, check out the [Bot Builder SDK](http://github.com/Microsoft/BotBuilder) on Github
* A [Microsoft Account](https://signup.live.com), which you will use to register and manage your bot in the Bot Framework
* An internet-accessible REST endpoint exposing the Bot Framework messages API
* Optionally, accounts on one or more communication services where your bot will converse

#### Register
{:.no_toc}

To register your bot, sign in to the [Bot Framework site](http://botframework.com) and provide the requisite details for your bot, including a bot profile image.

Once registered, use the dashboard to test your bot to ensure it is talking to the connector service and/or use the web chat control, an auto-configured channel, to experience what your users will experience when conversing with your bot.

#### Connect to Channels
{:.no_toc}

Connect your bot to the conversation channels of your choice using the channel configuration page and your developer credentials associated with that channel. The Skype and Web channels are auto-configured for you.

#### Test
{:.no_toc}

Test your bot's connection to the Bot Framework and try it out using Web chat control.

#### Publish
{:.no_toc}

Bots registered through Developer Portal and reviewed for publishing will appear in the [Bot Directory](http://bots.botframework.com) where users can discover, try, and add bots to their favorite conversation experiences. Public visibility of your bot in the directory is a setting made during registration and can be changed at any time. Note that additional steps are often required per channel in order to appear in channel-specific directories.

#### Measure
{:.no_toc}

If you host your bot in Azure you can link to [Azure Application Insights](https://azure.microsoft.com/en-us/services/application-insights/) analytics directly from the bot dashboard in the Bot Framework website. Naturally, a variety of analytics tools exist in the market to help developers gain insight into bot usage (which is certainly advisable to do).

#### Manage
{:.no_toc}

Once registered and connected to channels you can manage your bot via your bot's dashboard in the Bot Framework Developer Portal.

## What does the Bot Builder SDK provide to developers? How does it work?

The [Bot Builder SDK](http://github.com/Microsoft/BotBuilder) is an open source SDK hosted on GitHub that provides just what you need to build a great bot using Node.js, .NET or REST. From simple prompt and command dialogs to simple-to-use yet sophisticated “FormFlow” dialogs that help with tricky issues such as multi-turn and disambiguation – the SDK provides the libraries, samples and tools you need to get your bot up and running. Visit the Bot Builder SDK [Documentation](http://docs.botframework.com) to learn more.

## What does the Bot Directory provide to developers? How does it work?

The [Bot Directory](http://bots.botframework.com) (coming soon) is a publicly accessible list of all the bots registered with Bot Framework that have been submitted and reviewed to appear in the directory. Each bot has its own contact card which includes the bot name, publisher, description, and the channels on which it is available. Your users can tap in to view details on any bot, try your bot using the web chat control and add the bot to any channels on which it is configured. Bot cards also provide a way for users to report abuse as well.
The Bot Directory includes featured bots and is searchable to aid discovery. Developers can choose whether or not to list their bot in the directory during bot registration.

## Can I submit my bot to the Bot Directory?

The Bot Directory, the public directory of bots registered with Bot Framework, is open to developers for bot submission and review. The Bot Directory itself isn’t live yet, but when it is available, users will be able to discover, try and add bots to their favorite conversation experiences.

## Do I have to publish my bot to the Bot Directory in order for my bot to be available to users?

No, publishing your bot is an optional process. Certain channels do limit the number of users allowed to interact with a bot until a review process has been completed - this process is managed by the channel, not Microsoft Bot Framework.

## You state that the Bot Directory is “coming soon” – when will it be available?

Developers can elect to make their bots public and submit them for review from the bot's dashboard. We cannot provide a specific schedule for when the directory will go live at this time (a broadcast announcement will be made when the directory is made public, likely via a blog post).

## What channels does the Bot Framework currently support?

Supported channels as of July, 2016 are:

1. Text/sms
2. [Office 365 mail](http://www.office.com/)
3. [Skype](http://www.skype.com/) (auto-configured)
4. [Slack](http://slack.com/)
5. [GroupMe](http://groupme.com/)
6. [Telegram](http://telegram.org/)
7. [Facebook Messenger](http://www.messenger.com/)
8. [Kik](https://www.kik.com/)
9. Web (auto-configured, embeddable)
10. Direct Line (API to host your bot in your app)

## When will you add more conversation experiences to the Bot Framework?

We plan on making continuous improvements to the Bot Framework, including additional channels, but cannot provide a schedule at this time.  If you would like a specific channel added to the framework, [let us know](http://feedback.botframework.com).

## I have a communication channel I’d like to be configurable with Bot Framework. Can I work with Microsoft to do that?

We have not provided a general mechanism for developers to add new channels to Bot Framework, but you can connect your bot to your app via the [Direct Line API](http://docs.botframework.com). If you are a developer of a communication channel and would like to work with us to enable your channel in the Bot Framework [we’d love to hear from you](http://feedback.botframework.com).

## Is it possible for me to build a bot using the Bot Framework/SDK that is a “private or enterprise-only” bot that is only available inside my company?

At this point, we do not have plans to enable a private instance of the Bot Directory, but we are interested in exploring ideas like this with the developer community.

## Why did Microsoft develop the Bot Framework?

While the “CUI is upon us,” at this point few developers have the expertise and tools needed to create new conversational experiences or enable existing applications and services with a conversational interface their users can enjoy. We have created the Bot Framework to make it easier for developers to build and connect great bots to users, wherever they converse including on Microsoft's premier channels.

## If I want to create a bot for Skype, what tools and services should I use?

The Bot Framework is designed to build, connect and publish high quality, responsive, performant and scalable bots for Skype and many other channels. The SDK can be used to create text/sms, image, button and card-capable bots (which constitute the majority of bot interactions today across conversation experiences) as well as bot interactions which are Skype-specific such as rich audio and video experiences. 

If you already have a great bot and would like to reach the Skype audience, your bot can easily be connected to Skype (or any supported channel) via the Bot Builder for REST API (provided it has an internet-accessible REST endpoint).

## Who are the people behind the Bot Framework?

In the spirit of One Microsoft, the Bot Framework is a collaborative effort across many teams, including Microsoft Technology and Research, Microsoft’s Applications and Services Group and Microsoft’s Developer Experience teams.

## When did work begin on the Bot Framework?

The core Bot Framework work has been underway since the summer of 2015.

## Is the Bot Framework publicly available now?

Yes. The Bot Framework was released in preview on March 30th of 2016 in conjunction with Microsoft’s annual developer conference [/build](http://build.microsoft.com/).

## How long will the Bot Framework be in preview? Can I start building/shipping products based on a preview framework?

The Bot Framework is currently in preview; expect to see it become generally available by the end of CY 16. As indicated at Build 2016, Microsoft is making significant investments in Conversation as a Platform - among those investments is the Bot Framework. Building upon a preview offering is of course, at your discretion.

## What is the roadmap for Bot Framework?

We are excited to provide initial availability of the Bot Framework at [/build 2016](http://build.microsoft.com/) and plan to continuously improve the framework with additional tools, samples, and channels. The [Bot Builder SDK](http://github.com/Microsoft/BotBuilder) is an open source SDK hosted on GitHub and we look forward to the contributions of the community at large. [Feedback](http://feedback.botframework.com) as to what you’d like to see is welcome.

## The Bot Framework July 2016 Update saw some significant changes to the SDKs and service. Can you enumerate these changes?

The July 2016 update is largely in response to feedback received from the active Bot Framework community. The update is focused on quality, control and performance. Enhancements include:

* Automatic card normalization across channels
* More direct message handling; more control
* Additional dialog types and capabilities in the SDK
* Enhanced connection to Cognitive Services within the SDK
* Improvements to the Emulator and Direct Line API
* Skype channel auto-configured for any bot using Bot Framework

## Do I need to upgrade my bot with this service update?

Yes. You will need to [upgrade your bot](https://aka.ms/bf-migrate) with this release of the service. All bots written prior to the July release will need to upgrade to the latest SDK (v3) in order to continue to function. Bots written to versions of the SDK prior to V3 will cease functioning in roughly 90 days post the July 7th release. 

## Do the bots registered with the Bot Framework collect personal information? If yes, how can I be sure the data is safe and secure? What about privacy?

Each bot is its own service, and developers of these services are required to provide Terms of Service and Privacy Statements per their Developer Code of Conduct.  You can access this information from the bot’s card in the Bot Directory.

In order to provide the I/O service, the Bot Framework transmits your message and message content (including your ID), from the chat service you used to the bot. 

## How do you ban or remove bots from the service?

Users have a way to report a misbehaving bot via the bot’s contact card in the directory. Developers must abide by Microsoft terms of service to participate in the service.

## Where can I get more information and/or access the technology?

Simply visit [www.botframework.com](http://botframework.com) to learn more.

## How does the Bot Framework relate to Cognitive Services?

Both the Bot Framework and [Cognitive Services](http://www.microsoft.com/cognitive) are new capabilities introduced at [Microsoft Build 2016](http://build.microsoft.com) that will also be integrated into Cortana Intelligence Suite at GA. Both these services are built from years of research and use in popular Microsoft products. These capabilities combined with ‘Cortana Intelligence’ enable every organization to take advantage of the power of data, the cloud and intelligence to build their own intelligent systems that unlock new opportunities, increase their speed of business and lead the industries in which they serve their customers.

## What is Cortana Intelligence?

Cortana Intelligence is a fully managed Big Data, Advanced Analytics and Intelligence suite that transforms your data into intelligent action.  
It is a comprehensive suite that brings together technologies founded upon years of research and innovation throughout Microsoft (spanning advanced analytics, machine learning, big data storage and processing in the cloud) and:

* Allows you to collect, manage and store all your data that can seamlessly and cost effectively grow over time in a scalable and secure way.
* Provides easy and actionable analytics powered by the cloud that allow you to predict, prescribe and automate decision making for the most demanding problems. 
* Enables intelligent systems through cognitive services and agents that allow you to see, hear, interpret and understand the world around you in more contextual and natural ways.

With Cortana Intelligence, we hope to help our enterprise customers unlock new opportunities, increase their speed of business and be leaders in their industries.

## What is the Direct Line channel?

Direct Line is a REST API that allows you to add your bot into your service, mobile app, or webpage.

You can write a client for the Direct Line API in any language. Simply code to the [Direct Line protocol](/en-us/restapi/directline/), generate a secret in the Direct Line configuration page, and talk to your bot from wherever your code lives.

Direct Line is suitable for:

* Mobile apps on iOS, Android, and Windows Phone, and others
* Desktop applications on Windows, OSX, and more
* Webpages where you need more customization than the [embeddable Web Chat channel](/en-us/support/embed-chat-control2/) offers
* Service-to-service applications
