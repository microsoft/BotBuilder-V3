---
layout: page
title: Overview
permalink: /en-us/core-concepts/overview/
weight: 1000
parent1: Core Concepts
---

Microsoft Bot Framework is a comprehensive offering that you use to build and deploy high quality bots for your users to enjoy wherever they are talking. A bot is a web service that interacts with users in a conversational format. Users can start conversations with your bot from any channel that you've configured your bot to work on (for example, SMS, Skype, Slack, Facebook, and other popular services). 

You can design conversations to be freeform, natural language interactions or more guided ones where you provide the user choices or actions. The conversation can utilize simple text strings or something more complex such as rich cards that contain text, images, and action buttons.

The following conversation shows a bot that schedules solon appointments. The bot understands the user's intent, presents appointment options using action buttons, displays the user's selection when they tap an appointment, and then sends a confirmation prompt and a thumbnail card that contains the appointment's specifics. 

![solon bot example](/en-us/images/connector/salon_bot_example.png)

You can host your bot on any reachable service such as Azure. For information about hosting your bot on Azure, see [Publishing your Bot to Microsoft Azure](/en-us/csharp/builder/sdkreference/gettingstarted.html).

The Bot Connector service is a component of the Bot Framework that lets your bot easily connect with users on channels that you configure when you registered your bot. The Connector service sits between your bot and the channels, and passes the messages between them. The connector also normalizes the messages that the bot sends the channel, if necessary. Normalizing a message involves converting it from the Bot Framework's schema into the channel's schema. In cases where the channel does not support all aspects of the framework's schema, the Connector will try to convert the message to a format that the channel supports. For example, if the bot sends a message that contains a card with action buttons to the SMS channel, the Connector may render the card as an image and include the actions in the message's text (for example, Reply with Confirm). 

To send and receive messages from the connector, you can use the Bot Connector REST API. The API is a collection of endpoints and JSON objects that you use to start conversations, send messages, add attachments, and get members of the conversation. For details about the API, see [Bot Connector REST API](../reference).

The following sections provide details about building your bot by using the REST API.

|[Authentication](../authentication.md)|To use the REST API, each call must include an Authorization header and access token. This section describes how to get an access token, and how to verify that the messages you receive are coming from the bot connector service.
|[Starting a conversation](../conversation)|The user starts most conversations but sometimes you may want to start the conversation. For example, if you know the user is interested in a topic and you want to alert them to a related event or news article. This section describes how to start a conversation.
|[Sending and receiving messages](../messages)|A conversation is a series of messages between your bot and the user. This section describes a message and how to send a reply.
|[Adding attachments to a message](../attachments)|A message can be a simple text reply or something more complex such as a Hero card that contains text, images, and action buttons. This section describes how to add attachments such as images, links, and rich cards.
|[Adding channel-specific attachments](../channeldata)|Some channels provide features that require additional data that can't be described using the attachment schema. This section describes how to add channel-specific attachments to a message.
|[Saving user data](,,/userdata)|A bot may save data about the user, a conversation, or a single user within a conversation. This section describes how to save user data.

The Bot Framework also provides .NET and JavaScript developers Bot Builder SDKs that they can use to build their bots. In addition to modeling the Connector service, the SDKs also provide dialogs and form flows that you can use for guided conversations such as ordering a sandwhich. For details, see [Bot Builder for .NET](../../csharp/builder/overview) and [Bot Builder for Node.js](../../csharp/builder/overview).

One of the keys to building a great bot is effectively determining the user's intent when they ask your bot to do something. Determining intent can be handled by using regular expressions or by using a natural language service such as Microsoft's Language Understanding Intelligent Service [LUIS]( https://www.luis.ai/). 

Bing Cognitive services provides Vision, Speech, Language, and Search APIs that you can use to add intellignce to your bot. For details, see [Bot Intelligence](../../bot-intelligence/getting-started).

