namespace Microsoft.Bot.Builder.Connector
{
    /**
\page activities Activities
An Activity is the object that is used to communicate between a user and a bot. When you send an Activity
there are a number of properties that you can use to control your message and how it is presented to the
user when they receive it.

There more than one type of Activity which are used to convey system operations or channel system operations
to the bot.  They exist to give the %bot information about the state of the channel and the opportunity to respond
to them.

Each Activity being routed through the %Connector has a Type field. Primarily, these will be of type message unless they are system
notifications for the %Bot.

This table gives you basic overview of the Activity types:

| **ActivityType**              | **Description**                                                               | 
| ------------------------------|-------------------------------------------------------------------------------|
| **message**                   | a simple communication between a user <-> %bot                                | 
| **conversationUpdate**        | your %bot was added to a conversation or other conversation metadata changed  |
| **contactRelationUpdate**     | The %bot was added to or removed from a user's contact list                   |
| **typing**                    | The user or %bot on the other end of the conversation is typing               |
| **ping**                      | an activity sent to test the security of a bot.  |
| **deleteUserData**            | A user has requested for the bot to delete any profile / user data      | 


## conversationUpdate
> the membership or metadata of a conversation involving the %bot changed

Your %bot often needs to know when the state of the conversation it's in has changed.  This may represent the %bot being added to 
the conversation, or a person added or remove from the chat.  When these changes happen, your %bot will receive a conversationUpdate 
Activity.

In this event, the membersAdded and membersRemoved lists will contain the changes to the conversation since the last event. One of 
the members may be the Bot; which can be tested for by comparing the membersAdded\[n].id field with the recipient.id field. 

conversationUpdate is a great opportunity for the %Bot to send welcome messages to users.

## contactRelationUpdate
> The %bot was added to or removed from a user's contact list

For some channels your %bot can be a member of the user's contact list on that chat service (Skype for example). In the 
event the channel supports this action, it can notify the %Bot that this has occurred. When this event is delivered, 
the **Action** property will indicate whether the operation was an **add** or a **remove**.

## typing
> A message that indicates that the user or %Bot is typing

Typing is an indicator of activity on the other side of the conversation.  Generally it's used by Bots to 
cover "dead air" while the %bot is fulfilling a request of some sort.  The %Bot may also receive Typing 
messages from the user, for whatever purposes it might find useful.

## ping
> A message that is used to test that a %Bot has implemented security correctly
The bot receiving this should not send any response except for the HttpStatusCode response of OK, Forbidden or Unauthorized 

## deleteUserData
> A compliance request from the the user to delete any profile / user data 

Bots have access to users conversation data.  Many countries have legal requirements that a user
has the ability to request their data to be dropped.  If you receive a message of this type
you should remove any personally identifyable information (PII) for the user.  

\section message Message Activity
The message activity is the core object exchanged between the user and the bot.  It can represent a wide range of values from simple text input 
and response all the way to complex multiple card carousel with buttons and actions

\subsection text Text and Locale 
For many developers the Text property is the only property you need to worry about. A person sent you some text, or 
your %bot is sending some text back.  There are 2 core properties for this, the Text and Locale property.

| Property    | Description                               | Example
| ------------|-------- ----------------------------------| ----------
| **Text**    | A text payload in markdown syntax which will be rendered as appropriate on each channel| Hello, how are you?
| **Locale**  | The locale of the sender (if known)       | en

If all you do is give exchange simple one-line text responses, you don't have to read any further.

\subsection textformat Text Format
Each message has an optional .TextFormat property which represents how to interpret the Text property.  

| TextFormat Value | Description                               |  Notes |
| ------------ |------------------------------------------| --------|
| **plain**    | The text should be treated as raw text and no formatting applied at all |  |
| **markdown** | The text should be treated as markdown formatting and rendered on the channel as appropriate | *default* |
| **xml**      | The text is simple xml markup (subset of html) | *Skype Only* |

\subsubsection markdown Markdown 
The default text format is markdown which allows a nice balance of the bot being able to express what they want 
and for the each channel to render as accurately as they can.

The markdown that is supported:

|Style               | Markdown                                                               |Description                              | Example                                                             
|--------------------| -----------------------------------------------------------------------|-----------------------------------------| ------ -------------------------------------------------------------
| **Bold**           | \*\*text\*\*                                                           | make the text bold                      | **text**                                                            
| **Italic**         | \*text\*                                                               | make the text italic                    | *text*
| **Header1-5**      | # H1                                                                   | Mark a line as a header                 | #H1                                                       
| **Strikethrough**  | \~\~text\~\~                                                           | make the text strikethrough             | ~~text~~
| **Hr**             | \-\-\-                                                                 | insert a horizontal rule                |                                                                    |   
| **Unordered list** | \*                                                                     | Make an unordered list item             | * text
| **Ordered list**   | 1.                                                                     | Make an ordered list item starting at 1 | 1. text                                                          
| **Pre**            | \`text\`                                                               | Preformatted text(can be inline)        | `text`                                                              
| **Block quote**    | \> text                                                                | quote a section of text                 | > text                                                              
| **link**           | \[bing](http://bing.com)                                               | create a hyperlink with title           | [bing](http://bing.com)                                             
| **image link**     | \![duck]\(http://aka.ms/Fo983c) | link to an image                     | ![duck](http://aka.ms/Fo983c)

#### Markdown Paragraphs
As with most markdown systems, to represent a paragraph break you need to have a blank line.

Markdown like this:

           This is
           paragraph one

           This is 
           paragraph two


Will be rendered as

           This is paragraph one
           This is paragraph two

#### Markdown Fallback

Not all channels can represent all markdown fields.  As appropriate channels will fallback to a reasonable approximation, for 
example, bold will be represented in text messaging as \*bold\* 

> Tables: If you are communicating with a channel which supports fixed width fonts or html you can use standard table
> markdown, but because many channels (such as SMS) do not have a known display width and/or have variable width fonts it 
> is not possible to render a table properly on all channels.      

\subsubsection xml Xml 
Skype supports a subset of html tags it calls Xml Markup (sometimes referred to as XMM).

The tags that are supported are:

|Style               | Markdown                                                               |Description                              | Example                                                             
|--------------------| -----------------------------------------------------------------------|-----------------------------------------| ------ -------------------------------------------------------------
| **Bold**           | &lt;b&gt;text&lt;/b&gt;                                                | make the text bold                      | **text**                                                            
| **Italic**         | &lt;i&gt;text&lt;/i&gt;                                                | make the text italic                    | *text*
| **Underline**      | &lt;u&gt;text&lt;/u&gt;                                                | Mark a line as underline                | text
| **Strikethrough**  | &lt;s&gt;text&lt;/s&gt;                                                | make the text strikethrough             | text
| **link**           | &lt;a href="http:bing.com"&gt;bing&lt;/a&gt;                           | create a hyperlink with title           | [bing](http://bing.com)                                             


\subsection Entities
The Entities property is an array of open ended schema.org objects which allows the exchange of common contextual metadata between the channel and bot. 
For example, if a channel supports location information it could pass along that information as a schema.org location object.

\subsubsection mentions Mention Entities
Many communication clients have mechanisms to "mention" someone. Knowing that someone is 
mentioned can be an important piece of information for a %bot that the channel knows and needs to be able 
to pass to you.

Frequently a %bot needs to know that __they__ were mentioned, but with some channels
they don't always know what their name is on that channel. (again see Slack and Group me where names
are assigned per conversation)

To accomodate these needs the Entities property includes Mention objects, accessible through the GetMentions() method.
A Mention object is made up of:
| __Property__ | __Description__                     |                   
|--------------|-------------------------------------|
| __type__     | type of the entity ("mention") |
| __mentioned__| ChannelAccount of the person or user who was mentiond |
| __text__     | the text in the Activity.Text property which represents the mention. (this can be empty or null) |

Example:
The user on slack says:

> \@ColorBot pick me a new color

~~~
{   
    ...
    "entities": [{ 
        "type":"mention",
        "mentioned": { 
            "id": "UV341235", "name":"Color Bot" 
        },
        "text": "@ColorBot" 
    }]
    ...
}
~~~

This allows the %bot to know that they were mentioned and to ignore the @ColorBot part of the input when
trying to determine the user intent.

> NOTE: Mentions go both ways.  A %bot may want to mention a user in a reply to a conversation.If they fill out 
> the Mentions object with the mention information then it allows the Channel to map it to the mentioning semantics of the channel.

\subsubsection locations Location Entities
[MISSING]

\subsection channeldataproperty ChannelData Property
With the combination of the Attachments section below the common message schema gives you a rich pallete to describe your response in way 
that allows your message to "just work" across a variety of channels. 

If you want to be able to take advantage of special features or concepts for a channel we provide a way for you to send native
metadata to that channel giving you much deeper control over how your %bot interacts on a channel.  The way you do this is to pass
extra properties via the *ChannelData* property. 

Go to [ChannelData in Messages](/en-us/connector/custom-channeldata) for more detailed description of what each channel enables via the ChannelData Property.

\section attachmentscardsactions Attachments, Cards and Actions
Many messaging channels provide the ability to attach richer objects.In the %Bot %Connector we map
our attachment data structure to media attachments and rich cards on each channel.

\subsection attachments Attachments Property
The Attachments property is an array of Attachment objects which allow you to send and receive images and other content, including rich cards.
The primary fields for an Attachment object are:

| Name        | Description                               | Example   
| ------------|-------- ----------------------------------| ----------
| **ContentType** | The contentType of the ContentUrl property| image/png
| **ContentUrl**  | A link to content of type ContentType     | http://somedomain.com/cat.jpg 
| **Content**     | An embedded object of type contentType    | If contentType = "application/vnd.microsoft.hero" then Content would be a Json object for the HeroCard


Some channels allow you to represent a card responses made up of a title, link, description and images. There are multiple card formats, including HeroCard,
ThumbnailCard, Receipt Card and Sign in.  Additionally your card can optionally be displayed as a list or a carousel using the **AttachmentLayout**
property of the Acivity. See [Attachments](/en-us/connector/message-actions) for more info about Attachments.

\subsection imagefileattachments Media Attachments
To pass a simple media attachment (image/audio/video/file) to an activity you add a simple attachment data structure with a link to the
content, setting the contenttype, contentUrl and name properties.

| Property | Description | Example |
|-----|------| ---- |
| ContentType | mimetype/contenttype of the url | image/jpg |
| ContentUrl  | a link to the actual file | http://foo.com/1312312 |
| Name | the name of the file | foo.jpg |

If the content type is a image or media content type then it will be passed to the channel in a way that
allows the image to be displayed. If it is a file then it will simply come through as a link.

~~~

replyMessage.Attachments.Add(new Attachment()
{
    ContentUrl = "https://upload.wikimedia.org/wikipedia/en/a/a6/Bender_Rodriguez.png",
    ContentType = "image/png",
    Name = "Bender_Rodriguez.png"      
});
~~~

~~~{.json}

{
    "attachments": [
        {
            "contentType": "image/png",
            "contentUrl": "https://upload.wikimedia.org/wikipedia/en/a/a6/Bender_Rodriguez.png"
            "name":"Bender_Rodriguez.png"
        }
    ]
}

~~~

\subsection richcards Rich card attachments
We also have the ability to render rich cards as attachments.There are several types of cards supported:

| Card Type | Description | Supported Modes |
|-----------|-------------|-----------------|
| Hero Card | A card with one big image | Single or Carousel |
| Thumbnail Card | A card with a single small image | Single or Carousel |
| Receipt Card | A card that lets the user deliver an invoice or receipt | Single |
| Sign-In Card | A card that lets the %bot initiatea sign-in procedure | Single |

\subsubsection herocard Hero Card
The Hero card is a multipurpose card; it primarily hosts a single large image, a button, and a "tap action", along with text content to display on the card.

| Property | Description |
|-----|------|
| Title | Title of card|
| Subtitle | Link for the title |
| Text | Text of the card |
| Images[] | For a hero card, a single image is supported |
| Buttons[] | Hero cards support one or more buttons |
| Tap | An action to take when tapping on the card |

Sample using the C# SDK:

~~~{.cs}

Activity replyToConversation = message.CreateReply("Should go to conversation, with a hero card");
replyToConversation.Recipient = message.From;
replyToConversation.Type = "message";
replyToConversation.Attachments = new List<Attachment>();

List<CardImage> cardImages = new List<CardImage>();
cardImages.Add(new CardImage(url: "https://<ImageUrl1>"));
cardImages.Add(new CardImage(url: "https://<ImageUrl2>"));

List<CardAction> cardButtons = new List<CardAction>();

CardAction plButton = new CardAction()
{
Value = "https://en.wikipedia.org/wiki/Pig_Latin",
Type = "openUrl",
Title = "WikiPedia Page"
};
cardButtons.Add(plButton);

HeroCard plCard = new HeroCard()
{
           Title = "I'm a hero card",
           Subtitle = "Pig Latin Wikipedia Page",
           Images = cardImages,
           Buttons = cardButtons
};

Attachment plAttachment = plCard.ToAttachment();
replyToConversation.Attachments.Add(plAttachment);

var reply = await connector.Conversations.SendToConversationAsync(replyToConversation);

~~~

~~~{.json}

            {
              "attachments": [
                {
                  "contentType": "application/vnd.microsoft.card.hero",
                  "content": {
                    "title": "I'm a hero card",
                    "subtitle": "Pig Latin Wikipedia Page",
                    "images": [
                      {
                        "url": "https://<ImageUrl1>"
                      },
                      {
                        "url": "https://<ImageUrl2>"
                      }
                    ],
                    "buttons": [
                      {
                        "type": "openUrl",
                        "title": "WikiPedia Page",
                        "value": "https://en.wikipedia.org/wiki/Pig_Latin"
                      }
                    ]
                  }
                }
              ],
}

~~~

\subsubsection thumbnailcard Thumbnail Card
The Thumbnail card is a multipurpose card; it primarily hosts a single small image, a button, and a "tap action", along with text content to display on the card.

| Property | Description |
|-----|------|
| Title | Title of card|
| Subtitle | Link for the title |
| Text | Text of the card |
| Images[] | For a hero card, a single image is supported |
| Buttons[] | Hero cards support one or more buttons |
| Tap | An action to take when tapping on the card |

Sample using the C# SDK:

~~~{.cs}

Activity replyToConversation = message.CreateReply("Should go to conversation, with a thumbnail card");
replyToConversation.Recipient = message.From;
replyToConversation.Type = "message";
replyToConversation.Attachments = new List<Attachment>();

List<CardImage> cardImages = new List<CardImage>();
cardImages.Add(new CardImage(url: "https://<ImageUrl1>"));

List<CardAction> cardButtons = new List<CardAction>();

CardAction plButton = new CardAction()
{
Value = "https://en.wikipedia.org/wiki/Pig_Latin",
Type = "openUrl",
Title = "WikiPedia Page"
};
cardButtons.Add(plButton);

ThumbnailCard plCard = new ThumbnailCard()
{
           Title = "I'm a thumbnail card",
           Subtitle = "Pig Latin Wikipedia Page",
           Images = cardImages,
           Buttons = cardButtons
};

Attachment plAttachment = plCard.ToAttachment();
replyToConversation.Attachments.Add(plAttachment);

var reply = await connector.Conversations.SendToConversationAsync(replyToConversation);

~~~

~~~{.json}

            {
              "attachments": [
                {
                  "contentType": "application/vnd.microsoft.card.thumbnail",
                  "content": {
                    "title": "I'm a thumbnail card",
                    "subtitle": "Pig Latin Wikipedia Page",
                    "images": [
                      {
                        "url": "https://<ImageUrl1>"
                      }
                    ],
                    "buttons": [
                      {
                        "type": "openUrl",
                        "title": "WikiPedia Page",
                        "value": "https://en.wikipedia.org/wiki/Pig_Latin"
                      }
                    ]
                  }
                }
              ],
}

~~~

\subsubsection receiptcard Receipt Card
The receipt card allows the %Bot to present a receipt to the user.

| Property | Description |
|-----|------|
| Title | Title of card |
| Facts[] | Key Value pair list of information to display on the receipt |
| Items[] | The list of ReceiptItem objects on this receipt |
| Tap | An action to take when tapping on the card |
| Tax | Tax on this receipt |
| VAT | Any additional VAT on this receipt |
| Total | The Sum Total of the Receipt |
| Buttons[] | Hero cards support one or more buttons |


Sample using the C# SDK:

~~~{.cs}

Activity replyToConversation = message.CreateReply("Receipt card");
replyToConversation.Recipient = message.From;
replyToConversation.Type = "message";
replyToConversation.Attachments = new List<Attachment>();

List<CardImage> cardImages = new List<CardImage>();
cardImages.Add(new CardImage(url: "https://<ImageUrl1>"));

List<CardAction> cardButtons = new List<CardAction>();

CardAction plButton = new CardAction()
{
Value = "https://en.wikipedia.org/wiki/Pig_Latin",
Type = "openUrl",
Title = "WikiPedia Page"
};
cardButtons.Add(plButton);

ReceiptItem lineItem1 = new ReceiptItem()
{
           Title = "Pork Shoulder",
           Subtitle = "8 lbs",
           Text = null,
           Image = new CardImage(url: "https://<ImageUrl1>"),
           Price = "16.25",
           Quantity = "1",
           Tap = null
};

ReceiptItem lineItem2 = new ReceiptItem()
{
Title = "Bacon",
Subtitle = "5 lbs",
Text = null,
Image = new CardImage(url: "https://<ImageUrl2>"),
Price = "34.50",
Quantity = "2",
Tap = null
};

List<ReceiptItem> receiptList = new List<ReceiptItem>();
receiptList.Add(lineItem1);
receiptList.Add(lineItem2);

ReceiptCard plCard = new ReceiptCard()
{
           Title = "I'm a receipt card, isn't this bacon expensive?",
           Buttons = cardButtons,
           Items = receiptList,
           Total = "275.25",
           Tax = "27.52"
};

Attachment plAttachment = plCard.ToAttachment();
replyToConversation.Attachments.Add(plAttachment);

var reply = await connector.Conversations.SendToConversationAsync(replyToConversation);

~~~

~~~{.json}

            {
              "attachments": [
                {
                  "contentType": "application/vnd.microsoft.card.receipt",
                  "content": {
                    "title": "I'm a receipt card, isn't this bacon expensive?",
                    "items": [
                      {
                        "title": "Pork Shoulder",
                        "subtitle": "8 lbs",
                        "image": {
                          "url": "https://<ImageUrl1>"
                        },
                        "price": "16.25",
                        "quantity": "1"
                      },
                      {
                        "title": "Bacon",
                        "subtitle": "5 lbs",
                        "image": {
                          "url": "https://<ImageUrl2>"
                        },
                        "price": "34.50",
                        "quantity": "2"
                      }
                    ],
                    "total": "275.25",
                    "tax": "27.52",
                    "buttons": [
                      {
                        "type": "openUrl",
                        "title": "WikiPedia Page",
                        "value": "https://en.wikipedia.org/wiki/Pig_Latin"
                      }
                    ]
                  }
                }
              ],
}

~~~

\subsubsection signincard Sign-In Card

The Thumbnail card is a multipurpose card; it primarily hosts a single small image, a button, and a "tap action", along with text content to display on the card.

| Property | Description |
|-----|------|
| Title | Title of card|
| Text | Text of the card |
| Buttons[] | Hero cards support one or more buttons |
| Tap | An action to take when tapping on the card |

~~~{.json}

Activity replyToConversation = message.CreateReply(translateToPigLatin("Should go to conversation, sign-in card"));
replyToConversation.Recipient = message.From;
replyToConversation.Type = "message";
replyToConversation.Attachments = new List<Attachment>();

List<CardAction> cardButtons = new List<CardAction>();

CardAction plButton = new CardAction()
{
Value = "https://<OAuthSignInURL>",
Type = "signin",
Title = "Connect"
};
cardButtons.Add(plButton);

SigninCard plCard = new SigninCard(title: "You need to authorize me", button: plButton);

Attachment plAttachment = plCard.ToAttachment();
replyToConversation.Attachments.Add(plAttachment);

var reply = await connector.Conversations.SendToConversationAsync(replyToConversation);

~~~


Generates JSON

~~~{.json}

            {
                "type": "message/card.signin",
                "attachments": [
                {
                    "contentType": "application/vnd.microsoft.card.signin",
                    "content":
                    {
                        "text": "You need to authorize me",
                        "buttons": [
                        {
                            "type": "signin",
                            "title": "Connect",
                            "value": "https://<OAuthSignInURL>"
                        }
                        ]
                    }
                }
                ]
}

~~~


\subsection attachmentlayout AttachmentLayout property
You can send multiple rich card attachments in a single message. On most channels they will be sent
as a list of rich cards, but some channels (like Skype and Facebook) can render them as a carousel of rich cards.

As the developer you have the abiltity to control whether the list is rendered as a carousel or a vertical list using the **AttachmentLayout** property.
| AttachmentLayout Value | Description                    |  Notes |
| ------------ |------------------------------------------| --------|
| **list**     | Multiple attachments should be shown as a list| *default* |
| **carousel** | multiple attachments should be shown as a carousel if possible, else fallback to a list| |


~~~{.cs}

Activity replyToConversation = message.CreateReply("Should go to conversation, with a carousel");
replyToConversation.Recipient = message.From;
replyToConversation.Type = "message";
replyToConversation.AttachmentLayout = AttachmentLayouts.Carousel;
replyToConversation.Attachments = new List<Attachment>();

Dictionary<string, string> cardContentList = new Dictionary<string, string>();
cardContentList.Add("PigLatin", "https://<ImageUrl1>");
cardContentList.Add("Pork Shoulder", "https://<ImageUrl2>");
cardContentList.Add("Bacon", "https://<ImageUrl3>");

foreach(KeyValuePair<string, string> cardContent in cardContentList)
{
    List<CardImage> cardImages = new List<CardImage>();
    cardImages.Add(new CardImage(url:cardContent.Value ));

    List<CardAction> cardButtons = new List<CardAction>();

    CardAction plButton = new CardAction()
    {
        Value = $"https://en.wikipedia.org/wiki/{cardContent.Key}",
        Type = "openUrl",
        Title = "WikiPedia Page"
    };
    cardButtons.Add(plButton);

    HeroCard plCard = new HeroCard()
    {
        Title = $"I'm a hero card about {cardContent.Key}",
        Subtitle = $"{cardContent.Key} Wikipedia Page",
        Images = cardImages,
        Buttons = cardButtons
    };

    Attachment plAttachment = plCard.ToAttachment();
    replyToConversation.Attachments.Add(plAttachment);
}

replyToConversation.AttachmentLayout = AttachmentLayoutTypes.Carousel;

var reply = await connector.Conversations.SendToConversationAsync(replyToConversation);

~~~

~~~{.json}
    ...
"attachmentLayout":"carousel",
"attachments": [
    {
        "contentType": "application/vnd.microsoft.card.hero",
        "content": {
            "title": "I'm a hero card about Pig Latin",
            "subtitle": "PigLatin Wikipedia Page",
            "images": [
            {
                "url": "https://<ImageUrl1>"
            }
            ],
            "buttons": [
            {
                "type": "openUrl",
                "title": "WikiPedia Page",
                "value": "https://en.wikipedia.org/wiki/{cardContent.Key}"
            }
            ]
        }
        },
        {
        "contentType": "application/vnd.microsoft.card.hero",
        "content": {
            "title": "I'm a hero card about pork shoulder",
            "subtitle": "Pork Shoulder Wikipedia Page",
            "images": [
            {
                "url": "https://<ImageUrl2>"
            }
            ],
            "buttons": [
            {
                "type": "openUrl",
                "title": "WikiPedia Page",
                "value": "https://en.wikipedia.org/wiki/{cardContent.Key}"
            }
            ]
        }
        },
        {
        "contentType": "application/vnd.microsoft.card.hero",
        "content": {
            "title": "I'm a hero card about Bacon",
            "subtitle": "Bacon Wikipedia Page",
            "images": [
            {
                "url": "https://<ImageUrl3>"
            }
            ],
            "buttons": [
            {
                "type": "openUrl",
                "title": "WikiPedia Page",
                "value": "https://en.wikipedia.org/wiki/{cardContent.Key}"
            }
            ]
        }
    }
],
...
}   

~~~


\section customcapabilities Custom Channel Capabilities

\subsection channeldataproperty Activity.ChannelData Property
If you want to be able to take advantage of special features or concepts for a channel we provide a way for you to send native
metadata to that channel giving you much deeper control over how your %bot interacts on a channel.The way you do this is to pass
extra properties via the *ChannelData* property.

>NOTE: You do not need to use this feature unless you feel the need to access functionality not provided by the normal Activity.

\subsection customemailmessages Custom Email Messages
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
                   "subject":"Super awesome mesage subject",
                   "importance":"high"
               }
           }

~~~


\subsection customslackmessages Custom Slack Messages
           Slack supports the ability to create full fidelity slack cards using their message attachments property.The slack
channel gives access to this via the channelData field.

> See[Slack Message Attachments](https://api.slack.com/docs/attachments) for a description of all of the properties
that go into the attachments property

| **Property** | **Description**
|---------|  -----
| *attachments*  | An array of attachments *See[Slack Message Attachments](https://api.slack.com/docs/attachments)*
| *unfurl_links*  | true or false *See[Slack unfurling](https://api.slack.com/docs/unfurling)*
| *unfurl_media*  | true or false *See[Slack unfurling](https://api.slack.com/docs/unfurling)*

When slack processes a %bot %Connector message it will use the normal message properties to create a slack message, and
then it will merge in the values from the *channelData* property if they are provided by the sender.

Example Message:

~~~{.json}
           {
               "type": "message",
               "locale": "en-Us",
               "channelID":"slack",
               "text": "This is a test",
               "conversation": { "id":"123123123123", "topic":"awesome chat" },
               "from": { "id":"12345", "name":"My Bot"},
               "recipient": { "id":"67890", "name":"Joe Doe"},
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
~~~


\subsection customfacebookmessages Custom Facebook Messages
The Facebook adapter supports sending full attachments via the channelData field.This allows you to do anything
natively that Facebook supports via the attachment schema, such as reciept.

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

\subsection customtelegrammessages Custom Telegram Messages

The Telegram channel supports calling Telegram %Bot API methods via the channelData field.This allows your %bot to perform Telegram-specific actions, such as sharing a voice memo, or a sticker.

| **Property** | **Description**
|---------|  -----
| *method* | The Telegram %Bot API method to call.See below for supported methods.
| *parameters* | Associative array containing method parameters.Parameters are method-specific.

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


|---------|---------|---------
| [sendMessage](https://core.telegram.org/bots/api#sendmessage) | [forwardMessage](https://core.telegram.org/bots/api#forwardmessage) | [sendPhoto](https://core.telegram.org/bots/api#sendphoto)
| [sendAudio](https://core.telegram.org/bots/api#sendaudio) | [sendDocument](https://core.telegram.org/bots/api#senddocument) | [sendSticker](https://core.telegram.org/bots/api#sendsticker)
| [sendVideo](https://core.telegram.org/bots/api#sendvideo) | [sendVoice](https://core.telegram.org/bots/api#sendvoice) | [sendLocation](https://core.telegram.org/bots/api#sendlocation)
| [sendVenue](https://core.telegram.org/bots/api#sendvenue) | [sendContact](https://core.telegram.org/bots/api#sendcontact) | [sendChatAction](https://core.telegram.org/bots/api#sendchataction)
| [kickChatMember](https://core.telegram.org/bots/api#kickchatmember) | [unbanChatMember](https://core.telegram.org/bots/api#unbanchatmember) | [answerInlineQuery](https://core.telegram.org/bots/api#answerinlinequery)
| [editMessageText](https://core.telegram.org/bots/api#editmessagetext) | [editMessageCaption](https://core.telegram.org/bots/api#editmessagecaption) | [editMessageReplyMarkup](https://core.telegram.org/bots/api#editmessagereplymarkup)

\subsection customkikmessages Custom Kik Messages

The Kik adapter supports sending native Kik messages via the channelData field.This allows you to do anything
natively that Kik supports.

| **Property** | **Description**
|---------|  -----
| *messages*  | An array of messages. *See[Kik Messages](https://dev.kik.com/#/docs/messaging#message-formats)*

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

\section trackingstate Tracking Bot State


If a %bot is implemented in a stateless way then it is very easy to scale your %bot to handle load. 

Unfortunately a %bot is all about conversations and as soon as you introduce conversation into a %bot then
your %bot needs to track state in order to remember things like "what was the last question I asked them?". 

We make it easy for the %bot developer to track this information because we provide contextual information that
they can use to store data in their own store or database.

In addition, we provide a simple cookie like system for tracking state that makes it super easy for most bots to not have 
to worry about having their own store.

\subsection contextualproperties Contextual properties for State
Every Activity has several properties which are useful for tracking state.

| **Property**                  | **Description**                                    | **Use cases**                                                
|------------------------------ |----------------------------------------------------|----------------------------------------------------------
| **ChannelId + From.Id**       | A Users's address on a channel (ex: email address) | Remembering context for a user on a channel                 
| **Conversation**              | A unique id for a conversation                     | Remembering context all users in a conversation    
| **ChannelId + From.Id + Conversation**    | A user in a conversation                           | Remembering context for a user in a conversation   

You can use these keys to store information in your own database as appropriate to your needs.

\subsection botstateapi Bot State API
After writing a bunch of bots we came to the realization that many bots have pretty simple needs for tracking state. 
To support this case we have state objects exopsed by the %Bot State API which can be used by the developer for simple user & conversation keyed storage.

Here are the %Bot State Methods 

| **Method**                            | **Description**                                                | **Use cases**                                                
|------------------------------------|------------------------------------------------------------|----------------------------------------------------------
| **botState.GetUserData**                 | an object saved based on the channel and from.Id                       | Remembering context object with a user
| **botState.GetConversationData**         | an object saved based on the channel and conversationId                | Remembering context object with a conversation
| **botState.GetPrivateConversationData** | an object saved based on the channel, from.Id & conversationId      | Remembering context object with a person in a conversation
| **botState.SetUserData**                 | an object saved based on the channel and from.Id                      | Remembering context object with a user
| **botState.SetConversationData**         | an object saved based on the channel and conversationId                 | Remembering context object with a conversation
| **botState.SetPrivateConversationData** | an object saved based on the channel, from.Id & conversationId      | Remembering context object with a person in a conversation
| **botState.DeleteStateForUser**         | deletes all user data based on the from.Id  | When the user requests data be deleted or removes the %bot contact

When your %bot sends a reply you  simply set your object in one of the BotData records properties and it will be persisted and
played back to you on future messages when the context is the same. 

> NOTE: If the record doesn't exist, it will return a new BotData() record with a null .Data field and an ETag = "*", so that is suitable for
> changing and passing back to be saved

\subsection getsetproperties Get/SetProperty Methods
The C# library has helper methods called SetProperty() and GetProperty() which make it easy to get and set any type
of data from a BotData record, including complex objects.

Setting typed data
~~~{.cs}
BotData userData = await botState.GetUserDataAsync(botId: message.Recipient.Id, userId: message.From.Id);
userData.SetProperty<bool>("SentGreeting", true);
await BotState.SetUserDataAsync(userData);
~~~

Getting typed data
~~~{.cs}
StateClient sc = new StateClient(new Microsoft.Bot.Connector.MicrosoftAppCredentials());
BotData userData = await sc.BotState.GetUserDataAsync(botId: message.Recipient.Id, userId: message.From.Id);
if (userData.GetProperty<bool>("SentGreeting))
        ... do something ...;
~~~

Example of setting a complex type

~~~{.cs}
StateClient sc = new StateClient(new Microsoft.Bot.Connector.MicrosoftAppCredentials());
BotState botState = new BotState(sc);
botData = new BotData(eTag: "*");
botData.SetProperty<BotState>("UserData", myUserData);
var response = await sc.BotState.SetUserDataAsync(incomingMessage.ChannelId, incomingMessage.From.Id, botData);
~~~

Getting a complex type
~~~{.cs}
pigLatinBotUserData addedUserData = new pigLatinBotUserData();
var botData =  await botState.GetUserDataAsync(message.ChannelId, message.From.Id);
myUserData = botData.GetProperty<BotState>("UserData");
~~~

\subsection concurrency Concurrency
These botData objects will fail to be stored if another instance of your bot has changed the object already.
    
Example of using the REST API client library:
~~~{.cs}
var client = new ConnectorClient();
try
{
    // get the user data object
    var userData = await botState.GetUserDataAsync(botId: message.Recipient.Id, userId: message.From.Id);
         
    // modify it...
    userData.Data = ...modify...;
           
    // save it
    await botState.SetUserDataAsync(botId: message.Recipient.Id, userId: message.From.Id, userData);
}
catch(HttpOperationException err)
{
    // handle precondition failed error if someone else has modified your object
}
~~~

    **/
}
