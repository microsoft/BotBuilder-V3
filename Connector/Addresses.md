---
layout: page
title: Addresses
permalink: /connector/channelaccounts/
weight: 204
parent1: Bot Connector SDK
---

The Bot Connector uses ChannelAccount records to represent an contact address for a user or bot
on a communication channel.  Numerous fields in the message have ChannelAccount 
references in them to represent the relationships between the users that are participating in
a conversation. 

## ChannelAccount
The ChannelAccount object is a core object which describes an alias for a user. 

| **Property** | **Description**                     | **Examples**                        
|--------------|-------------------------------------|---------------------------------
|**ChannelId** | The channel that the address is for | email, slack, groupme, sms, etc.
|**Address**   | The address on the channel          | joe@hotmail.com, +14258828080, etc.
|**Name**      | A name for the user or bot          | Joe Smith 
|**Id**        | A global id which represents a bot or user | 1jsk1jkdidr4F

Each user has 1..N ChannelAccounts which represent their identities on each channel.

Each bot has 1..N ChannelAccounts which represent their identities on each channel. 

The Connector is primarily a switch which routes messages between channel account records.

## Message properties that use ChannelAccounts

| **Property** | **Description**                                             
|--------------|-------------------------------------
|**Message.From**       | The channel for the sender         
|**Message.To**         | The channel for the recipient      
|**Message.Participants** | ChannelAccount array of known participants in the conversation           
|**Message.Mentions**   | A collection of user mentions (more on this below)

When you receive a message from a user it will have the From field set to the
user who created the message and the To field will always be your bot's identity
in that conversation.  

>It is important to note that a bot doesn't always know
their identies before hand because some channels assign out new identities for
a bot when a bot is added to a conversation. (For example groupme and slack do this.)

This is why it is important when you are replying to a conversation to create a new 
message which swaps the From and To fields (see [Replying to a message](/connector/replying/) for 
more details). 

The thing that makes a message a "reply" message is if it contains the **ConversationId** property.

* If there is a ConversationId, the message will be routed to the conversation.
* If there is NOT a ConversationId, the message will be considered a request to *create a new conversation* 
with all of the participants.  See [Starting new conversations](/connector/new-conversations/) for more info.

The Message.TotalParticipants field will be used to determine if it is a group setting in order to adjust
the behavior of bots.  In a group setting your bot should probably use *Spoke if spoken to* mode to 
have good user experience.  See [Bot Options](/connector/bot-options/) for details on that.

## Mentions
Many communication clients have mechanisms to "mention" someone.  Knowing that someone is 
mentioned can be an important piece of information for a bot that 
the channel knows and needs to be able to pass to you.  

At the same time, sometimes the bot needs the ability to strip out information that they don't want to have
in their input as it will mess up their pattern matching.

Even more importantly, a bot frequently needs to know that **they** were mentioned, but with some channels
they don't always know what their name is on that channel. 

To accomodate these needs the Message an array of Mention objects.  The Mention object is made up of
* Mentioned - ChannelAccount of the person or user who was mentiond
* Text - (optionally) the text in the message.Text property which represents the mention.

Example:
The user on slack says:

> @ColorBot pick me a new color

{% highlight json %}
    {   
        ...
       "mentions": [{ "Mentioned": { "ChannelId": "slack", "Address":"B1332231" },"Text": "@ColorBot" }]
        ...
    }
{% endhighlight %}

This allows the bot to know that they were mentioned and to ignore the @ColorBot part of the input when
trying to determine the user intent.

> NOTE: Mentions go both ways.  A bot may want to mention a user in a reply to a conversation.  If they fill out the Mentions
object with the mention information then it allows the Channel to map it to the mentioning semantics of the channel.
