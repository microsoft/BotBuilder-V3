/*-----------------------------------------------------------------------------
This Bot demonstrates how to create a custom prompt using an IntentDialog.  
This example is meant to be a replacement for the "./basics-validatedPrompt"
example.  

The advantage of using an IntentDialog to create a custom prompt is that you
can build prompts of arbitrary complexity and that support branching.  For 
instance you could potentially build a prompt that asks the user to select a
pizza topping and supports answering questions like "which ones are gluten free?"

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');
var meaningOfLife = require('./meaningOfLife');

// Bot Storage: Here we register the state storage for your bot. 
// Default store: volatile in-memory store - Only for prototyping!
// We provide adapters for Azure Table, CosmosDb, SQL Azure, or you can implement your own!
// For samples and documentation, see: https://github.com/Microsoft/BotBuilder-Azure
var inMemoryStorage = new builder.MemoryBotStorage();

// Setup bot and define root waterfall
var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector, [
    function (session) {
        // Ask user the meaning of life
        meaningOfLife.beginDialog(session);
    },
    function (session, results) {
        // Check their answer
        if (results.response) {
            session.send("That's correct! You are wise beyond your years...");
        } else {
            session.send("Sorry you couldn't figure it out. Everyone knows that the meaning of life is 42.");
        }
    }
]).set('storage', inMemoryStorage); // Register in memory storage

// Create prompts
meaningOfLife.create(bot);
