# Bot Builder Examples
Bot Builder for Node.js examples are organized into groups and designed to illustrate the techniques needed to build great bots. To use the samples clone our GitHub repository using Git.

    git clone git@github.com:Microsoft/BotBuilder.git

The node examples below can then be found under the "Node/examples" directory. 

## Hello World
These examples show a simple "Hello World" sample for each bot type supported by the framework. 

|**Example**     | **Description**                                   
| ---------------| ---------------------------------------------
|[hello-TextBot](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/hello-TextBot) | "Hello World" for TextBot class.      
|[hello-BotConnectorBot](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/hello-BotConnectorBot) | "Hello World" for BotConnectorBot class.  
|[hello-SkypeBot](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/hello-SkypeBot) | "Hello World" for SkypeBot class.
|[hello-SlackBot](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/hello-SlackBot) | "Hello World" for SlackBot class.
|[hello-AzureWebApp](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/hello-AzureWebApp) | Visual Studio example solution for deploying a "Hello World" bot to Azure.

## Basic Techniques
These examples show the basic techniques needed to build a great bot. All of the examples use the TextBot class and can be executed from a console window. 

|**Example**     | **Description**                                   
| ---------------| ---------------------------------------------
|[basics-waterfall](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-waterfall) | Shows how to use a waterfall to prompt the user with a series of questions.
|[basics-naturalLanguage](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-naturalLanguage) | Shows how to use a LuisDialog to add natural language support to a bot.
|[basics-multiTurn](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-multiTurn) | Shows how to implement simple multi-turns using waterfalls.
|[basics-firstRun](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-firstRun) | Shows how to create a First Run experience using a piece of middleware.
|[basics-logging](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-logging) | Shows how to add logging/filtering of incoming messages using a piece of middleware. 
|[basics-validatedPrompt](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/basics-validatedPrompt) | Shows how to create a custom prompt that validates a users input. 
      
## Sample Bots
These are complete bots designed to run on all of Bot Builders supported platforms. Consult the README.md file for each bot for usage instructions.

|**Example**     | **Description**                                   
| ---------------| ---------------------------------------------
|[todoBot](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/todoBot) | A bot for managing a users to-do list.
|[todoBotLuis](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/todoBotLuis) | A bot for managing a users to-do list using a natural language interface.
|[testBot](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/testBot) | A bot for testing various features of the framework. 

