namespace Microsoft.Bot.Builder.Connector
{
    /**
\page activities Activities
An Activity is the object that is used to communicate between a user and a bot. When you send an Activity
there are a number of properties that you can use to control your message and how it is presented to the
user when they receive it.

There are more than one type of Activity which are used to convey system operations or channel system operations
to the bot. They exist to give the %bot information about the state of the channel and the opportunity to respond
to them.

Each Activity being routed through the %Connector has a Type field. Primarily, these will be of type message unless they are system
notifications for the %Bot.

This table gives you basic overview of the Activity types:

| **ActivityType**              | **Interface**    | **Description**                                                               | 
| ------------------------------|------------------|----------------------------------------------------------------|
| **message**                   | IMessageActivity | a simple communication between a user <-> %bot                                | 
| **conversationUpdate**        | IConversationUpdateActivity| your %bot was added to a conversation or other conversation metadata changed  |
| **contactRelationUpdate**     | IContactRelationUpdateActivity| The %bot was added to or removed from a user's contact list                   |
| **typing**                    | ITypingActivity | The user or %bot on the other end of the conversation is typing               |
| **ping**                      | n/a | an activity sent to test the security of a bot.  |
| **deleteUserData**            | n/a | A user has requested for the bot to delete any profile / user data      | 


\section message Message
The message activity is the core object exchanged between the user and the bot. It can represent a wide range of values from simple text input 
and response all the way to complex multiple card carousel with buttons and actions

\subsection textproperties Text and Locale Properties
For many developers the Text property is the only property you need to worry about. A person sent you some text, or 
your %bot is sending some text back. There are 2 core properties for this, the Text and Locale property.

| Property    | Description                               | Example
| ------------|-------- ----------------------------------| ----------
| **Text**    | A text payload in markdown syntax which will be rendered as appropriate on each channel| Hello, how are you?
| **Locale**  | The locale of the sender (if known)       | en

If all you do is exchange simple one-line text responses, you don't have to read any further.

\subsection textformat TextFormat Property
Each message has an optional TextFormat property which represents how to interpret the Text property.  

| TextFormat Value | Description                               |  Notes |
| ------------ |------------------------------------------| --------|
| **plain**    | The text should be treated as raw text and no formatting applied at all |  |
| **markdown** | The text should be treated as markdown formatting and rendered on the channel as appropriate | *default* |
| **xml**      | The text is simple XML markup (subset of HTML) | *Skype Only* |

\subsubsection markdown Markdown 
The default text format is markdown which allows a nice balance of the bot being able to express what they want 
and for the each channel to render as accurately as they can.

The markdown that is supported:

|Style               | Markdown                                            | Description                             | Example                                                             
|--------------------| --------------------------------------------------- |---------------------------------------- | ------ -------------------------------------------------------------
| **Bold**           | \*\*text\*\*                                        | make the text bold                      | **text**                                                            
| **Italic**         | \*text\*                                            | make the text italic                    | *text*
| **Header1-5**      | # H1                                                | Mark a line as a header                 | #H1                                                       
| **Strikethrough**  | \~\~text\~\~                                        | make the text strikethrough             | ~~text~~
| **Hr**             | \-\-\-                                              | insert a horizontal rule                |                                                                    |   
| **Unordered list** | \*                                                  | Make an unordered list item             | * text
| **Ordered list**   | 1.                                                  | Make an ordered list item starting at 1 | 1. text                                                          
| **Pre**            | \`text\`                                            | Preformatted text(can be inline)        | `text`                                                              
| **Block quote**    | \> text                                             | quote a section of text                 | > text                                                              
| **link**           | \[bing](http://bing.com)                            | create a hyperlink with title           | [bing](http://bing.com)                                             
| **image link**     | \![duck]\(http://aka.ms/Fo983c) | link to an image  | ![duck](http://aka.ms/Fo983c)

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

Not all channels can represent all markdown fields. As appropriate channels will fallback to a reasonable approximation, for 
example, bold will be represented in text messaging as \*bold\* 

> Tables: If you are communicating with a channel which supports fixed width fonts or HTML you can use standard table
> markdown, but because many channels (such as SMS) do not have a known display width and/or have variable width fonts it 
> is not possible to render a table properly on all channels.      

\subsubsection xml XML 
Skype supports a subset of HTML tags it calls XML Markup (sometimes referred to as XMM).

The tags that are supported are:

|Style               | Markdown                                                               |Description                              | Example                                                             
|--------------------| -----------------------------------------------------------------------|-----------------------------------------| ------ -------------------------------------------------------------
| **Bold**           | &lt;b&gt;text&lt;/b&gt;                                                | make the text bold                      | **text**                                                            
| **Italic**         | &lt;i&gt;text&lt;/i&gt;                                                | make the text italic                    | *text*
| **Underline**      | &lt;u&gt;text&lt;/u&gt;                                                | Mark a line as underline                | text
| **Strikethrough**  | &lt;s&gt;text&lt;/s&gt;                                                | make the text strikethrough             | text
| **link**           | &lt;a href="http:bing.com"&gt;bing&lt;/a&gt;                           | create a hyperlink with title           | [bing](http://bing.com)                                             

\subsection attachmentsproperty Attachments Property
The Attachments property is an array of Attachment objects which allow you to send and receive images and other content, including rich cards.
The primary fields for an Attachment object are:

| Name        | Description                               | Example   
| ------------|-------- ----------------------------------| ----------
| **ContentType** | The contentType of the ContentUrl property| image/png
| **ContentUrl**  | A link to content of type ContentType     | http://somedomain.com/cat.jpg 
| **Content**     | An embedded object of type contentType    | If contentType = "application/vnd.microsoft.hero" then Content would be a JSON object for the HeroCard


Some channels allow you to represent a card responses made up of a title, link, description and images. There are multiple card formats, including HeroCard,
ThumbnailCard, Receipt Card and Sign in. Additionally your card can optionally be displayed as a list or a carousel using the **AttachmentLayout**
property of the Activity. See [Attachments, Cards and Actions](/en-us/csharp/builder/sdkreference/attachments.html) for more info about Attachments.

\subsection attachmentlayoutproperty AttachmentLayout property
You can send multiple rich card attachments in a single message. On most channels they will be sent
as a list of rich cards, but some channels (like Skype and Facebook) can render them as a carousel of rich cards.

As the developer you have the ability to control whether the list is rendered as a carousel or a vertical list using the **AttachmentLayout** property.
| AttachmentLayout Value | Description                    |  Notes |
| ------------ |------------------------------------------| --------|
| **list**     | Multiple attachments should be shown as a list| *default* |
| **carousel** | multiple attachments should be shown as a carousel if possible, else fall back to a list| |


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


\subsection entities Entities Property
The Entities property is an array of open ended [schema.org](http://schema.org) objects which allows the exchange of 
common contextual metadata between the channel and bot. 

\subsubsection mentions Mention Entities
Many communication clients have mechanisms to "mention" someone. Knowing that someone is 
mentioned can be an important piece of information for a %bot that the channel knows and needs to be able 
to pass to you.

Frequently a %bot needs to know that __they__ were mentioned, but with some channels
they don't always know what their name is on that channel. (again see Slack and Group me where names
are assigned per conversation)

To accommodate these needs the Entities property includes Mention objects, accessible through the GetMentions() method.

> The Mention object 
| **Property** | **Description**                     |                   
|--------------|-------------------------------------|
| **type**     | type of the entity ("mention") |
| **mentioned**| ChannelAccount of the person or user who was mentioned |
| **text**     | the text in the Activity.Text property which represents the mention. (this can be empty or null) |

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

> NOTE: Mentions go both ways. A %bot may want to mention a user in a reply to a conversation. If they fill out 
> the Mentions object with the mention information then it allows the Channel to map it to the mentioning semantics of the channel.

\subsubsection places Place Entities
Place represents information from <https://schema.org/Place>. We currently send address and geographical information from the channels.

The %Connector client library defines two typed classes to make it easier to work with:

> The Place Object
| **Property**    | **Description**                     |                   
|--------------   |-------------------------------------|
| **Type**        | 'Place' |
| **Address**     | string description or PostalAddress (future) |
| **Geo**         | GeoCoordinates |
| **HasMap**      | URL to a map or complex "Map" object (future) |
| **Name**        | Name of the place |

> GeoCoordinates Object
| **Property**    | **Description**                     |                   
|--------------   |-------------------------------------|
| **Type**        | 'GeoCoordinates' |
| **Name**        | Name of the place |
| **Longitude**   | Longitude of the location [WGS 84](https://en.wikipedia.org/wiki/World**Geodetic**System)|
| **Latitude**    | Latitude of the location [WGS 84](https://en.wikipedia.org/wiki/World**Geodetic**System)|
| **Elevation**   | Elevation of the location [WGS 84](https://en.wikipedia.org/wiki/World_Geodetic_System)|

Example of adding Geo place data to Entities using strong types:
~~~{.cs}
	var entity = new Entity();
	entity.SetAs(new Place()
	{
		Geo = new GeoCoordinates()
		{
			Latitude = 32.4141,
			Longitude = 43.1123123,
		}
	});
    entities.Add(entity);
~~~

~~~{.json}
"entities":[
    {
      "type": "Place",
      "geo": {
        "latitude": 32.4141,
        "longitude": 43.1123123,
        "type": "GeoCoordinates"
      }
    }
]
~~~

When consuming entities you can use dynamic keyword like this:
~~~{.cs}
    if (entity.Type == "Place")
    {
	    dynamic place = entity.Properties;
        if (place.geo.latitude > 34)
            // do something
    }
~~~

Or you can use the strongly typed classes like this:
~~~{.cs}
    if (entity.Type == "Place")
    {
    	Place place = entity.GetAs<Place>();
	    GeoCoordinates geo = place.Geo.ToObject<GeoCoordinates>();
        if (geo.Latitude > 34)
            // do something
    }   
~~~

\subsection channeldataproperty ChannelData Property
With the combination of the Attachments section below the common message schema gives you a rich palette to describe your response in way 
that allows your message to "just work" across a variety of channels. 

If you want to be able to take advantage of special features or concepts for a channel we provide a way for you to send native
metadata to that channel giving you much deeper control over how your %bot interacts on a channel. The way you do this is to pass
extra properties via the *ChannelData* property. 

Go to [ChannelData in Messages](/en-us/csharp/builder/sdkreference/channels.html) for more detailed description of what each channel enables via the ChannelData Property.


\section conversationUpdate Conversation Update 
> the membership or metadata of a conversation involving the %bot changed

Your %bot often needs to know when the state of the conversation it's in has changed. This may represent the %bot being added to 
the conversation, or a person added or remove from the chat. When these changes happen, your %bot will receive a conversationUpdate 
Activity.

Conversation Update properties
| **Properties**     | **Description**                    |  
| ------------------ |------------------------------------------| 
| **MembersAdded**   | array of ChannelAccount[] for the added accounts | 
| **MembersRemoved** | array of ChannelAccount[] for the removed accounts | 

In this event, the membersAdded and membersRemoved lists will contain the changes to the conversation since the last event. One of 
the members may be the Bot; which can be tested for by comparing the membersAdded\[n].id field with the recipient.id field. 

conversationUpdate is a great opportunity for the %Bot to send welcome messages to users.

\section contactrelationupdate Contact Relation Update 
> The %bot was added to or removed from a user's contact list

For some channels your %bot can be a member of the user's contact list on that chat service (Skype for example). In the 
event the channel supports this action, it can notify the %Bot that this has occurred. When this event is delivered, 
the **Action** property will indicate whether the operation was an **add** or a **remove**.

| **Action values** | **Description**                    |  
| ----------------- |------------------------------------------| 
| **add**           | if the user in the From property added the bot to their contacts | 
| **remove**        | if the user in the From property removed the bot from their contacts | 

\section typing Typing 
> A message that indicates that the user or %Bot is typing

Typing is an indicator of activity on the other side of the conversation. Generally it's used by Bots to 
cover "dead air" while the %bot is fulfilling a request of some sort. The %Bot may also receive Typing 
messages from the user, for whatever purposes it might find useful.

\section ping Ping 
> A message that is used to test that a %Bot has implemented security correctly
The bot receiving this should not send any response except for the HttpStatusCode response of OK, Forbidden or Unauthorized 

\section deleteuserdata Delete User Data 
> A compliance request from the the user to delete any profile / user data 

Bots have access to users conversation data. Many countries have legal requirements that a user
has the ability to request their data to be dropped. If you receive a message of this type
you should remove any personally identifiable information (PII) for the user.  


    **/
}
