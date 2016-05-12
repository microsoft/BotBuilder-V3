---
layout: page
title: BotBuilder v0.10.0
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

### v0.10.0
* Added logic to automatically detect messages from the emulator. This removes the need to manually set an environment variable to configure talking to the emulator.
* Added support for new Action attachment type (buttons.)
* Exposed static LuisDialog.recognize() method. Can be used to manually call a LUIS model.
* Added support to Prompts.choice() to render choices as buttons using ListStyle.button.
* Added new ListStyle.auto option to Prompts.choice() which automatically selects the appropriate rendering option based on the channel and number of choices. This is the new default style.
* Added support to all Prompts for passing in an array of prompt & re-prompt strings. A prompt will be selected at random.
* Added support to all Prompts for passing in an IMessage. This lets you specify prompts containing images and other future attachment types.
* Updated LKG build and package.json version.

### v0.9.2
* Fixed an undefined bug in Message.setText()
* Updated LKG build and package.json version.

### v0.9.1
* Changed Math.round to Math.floor to fetch random array element  
* Updated LKG build and package.json version.

### v0.9.0

__Breaking Changes__

None of these changes are likely to effect anyone but they could so here are the ones that may break things:

* Updated arguments passed to BotConnectorBot.listen().
* Renamed ISessionArgs to ISessionOptions and also renamed Session.args to Session.options.
* Made Session.createMessage() private. It doesn't need to be public now that we have new Message builder class.
* Changed EntityRecognizer.parseNumber() to return Number.NaN instead of undefined when a number isn't recognized.


__Other Changes__

* Significant improvements to the Reference Docs.
* Fixed a couple of bugs related to missing intents coming back from LUIS. 
* Fixed a deep copy bug in MemoryStorage class. I now use JSON.stringify() & JSON.parse() to ensure a deep copy.
* Made dialogId passed to Session.reset() optional.
* Updated Message.setText() to support passing an array of prompts that will be chosen at random.
* Added methods to Message class to simplify building complex multi-part and randomized prompts.
* BotConnectorBot changes needed to support continueDialog() method that's in development.
* Fixed a typo in the import of Consts.ts for Mac builds.
* Updated LKG build and package.json version.

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

