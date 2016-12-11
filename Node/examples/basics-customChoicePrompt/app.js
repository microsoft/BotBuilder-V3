/*-----------------------------------------------------------------------------
This Bot demonstrates how to create a custom prompt the conditionally uses a 
choice() prompt. The user can either pick an option from the list of choices 
or enter a new choice.  The sample also shows how to prompt the user to add
their choice to the list.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');
var savedListPrompt = require('./savedListPrompt');

// Setup bot and define root waterfall
var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector, [
    function (session) {
        // Prompt for message to send
        savedListPrompt.beginDialog(session, {
            field: 'savedMessages',
            choicesPrompt: "What message would you like to send? Choose a saved message from the list or enter a new one.",
            noChoicesPrompt: "What message would you like to send?"
        });
    },
    function (session, results) {
        session.send("Sending message: " + results.response);
    }
]);

// Create prompts
savedListPrompt.create(bot);
