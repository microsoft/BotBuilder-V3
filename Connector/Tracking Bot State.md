---
layout: page
title: Tracking Bot State
permalink: /connector/tracking-bot-state/
weight: 208
parent1: Bot Connector
parent2: Messages
---

* TOC
{:toc}


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

When your bot sends a reply you  simply set your object in one of the BotData records properties and it will be persisted and
played back to you on future messages when the context is the same. 

Example of setting the data on a reply message

{% highlight C# %}

    Message replyMessage = sourceMessage.CreateReplyMessage("my response);
    replyMessage.BotPerUserConversationData = myState;
    return replyMessage;
	
{% endhighlight %}

When a new message comes from that user in that conversation the botPerUserConversationData will have your object restored in it, giving 
you access to your persisted state for that user

{% highlight C# %}

    var myState = replyMessage.BotPerUserConversationData;
	
{% endhighlight %}

Our C# nuget client library has extension methods for the Message class to make it easy to deal with setting and getting the object back as a typed value.

{% highlight C#  %}

    // Set a property on the BotUserData 
    public static void SetBotUserData(this Message message, string property, object data)
    
    // Set a property on the BotConversationData 
    public static void SetBotConversationData(this Message message, string property, object data)
    
    // Set a property on the BotPerUserInConversationData 
    public static void SetBotPerUserInConversationData(this Message message, string property, object data)

    // Get a property off of the BotUserData 
    public static TypeT GetBotUserData<TypeT>(this Message message, string property)

    // Get a property off of the BotConversationData 
    public static TypeT GetBotConversationData<TypeT>(this Message message, string property)

    // Get a property off of the BotPerUserInConversationData 
    public static TypeT GetBotPerUserInConversationData<TypeT>(this Message message, string property)
	
{% endhighlight %}

An example of using these helper extensions:

{% highlight C# %}

    MyState myState = new MyState() { ... };
    replyMessage.SetBotConversationData(myState);
    return replyMessage;
    ...
    myState = futureMessage.GetBotConversationData<MyState>();

{% endhighlight %}


## Concurrency
When these botData objects are being set on a message object they are not able to be stored
in a way which guarentees you won't overwrite data from another overlapping message from your bot

For many bots which have low load or simple sequential conversations with non-overlapping messages
 the convenience of just storing your state inline is worth the possibility of stomping on a
previous message.   

Other bots can are sensitive to data getting stomped and desire a more reliable storage system.
For these bots you can use the REST API to store the same BotData records but with ETags to ensure consistency.

Or you can simply use the userId and conversationId to store you own data in your own database.  

Example of using the REST API client library:
{% highlight C# %}
    var client = new ConnectorClient();
    try
    {
        // get the user data object
        var userData = await client.Bots.GetBotUserData(botId: message.To.Id, userId: message.From.Id);
        
        // modify it...
        userData.Data = ...modify...;
        
        // save it
        await client.Bots.SetBotUserData(botId: message.To.Id, userId: message.From.Id, userData);
    }
    catch(HttpOperationException err)
    {
        // handle precondition failed error if someone else has modified your object
    }
{% endhighlight %}
