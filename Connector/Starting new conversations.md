---
layout: page
title: Starting a conversation
permalink: /connector/new-conversations/
weight: 220
parent1: Bot Connector SDK
---

The thing that makes a message a "reply" message is when it contains the **ConversationId** property.  

* If there is a ConversationId, the message will be routed to the conversation.
* If there is NOT a ConversationId, the message will be considered a request to *create a new conversation* 
with all of the participants.  In this case you can set To.ChannelId and not bother to set To.Address

## Example of starting a new conversation with a user
{% highlight C# %}
    var connector = new ConnectorClient();
    Message message = new Message();
    message.From = botChannelAccount;
    message.To = new ChannelAddress() {ChannelId = "email", "Address":"joe@hotmail.com"};
    message.Text = "Hey, what's up homey?";
    message.Language = "en";
    connector.SendMessage(message);
{% endhighlight %}


## Example of starting a new conversation with a set of users
{% highlight C# %}
    var connector = new ConnectorClient();
    List<ChannelAccount> participants = new List<ChannelAccount>();
    ... add channelaccounts to participants....    
    
    Message message = new Message();
    message.From = botChannelAccount;
    message.To = new ChannelAddress() {ChannelId = "slack"};
    message.Participants = participants.ToArray();
    message.Text = "Hey, what's up everyone?";
    message.Language = "en";
    connector.SendMessage(message);
{% endhighlight %}


> NOTE: All of the ChannelAccounts need to be on the same Channel, aka, they need the same ChannelId value.
