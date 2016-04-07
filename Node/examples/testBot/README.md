# TestBot
A bot for testing various features of the framework. 

## Adding Tests
New tests can be added to the bot by simply adding a new my-test.js file to the tests directory. The test should have the following basic structure.

```JavaScript
var builder = require('../../../');

module.exports = {
    description: "Says 'Hello World'",
    async: true,
    addDialogs: addDialogs,
    run: run 
};

function addDialogs(bot) {
    bot.add('/tests/hello-world', function (session) {
        session.endDialog('Hello World');
    });
}

function run(session) {
    session.beginDialog('/tests/hello-world');
}
```

## TextBot Usage
To run the bot from a console window execute "node textBot.js" and type “/help”.

## BotConnectorBot Usage
To run the bot using the Bot Framework Emulator open a console window and execute:

    set EMULATOR_PORT=9000
    node botConnectorBot.js

Then launch the Bot Framework Emulator, connect to http://localhost:8080/v1/messages, and say "/help".

To publish the bot to the Bot Connector Service follow the steps outlined in the article below.

    http://docs.botframework.com/builder/node/bots/BotConnectorBot/#publishing

## SkypeBot Usage
To run the bot using Skype you'll need to follow Skypes Getting Started guide and register a new bot with the Skype Developer Portal.

    http://docs.botframework.com/builder/node/bots/SkypeBot/#usage

To test the bot using ngrok open a console window and execute:

    ngrok 8080

Then update your bots callback address in the Skype Developer Portal to match the ngrok address. 

Next open a second console window and execute:

    set APP_ID=YourAppId
    set APP_SECRET=YourAppSecret
    node skypeBot.js

Then add the bot to your contacts list using the join link in the portal and say "/help".

## SlackBot Usage
To run the bot in Slack for follow BotKits Getting Started guide and create an integration for the bot.

    http://howdy.ai/botkit/docs/#getting-started

Next open a console window and execute:

    set token=YourToken
    node slackBot.js

Then find the bot slack and say "/help".

