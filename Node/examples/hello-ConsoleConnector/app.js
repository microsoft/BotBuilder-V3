/*-----------------------------------------------------------------------------
A simple "Hello World" bot that can be run from a console window.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.

-----------------------------------------------------------------------------*/

var builder = require('../../core/');

// Create console connector and listen to stdin for messages 
var connector = new builder.ConsoleConnector().listen();

// Create your bot with a function to receive messages from the user
var bot = new builder.UniversalBot(connector, function (session) {
    session.send("You said: %s", session.message.text);
});
