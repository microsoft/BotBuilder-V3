---
layout: page
title: BotBuilder v0.6.3
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

