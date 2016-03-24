---
layout: page
title: Message Types
permalink: /connector/message-types/
weight: 206
parent1: Bot Connector SDK
---

Your bot's end point will recieve Message objects that are communication.
There more than one type of message which are used to convey system operations or channel system operations
to the bot.  They exist to give the bot information about the state of the channel and the opportunity to respond
to them.

This table gives you basic overview of the message types:

| **MessageType**           | **Description**                                                        
| --- ----------------------|---- ---------------------------------------------------------------------
| **Message**                   | a simple communication between a user <-> bot                            
| **Ping**                      | A system request to test system availability                              
| **DeleteUserData**            | A compliance request from the the user to delete any profile / user data  
| **BotAddedToConversation**    | your bot was added to a conversation                                     
| **BotRemovedFromConversation**| The bot was removed from a conversation                                  
| **UserAddedToConversation**   | A notification that a new user has been added to a conversation          
| **UserRemovedFromConversation**| A notification that a user has been removed from a conversation           
| **EndOfConversation**         | A message that indicates that a participant is ending the conversation.  

## Message 
> a simple communication between a user <-> bot

Each message being routed through the connector has a Type field.  Most of the time the message types 
are "Message", meaning a simple message between a user and a bot.


## Ping
>A system request to test system availability

The ping message is used to establish that a bot has been configured correctly and is responsive

Responses should be inline with the same message type

## DeleteUserData
>A compliance request from the the user to delete any profile / user data 

Bots have access to users conversation data.  Many countries have legal requirements that a user
has the ability to request their data to be dropped.  If you receive a message of this type
you should remove any personally identifyable information (PII) for the user.  

Responses should be inline with the same message type to signal that the bot has handled it.  If there 
is text it will be forwarded on to the user.  If there is no content then a default message will
be sent to the user to signify that it has been complied with.

## BotAddedToConversation
> your bot was added to a conversation

For some channels your bot can be added to the conversation without any messages being sent.

In those cases an *BotAddedToConversation* message will be sent to the bot
with the To address having the bot's address in that conversation space and the
conversationId set to the conversation id for the conversation.

The message From field will have an Address of **$service$**

Responses should be inline with the same message type to signal that the bot has handled it.  If there 
is text it will be forwarded on to the user. If there is no text then nothing will be presented to 
the user.

## BotRemovedFromConversation
> The bot was removed from a conversation

For some channels your bot can be removed from the conversation without any messages being sent.

If the channel adapter has the ability to detect this, a *BotRemovedFromConversation* message will be sent to the bot
The message From field will be the ChannelId:$service

Responses should be inline with the same message type to signal that the bot has handled it.  If there 
is text it will be forwarded on to the user

## UserAddedToConversation
> A notification that a new user has been added to a conversation

Some channels have the ability to add a user to a conversation without sending a message.

If the channel adapter has the ability to detect this, a UserAddedToConversation message will be sent to the bot
The From field will be the user that was added

Responses should be inline with the same message type to signal that the bot has handled it.  If there 
is text it will be forwarded on to the user
         
## UserRemovedFromConversation
>A notification that a user has been removed from a conversation
         
Some channels have the ability to remove a user from a conversation without sending a message

If the channel adapter has the ability to detect this, a UserRemovedFromConversation message will be sent to the bot
The From field will be the user that was removed

Responses should be inline with the same message type to signal that the bot has handled it.  If there 
is text it will be forwarded on to the user


## EndOfConversation
> A message that indicates that a participant is ending the conversation.  

This can be a message from the bot signaling that it is done with conversation, or a message
from the channel signalling that a user has closed a window, etc.  
This is only a hint message type, and otherwise acts like a normal message

Responses should be inline with the same message type to signal that the bot has handled it.  If there 
is text it will be forwarded on to the user


