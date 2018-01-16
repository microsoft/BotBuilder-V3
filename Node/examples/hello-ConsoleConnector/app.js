/*-----------------------------------------------------------------------------
A simple "Hello World" bot that can be run from a console window.

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

// Create console connector and listen to stdin for messages 
var connector = new builder.ConsoleConnector().listen();

// Create your bot with a function to receive messages from the user
var bot = new builder.UniversalBot(connector, function (session) {
    session.send("You said: %s", session.message.text);
}).set('storage', inMemoryStorage); // Register in memory storage
