/*-----------------------------------------------------------------------------
This Bot demonstrates how to create a custom prompt that validates a users 
input.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.

-----------------------------------------------------------------------------*/

var builder = require('../../core/');

var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector);

bot.dialog('/', [
    function (session) {
        // call custom prompt
        session.beginDialog('/meaningOfLife', { 
            prompt: "What's the meaning of life?", 
            retryPrompt: "Sorry that's incorrect. Guess again." 
        });
    },
    function (session, results) {
        // Check their answer
        if (results.response) {
            session.send("That's correct! The meaning of life is 42.");
        } else {
            session.send("Sorry you couldn't figure it out. Everyone knows that the meaning of life is 42.");
        }
    }
]);

bot.dialog('/meaningOfLife', builder.DialogAction.validatedPrompt(builder.PromptType.text, function (response) {
    return response === '42';
}));
