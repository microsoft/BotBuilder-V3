namespace Microsoft.Bot.Builder.Connector
{
    /**
\page changes Changes from V1 -> V3

We learned a lot from the V1 release of the %Bot Framework.  Thanks to the thousands of developers that we have engaged with
there were a number of changes which felt would improve the experience.

The changes were focused around
- *Performance* 
- *Simplication* 
- *Better control* 
- *New features*

\section changeperf Performance 
> *lowering latency and providing the fastest experience for bots*

We removed a number of intermediate servers which were slowing the system down. In addition, we added the *ServiceUrl* property which 
allowed a bot to have a data driven straight connection back to the environment it was communicating with. 

We also moved a number of services (such as BotState and translation) that we were attempting to do inline into the bot.  
This removed extra work we had to do (such as language switching models) which was inefficient for us to do.

\section changesimple Simplification 
> *making things easier to understand and use*

We combined the Skype and %Bot Framework SDKs and portals into One portal, One SDK, and One Schema, makeing things simpler for you to 
create great bots!

We simplified a number of the properties in the message formats
- Moved **ChannelId** to root of message object to reduce duplicated data
- Added **ServiceUrl** to support data driven direct connection to service
- Added **Conversation** property as an address simplifying and consolidated multiple conversation related properties (ChannelConversationId, ConversationId, etc.)
- Removed botData inline from the message
- Removed returning messages as responses and Async, making all messages sent by the bot done in 1 way.  It turns out that this one simple "feature" was
causing developers to contort themselves into unnatural positions by having to thread a message through complex call stacks. 

\section changescontrol Better control
> *giving more flexibility and control to the developers*

Moving the translation feature into the bot gives the developer the ability to use it exactly the way that they want, using
the domain knowledge of languages and context of the conversation to control exactly when and how translation is applied.

Moving the bot state annotation into the bot builder SDK gives better control to the developer because they are able to ask for 
just the data they want, when they want it. 

The new rich card types give you a much better understanding of what the card will be.  Our old format was loose in the definition
of the properties, which meant it was not very clear what to expect when it was rendered.

\section changenew New features
> *adding new cool things for you to use*

- *Standard %Microsoft Auth* We changed our auth scheme to use Msa AppId Jwt tokens. This change is sets your bot up to 
    be able to have access to enterprise customers.
- *Rich Card Types* - Added well defined rich card types which give developers more consistent rich rendering of their content.

\section porting Guide to Porting from V1

1. references to ChannelAccount.ChannelId should point to message.ChannelId
2. references to ChannelAccount.Id and Channel.Address should all point to ChannelAccount.Id
3. Remove references to BotState annotations on the message, and switch to using the Bot State API
4. References to message.To should be message.Recipient
5. References to ChannelConversationId, ConversationId, should become simply *message.Conversation*
6. If you were using our old attachment structures use the new strongly typed rich cards to have more precise control over your cards.


    **/
}
