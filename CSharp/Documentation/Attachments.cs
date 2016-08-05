namespace Microsoft.Bot.Builder.Connector
{
    /**
\page attachments Attachments, Cards and Actions

Many messaging channels provide the ability to attach richer objects. In the %Bot %Connector we map
our attachment data structure to media attachments and rich cards on each channel.

\section imagefileattachments Media Attachments
To pass a simple media attachment (image/audio/video/file) to an activity you add a simple attachment data structure with a link to the
content, setting the ContentType, ContentUrl and Name properties.

| **Property** | **Description** | **Example** |
|-----|------| ---- |
| **ContentType** | mimetype/contenttype of the URL | image/jpg |
| **ContentUrl**  | a link to the actual file | http://foo.com/1312312 |
| **Name** | the name of the file | foo.jpg |

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

\section richcards Rich card attachments
We also have the ability to render rich cards as attachments. There are several types of cards supported:

| **Card Type** | **Description** | **Supported Modes** |
|-----------|-------------|-----------------|
| **Hero Card** | A card with one big image | Single or Carousel |
| **Thumbnail Card** | A card with a single small image | Single or Carousel |
| **Receipt Card** | A card that lets the user deliver an invoice or receipt | Single |
| **Sign-In Card** | A card that lets the %bot initiate a sign-in procedure | Single |

\subsection herocard Hero Card
The Hero card is a multipurpose card; it primarily hosts a single large image, a button, and a "tap action", along with text content to display on the card.

| **Property** | **Description** |
|-----|------|
| **Title** | Title of card|
| **Subtitle** | Link for the title |
| **Text** | Text of the card |
| **Images[]** | For a hero card, a single image is supported |
| **Buttons[]** | Hero cards support one or more buttons |
| **Tap** | An action to take when tapping on the card |

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

\subsection thumbnailcard Thumbnail Card
The Thumbnail card is a multipurpose card; it primarily hosts a single small image, a button, and a "tap action", along with text content to display on the card.

| **Property** | **Description** |
|-----|------|
| **Title** | Title of card|
| **Subtitle** | Link for the title |
| **Text** | Text of the card |
| **Images[]** | For a hero card, a single image is supported |
| **Buttons[]** | Hero cards support one or more buttons |
| **Tap** | An action to take when tapping on the card |

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

\subsection receiptcard Receipt Card
The receipt card allows the %Bot to present a receipt to the user.

| **Property** | **Description** |
|-----|------|
| **Title** | **Title of card** |
| **Facts[]** | Key Value pair list of information to display on the receipt |
| **Items[]** | The list of ReceiptItem objects on this receipt |
| **Tap** | An action to take when tapping on the card |
| **Tax** | Tax on this receipt |
| **VAT** | Any additional VAT on this receipt |
| **Total** | The Sum Total of the Receipt |
| **Buttons[]** | Hero cards support one or more buttons |


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

\subsection signincard Sign-In Card

The Sign-In card is a card representing a request to sign in the user;

| **Property**  | **Type**      | **Description**
|-----------|---------- | ----- 
| **Text**      | string    | Text of the card 
| **Buttons[]** | Action[]  | Action to use to perform for Sign-In

~~~{.cs}

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

\section cardaction  Button and Card Actions 

The CardAction type is used to represent the information needed to process a button or a tap on a section of a rich card.

| **Property**  | **Type**      | **Description**
|-----------|---------- | ----- 
| **Type**      | string    | action types as specified in table below
| **Title**     | string    | Title for button
| **Image**     | string    | Image URL for button
| **Value**     | string    | value to perform action

| **Action types**	| **Content of value property**
|---------------|------------------------------
| **openUrl**	    | URL to be opened in the built-in browser.
| **imBack**	    | Text of message which client will sent back to bot as ordinary chat message. All other participants will see that was posted to the bot and who posted this.
| **postBack**	    | Text of message which client will post to bot. Client applications will not display this message.
| **call**	        | Destination for a call in following format: "tel:123123123123"
| **playAudio**	    | playback audio container referenced by URL
| **playVideo**	    | playback video container referenced by URL
| **showImage**	    | show image referenced by URL
| **downloadFile**  | download file referenced by URL
| **signin**        | OAuth flow URL
__Note__: Only the following action types are supported by Skype: `openUrl`, `imBack`, `call`, `showImage`, `signin` 

~~~{.cs}
CardAction button = new CardAction()
{
    Type = "imBack",
    Title = "Hello"
    Value = "I sez hello!"
};
~~~


Generates JSON
~~~{.json}
{
    "type": "imBack",
    "title": "Hello",
    "value": "I sez hello!"
}
~~~

    **/
}
