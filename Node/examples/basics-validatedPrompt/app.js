/*-----------------------------------------------------------------------------
This Bot demonstrates how to create a custom prompt that validates a users 
input.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.

-----------------------------------------------------------------------------*/

var builder = require('../../');

var bot = new builder.TextBot();
bot.add('/', [
    function (session) {
        // call custom prompt
        session.beginDialog('/meaningOfLifePrompt', { maxRetries: 3 });
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

bot.add('/meaningOfLifePrompt', function (session, results) {
    results = results || {};

    // Validate response
    var valid = false;
    if (results.response) {
        valid = (results.response == '42');
    } 

    // Return results or prompt the user     
    if (valid || (results.resumed == builder.ResumeReason.canceled)) {
        // The user either answered the question correctly or they canceled by saying "nevermind"  
        session.endDialog(results);
    } else if (!session.dialogData.hasOwnProperty('maxRetries')) {
        // First call to the pormpt so process args passed to the prompt
        session.dialogData.maxRetries = results.maxRetries || 2;
        builder.Prompts.text(session, "What's the meaning of life?");
    } else if (session.dialogData.maxRetries > 0) {
        // User guessed wrong but they have retries left
        session.dialogData.maxRetries--;
        builder.Prompts.text(session, "Sorry that's not it. Guess again. What's the meaning of life?");
    } else {
        // User to failed to guess in alloted number of tries
        session.endDialog({ resumed: builder.ResumeReason.notCompleted });
    }
});

bot.listenStdin();