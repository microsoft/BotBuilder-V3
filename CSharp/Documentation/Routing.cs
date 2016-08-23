namespace Microsoft.Bot.Builder.Connector
{
    /** 
\page routing Sending and Receiving Activities
Every activity contains information used for routing the activity to the appropriate destination. Bots 
receive activities from the user and send them back, just like people exchange messages.

\section routingactivities Routing Activities
Activities have properties which enable the connector service to deliver to the proper recipient, communicating 
who created the message, the context of the message and the recipient of the message.

The %Connector service models this as **From** -> **Recipient** as part of a **Conversation**.  

| __Property__     | __Description__                     |
|------------------|-------------------------------------|
| __From__         | Sender of the activity              | 
| __Recipient__    | Recipient of the activity           |
| __Conversation__ | Conversation the message was part of|

When you receive a Activity from a user it will have the From field set to the
user who created the message and the Recipient field will always be your bot's identity
in that conversation.

> NOTE: a %bot doesn't always know it's identity because some channels assign new identities when 
> the %bot is added to a conversation. (e.g. Slack)
> As a result, it is important when you are create a reply message you use the incoming Recipient property as the 
> outgoing From. (see [Replying to an Activity](/en-us/connector/replying/) for more details). 

\subsection channelid ChannelId and ServiceUrl
There are 2 top level properties which tell the bot what channel it is working with and the url 
that should be used for API operations like sending a reply. 

| __Property__     | __Description__                     | __Examples__                         | 
|------------------|-------------------------------------|--------------------------------------|
| __ChannelId__    | The channel for the activity        | skype                                |
|  __ServiceUrl__  | The url to use for sending activities back | http://skype.botframework.com |

\section connectorclient Creating Connector Client
The ServiceUrl provides the appropriate endpoint for API calls. All you have to do is pass it into the 
constructor of the ConnectorClient() class.

~~~{.cs}
var connector = new ConnectorClient(incomingMessage.ServiceUrl);
~~~

> NOTE: Even though ServiceUrl values may seem stable bots should not rely on that and instead always use the ServiceUrl value

\subsection ChannelAccounts
The ChannelAccount record is used to represent an address a user or bot on a communication channel. 

| __Property__ | __Description__                     | __Examples__                    | 
|--------------|-------------------------------------|---------------------------------|
|  __Id__      | The address on the channel          | joe @hotmail.com, +14258828080, etc.|
| __Name__     | A name for the user or %bot         | Joe Smith                       |

Each user has 1..N ChannelAccounts which represent their identities on each channel.

Each %bot has 1..N ChannelAccounts which represent their identities on each channel.


\subsubsection ConversationAccount
The ConversationAccount object is basically the same as a ChannelAccount with some additional metadata.

| __Property__ | __Description__                     | __Examples__                    | 
|--------------|-------------------------------------|---------------------------------|
|  __Id__      | A unique id for the conversation on the channel | Xy1xvh3jhv 
| __Name__     | A name for the conversation         | Fantasy football                |
|  __IsGroup__ | if true, indicates a group conversation (default is false)| bool |


\section replying Replying to messages 
When your %bot receives a message Activity it will want to respond. 

To do that, you need a new Activity() with

- The **From** <--> **Recipient** fields swapped from the original message(so that it will be routed back to where it came from using the identify for your bot in that conversation)
- The **Conversation** from the original message on it (route to the same conversation)
- Text (and Attachments as appropriate)

To make this easy to do we have an extension method on the Activity class called **CreateReply()**.

To create a reply message for an existing incoming activity:
~~~{.cs}
// create properly formatted reply message
var replyMessage = incomingMessage.CreateReply("Yo, what's up?");
~~~

\subsection replytoactivity ReplyToActivity()
To reply simply create a reply message and call the **ReplyToActivity()** method. The %Connector service will take care of the details
of delivering your message using the appropriate channel semantics.  

~~~{.cs}
var connector = new ConnectorClient(incomingMessage.ServiceUrl);
var replyMessage =  incomingMessage.CreateReply("Yo, I heard you.", "en");
await connector.Conversations.ReplyToActivityAsync(replyMessage); 
~~~

\subsection sendtoconversation SendToConversation()
The **SendToConversation()** method is almost identical to ReplyToActivity() except that it doesn't maintain any sort of 
threading. It is used when don't have an activity to reply to. If you do have an activity to reply to you should us the 
ReplyToActivity() method.

~~~{.cs}
var connector = new ConnectorClient(incomingMessage.ServiceUrl);
IMessageActivity newMessage =  Activity.CreateMessageActivity();
newMessage.Type = ActivityTypes.Message;
newMessage.From = botAccount;
newMessage.Conversation = conversation;
newMessage.Recipient = userAccount;
newMessage.Text = "Yo yo yo!";
await connector.Conversations.SendToConversationAsync((Activity)newMessage); 
~~~

\subsection multiplereplies Multiple replies
It is perfectly fine to send as many messages you want via these methods.  

~~~{.cs}
var connector = new ConnectorClient(incomingMessage.ServiceUrl);
connector.Conversations.ReplyToActivity(incomingMessage.CreateReply("Yo, I heard you 1.", "en"));
connector.Conversations.ReplyToActivity(incomingMessage.CreateReply("Yo, I heard you 2.", "en"));
connector.Conversations.ReplyToActivity(incomingMessage.CreateReply("Yo, I heard you 3.", "en"));
~~~
> NOTE: Most channels have built in throttling levels and you will be subject to whatever limits the channel sets.

\section conversation Starting Conversations

To initiate a conversation you need to call the CreateConversation() or CreateDirectConversation() methods to get a 
ConversationAccount record from the channel. Once you have the ConversationAccount can use it in a message call 
to SendToConversation().

\subsection conversationuser Create 1:1 Conversations
The **CreateDirectConversation()** method is used to create a private 1:1 conversation between the bot and the user.

To initialize a **ConnectorClient** you use ServiceUrl persisted from previous messages.

Example:
~~~{.cs}
var userAccount = new ChannelAccount(name: "Larry", id: "@UV357341");
var connector = new ConnectorClient(new Uri(incomingMessage.ServiceUrl));
var conversationId = await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount);

IMessageActivity message =  Activity.CreateMessageActivity();
message.From = botAccount;
message.Recipient = userAccount;
message.Conversation = new ConversationAccount(id: conversationId.Id);
message.Text = "Hello";
message.Locale = "en-Us";
await connector.Conversations.SendToConversationAsync((Activity)message); 
~~~


\subsection conversationmultiple Create Group Conversations
The **CreateConversation()** method is used to create a new group conversation.
> NOTE: Currently Email is the only channel which supports bot initiated group conversations

Example: 
~~~{.cs}
var connector = new ConnectorClient(new Uri(incomingMessage.ServiceUrl));
List<ChannelAccount> participants = new List<ChannelAccount>();
participants.Add(new ChannelAccount("joe@contoso.com", "Joe the Engineer"));
participants.Add(new ChannelAccount("sara@contso.com", "Sara in Finance"));

ConversationParameters cpMessage = new ConversationParameters(message.Recipient, true, participants, "Quarter End Discussion");
var conversationId = await connector.Conversations.CreateConversationAsync(cpMessage);

IMessageActivity message = Activity.CreateMessageActivity();
message.From = botAccount;
message.Recipient = new ChannelAccount("lydia@contoso.com", "Lydia the CFO"));
message.Conversation = new ConversationAccount(id: conversationId.Id);
message.ChannelId = incomingMessage.ChannelId;
message.Text = "Hey, what's up everyone?";
message.Locale = "en-Us";
await connector.Conversations.SendToConversationAsync((Activity)message); 
~~~

    **/
}
