namespace Microsoft.Bot.Builder.Connector
{
    /**
\page configure Configure

\section botoptions Bot Options

When you configure your %bot there are several optional features you can select which are described in more depth here.

\subsection listeningspeaking Listening and speaking modes

### Listen to all messages
- **Option is off** *(default)*-  when this is option is off, bots are in **Group conversation mode**. 
- **Option is on**-  the %bot will receive ALL messages in the conversation.  It is up to the bot
to make sure that it's interaction is appropriate for the conversation.

### Group conversation mode 
1. if %bot is in a conversation which is only the user and the bot, all messages will be sent to the %bot regardless of mentions.
2. if in group conversation
    - if a user mentions the %bot then the message will be sent to the %bot and the user and %bot will be in an *Active Conversation*
    - While in *Active Conversation* all future messages from that user will be sent to the %bot regardless of mentions until
        - the user says a goodbye statement (like 'see you later', or 'goodbye', etc.) 
        - 5 minutes of inactivity pass

\subsection publishdirectory Publish in %Bot directory
    - **Off** *(default)*- Your %bot will only be visible to you or to someone you give the link to your contact card to. 
    - **On**- Your %bot will show up on the [Bot Gallery](https://bots.botframework.com)

\section configurationconventions Configuration conventions
\subsection serialization Serialization
All of the objects described use lower-camel casing on the wire.  The C# nuget library uses
strongly typed names that are pascal cased. Our documentation sometimes will use one or the
other but they are interchangable.

| **C# property** | wire serialization | javascript name |
| ----------------| ------------------ | --------------- |
| Conversation    | conversation       | conversation|


\subsection exampleserialization Example serialization
~~~{.json}

{
     "type": "Message",
     "conversation": {
       "Id": "GZxAXM39a6jdG0n2HQF5TEYL1vGgTG853w2259xn5VhGfs"
     },
     "timestamp": "2016-03-22T04:19:11.2100568Z",
     "channelid": "skype",
     "text": "You said:test",
     "attachments": [],
     "from": {
       "name": "Test Bot",
       "id": "MyTestBot",
     },
     "recipient": {
       "name": "tom",
       "id": "1hi3dbQ94Kddb",
     },
     "locale": "en-Us",
     "replyToId": "7TvTPn87HlZ",
     "entities": [],
}

~~~

\section securing Securing your bot

Developers should ensure that their bot's endpoint can only be called by the %Bot %Connector.

To do this you should
- Configure your endpoint to only use HTTPS
- Use the %Bot Framework SDK's authentication: MicrosoftAppId Password: MicrosoftAppPassword 

\subsection botauthattributes BotAuthentication Attribute
To make it easy for our C# developers we have created an attribute which does this for your method or controller.

To use with the AppId and AppSecret coming from the web.config

~~~{.cs}
       [BotAuthentication()]
       public class MessagesController : ApiController
       {
       }
~~~

Or you can pass in the appId appSecret to the attribute directly:

~~~{.cs}
       [BotAuthentication("..MicrosoftappId...","...MicrosoftappSecret...")]
       public class MessagesController : ApiController
       {
       }
~~~

\subsection implementingvalidation Implementing your own caller validation
[[ Content coming soon ]]

    **/
}
