---
layout: page
title: Replying to a message
permalink: /connector/replying/
weight: 202
parent1: Bot Connector
parent2: Routing
---


* TOC
{:toc}

When your bot receives a message it most likely will want to respond. The minimum amount of information that is needed
to respond is to send back the text that you want to send back to the user as a reply.  

To do that, you need a new Message() with
 
* The From and To fields swapped from the original message (so that it will be routed back to where it came from)
* The conversationId from the original message on it (so you can send it back to the same conversation)
* The new Text
* The language of your text.

To make this super easy to do in C# we created an extension method on the message class called **CreateReplyMessage()**.

To create a proper reply message all you have to do is:
{% highlight C# %}
    var replyMessage = incomingMessage.CreateReplyMessage("Yo, I heard you.", "en");
{% endhighlight %}

## Replying to a message immediately
When you receive a message if you are able to return directly you can just return your reply message as the response.

{% highlight C# %}
    public Message Post(Message incomingMessage)
    {
        return incomingMessage.CreateReplyMessage("Yo, I got it.", "en");
    }
{% endhighlight %}

## Don't send a reply
You do not always need to reply inline.  It is perfectly acceptable to return empty body with a HttpStatusCode.OK 
or HttpStatusCode.NoContent to signify that there is no reply.

{% highlight C# %}
    public Message Post(Message incomingMessage)
    {
        // no reply
        return null;
    }
{% endhighlight %}
 
## Replying to the message later

If you want to reply at a later point, it is almost exactly the same process.  You create a reply message and
send it to the user, only you do it through an explicit POST to /bots/V1/messages endpoint.

{% highlight C# %}
    var replyMessage =  incomingMessage.CreateReplyMessage("Yo, I heard you.", "en");
    return null; // no reply
    ...

    // send the reply later    
    var connector = new ConnectorClient();
    await connector.Messages.SendMessageAsync(replyMessage); 
       
{% endhighlight %}

## Multiple replies
The reply mechanism is really a "add message to conversation", and so there is nothing wrong with sending
multiple replies at a later point. 

{% highlight C# %}
    var connector = new ConnectorClient();
    connector.Messages.SendMessage(incomingMessage.CreateReplyMessage("Yo, I heard you 1.", "en"));
    connector.Messages.SendMessage(incomingMessage.CreateReplyMessage("Yo, I heard you 2.", "en"));
    connector.Messages.SendMessage(incomingMessage.CreateReplyMessage("Yo, I heard you 3.", "en"));
{% endhighlight %}

> NOTE: When you send a reply at a later time like this (*what we call an async reply*) your bot
is subject to throttling.

## Including bot state with your reply 
Just like any message, if you set one of the BotData fields on the message when it is routed through the
switch it will be persisted.  The next time you receive a message from that source you will
get the same object back. Read [Tracking Bot State](/connector/tracking-bot-state) to get details on tracking
bot state via messages.  


