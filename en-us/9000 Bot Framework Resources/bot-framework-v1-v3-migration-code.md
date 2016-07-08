---
layout: page
title:  Upgrade your bot code to V3
permalink: /en-us/support/upgrade-code-to-v3/
weight: 9200
parent1: none
---

At Build 2016 Microsoft announced the Microsoft Bot Framework and its initial iteration of the Bot Connector API, along with Bot Builder and Bot Connector SDK's.  Since Build we've been collecting your feedback and actively working to improve the REST API and SDK's to be ready for the future, better support for attachments, and improved performance.

We're now introducing a new iteration of our API (V3).  In this API, there are a number of small changes designed to make the API more adaptable to future requirements.

The good news is our overall model is the same, though the syntax of interacting with the Bot Connector has been updated.  Below are some highlights that are detailed in the Bot Framework documentation.

## Getting Started Migrating your Bot
{:.no_toc}

The changes below will require at least some code change to your bot.  In order to not disrupt your existing users, we suggest the following:

1. Create a new branch of your Bot's source code
2. Update to the new SDK for your bot's language
3. Make appropriate syntax changes
4. Test with the Bot Framework Channel Emulator on your desktop and then in the cloud
5. [Upgrade your bot registration in the Bot Framework Developer Portal](/en-us/support/upgrade-to-v3/)

The goal of these steps are to ensure continued support for your current users.  If that's not an issue, you can just update your current deployment in place.

* TOC
{:toc}

## BotBuilder and Connector are now one SDK

Instead of downloading separate SDKs for the builder and connector in separate NuGet (or NPM packages), both are included in the BotBuilder package.  On NuGet - Microsoft.Bot.Builder, and on NPM - botbuilder.  The standalone Microsoft.Bot.Connector SDK will not be updated going forward.

## Message is now Activity

The Message object has been replaced with the Activity object; which currently returns a number of types of activities, including but not limited to messages.  Learn more about working with [Activity](/en-us/csharp/builder/sdkreference/connector.html#replying) objects.

## Activity Types & Events

Some of the events have been renamed/refactored.  In addition, a new ActivityTypes enumeration has been added to the Connector to take away the need to remember specific message types.

- **conversationUpdated** - replaces Bot/User Added/Removed To/From Conversation with a single method
- New: **Typing** - lets your bot indicate whether the user or bot is typing
- New: **contactRelationUpdated** - lets your Bot know if a bot has been added or removed as a contact for a user

For conversationUpdated, the MembersRemoved and MembersAdded lists will now tell you who was added or removed.  For contactRelation, the new Action property will tell you whether the user was adding or removing the bot from their contact list.  Read here for more on and [ActivityTypes](/en-us/csharp/builder/sdkreference/connector.html#messagetypes).

## Addressing

Addressing Activity objects has been changed slightly (see table below).  You can learn more here: [Addresses in Activities](/en-us/csharp/builder/sdkreference/connector.html#addresses)

|V1 Field|	V3 Field|
|--------|--------|
|From Object|From Object|
|To Object|	Recipient Object|
|ChannelConversationID|	Conversation Object|
|ChannelId| ChannelId|

## Sending Replies

In Bot Framework API V3, all replies to the user will be sent asynchronously over a separately initiated HTTP request rather than inline with the HTTP POST for the incoming message to the bot.  Since no message will be returned inline to the user through the Connector, the return type of your bot's post method will be HttpResponseMessage.  This means that your bot doesn't synchronously "return" the string that you wish to send to the user, but instead sends a reply message at any point in your code instead of having to reply back as a response to the incoming POST:

- SendToConversation*
- ReplyToConversation*

The difference between the two is that for conversations that support it, ReplyToConversation will attempt to thread the conversation such as in e-mail.

## Bot Data Storage (Bot State)

In BotFramework V1 API, the Bot data APIs were folded into the Messaging API, which was somewhat confusing.  Instead, the Bot data APIs have been separated out into their own API, called BotState.  Read more on [Bot State](/en-us/csharp/builder/sdkreference/connector.html#trackingstate) here.

Action:

- Call to BotState API to get your state instead of assuming it will be on the message object
- Call to BotState API to store your state instead of passing it as part of the message object

## Creating New Conversations

Determining how to create new conversation with the user, and understanding how that user would be addressed in the V1 API was confusing.  The V3 API has made this more clear by adding the CreateDirectConversation and CreateConversation methods for direct messages (DMs) and open discussions respectively.  For example, to create a DM:

{% highlight csharp %}

var conversation = 
    await connector.Conversations.CreateDirectConversationAsync(
        incomingMessage.Recipient, incomingMessage.From);
addedMessage.Conversation = new conversationAccount(id: conversation.Id);
var reply = await connector.Conversations.SendToConversationAsync(addedMessage);

{% endhighlight %}

Read more about [Creating Conversations](/en-us/csharp/builder/sdkreference/connector.html#conversation) here.

## Attachments and Options

The V1 API had support for attachments and options, however the design was somewhat incomplete.  The V3 API has a cleaner implementation of attachments and cards.  The Options type itself has been removed and replaced with cards.  The full topic on [Attachments](/en-us/csharp/builder/sdkreference/connector.html#attachmentscardsactions) is here.

## Updates to the Auth properties in Web.Config

In V1, the authentication properties were stored with these keys:

- AppID
- AppSecret

In V3, to reflect changes to the underlying auth model, these keys have been changed to:

- MicrosoftAppID
- MicrosoftAppPassword

