/*-----------------------------------------------------------------------------
This Bot demonstrates how to trigger a dialog using a custom onFindAction() 
function that will be run for every incoming message. This custom function lets
you not only trigger the dialog but also pass it custom arguments.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

// Bot Storage: Here we register the state storage for your bot. 
// Default store: volatile in-memory store - Only for prototyping!
// We provide adapters for Azure Table, CosmosDb, SQL Azure, or you can implement your own!
// For samples and documentation, see: https://github.com/Microsoft/BotBuilder-Azure
var inMemoryStorage = new builder.MemoryBotStorage();

// Setup bot and default message handler
var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector, function (session) {
    session.send("You said: '%s'. Try asking for 'help'.", session.message.text);
}).set('storage', inMemoryStorage); // Register in memory storage

// Add help dialog with a trigger action bound to a custom onFindAction() function looking 
// for the user to ask for help.
bot.dialog('helpDialog', function (session, args) {
    session.endDialog(args.topic + ": This bot will echo back anything you say.");
}).triggerAction({ 
    onFindAction: function (context, callback) {
        // Recognize users utterance
        switch (context.message.text.toLowerCase()) {
            case 'help':
                // You can trigger the action with callback(null, 1.0) but you're also
                // allowed to return additional properties which will be passed along to
                // the triggered dialog.
                callback(null, 1.0, { topic: 'general' });
                break;
            default:
                callback(null, 0.0);
                break;
        }
    } 
});

