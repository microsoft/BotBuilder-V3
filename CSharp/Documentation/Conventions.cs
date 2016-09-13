namespace Microsoft.Bot.Builder.Connector
{
    /**
\page connectormisc Conventions & Security

\section serialization Class library vs. Serialization Conventions

All of the objects described use lower-camel casing on the wire. The C# libraries use
strongly typed names that are pascal cased. Our documentation sometimes will use one or the
other but they are interchangeable.

| **C# property** | wire serialization | javascript name |
| ----------------| ------------------ | --------------- |
| ReplyToId       | replyToId          | replyToId       |


\subsection exampleserialization Example serialization
~~~{.json}

{
     "type": "message",
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
       [BotAuthentication]
       public class MessagesController : ApiController
       {
       }
~~~

Or you can pass in the appId appSecret to the attribute directly:

~~~{.cs}
       [BotAuthentication(MicrosoftAppId = "_MicrosoftappId_")]
       public class MessagesController : ApiController
       {
       }
~~~

    **/
}
