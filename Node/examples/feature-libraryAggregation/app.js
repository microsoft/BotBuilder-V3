/*-----------------------------------------------------------------------------
This examples shows how to create an uber bot that aggregates multiple child 
bots and libraries. As of v3.5 all bots are libraries and they can easily be 
aggregated just like any library. The only real requirement is that each bot 
should have a unique namespace.

To deal with situations like 'help' where you want each bot, including the uber 
bot, to provide their own help dialogs, the framework will naturally trigger the 
dialog/action it determines is most relevant to the user. If the user is 
currently interacting with a child bot they'll get that child bots help dialog 
otherwise they'll get the uber bots help dialog. To understand the specific 
rules for how the framework decides what to choose, review the reference docs
the Library.bestRouteResult() method. 

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

// Setup your uber bot as you normally would 
var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector);

// Import child libraries
var bot1 = require('./bot1');
var bot2 = require('./bot2');
var profile = require('./profileLib');

// Initialize them with their parent bot
bot1.create(bot);
bot2.create(bot);
profile.create(bot);

// Add default dialog to uber bot
bot.dialog('/', [
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
]);

