/*-----------------------------------------------------------------------------
This examples shows how to create an uber bot that aggregates multiple child 
libraries. Since all bots are also libraries you can easily add sub-bots to 
your bot just like you would any library. The only real requirement is that each 
sub-bot should have a unique namespace.

To deal with situations like 'help' where you want each library, including the 
uber bot, to provide their own help dialogs, the framework will naturally 
trigger the dialog/action it determines is most relevant to the user. If the 
user is currently interacting with a child library they'll get that child 
libraries help dialog otherwise they'll get the uber bots help dialog. To 
understand the specific rules for how the framework decides what to choose, 
review the reference docs the Library.bestRouteResult() method. 

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

// Import child libraries
var bot1 = require('./bot1');
var bot2 = require('./bot2');
var profile = require('./profileLib');

// Bot Storage: Here we register the state storage for your bot. 
// Default store: volatile in-memory store - Only for prototyping!
// We provide adapters for Azure Table, CosmosDb, SQL Azure, or you can implement your own!
// For samples and documentation, see: https://github.com/Microsoft/BotBuilder-Azure
var inMemoryStorage = new builder.MemoryBotStorage();

// Setup your uber bot as you normally would 
var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector, [
    function (session, args, next) {
        // Ask user their name on first run
        if (!session.userData.name) {
            profile.changeName(session);
        } else {
            next();
        }
    },
    function (session, results) {
        session.send("Hi %s... Say either 'hello bot1', 'hi bot2', or 'change name'.", session.userData.name);
    }
]).set('storage', inMemoryStorage); // Register in memory storage

// Add libraries to bot
bot.library(bot1.createLibrary());
bot.library(bot2.createLibrary());
bot.library(profile.createLibrary());
