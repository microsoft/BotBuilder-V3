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

The Bot Builder SDK is an open source SDK hosted on GitHub that provides everything you need to build great dialogs within your Node.js- or C#-based bot.

### Bot Directory
{:.no_toc}

The Bot Directory is a public directory of all the bots registered through the Bot Connector. Users can discover, try, and add bots to their favorite conversation experiences from the Bot Directory.

## Why did Microsoft develop the Bot Framework?
The CUI, or conversational user interface, has perhaps finally arrived. 

A plethora of chatty bots are offering to do things for us in our messaging experiences. A series of personal agent services have emerged that leverage machines, humans or both to complete tasks for us (x.ai, Clara Labs, Fancy Hands, Task Rabbit, Facebook "M" to name a few) – the commanding interface for these experiences is email, text or a voice. Conversation-driven UI now enables us to do everything from grabbing a taxi, to paying the electric bill or sending money to a friend. Offerings such as Siri, Google Now and Cortana demonstrate value to millions of people every day, particularly on mobile form factors, where the CUI is often superior to the GUI or complements it. 

Bots and conversation agents are rapidly becoming an integral part of one's digital experience – they are as vital a way for users to interact with a service or application as a web site or mobile experience. Yet, at this point, few developers have the expertise and tools needed to create such experiences or enable them with quality for their existing services and applications. 

We have created the Bot Framework to make it easier for developers to build and connect great bots to users, wherever they converse.

## Who are the people behind the Bot Framework?

The Bot Framework is a collaborative effort from Microsoft Research Fuse Labs and [list other teams, ideally Skype and Bing?] who worked together to [how we worked together].

## When did work begin on the Bot Framework?

The core Bot Framework work has been underway since the summer of 2015. [confirm]

## Is the Bot Framework publicly available now?

The Bot Framework will be released on March 30th, in conjunction with Microsoft's annual developer conference \\Build.

## Who are the target users for the Bot Framework? How will they benefit?

The Bot Framework is targeted at developers who want to create a new service with a great bot interface or enable an existing service with one. 

Developers writing a bot all face the same problems: bots require basic I/O, they must have language and dialog skills, and they must connect to users – preferably in any conversation experience the user chooses. The Bot Framework provides tools to easily solve these problems and more for developers while also providing a way to for users to discover, try and add bots to the conversation experiences they love.

## What does the Bot Connector Service provide to developers? How does it work?

Bot Connector enables access to many of the world's top conversation experiences with a minimum of effort on your part.

#### How it works
{:.no_toc}

Bot Connector lets you connect your bot with many different communication experiences. If you write a bot – sometimes called a conversation agent – and expose a Bot Connector-compatible API on the internet (a REST api), Bot Connector will forward messages from your bot to a user, and will send their messages back. 
To get started, you must have:

* A bot (if you don't have one, check out the Bot Builder SDK)
* A Microsoft Account, which you will use to register your bot in the developer portal
* An Azure-accessible REST endpoint exposing the Bot Connector messages API
* Optionally, accounts on one or more communication services where your bot will converse.

#### Register
{:.no_toc}

To register your bot, sign in to the Bot Framework Developer Portal and provide the requisite details for your bot, including a bot profile image.

[consider screenshot]

Once registered, use the dashboard to test your bot to ensure it is talking to the Bot Connector service and/or use the web chat control, an auto-configured channel, to experience what your users will experience when conversing with your bot.

#### Connect to Channels
{:.no_toc}

Once registered, connect your bot to the conversation channels of your choice using the channel configuration page.

[consider screenshot]

#### View in Bot Directory
{:.no_toc}

Bots registered through Bot Connector appear in the publicly accessible Bot Directory where users can discover, try and choose to add bots to their favorite conversation experiences. 

[screenshots]

#### Measure
{:.no_toc}

As an Azure service [confirm], Bot Connector also provides ways for you to analyze and measure your bot. Access Azure Analytics through your bot dashboard on the developer portal.

[consider screenshot]

#### Manage
{:.no_toc}

Once registered and connected to channels you can manage your bot through the portal.

[consider screenshot]

Bot Connector is the easiest way to achieve broad reach for your text, image or card-capable bot.

## What channels does the Bot Framework currently support?

[list channels]

## What does the Bot Builder SDK provide to developers? How does it work?

The Bot Builder SDK is an open source SDK that provides everything you need to build a great bot using two popular languages: Node.js and C\#. From simple prompt and command dialogs to simple to use yet sophisticated "FormFlow" dialogs – the SDK provides the libraries, samples and tools you need to get you up and running. Visit the Bot Builder SDK Documentation to learn more.

## [consider a Q for defining each dialog, particularly FormFlow as it is a differetiator; also need approval for that name]
[answer]

## What does the Bot Directory provide to developers? How does it work?

The Bot Directory is a publicly accessible list of all the bots registered with Bot Connector. Each Bot has its own contact card which includes the bot name, publisher, description, and the channels on which it is available. Your users can tap in to view details on any bot, try it out using the web chat control and add the bot to the channels of their choosing.

The Bot Directory includes a featured set of bots and is searchable for easy discovery.

## Can you give some examples of real-world problems that can be solved with the Bot Framework?

Sure [answers, include special services that Connector provides automatically such as translation, others]

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
