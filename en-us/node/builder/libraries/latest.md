---
layout: page
title: Release Notes
permalink: /en-us/node/builder/libraries/latest/
weight: 2695
parent1: Bot Builder for Node.js
parent2: Libraries
---


Bot Builder for Node.js is targeted at Node.js developers creating new bots from scratch. By building your bot using the Bot Builder framework you can easily adapt it to run on nearly any communication platform. This gives your bot the flexibility to be wherever your users are.

* [Bot Builder for Node.js Reference](/en-us/sdkreference/nodejs/modules/_botbuilder_d_.html)
* [Bot Builder on GitHub](https://github.com/Microsoft/BotBuilder)

## Install
Get the latest version of Bot Builder using npm.

    npm install --save botbuilder
    npm install --save botbuilder-calling

## Release Notes
The framework is still in preview mode so developers should expect breaking changes in future versions of the framework. A list of current issues can be found on our [GitHub Repository](https://github.com/Microsoft/BotBuilder/issues).

### v3.1.0
* Removed try catches that were causing errors to be ate. Added logic to dump stack trace because node isn't always dumping on uncaught exceptions.
* Added NODE_DEBUG logging switch to enable logging of channels other than the emulator.
* Implemented actions.
* Updated reference docs.
* Added keyboard concept and updated Prompts.choice() to use keyboards.
* Added basic support for Facebook quick_replies using keyboards.
* Fixed auth issues around ChatConnector.
* Added new CardAction.dialogAction() type.
* Removed 'cancel' checks from Prompts.
* Updated prompts to not by default exit out after too many retries.
* Added Session.sendTyping() method.
* Updated LKG build and package.json version.

### v3.0.1
* Fixed an issue with channelData being sent for messages without channel data.
* Fixed an issue where we weren't reporting a lot of errors. 
* Added logic to properly verify the bots identity when called from the emulator.
* Added console logging to report failures around security with recommendations.
* Added a new tracing system that logs the bots session & dialog activity to the console for the emulator.  
* Updated LKG build and package.json version.

### v3.0.0
* See [What's New](/en-us/node/builder/whats-new/) for a relatively complete list of changes. 

### v1.0.0

__Breaking Changes__

These changes will impact some bots. 

* Simple closure based handlers are now single step waterfalls.
* If a dialog steps past the end of a waterfall the dialog is automatically ended.

__Other Changes__

* Exposed SimpleDialog class from both module & docs. 
* DialogAction.validatedPrompt() now returns a Dialog which makes it more strongly typed.
* Fixed an issue with the LuisDialog on() & onDefault() handlers eating exceptions.
* Fixed issue with telegram not showing buttons on re-prompt.  
* Updated LKG build and package.json version.

### v0.11.1
* Fixed a bug causing multiple messages to get rejected by the live servers.
* Updated LKG build and package.json version.

### v0.11.0
* Added Prompts.attachment() method. 
* Updated Message.randomPrompt() to take a string or an array.
* Updated Session to clone() raw IMessage entries before sending (fixes a serialization bug)
* Fixed issue where configured BotConnectorBot endpoint wasn't getting used in production.
* Tweaked the way built-in dialogs get registered.
* Added support for showing Prompts.confirm() using buttons.
* Improved the way re-prompting works.
* Created type specific default re-prompts.
* Minor tweak to the way the emulators callback URL is calculated.
* Updated LKG build and package.json version.

### v0.10.2
* Fixed a bug in CommandDialog preventing onDefault() handlers from resuming properly.
* Updated LKG build and package.json version.

### v0.10.1
* Fixed a bug preventing BotConnectorBot configured greeting messages from being delivered.
* Fixed a couple of issues with Prompts.choice() when not using ListStyle.auto.
* Updated LKG build and package.json version.

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

