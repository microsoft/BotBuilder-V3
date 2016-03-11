---
layout: page
title: Developer tips
permalink: /bot-connector-developer-tips/
weight: 350
parent1: Bot Connector SDK
---

* TOC
{:toc}


## New user?
To determine if this is a new user, bot's can store data in the "BotUser" attribute of the message such as newUser=False when responding to the user.  Any returning user after that will have the newUser=False attribute set.

## @Mentions behaviour

On Incoming messages to the Bot, there is an @mentions record that includes all of the people that were mentioned in the message.  The default template strips those tokens out of the message body at entry to the Post.

For an Outbound message, the intent is to follow the same behavior, but requires some work on the part of the developer:

1. A "ReplyMessage" generated from Message.CreateReplyMessage() will automatically prefix the outgoing message with a mention for the incoming messages From ChannelAccount even if not included in the text
* Note, if using new Message(), the first mention will not automatically be included in the output
2. To add additional mentions, the developer must:
* Put a replaceable string token for each user to be mentioned in the output text
* Add Mention objects to the Mentions List on the message that contain both the ChannelAccount object for the mentioned user, and the string token that represents them in the outgoing message
3. The channel adapter will automatically do the appropriate string replace functions with channel-specific tokens, and in the case the mentioned user is not found in the text, will append additional tokens  for the ChannelAccounts to the end of the text
	
