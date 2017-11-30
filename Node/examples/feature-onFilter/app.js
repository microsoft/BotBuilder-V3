/*-----------------------------------------------------------------------------
This Bot demonstrates how to filter intents from recognizers via the new
.onFilter() method. This example uses LUIS recognized intents and the  
.onFilter() method to change the intent from one intent to another.

# RUN THE BOT:

    Run `npm install` in BotBuilder/Node and BotBuilder/Node/core.

    IMPORTANT: Inside of this file, assign your LUIS Endpoint Key to the 
    LuisKey variable. (Line 56)

    Run the bot from the command line using "node app.js" and open the Bot 
    Framework Emulator. Enter into the address bar http://localhost:3978
    /api/messages to open a conversation with the bot. 
    
    Typing "Hi" or "Bye" will reach the "Greetings" and "Farewell" dialogs.
    
    Typing an utterance along the lines of "Can I have water please?" or "Do 
    you have water?" will trigger the "AskForWater" intent. However, the 
    .onFilter() method will change the intent in the chatbot to "Filtered".

    Anything triggering the default handler will send instructions to the user 
    on how to use the bot.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core'); 
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

// Place your LUIS Endpoint Key here
var LuisKey = '<Your-LUIS-Endpoint-Key>';
var LuisModel = 'https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/c6d76fec-11f6-48fd-b5e2-3bfe708e1060?subscription-key=' + LuisKey;

// Create our LuisRecognizer and filter out an intent
var recognizer = new builder.LuisRecognizer(LuisModel)
    .onFilter(function(context, result, callback) {
        // If the "AskForWater" intent is returned from LUIS, the intent is changed to "Filtered".
        if (result.intent == 'AskForWater') {
            callback(null, { score: 1.0, intent: 'Filtered' });
        } else {
        // Otherwise we pass through the result from LUIS 
            callback(null, result);
        }
    });

// Create our IntentDialog and add recognizer
var intents = new builder.IntentDialog({ recognizers: [recognizer] });

bot.dialog('/', intents);

// Match our "Greetings" and "Farewell" intents with their dialogs, along with the new "Filtered" intent.
intents.matches('Greetings', 'Greetings');
intents.matches('Farewell', 'Farewell');
intents.matches('Filtered', 'Filtered');

// If no intent is recognized, inform user of how to trigger intents.
intents.onDefault(function(session, args) {
    session.send('You\'ve reached the None intent handler!');
    session.endDialog('Trigger the "AskForWater" intent by asking for water, then watch as you are redirected to the "Filtered" intent handler. You can also say "Hi" or "Bye" to reach the "Greetings" and "Farewell" intent handlers.');
});

//=========================================================
// Bots Dialogs
//=========================================================

bot.dialog('Farewell', function(session, args) {
    session.endDialog('Good-bye!');
});

bot.dialog('Greetings', function(session, args) {
    session.endDialog('Hi there!');
});

// Add a dialog for the "Filtered" intent. This dialog is only reached when the user asks for water
bot.dialog('Filtered', function (session, args) {
    session.send('You asked for water and instead reached the "Filtered" intent.');
    session.endDialog('I\'m sorry, but I\'m only a chatbot and cannot give you water...');
});
