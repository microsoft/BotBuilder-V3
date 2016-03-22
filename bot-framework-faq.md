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

The Microsoft Bot Framework provides everything you need to build and connect intelligent bots to interact naturally wherever your users are talking, from text/sms to Skype, Slack, Office 365 mail and other popular services.

Bots (or conversation agents) are rapidly becoming an integral part of one’s digital experience – they are as vital a way for users to interact with a service or application as is a web site or a mobile experience. Developers writing bots all face the same problems: bots require basic I/O; they must have language and dialog skills; and they must connect to users – preferably in any conversation experience and language the user chooses. The Bot Framework provides tools to easily solve these problems for developers e.g., automatic translation to more than 30 languages, user and conversation state management, debugging tools, an embeddable web chat control and a way for users to discover, try, and add bots to the conversation experiences they love.

The Bot Framework has a number of components including the Bot Connector, Bot Builder SDK, and the Bot Directory.

### Bot Connector
{:.no_toc}

The Bot Connector lets you connect your bot(s) seamlessly to text/sms, Office 365 mail, Skype, Slack, and other services. Simply register your bot, configure desired channels and publish in the Bot Directory. 

### Bot Builder SDK
{:.no_toc}

The Bot Builder SDK is [an open source SDK hosted on GitHub] (https://github/Microsoft/BotBuilder) that provides everything you need to build great dialogs within your Node.js- or C#-based bot.

### Bot Directory
{:.no_toc}

The Bot Directory is a public directory of all the bots registered through the Bot Connector. Users can discover, try, and add bots to their favorite conversation experiences from the [Bot Directory] (http://bots.botframework.com).

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

The Bot Framework will be released as a preview in conjunction with Microsoft’s annual developer conference [//Build] (http://build.microsoft.com/).

## How long will the Bot Framework be in preview? Can I start building/shipping products based on a preview framework?

The Bot Framework is currently in preview; expect to see it become generally available by the end of CY 16.

## Who are the target users for the Bot Framework? How will they benefit?

The Bot Framework is targeted at developers who want to create a new service with a great bot interface or enable an existing service with a great bot interface. 

Developers writing a bot all face the same problems: bots require basic I/O, they must have language and dialog skills, and they must connect to users – preferably in any conversation experience and in any language the user chooses. The Bot Framework provides tools to address these problems while also providing a way for users to discover, try, and add bots to the conversation experiences they love via the Bot Directory. 

Additionally, as a participant in the Bot Framework, your bot will also be enabled with automatic translation, user and conversation state management, a web chat control, and debugging tools including the Bot Framework Emulator.

## What does the Bot Connector provide to developers? How does it work?

Bot Connector is the easiest way to achieve broad reach for your text/speech, image, and/or card-capable bot. In addition to enabling broad reach to the conversation experiences your users love, the Bot Connector also provides automatic translation to more than 30 languages, an embeddable web chat control, user and conversation state management, and debugging through the Bot Framework Emulator.

#### How it works
{:.no_toc}

Bot Connector lets you connect your bot with many different communication experiences. If you write a bot – sometimes called a conversation agent – and expose a Bot Connector-compatible API on the internet (a REST API), Bot Connector will forward messages to your bot from your user, and will send messages back to the user. 
To connect your bot to your users you must have:


* A bot (if you don’t have one, check out the [Bot Builder SDK] (http://github/Microsoft/BotBuilder) on Github
* A Microsoft Account, which you will use to register and manage your bot in the Bot Framework
* An internet-accessible REST endpoint exposing the Bot Connector messages API
* Optionally, accounts on one or more communication services where your bot will converse.

#### Register
{:.no_toc}

To register your bot, sign in to the Bot Framework website and provide the requisite details for your bot, including a bot profile image.

[consider screenshot]

Once registered, use the dashboard to test your bot to ensure it is talking to the Bot Connector service and/or use the web chat control, an auto-configured channel, to experience what your users will experience when conversing with your bot.

#### Connect to Channels
{:.no_toc}

Connect your bot to the conversation channels of your choice using the channel configuration page and your developer credentials associated with that channel.

[consider screenshot]

#### View in Bot Directory
{:.no_toc}

Bots registered through Bot Connector appear in the publicly accessible [Bot Directory] (http://bots.botframework.com) where users can discover, try, and add bots to their favorite conversation experiences. Public visibility of your bot in the directory is a setting made during registration and can be changed at any time.

[screenshots]

#### Measure
{:.no_toc}

If you host your bot in Azure you can link to [Azure Application Insights] (https://www.visualstudio.com/features/application-insights) analytics directly from the Bot Connector dashboard in the Bot Framework website. Naturally, a variety of analytics tools exist in the market to help developers gain insight into bot usage (which is certainly advisable to do).

[consider screenshot]

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

We have not provided a mechanism for developers to add new channels to the Bot Connector, but if you are a developer of a communication channel and would like to work with us to enable your channel in the Bot Connector [we’d love to hear from you] (http://feedback.botframework.com).

## What does the Bot Builder SDK provide to developers? How does it work?

The Bot Builder SDK is an open source SDK hosted on GitHub that provides everything you need to build a great bot using Node.js or C\#. From simple prompt and command dialogs to simple-to-use yet sophisticated “FormFlow” dialogs that help with tricky issues such as multi-turn and disambiguation – the SDK provides the libraries, samples and tools you need to get up and running. Visit the Bot Builder SDK [Documentation] (http://docs.botframework.com) to learn more.

## Is it possible for me to build a bot using the Bot Framework/SDK that is a “private or enterprise-only” bot that is only available inside my company?

At this point, we do not have plans to enable a private instance of the Bot Directory, but we are interested in exploring ideas like this with the developer community.

## What does the Bot Directory provide to developers? How does it work?

The [Bot Directory] (http://bots.botframework.com) is a publicly accessible list of all the bots registered with Bot Connector. Each Bot has its own contact card which includes the bot name, publisher, description, and the channels on which it is available. Your users can tap in to view details on any bot, try your bot using the web chat control and add the bot to any channels on which it is configured. Bot cards also provide a way for users to report abuse as well.

The Bot Directory includes featured bots and is searchable to aid discovery. Developers can choose whether or not to list their bot in the directory during bot registration.

## How do I get my bot in the featured list in the Bot Directory?

For now, presence in the featured list is determined by the Bot Framework team. If you think your bot should be featured feel free to provide that [feedback] (http://feedback.botframework.com).


## What is the roadmap for Bot Framework?

[answer]

## How does Microsoft Bot Framework compare with other bot development tools?

[answer]

## Where can I get more information and/or access the technology?

Simply visit www.botframework.com to learn more.

## I'm a developer, what do I need to get started?

You can get started simply by visiting the Bot Framework site. The Bot Builder SDK is open source and available to all.

To register a bot in the Bot Connector service, you'll need a Microsoft account and [need to complete this answer].

## I'm an end user looking for great bots, what do I need to do?

Visit the Bot Directory to find bots for the conversations experiences you love – from sms to Office 365 mail to Skype, Slack, Twitter and more.

## Are there other related offerings from Microsoft I should know about that can help me build great bots?

[cross link to Skype, Bing, Cortana Intelligence]

## Do the bots registered with the Bot Connector collect personal information? If yes, how can I be sure the data is safe and secure? What about privacy?

[answer]

## Bot Connector sounds a little too good to be true. I've heard the promise of write once, run anywhere before. Does Bot Connector really just provide a lowest common denominator solution that won't be satisfying in the end?

[answer]

## I heard there was a separate SDK to write Bots for Skype. How do I know what to use?

[answer]

## When will I be able to create a bot for Skype for Business?

[answer]

## I want to create a bot for Bing [other Microsoft service]. How do I do that?

[answer]

## When will Bot Builder SDK provide support for other languages such as Python?

[answer]

## When will you add more conversation experiences to Bot Connector?

[answer]

## I have a conversation experience I'd like to have configurable on Bot Connector. Can I work with Microsoft to do that?

[answer]

## I have a bot I wrote for Slack. Can I use the Bot Connector Service to make my bot available to the other conversation experiences you feature?

[answer]
