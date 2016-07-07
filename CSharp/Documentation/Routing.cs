namespace Microsoft.Bot.Builder.Connector
{
    /** 
\page routing Routing and Addresses
Every activity contains information used for routing the activity to the appropriate destination.  Either an 
Activity is outbound to the bot, or it is inbound from the bot to a user.

\section replying Replying to a message Activity
When your %bot receives a message Activity it most likely will want to respond. The minimum amount of information that 
is needed to respond is to send back the text that you want to send back to the user as a reply.

To do that, you need a new Activity() with

- The From and Recipient fields swapped from the original message(so that it will be routed back to where it came from)
- The conversation from the original message on it(so you can send it back to the same conversation)
- The new Text

To make this super easy to do in C# we created an extension method on the Activity class called **CreateReply()**.

To create and send a proper reply message all you have to do is:
~~~
var replyMessage = incomingMessage.CreateReply("Yo, I heard you.", "en-Us");
var response = await connector.Conversations.ReplyToActivityAsync(replyMessage);
~~~

\subsection replyinglater Replying to the message later

To reply to your user, you create a reply message and send it to the user.The difference between ReplyToActivityAsync
and SendToConversationAsync is just that Reply, on channels that support it, will attempt to maintain "threading" in 
the conversation whereas SendToConversation will simply append to the conversation.

~~~
var replyMessage =  incomingMessage.CreateReply("Yo, I heard you.", "en");
return null; // no reply
...

send the reply later    
var connector = new ConnectorClient(incomingMessage.ServiceUrl);
await connector.Conversations.ReplyToActivityAsync(replyMessage); 

~~~

\subsection multiplereplies Multiple replies
The reply mechanism is really a "add message to conversation", and so there is nothing wrong with sending
multiple replies at a later point.

~~~
var connector = new ConnectorClient(incomingMessage.ServiceUrl);
connector.Conversations.ReplyToActivity(incomingMessage.CreateReply("Yo, I heard you 1.", "en"));
connector.Conversations.ReplyToActivity(incomingMessage.CreateReply("Yo, I heard you 2.", "en"));
connector.Conversations.ReplyToActivity(incomingMessage.CreateReply("Yo, I heard you 3.", "en"));
~~~


\section conversation Starting a conversation

The difference between a new conversation, and a reply to an existing conversation is the value of the 
Conversation property. Conversation is a required property on an Activity.

- If the Conversation property is set using the value from an existing conversation, it will continue that conversation.
- A new Conversation can be created with the CreateConversation or CreateDirectConversation methods.

\subsection conversationuser Example of starting a new conversation with a user
~~~{.cs}
           var connector = new ConnectorClient(incomingMessage.ServiceUrl);
           Activity message = new Activity();
           message.From = botChannelAccount;
           message.ChannelId = "slack";
           message.Recipient = new ChannelAccount() { name: "Larry", "id":"@UV357341"};
           message.Text = "Hey, what's up homey?";
           message.Locale = "en-Us";
           var ConversationId = await connector.Conversations.CreateDirectConversationAsync(incomingMessage.Recipient, incomingMessage.From);
           message.Conversation = new ConversationAccount(id: ConversationId.Id);
           var reply = await connector.Conversations.ReplyToActivityAsync(message);
~~~


\subsection conversationmultiple Example of starting a new conversation with a set of users
~~~{.cs}
           var connector = new ConnectorClient();
           List<ChannelAccount> participants = new List<ChannelAccount>();
           participants.Add(new ChannelAccount("joe@contoso.com", "Joe the Engineer"));
           participants.Add(new ChannelAccount("sara@contso.com", "Sara in Finance"));

           ConversationParameters cpMessage = new ConversationParameters(message.Recipient, participants, "Quarter End Discussion");
           var ConversationId = connector.Conversations.CreateConversationAsync(cpMessage);

           Activity message = new Activity();
           message.From = botChannelAccount;
           message.Recipient = new ChannelAccount("lydia@contoso.com", "Lydia the CFO"));
           message.Conversation = ConversationId;
           message.ChannelId = "email";
           message.Text = "Hey, what's up everyone?";
           message.Locale = "en-Us";
           var reply = await connector.Conversations.ReplyToActivityAsync(message);
~~~


> NOTE: All of the ChannelAccounts need to be on the same Channel as the Activity has a single ChannelId


\section addresses Addresses in messages
The %Bot Framework API uses ChannelAccount records to represent an contact address for a user or bot
on a communication channel.Numerous fields in the Activity object have ChannelAccount
references in them to represent the relationships between the users and bots that are participating in
a conversation. 

\subsection ChannelAccount
The ChannelAccount object is a core object which describes an alias for a user. 

| __Property__ | __Description__                     | __Examples__                    | 
|--------------|-------------------------------------|---------------------------------|
| __name__     | A name for the user or %bot         | Joe Smith                       |
|  __id__      | The address on the channel          | joe @hotmail.com, +14258828080, etc.|

Each user has 1..N ChannelAccounts which represent their identities on each channel.

Each %bot has 1..N ChannelAccounts which represent their identities on each channel.


\subsection ConversationAccount
The ConversationAccount object is basically the same as a ChannelAccount with some additional metadata.

| __Property__ | __Description__                     | __Examples__                    | 
|--------------|-------------------------------------|---------------------------------|
| __name__     | A name for the conversation         | Fantasy football                |
|  __id__      | A unique id for the conversation on the channel | Xy1xvh3jhv 
|  __isGroup__ | if true, indicates a group conversation (default is false)| bool |

\subsection channelaccounts Activity object properties that use ChannelAccounts

| __Property__ | __Description__                     |                   
|--------------|-------------------------------------|
| __From__       | The address for the sender | 
| __Conversation__ | The address for the recipient |
| __Recipient__  | The address for the recipient |
| __Entities__   | A mixed collection of entities including mentions | 
| __UsersAdded__ | A array of users added to the group conversation | 
| __UsersRemoved__ | A array of users removed from the group conversation | 
| n/a | ChannelAccounts of known participants in the conversation (see Conversations.GetActivityMembers()) | 

When you receive a Activity from a user it will have the From field set to the
user who created the message and the Recipient field will always be your bot's identity
in that conversation.

>It is important to note that a %bot doesn't always know
it's identity before hand because some channels assign out new identities for
a %bot when a %bot is added to a conversation. (For example groupme and slack do this.)

As a result, it is important when you are replying to a conversation to create a new message which appropriately 
sets the From and Recipient fields(see [Replying to an Activity](/en-us/connector/replying/) for more details). 

The difference between a new conversation, and a reply to an existing conversation is the value of the Conversation 
property.Conversation is a required property on an Activity.

- If the Conversation property is set using the value from an existing conversation, it will continue that conversation.
- A new Conversation can be created with the CreateConversation or CreateDirectConversation methods.

\subsection Mentions
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

    **/
}