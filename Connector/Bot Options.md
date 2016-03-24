---
layout: page
title:  Bot Options
permalink: /connector/bot-options/
weight: 209
parent1: Bot Connector SDK
---

# Bot options
When you configure your bot there are several optionals features you can select which are described in more depth here.

----

## Listen to all messages
* **Option is off** *(default)*-  when this is option is off, bots are in **Spoke Only When Spoken To** mode. 
* **Option is on**-  the bot will receive ALL messages in the conversation.  It is up to the bot
 to make sure that it's interaction is appropriate for the conversation.


### Spoke Only When Spoken To 
1. if bot is in a conversation which is only the user and the bot, all messages will be sent to the bot regardless of mentions.
2. if in group conversation
    
    a. if a user mentions the bot then the message will be sent to the bot and the user and bot will be in an *Active Conversation*
   
    b. While in *Active Conversation* all future messages from that user will be sent to the bot regardless of mentions until
    * the user says a goodbye statement (like 'see you later', or 'goodbye', etc.) 
    * 5 minutes of inactivity pass
 
----

## Translate channel messages
* **On** *(default)*-  when this is option is on machine translation is available for any user to use. 
* **Off**-  the bot connector will do no translation.


### Translation behavior
The bot can list the languages it supports in the Languages field of the bot registrations. 

If an incoming message matches the bot's native language then no translation should happen. 
If the user asks to change the language to another language then we will remember for the conversation what language is active like this:

> **User:** I would like to speak spanish

> **Bot:** Voy a usar el español. Déjeme saber si usted quiere cambiar hacia atrás.

All future messages for that conversation will then be translated from the selected language to the bot's first language in their
bot settings.  All responses from the bot will be translated from the message.Language of the response message back to the 
selected conversation language to be displayed to the user. 

If the user asks to switch to another language then the same process happens again. 

The history of all language selections for the user are remembered, and if a user starts a new conversation with a bot
which supports translation then the default of the new conversation will be the most frequently used language for the user.

From a users perspective it should look like this:

* user switches to language 
* conversation continues in language
* next bot they talk to will default to most observed language 

At any time user can switch to a different language and the selected language will be sticky for a conversation.

When you receive a message that has been translated you will see the following changes:

|**Property**               | **Description**                                   
| ----------------------| ---- -----------------------------------------
|*message.SourceLanguage* | original language code the user sent you      
|*message.SourceText*    | original untranslated text the user sent you  
|*message.Language*      | The language for Text 
|*message.Text*          | the translated text                 

----

## Disable all logging
* **Off** *(default)*- Microsoft will use anonymized conversation data to train future systems. [See privacy polcy](link) 
* **On**- Microsoft will not log any of the conversation with your bot.

----

## Publish in Bot directory
* **Off** *(default)*- Your bot will only be visible to you or to someone you give the link to your contact card to. 
* **On**- Your bot will show up on the [Bot Gallery](https://bots.botframework.com)

