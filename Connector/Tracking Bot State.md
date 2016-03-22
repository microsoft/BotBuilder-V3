---
layout: page
title: Tracking Bot State
permalink: /connector/tracking-bot-state
weight: 270
parent1: Bot Connector SDK
---

# Tracking Bot State
If a bot is implemented in a stateless way then it is very easy to scale your bot to handle load. 

Unfortunately a bot is all about conversations and as soon as you introduce conversation into a bot then
your bot needs to track state in order to remember things like "what was the last question I asked them?". 

We make it easy for the bot developer to track this information because we provide contextual information that
they can use to store data in their own store or database.

In addition, we provide a simple cookie like system for tracking state that makes it super easy for most bots to not have 
to worry about having their own store.

## Contextual properties
Every message has several properties which are useful for tracking state.

|**Property**                    | **Description**                                        | **Use cases**                                                
|----------------------------|----------------------------------------------------|----------------------------------------------------------
|**From.Id**                     | An ID for the user across all channels and conversations| Remembering context for a user
|**From.Channel + From.Address** | A Users's address on a channel (ex: email address) | Remembering context for a user on a channel                 
|**ConversationId**              | A unique id for a conversation                     | Remembering context all users in a conversation    
|**From.Id + ConversationId**    | A user in a conversation                           | Remembering context for a user in a conversation   

You can use these keys to store information in your own database as appropriate to your needs.

## Message BotData properties
After writing a bunch of bots we came to the realization that many bots have pretty simple needs for tracking state. 
To support this case we allow a state object to be sent as part of the bot's message which will be persisted
and played back to them when new messages come back from those sources.

Here are the properties on the message. 

|**Property**                            | **Description**                                                | **Use cases**                                                
|------------------------------------|------------------------------------------------------------|----------------------------------------------------------
|**message.BotUserData**                 | an object saved based on the from.Id                       | Remembering context object with a user
|**message.BotConversationData**         | an object saved based on the conversationId                | Remembering context object with a conversation
|**message.BotPerUserInConversationData**| an object saved based on the from.Id + conversationId      | Remembering context object with a person in a conversation

When your bot sends a reply you  simply set your object in one of the BotData records properties and it be persisted and
played back to you on future messages. 

Example of setting the data on a reply message
{% highlight C# %}

    Message replyMessage = sourceMessage.CreateReplyMessage("my response);
    replyMessage.BotPerUserConversationData = myState;
    return replyMessage;
	
{% endhighlight %}

*When a new message comes from that user in that conversation the botPerUserConversationData will have your state*
{% highlight C# %}

    var myState = replyMessage.BotPerUserConversationData;
	
{% endhighlight %}

With the C# nuget library the message has methods to make it easy to deal with setting and getting the object back as a typed value.

    /// Set BotUserData as a versioned record
    public static void SetBotUserData(this Message message, object data, string version = v1)
    
    /// Set BotConversationData as a versioned record
    public static void SetBotConversationData(this Message message, object data, string version = v1)
    
    /// Set BotPerUserInConversationData as a versioned record
    public static void SetBotPerUserInConversationData(this Message message, object data, string version = v1)

    /// Get BotUserData based on version
    public static TypeT GetBotUserData<TypeT>(this Message message, string version = v1)

    /// Get BotConversationData based on version
    public static TypeT GetBotConversationData<TypeT>(this Message message, string version = v1)

    /// Get BotPerUserInConversationData based on version
    public static TypeT GetBotPerUserInConversationData<TypeT>(this Message message, string version = v1)

An example of using these helper extensions:

   
    MyState myState = new MyState() { ... };
    replyMessage.SetBotConversationData(myState);
    return replyMessage;
    ...
    myState = futureMessage.GetBotConversationData<MyState>();


> NOTE: These properties are not concurrent under heavy load, but most bots don't have situations where they have lots
> of overlapping messages with the same people in the same conversation.  If your bot is sensitive to this overlapping 
> messages you should store the data in your own database where you can control the concurrency.

 