# Bot Builder for Node.js
Bot Builder for Node.js is a powerful framework for constructing bots that can handle both freeform interactions and more guided ones where the possibilities are explicitly shown to the user. It is easy to use and models frameworks like Express and Restify to provide a familiar way to write Bots.

High Level Features:

* Powerful dialog system with dialogs that are isolated and composable.
* Built-in prompts for simple things like Yes/No, strings, numbers, enumerations.
* Built-in dialogs that utilize powerful AI frameworks like [LUIS](http://luis.ai).
* Bots are stateless which helps them scale.
* Bots can run on almost any platform like the [Microsoft Bot Framework](http://botframework.com), [Skype](http://skype.com), and [Slack](http://slack.com).
 
## Install
Get the BotBuilder module using npm.

    npm install --save botbuilder

## Build a bot
Say hello in a few lines of code.
 
    var restify = require('restify');
    var builder = require('botbuilder');

    var server = restify.createServer();

    var helloBot = new builder.TextBot();
    helloBot.add('/', function (session) {
        session.send('Hello World');
    });

    server.post('/v1/messages', helloBot.listen());

    server.listen(8080, function () {
        console.log('%s listening to %s', server.name, server.url); 
    });

## Test your bot
Use the Bot Framework Emulator to test your bot on localhost (Windows Only)

* Download it [here](http://aka.ms/bf-bc-emulator)
* Learn how to use it [here](http://docs.botframework.com/botframework/bot-framework-emulator/)

## Publish your bot
Deploy your bot to the cloud and then register it with the Microsoft Bot Framework.

* [Publishing a bot to Microsoft Azure](http://docs.botframework.com/connector/getstarted/#publishing-your-bot-application-to-microsoft-azure)
* [Registering a bot with the Microsoft Bot Framework](http://docs.botframework.com/connector/getstarted/#registering-your-bot-with-the-microsoft-bot-framework)

## Dive deeper
Learn how to build great bots.

* [Core Concepts Guide](http://docs.botframework.com/builder/node/guides/core-concepts/)
* [Bot Builder for Node.js Reference](http://docs.botframework.com/sdkreference/nodejs/modules/_botbuilder_d_.html)
