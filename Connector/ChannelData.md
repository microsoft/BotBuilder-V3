---
layout: page
title:  Custom messages for channels
permalink: /connector/custom-channeldata/
weight: 207
parent1: Bot Connector SDK
parent2: Messages
---

* TOC
{:toc}

## Message.ChannelData Property
The  default message with markdown gives you a pretty rich pallete to describe your response in way that allows your message to "just work" across
a variety of channels.  Most of the heavy lifting is done by the channel adapter, adapating your message to the way it is expressed on that channel.

If you want to be able to take advantage of special features or concepts for a channel we provide a way for you to send native 
metadata to that channel giving you much deeper control over how your bot interacts on a channel.  The way you do this is to pass 
extra properties via the *ChannelData* property.

>NOTE: You do not need to use this feature unless you feel the need to access functionality not provided by the normal message. 


## Custom Email Messages
The Email channel optionally supports the ability to send custom properties to take advantage of what you can do with email.

When you receive a message via email the channelData will have metadata from the source message.  

When you reply to a message via the email channel you can specify the following properties:

|**Property** |**Description**
|---------|  -----
|*HtmlBody*   | The HTML to use for the body of the message
|*Subject*    | The subject to use for the message
|*Importance* | The importance flag to use for the message (Low/Normal/High)


Example Message:
{% highlight json %}
    {
        "type": "Message", 
        "language": "en", 
        "text": "I send you awesome message", 
        "channelData": 
        {
            "htmlBody" : "<html><body style = \"font-family: Calibri; font-size: 11pt;\" >This is more than awesome</body></html>",
            "subject":"Super awesome mesage subject",
            "importance":"high"
        }
    }
{% endhighlight %}


## Custom Slack Messages
Slack supports the ability to create full fidelity slack cards using their message attachments property.  The slack
channel gives access to this via the channelData field.

> See [Slack Message Attachments](https://api.slack.com/docs/attachments) for a description of all of the properties
that go into the attachments property

|**Property** | **Description**
|---------|  -----
|*attachments*  | An array of attachments *See [Slack Message Attachments](https://api.slack.com/docs/attachments)*
|*unfurl_links*  | true or false *See [Slack unfurling](https://api.slack.com/docs/unfurling)*
|*unfurl_media*  | true or false *See [Slack unfurling](https://api.slack.com/docs/unfurling)*

When slack processes a bot connector message it will use the normal message properties to create a slack message, and
then it will merge in the values from the *channelData* property if they are provided by the sender. 

Example Message:

{% highlight json %}
    {
        "type": "Message", 
        "language": "en", 
        "text": "This is a test", 
        "channelData": 
        {
            "attachments": [
                {
                    "fallback": "Required plain-text summary of the attachment.",

                    "color": "#36a64f",

                    "pretext": "Optional text that appears above the attachment block",

                    "author_name": "Bobby Tables",
                    "author_link": "http://flickr.com/bobby/",
                    "author_icon": "http://flickr.com/icons/bobby.jpg",

                    "title": "Slack API Documentation",
                    "title_link": "https://api.slack.com/",

                    "text": "Optional text that appears within the attachment",

                    "fields": [
                        {
                            "title": "Priority",
                            "value": "High",
                            "short": false
                        }
                    ],

                    "image_url": "http://my-website.com/path/to/image.jpg",
                    "thumb_url": "http://example.com/path/to/thumb.png"
                }
            ],   
            "unfurl_links":false,
            "unfurl_media":false,
        },
        ... 
    }
{% endhighlight %}


## Custom Twitter Messages
The Twitter channel optionally supports the ability to create custom [twitter cards](https://dev.twitter.com/cards/overview) via the channelData property.  


|**Property**   | ** Description**
|---------------| -----
|*cardData*   | An array of name content pairs

When slack processes a bot connector message it will use the normal message properties to create a slack message, and
then it will merge in the values from the *channelData* property if they are provided by the sender. 

Example Message:

{% highlight json %}
    {
        "type": "Message", 
        "language": "en", 
        "text": "This is a test", 
        "channelData": 
        {
            "cardData": [
                { "name" : "twitter:card", "content": "summary" },
                { "name" : "twitter:site", "content": "@flickr" },
                { "name" : "twitter:title", "content": "Small Island Developing States Photo Submission" },
                { "name" : "twitter:description", "content": "View the album on Flickr." },
                { "name" : "twitter:image", "content": "https://farm6.staticflickr.com/5510/14338202952_93595258ff_z.jpg" },
            ],   
        },
        ... 
    }
{% endhighlight %}
  