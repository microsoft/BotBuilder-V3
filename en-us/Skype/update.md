---
layout: page
title: Latest update
permalink: /en-us/skype/update/
weight: 5010
parent1: Skype bots
---

Microsoft has brought together the Skype Bot developer tools and the Microsoft Bot Framework into one environment which we’re calling the Microsoft Bot Framework “V3”. You can now develop bots which use new Skype platform features – such as visual cards and group bots – and publish to multiple channels from one place.

You can start developing using the new SDK today and test it using the emulator or the <a href="https://web.skype.com/en/?ecsoverride=developer" target='_blank'>developer version of the Skype Web App</a>. You will be able to publish bots built using the new API when Skype apps supporting the new features are released for desktop and mobile platforms in a few weeks.

Existing bots registered in the Skype Bot Portal and developed using the Skype Bot SDK will continue to work but we recommend you move to the new environment as soon as possible to get access to the latest features and updates.

To update to the new environment you need to register a new bot with the Microsoft Bot Framework and update your bot to the latest API. If you have a published bot you can request migration of the existing Skype bot and users (see below).


* TOC
{:toc}

## What's new

<div class="docs-text-note">To test new features use the <a href="https://web.skype.com/en/?ecsoverride=developer" target='_blank'><b>developer version of the Skype Web App</b></a> until updated Skype apps for desktop and mobile are available at the end of July.</div>

### Cards
{:.no_toc}

Create [visual cards](/en-us/skype/getting-started/#navtitle) for compelling user to bot interactions with images, carousels, receipts and action buttons.

![Carousel card](/en-us/images/skype/skype-bot-carousel-card.png)

### Sign in
{:.no_toc}

Create a [sign in card](/en-us/skype/getting-started/#navtitle) for authenticating a user with your service via OAuth or other login methods

![Sign in card](/en-us/images/skype/skype-bot-signin-card.png)

### Groups
{:.no_toc}

Make Skype Bots that are more productive - or just entertaining - for [groups](/en-us/skype/getting-started/#groups) of users.  Bots can now be a part of and respond to group conversations.

![Groups](/en-us/images/skype/skype-bot-at-mention.png)

### Plus
{:.no_toc}

* Publish your bot in Skype directly from the Microsoft Bot Framework - to remove bot user limits, and request promotion in the Skype and Microsoft bot directories
* Updates to the API schema unifying the Microsoft Bot Framework and Skype Bot Platform (this requires a few simple updates to your bot)
* Try out a preview of built-in [Bing Entity and Intent Detection](/en-us/skype/getting-started/#bing-entity-and-intent-detection-preview), which adds natural language understanding to messages sent from Skype to your bot
* Review the updated and combined [Terms of Use](https://aka.ms/bf-terms) and [Developer Code of Conduct](https://aka.ms/bf-conduct)

## How to update an existing bot

**If your bot was already registered in the Microsoft Bot Framework and developed using the Microsoft Bot Framework SDK ("V1")** you can follow [this guide](https://aka.ms/bf-migrate) on how to upgrade it to use the latest SDK ("V3").

**If your bot was registered and developed using the Skype Bot Portal and Skype Bot SDK** follow these steps to update your bot:
1.	[Register](https://dev.botframework.com/bots/new) a new bot in the Microsoft Bot Framework
2.	Update your bot to use the new Microsoft Bot Framework V3
3.	(For published bots only) Migrate your existing bot and users


### 1. Register a new bot in Microsoft Bot Framework
{:.no_toc}

1.	Go to the Microsoft Bot Framework and tap “Register a bot”

![Microsoft Bot Framework](/en-us/images/skype/bot-framework.png)

2.	Register your bot and get a new Microsoft App ID and Secret

![Microsoft App ID](/en-us/images/skype/bot-framework-app-id.png)

You can continue to use the same bot webhook but the bot at that endpoint will need to be updated to the latest V3 API.

If your existing bot is already published see below for how to migrate existing users.

### 2. Update your bot to use the new V3 API
{:.no_toc}

Update your bot code to use the Microsoft Bot Framework V3 API. 

See the [Skype Getting Started](/en-us/skype/getting-started) guide for details on the latest Skype bot platform features, plus the [C# SDK](/en-us/csharp/builder/sdkreference/index.html), [Node SDK](/en-us/node/builder/overview/#navtitle) or [Skype REST API](#).

You can test using the Microsoft Bot Framework Emulator or in Skype using the <a href="https://web.skype.com/en/?ecsoverride=developer" target='_blank'><b>developer version of the Skype Web App</b></a>.

### 3. (For published bots only) Migrate your existing bot and users
{:.no_toc}

If you have an approved published bot you may want to keep it running while you update to the new API, and then point the existing bot identity to the new bot.

To do this you can:
1.	Clone your existing bot and deploy it to a new endpoint as you update it to the new API, and test using the emulator and the developer version of the Skype Web app

2.	Migrate your existing Bot ID to the new bot App ID, which will point the bot to your new endpoint

To do this send an email to bothelp@microsoft.com with:

* The existing bot name and Bot ID from the Skype Bot Portal

![Skype Bot Portal My Bots](/en-us/images/skype/skype-bot-portal-my-bots.png)

![Skype Bot Portal Bot Details](/en-us/images/skype/skype-bot-portal-details.png)

* Your new Microsoft Bot Framework App ID (from the Microsoft Bot Framework Portal)

![Microsoft Bot Framework App ID](/en-us/images/skype/bot-framework-app-id.png)

We'll update the App ID in your new bot the Microsoft Bot Framework after which your users will start using the new bot.

**Note that this final migration step will not happen until Skype apps are available with the new features at the end of July 2016.**
