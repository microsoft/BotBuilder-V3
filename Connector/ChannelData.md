---
layout: page
title:  Custom Channel Messages
permalink: /connector/custom-channeldata/
weight: 207
parent1: Bot Connector
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
        "from": { "channelID":"email", "address":"mybot@gmail.com"},
        "to": { "channelID":"email", "address":"joe@gmail.com"},
        "conversationId":"123123123123",
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
        "conversationId":"123123123123",
        "from": { "channelID":"slack", "address":"12345"},
        "to": { "channelID":"slack", "ddress":"67890"},
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


## Custom Facebook Messages
The Facebook adapter supports sending full attachments via the channelData field.  This allows you to do anything
natively that Facebook supports via the attachment schema, such as reciept.

|**Property** | **Description**
|---------|  -----
|*notification_type*  | Push notification type: REGULAR, SILENT_PUSH, NO_PUSH
|*attachment*  | A Facebook formatted attachment *See [Facebook Send API Reference](https://developers.facebook.com/docs/messenger-platform/send-api-reference#guidelines)*

Example Message:

{% highlight json %}

    {
        "type": "Message",
        "language": "en",
        "text": "This is a test",
        "conversationId":"123123123123",
        "from": { "channelID":"facebook", "address":"12345"},
        "to": { "channelID":"facebook", "address":"67890"},
        "channelData":
        {
            "notification_type" : "NO_PUSH",
            "attachment":
            {
                "type":"template",
                "payload":
                {
                    "template_type":"receipt",
                    "recipient_name":"Stephane Crozatier",
                    "order_number":"12345678902",
                    "currency":"USD",
                    "payment_method":"Visa 2345",
                    "order_url":"http://petersapparel.parseapp.com/order?order_id=123456",
                    "timestamp":"1428444852",
                    "elements":
                    [
                        {
                            "title":"Classic White T-Shirt",
                            "subtitle":"100% Soft and Luxurious Cotton",
                            "quantity":2,
                            "price":50,
                            "currency":"USD",
                            "image_url":"http://petersapparel.parseapp.com/img/whiteshirt.png"
                        },
                        {
                            "title":"Classic Gray T-Shirt",
                            "subtitle":"100% Soft and Luxurious Cotton",
                            "quantity":1,
                            "price":25,
                            "currency":"USD",
                            "image_url":"http://petersapparel.parseapp.com/img/grayshirt.png"
                        }
                    ],
                    "address":
                    {
                        "street_1":"1 Hacker Way",
                        "street_2":"",
                        "city":"Menlo Park",
                        "postal_code":"94025",
                        "state":"CA",
                        "country":"US"
                    },
                    "summary":
                    {
                        "subtotal":75.00,
                        "shipping_cost":4.95,
                        "total_tax":6.19,
                        "total_cost":56.14
                    },
                    "adjustments":
                    [
                        {
                            "name":"New Customer Discount",
                            "amount":20
                        },
                        {
                            "name":"$10 Off Coupon",
                            "amount":10
                        }
                    ]
                }
            }
        }
    }
{% endhighlight %}

## Custom Telegram Messages

The Telegram channel supports calling Telegram Bot API methods via the channelData field.  This allows your bot to perform Telegram-specific actions, such as sharing a voice memo, or a sticker.

|**Property** | **Description**
|---------|  -----
|*method* | The Telegram Bot API method to call. See below for supported methods.
|*parameters* | Associative array containing method parameters. Parameters are method-specific.

>See the [Telegram Bot API Documentation](https://core.telegram.org/bots/api) for a description of all available methods, parameters, and types.

Special Notes:

1. The `chat_id` parameter is common to all Telegram methods. If not provided, the framework will fill in this value for you.
2. The Telegram channel expresses Telegram's `InputFile` type differently than the way it appears in the [Telegram Bot API](https://core.telegram.org/bots/api#inputfile). Rather than sending the file contents, your bot should pass the file's `url` and `mediaType`. This is shown in the example message below.
3. When your bot receives a Connector message from the Telegram channel, the original Telegram message will be present in the channelData field.

Example Message:

{% highlight json %}
{
    "type": "Message",
    "from": { "channelID":"telegram", "address":"12345"},
    "to": { "channelID":"telegram", "address":"67890"},
    "conversationId":"123123123123",
    "channelData":
    {
        "method": "sendSticker",
        "parameters":
        {
            "sticker":
            {
                "url": "https://upload.wikimedia.org/wikipedia/commons/3/33/LittleCarron.gif",
                "mediaType": "image/gif"
            }
        }
    }
}
{% endhighlight %}

You may pass multiple Telegram methods as an array:

{% highlight json %}
{
    "type": "Message",
    "from": { "channelID":"telegram", "address":"12345"},
    "to": { "channelID":"telegram", "address":"67890"},
    "conversationId":"123123123123",
    "channelData":
    [
        {
            "method": "sendSticker",
            "parameters":
            {
                "sticker":
                {
                    "url": "http://www.gstatic.com/webp/gallery/1.webp",
                    "mediaType": "image/webp"
                }
            }
        },
        {
            "method": "sendMessage",
            "parameters":
            {
                "text": "<b>This message is HTML-formatted.</b>",
                "parse_mode": "HTML"
            }
        }
    ]
}
{% endhighlight %}


Supported Methods:


|---------|---------|---------
| [sendMessage](https://core.telegram.org/bots/api#sendmessage) | [forwardMessage](https://core.telegram.org/bots/api#forwardmessage) | [sendPhoto](https://core.telegram.org/bots/api#sendphoto)
| [sendAudio](https://core.telegram.org/bots/api#sendaudio) | [sendDocument](https://core.telegram.org/bots/api#senddocument) | [sendSticker](https://core.telegram.org/bots/api#sendsticker)
| [sendVideo](https://core.telegram.org/bots/api#sendvideo) | [sendVoice](https://core.telegram.org/bots/api#sendvoice) | [sendLocation](https://core.telegram.org/bots/api#sendlocation)
| [sendVenue](https://core.telegram.org/bots/api#sendvenue) | [sendContact](https://core.telegram.org/bots/api#sendcontact) | [sendChatAction](https://core.telegram.org/bots/api#sendchataction)
| [kickChatMember](https://core.telegram.org/bots/api#kickchatmember) | [unbanChatMember](https://core.telegram.org/bots/api#unbanchatmember) | [answerInlineQuery](https://core.telegram.org/bots/api#answerinlinequery)
| [editMessageText](https://core.telegram.org/bots/api#editmessagetext) | [editMessageCaption](https://core.telegram.org/bots/api#editmessagecaption) | [editMessageReplyMarkup](https://core.telegram.org/bots/api#editmessagereplymarkup)

## Custom Kik Messages

The Kik adapter supports sending native Kik messages via the channelData field.  This allows you to do anything
natively that Kik supports.

|**Property** | **Description**
|---------|  -----
|*messages*  | An array of messages. *See [Kik Messages](https://dev.kik.com/#/docs/messaging#message-formats)*

Example Message:

{% highlight json %} 
{
    "type": "Message",
    "from": { "channelID":"kik", "address":"12345"},
    "to": { "channelID":"kik", "address":"67890"},
    "conversationId":"123123123123",
	"channelData": {
		"messages": [
		{
			"chatId": "c6dd81652051b8f02796e152422cce678a40d0fb6ad83acd8f91cae71d12f1e0",
			"type": "link",
			"to": "kikhandle",
			"title": "My Webpage",
			"text": "Some text to display",
			"url": "http://botframework.com",
			"picUrl": "http://lorempixel.com/400/200/",
			"attribution": {
				"name": "My App",
				"iconUrl": "http://lorempixel.com/50/50/"
			 },
			"noForward": true,
			"kikJsData": {
			  "key": "value"
			}
		}
		]
	}
}
{% endhighlight %}