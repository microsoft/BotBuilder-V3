/*-----------------------------------------------------------------------------
This Bot demonstrates how to add a trigger action to a dialog that will cause 
it to automatically be pushed onto the dialog stack based on a users utterance.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector);

// Add default dialog
bot.dialog('/', function (session) {
    session.send("You said: '%s'. Try asking for 'help'.", session.message.text);
});

// Add help dialog with a trigger action bound to a regular expression looking for the users
// to ask for help.
bot.dialog('/help', function (session) {
    session.endDialog("This bot will echo back anything you say.");
}).triggerAction({ matches: /^(help|options)/i });

