
## Overview
Bot Builder for Node.js examples are organized into groups and designed to illustrate the techniques needed to build great bots. To use the samples clone our GitHub repository using Git.

    git clone https://github.com/Microsoft/BotBuilder.git
    cd BotBuilder/Node
    npm install

The node examples below can then be found under the "Node/examples" directory. 

## Hello World
These examples show a simple "Hello World" sample for each bot type supported by the framework. 

|**Example**     | **Description**                                   
| ---------------| ---------------------------------------------
|[hello-ConsoleConnector](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/hello-ConsoleConnector) | "Hello World" for ConsoleConnector class.      
|[hello-ChatConnector](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/hello-ChatConnector) | "Hello World" for ChatConnector class.  

## Basic Techniques
These examples show the basic techniques needed to build a great bot. All of the examples use the TextBot class and can be executed from a console window. 

|**Example**     | **Description**                                   
| ---------------| ---------------------------------------------
|[basics-waterfall](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-waterfall) | Shows how to use a waterfall to prompt the user with a series of questions.
|[basics-loops](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-loops) | Shows how to use session.replaceDialog() to create loops. 
|[basics-menus](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-menus) | Shows how to create a simple menu system for a bot. 
|[basics-naturalLanguage](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-naturalLanguage) | Shows how to use a LuisDialog to add natural language support to a bot.
|[basics-multiTurn](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-multiTurn) | Shows how to implement simple multi-turns using waterfalls.
|[basics-firstRun](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-firstRun) | Shows how to create a First Run experience using a piece of middleware.
|[basics-logging](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-logging) | Shows how to add logging/filtering of incoming messages using a piece of middleware.
|[basics-localization](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-localization) | Shows how to implement multiple language support for a bot.
|[basics-customPrompt](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-customPrompt) | Shows how to create a custom prompt of arbitrary complexity. 
|[basics-libraries](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-libraries) | Shows how to package up a set of dialogs as a library that can be shared across multiple bots. 

## Demo Bots
These are bots designed to showcase what's possible on specific channels. They're great sources of code fragments if you're looking to have you bot lightup specific features for a channel.

|**Example**     | **Description**                                   
| ---------------| ---------------------------------------------
|[demo-skype](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/demo-skype) | A bot designed to showcase what's possible on skype.
|[demo-skype-calling](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/demo-skype-calling) | A bot designed to show how to build a calling bot for skype.
|[demo-facebook](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/demo-facebook) | A bot designed to showcase what's possible on Facebook.

**You can find more samples in the [Bot Builder SDK Samples repo](https://github.com/Microsoft/BotBuilder-Samples/tree/master/Node)**
