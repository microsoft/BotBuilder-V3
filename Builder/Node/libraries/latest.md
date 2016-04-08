---
layout: page
title: BotBuilder v0.8.0
permalink: /builder/node/libraries/latest/
weight: 690
parent1: Bot Builder for Node.js
parent2: Libraries
---


Bot Builder for Node.js is targeted at Node.js developers creating new bots from scratch. By building your bot using the Bot Builder framework you can easily adapt it to run on nearly any communication platform. This gives your bot the flexibility to be wherever your users are.

* [Bot Builder for Node.js Reference](/sdkreference/nodejs/modules/_botbuilder_d_.html)
* [Bot Builder on GitHub](https://github.com/Microsoft/BotBuilder)

## Install
Get the latest version of BotBuilder using npm.

    npm install --save botbuilder

## Release Notes
The framework is still in preview mode so developers should expect breaking changes in future versions of the framework. A list of current issues can be found on our [GitHub Repository](https://github.com/Microsoft/BotBuilder/issues).

### v0.8.0
* Added minSendDelay option that slows down the rate at which a bot sends replies to the user. The default is 1 sec but can be set using an option passed to the bot. See TextBot.js unit test for an example of that.
* Added support to SlackBot for sending an isTyping indicator. This goes along with the message slow down.
* Added a new Message builder class to simplify returning messages with attachments. See the send-attachment.js test in TestBot for an example of using it.
* Added a new DialogAction.validatedPrompt() method to simplify creating custom prompts with validation logic. See basics-validatedPrompt example for a sample of how to use it.
* SlackBot didn't support returning image attachments so I added that and fixed a couple of other issues with the SlackBot. 
* Updated the LKG build, unit tests, and package.json version.
  
### v0.7.2
* Fixed bugs preventing BotConnectorBot originated messages from working. Also resolved issues with sending multiple messages from a bot.
* Fixed bugs preventing SlackBot originated messages from working.
* Updated LKG build and package.json version.

### v0.7.1
* Fixed a critical bug in Session.endDialog() that was causing Session.dialogData to get corrupted.
* Updated LKG build and package.json version.

### v0.7.0
* Making Node CommandDialog robust against undefined matched group.
* Added the ability to send a message as part of ending a dialog.. 
* Updated LKG build and package.json version.

### v0.6.5
* Fixed bad regular expressions used by Prompts.confirm() and adding missing unit tests for Prompts.confirm().
* Updated LKG build and package.json version.

### v0.6.4
* LUIS changed their scheme for the prebuilt datetime entity and are no longer returning a resolution_type which caused issues for EntityRecognizer.resolveTime(). I know use either resolution_type or entity.type.
* Updated LKG build and package.json version.

### v0.6.3
* LUIS changed their schema for the pre-built Cortana app which caused the basics.naturalLanguage example to stop working. This build fixes that issue.
* Updated LKG build and package.json version.

### v0.6.2
* Fixed an issue where Session.endDialog() was eating error messages when a dialog throws an exception. Now exceptions result in the 'error' event being emitted as expected. 
* Updated BotConnectorBot.verifyBotFramework() to only verify authorization headers over HTTPS.
* Removed some dead code from LuisDialog.ts.
* Updated LKG build and package.json version.

### v0.6.1
* Fixed an issue with SlackBot & SkypeBot escapeText() and unescapeText() methods not doing  a global replace.
* Changed the URL that the BotConnectorBot sends outgoing bot originated messages to. We had an old server link. 
* Updated LKG build and package.json version.

