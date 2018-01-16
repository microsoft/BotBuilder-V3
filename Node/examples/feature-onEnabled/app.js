/*-----------------------------------------------------------------------------
This Bot demonstrates how to enable and disable intent recognizers via the new
.onEnabled() method. This example uses session.conversationData to hold the
data to enable and disable two RegExpRecognizers.

# RUN THE BOT:

    Run `npm install` in BotBuilder/Node and BotBuilder/Node/core.

    Run the bot from the command line using "node app.js" and open the Bot 
    Framework Emulator. Enter into the address bar http://localhost:3978
    /api/messages to open a conversation with the bot. 
    
    Type anything to reach the 'RecognizerMenu' dialog which will send a 
    one-time message on how to use the bot.

-----------------------------------------------------------------------------*/

var builder = require("../../core");
var restify = require('../../node_modules/restify');

//=========================================================
// Bot Setup
//=========================================================

// Setup Restify Server
var server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
    console.log('%s listening to %s', server.name, server.url);
});

// Bot Storage: Here we register the state storage for your bot. 
// Default store: volatile in-memory store - Only for prototyping!
// We provide adapters for Azure Table, CosmosDb, SQL Azure, or you can implement your own!
// For samples and documentation, see: https://github.com/Microsoft/BotBuilder-Azure
var inMemoryStorage = new builder.MemoryBotStorage();


// Create chat connector for communicating with the Bot Framework Service
var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});

server.post('/api/messages', connector.listen());

var bot = new builder.UniversalBot(connector).set('storage', inMemoryStorage); // Register in memory storage

//=========================================================
// Bot Recognizers
//=========================================================

// Create a 'greetings' RegExpRecognizer that can be turned off
var greetings = new builder.RegExpRecognizer('Greetings', /hello|hi|hey|greetings/i)
    .onEnabled(function (session, callback) {
        // Check to see if this recognizer should be enabled
        if (session.conversationData.useGreetings) {
            callback(null, true);
        } else {
            callback(null, false);
        }
    });

// Create a 'farewell' RegExpRecognizer that can be turned off
var farewell = new builder.RegExpRecognizer('Farewell', /goodbye|good bye|good-bye|bye|farewell/i)
    .onEnabled(function (session, callback) {
        // Check to see if this recognizer should be enabled
        if (session.conversationData.useFarewell) {
            callback(null, true);
        } else {
            callback(null, false);
        }
    });

// Create a recognizer to take the user back to the Recognizer Menu
var menu = new builder.RegExpRecognizer('RecognizerMenu', /help|recognizer|recognizer menu|menu/i);

// Create our IntentDialog and add recognizers
var intents = new builder.IntentDialog({ recognizers: [greetings, farewell, menu] });

bot.dialog('/', intents);

// If no intent is recognized, direct user to Recognizer Menu
intents.onDefault('RecognizerMenu');

// Match our "Greetings" and "Farewell" intents with their dialogs
intents.matches('Greetings', 'Greetings');
intents.matches('Farewell', 'Farewell');

//=========================================================
// Bots Dialogs
//=========================================================

// Add a recognizer menu dialog
bot.dialog('RecognizerMenu', [
    function (session, args, next) {
        // Send a (one-time) message to user on how to use the bot
        if (!session.conversationData.notFirstVisit) {
            session.send('Reach the Recognizer Menu by typing "help" or "menu". Otherwise type "Hello" or "Good-bye".');
            session.conversationData.useFarewell = true;
            session.conversationData.useGreetings = true;
            session.conversationData.notFirstVisit = true;
        }
        next();
    },
    function (session) {
        // Helper function to tell user status of recognizers
        session.send(recognizerCheck(session));
        builder.Prompts.choice(session, 'Please select a recognizer to disable:', ['Greeting Recognizer', 'Farewell Recongnizer', 'Re-enable Recognizers', 'Do nothing']);
    },
    function (session, results) {
        // Handles recognizers status based on user's choice
        switch (results.response.index) {
            case 0:
                // Turn off 'greetings' recognizer
                session.send('Turning off Greeting Recognizer. Anything matching the "Greeting" intent will redirect to the Recognizer Menu.');
                session.conversationData.useGreetings = false;
                break;
            case 1:
                // Turn off 'farewell' recignizer
                session.send('Turning off Farewell Recognizer. Anything matching the "Farewell" intent will redirect to the Recognizer Menu.');
                session.conversationData.useFarewell = false;
                break;
            case 2:
                // Re-enable all recognizers
                session.send("Re-enabling all recognizers.");
                session.conversationData.useFarewell = true;
                session.conversationData.useGreetings = true;
                break;
            case 3:
                // End dialog while doing nothing
                session.send('Doing nothing.');
                break;
        }
        session.endDialog();
    }
]).triggerAction({ matches: 'RecognizerMenu' });

// Add a greetings dialog
bot.dialog('Greetings', [
    function (session) {
        session.endDialog('Greetings!');
    }
]);

// Add a farewell dialog
bot.dialog('Farewell', [
    function (session) {
        session.endDialog('Farewell!');
    }
]);

//=========================================================
// Helper Function
//=========================================================

// Composes message for user based on session.conversationData.useFarewell and session.conversationData.useGreetings
function recognizerCheck (context) {
    var conversation = context.conversationData;
    var status = '';
    if (conversation.useGreetings || conversation.useFarewell) {
        status += 'Recognizing ';
        if (conversation.useGreetings) {
            status += '\'Greetings\'';
        }
        if (conversation.useFarewell) {
            if (conversation.useGreetings) {
                status += ' and \'Farewells\'';
            } else {
                status += '\'Farewells\'';
            }
        }
        status += '.';
    } else {
        status += 'Not recognizing \'Greetings\' or \'Farewells\'.';
    }
    return status;
}