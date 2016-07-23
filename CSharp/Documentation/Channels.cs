namespace Microsoft.Bot.Builder.Connector
{
    /**
\page channels Channels Custom Channel Capabilities
If you want to be able to take advantage of special features or concepts for a channel we provide a way for you to send native
metadata to that channel giving you much deeper control over how your %bot interacts on a channel. The way you do this is to pass
extra properties via the *ChannelData* property.

>NOTE: You do not need to use this feature unless you feel the need to access functionality not provided by the normal Activity.

\section customemailmessages Custom Email Messages
The Email channel optionally supports the ability to send custom properties to take advantage of what you can do with email.


When you receive a message via email the channelData will have metadata from the source message.


When you reply to a message via the email channel you can specify the following properties:

| **Property** | **Description**
|---------|  -----
| *HtmlBody*   | The HTML to use for the body of the message
| *Subject*    | The subject to use for the message
| *Importance* | The importance flag to use for the message (Low/Normal/High)


Example Message:

~~~{.json}

    {
        "type": "message",
        "locale": "en-Us",
        "channelID":"email",
        "from": { "id":"mybot@gmail.com", "name":"My bot"},
        "recipient": { "id":"joe@gmail.com", "name":"Joe Doe"},
        "conversation": { "id":"123123123123", "topic":"awesome chat" },
        "channelData":
        {
            "htmlBody" : "<html><body style = \"font-family: Calibri; font-size: 11pt;\" >This is more than awesome</body></html>",
            "subject":"Super awesome message subject",
            "importance":"high"
        }
    }

~~~


\section customslackmessages Custom Slack Messages
           Slack supports the ability to create full fidelity slack messages. The slack
channel allows bots to pass custom Slack messages via the ChannelData field. Custom messages passed via ChannelData will
be posted directly to Slack via their chat.postMessage api.

> See [Slack Messages](https://api.slack.com/docs/messages) for a description of the Slack message format
>
> See [Slack Attachments](https://api.slack.com/docs/attachments) for a description of Slack attachments

Example outgoing message with custom Slack message in ChannelData:

~~~{.json}
{
    "type": "message",
    "locale": "en-Us",
    "channelId":"slack",
    "conversation": { "id":"123123123123", "topic":"awesome chat" },
    "from": { "id":"12345", "name":"My Bot"},
    "recipient": { "id":"67890", "name":"Joe Doe"},
    "channelData":
    {
        "text": "Now back in stock! :tada:",
        "attachments": [
            {
                "title": "The Further Adventures of Slackbot",
                "author_name": "Stanford S. Strickland",
                "author_icon": "https://api.slack.com/img/api/homepage_custom_integrations-2x.png",
                "image_url": "http://i.imgur.com/OJkaVOI.jpg?1"
            },
            {
                "fields": [
                    {
                        "title": "Volume",
                        "value": "1",
                        "short": true
                    },
                    {
                        "title": "Issue",
                        "value": "3",
                        "short": true
                    }
                ]
            },
            {
                "title": "Synopsis",
                "text": "After @episod pushed exciting changes to a devious new branch back in Issue 1, Slackbot notifies @don about an unexpected deploy..."
            },
            {
                "fallback": "Would you recommend it to customers?",
                "title": "Would you recommend it to customers?",
                "callback_id": "comic_1234_xyz",
                "color": "#3AA3E3",
                "attachment_type": "default",
                "actions": [
                    {
                        "name": "recommend",
                        "text": "Recommend",
                        "type": "button",
                        "value": "recommend"
                    },
                    {
                        "name": "no",
                        "text": "No",
                        "type": "button",
                        "value": "bad"
                    }
                ]
            }
        ]
    },
    ...
}
~~~

When a user clicks a button in Slack, a message will be sent to your bot with _ChannelData_ containing a _Payload_ corresponding to the message action.
The payload contains the original message as well as information about which button was clicked and who clicked it. Your bot can then take whatever 
action is necessary in response to the button click, including modifying the original message and posting to directly back to Slack via the _response_url_
that's included in the payload.

> See [Slack Buttons](https://api.slack.com/docs/message-buttons) for a description of interactive Slack messages
>
> To support Slack buttons, you must follow the instructions to enable Interactive Messages when [configuring](https://dev.botframework.com/bots) your bot on the Slack channel.

Example incoming button click message:

~~~{.json}
{
    "type": "message",
    "serviceUrl": "https://slack.botframework.com",
    "channelId": "slack",
    "from": {...},
    "conversation": {...},
    "recipient": {...},
    "text": "recommend",
    "entities": [...],
    "channelData": {
        "Payload": {
            "actions": [
            {
                "name": "recommend",
                "value": "recommend"
            }
            ],
            "callback_id": "comic_1234_xyz",
            "team": {...},
            "channel": {...},
            "user": {...},
            "attachment_id": "3",
            "token": "...",
            "original_message": {
            "text": "New comic book alert!\n",
            "username": "TestBot-V3 (Prod)",
            "bot_id": "B1Q3CDE1M",
            "attachments": [
                {
                "fallback": "332x508px image",
                "image_url": "http://i.imgur.com/OJkaVOI.jpg?1",
                "image_width": 332,
                "image_height": 508,
                "image_bytes": 60672,
                "author_name": "Stanford S. Strickland",
                "title": "The Further Adventures of Slackbot",
                "id": 1,
                "author_icon": "https://api.slack.com/img/api/homepage_custom_integrations-2x.png",
                "fields": [
                    {
                    "title": "Volume",
                    "value": "1",
                    "short": true
                    },
                    {
                    "title": "Issue",
                    "value": "3",
                    "short": true
                    }
                ]
                },
                {
                "text": "After @episod pushed exciting changes to a devious new branch back in Issue 1, Slackbot notifies @don about an unexpected deploy...",
                "title": "Synopsis",
                "id": 2,
                "fallback": "NO FALLBACK DEFINED"
                },
                {
                "callback_id": "comic_1234_xyz",
                "fallback": "Would you recommend it to customers?",
                "title": "Would you recommend it to customers?",
                "id": 3,
                "color": "3AA3E3",
                "actions": [
                    {
                    "id": "1",
                    "name": "recommend",
                    "text": "Recommend",
                    "type": "button",
                    "value": "recommend"
                    },
                    {
                    "id": "2",
                    "name": "no",
                    "text": "No",
                    "type": "button",
                    "value": "bad"
                    }
                ]
                }
            ],
            "type": "message",
            "subtype": "bot_message",
            },
            "response_url": "https://hooks.slack.com/actions/..."
        }
    }
}
~~~


\section customfacebookmessages Custom Facebook Messages
The Facebook adapter supports sending full attachments via the channelData field. This allows you to do anything
natively that Facebook supports via the attachment schema, such as receipt.

| **Property** | **Description**
|---------|  -----
| * notification_type*  | Push notification type: REGULAR, SILENT_PUSH, NO_PUSH
| * attachment*  | A Facebook formatted attachment * See[Facebook Send API Reference](https://developers.facebook.com/docs/messenger-platform/send-api-reference#guidelines)*

Example Message:

~~~{.json}

           {
               "type": "message",
               "locale": "en-Us",
               "text": "This is a test",
               "channelID":"facebook", 
               "conversation": { "id":"123123123123", "topic":"awesome chat" },
               "from": { "id":"12345", "name":"My Bot"},
               "recipient": {  "id":"67890", "name":"Joe Doe"},
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
~~~

\section customtelegrammessages Custom Telegram Messages

The Telegram channel supports calling Telegram %Bot API methods via the channelData field. This allows your %bot to perform Telegram-specific actions, such as sharing a voice memo, or a sticker.

| **Property** | **Description**
|---------|  -----
| *method* | The Telegram %Bot API method to call. See below for supported methods.
| *parameters* | Associative array containing method parameters. Parameters are method-specific.

>See the [Telegram Bot API Documentation](https://core.telegram.org/bots/api) for a description of all available methods, parameters, and types.


Special Notes:

1. The `chat_id` parameter is common to all Telegram methods.If not provided, the framework will fill in this value for you.
2. The Telegram channel expresses Telegram's `InputFile` type differently than the way it appears in the [Telegram Bot API](https://core.telegram.org/bots/api#inputfile). Rather than sending the file contents, your %bot should pass the file's `url` and `mediaType`. This is shown in the example message below.
3. When your %bot receives a %Connector message from the Telegram channel, the original Telegram message will be present in the channelData field.

Example Message:

~~~{.json}
{
           "type": "message",
           "locale": "en-Us",
           "channelID":"telegram", 
           "from": { "id":"12345", "name":"My Bot"},
           "recipient": { "id":"67890"}, "name":"Joe Doe"},
           "conversation": { "id":"123123123123", "topic":"awesome chat" },
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
~~~

You may pass multiple Telegram methods as an array:

~~~{.json}
{
           "type": "message",
           "locale": "en-Us",
           "channelID":"telegram", 
           "from": { "id":"12345", "name":"My Bot"},
           "recipient": { "id":"67890"}, "name":"Joe Doe"},
           "conversation": { "id":"123123123123", "topic":"awesome chat" },
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
~~~


Supported Methods:

|         |         |        |
|---------|---------|---------|
| [sendMessage](https://core.telegram.org/bots/api#sendmessage) | [forwardMessage](https://core.telegram.org/bots/api#forwardmessage) | [sendPhoto](https://core.telegram.org/bots/api#sendphoto)
| [sendAudio](https://core.telegram.org/bots/api#sendaudio) | [sendDocument](https://core.telegram.org/bots/api#senddocument) | [sendSticker](https://core.telegram.org/bots/api#sendsticker)
| [sendVideo](https://core.telegram.org/bots/api#sendvideo) | [sendVoice](https://core.telegram.org/bots/api#sendvoice) | [sendLocation](https://core.telegram.org/bots/api#sendlocation)
| [sendVenue](https://core.telegram.org/bots/api#sendvenue) | [sendContact](https://core.telegram.org/bots/api#sendcontact) | [sendChatAction](https://core.telegram.org/bots/api#sendchataction)
| [kickChatMember](https://core.telegram.org/bots/api#kickchatmember) | [unbanChatMember](https://core.telegram.org/bots/api#unbanchatmember) | [answerInlineQuery](https://core.telegram.org/bots/api#answerinlinequery)
| [editMessageText](https://core.telegram.org/bots/api#editmessagetext) | [editMessageCaption](https://core.telegram.org/bots/api#editmessagecaption) | [editMessageReplyMarkup](https://core.telegram.org/bots/api#editmessagereplymarkup)

\section customkikmessages Custom Kik Messages

The Kik adapter supports sending native Kik messages via the channelData field. This allows you to do anything
natively that Kik supports.

| **Property** | **Description**
|---------|  -----
| *messages*  | An array of messages. *See [Kik Messages](https://dev.kik.com/#/docs/messaging#message-formats)*

Example Message:

~~~{.json} 
{
           "type": "message",
           "locale": "en-Us",
           "channelID":"kik", 
           "from": { "id":"12345", "name":"My Bot"},
           "recipient": { "id":"67890"}, "name":"Joe Doe"},
           "conversation": { "id":"123123123123", "topic":"awesome chat" },
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
~~~


    **/
}
