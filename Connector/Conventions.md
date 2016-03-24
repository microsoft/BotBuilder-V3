---
layout: page
title:  Configuration conventions
permalink: /connector/doc-conventions/
weight: 210
parent1: Bot Connector SDK
parent2: Configuring your bot
---

* TOC
{:toc}

## Serialization
All of the objects described use lower-camel casing on the wire.  The C# nuget library uses
strongly typed names that are pascal cased. Our documentation sometimes will use one or the
other but they are interchangable.

| **C# property** | wire serialization | javascript name |
| ----| ---- | ---- |
| ConversationId | conversationId | conversationId|


## Example serialization
{% highlight json %}

{
  "type": "Message",
  "id": "23IG9F0yL0S",
  "conversationId": "GZxAXM39a6jdG0n2HQF5TEYL1vGgTG853w2259xn5VhGfs",
  "created": "2016-03-22T04:19:11.2100568Z",
  "language": "en",
  "text": "You said:test",
  "attachments": [],
  "from": {
    "name": "Test Bot",
    "channelId": "test",
    "address": "TestBot",
    "id": "MyTestBot",
    "isBot": true
  },
  "to": {
    "name": "tom",
    "channelId": "test",
    "address": "tom",
    "id": "1hi3dbQ94Kddb",
    "isBot": false
  },
  "replyToMessageId": "7TvTPn87HlZ",
  "participants": [
    {
      "name": "tom",
      "channelId": "test",
      "address": "tom",
      "id": "1hi3dbQ94sdKb",
      "isBot": false
    },
    {
      "name": "Test Bot",
      "channelId": "test",
      "address": "MyTestBot",
      "id": "MyTestBot",
      "isBot": true
    }
  ],
  "totalParticipants": 2,
  "mentions": [],
  "channelConversationId": "12345",
  "hashtags": []
}

{% endhighlight %}
