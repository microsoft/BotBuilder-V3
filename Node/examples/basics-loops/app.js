/*-----------------------------------------------------------------------------
This Bot demonstrates how to use session.replaceDialog() to create a simple
loop that dyanmically populates a form.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector);

bot.dialog('/', [
    function (session) {
        session.beginDialog('/q&a');
    },
    function (session, results) {
        session.send("Thanks %(name)s... You're %(age)s and located in %(state)s.", results.response);
    }
]);

bot.dialog('/q&a', [
    function (session, args) {
        // Save previous state (create on first call)
        session.dialogData.index = args ? args.index : 0;
        session.dialogData.form = args ? args.form : {};

        // Prompt user for next field
        builder.Prompts.text(session, questions[session.dialogData.index].prompt);
    },
    function (session, results) {
        // Save users reply
        var field = questions[session.dialogData.index++].field;
        session.dialogData.form[field] = results.response;

        // Check for end of form
        if (session.dialogData.index >= questions.length) {
            // Return completed form
            session.endDialogWithResult({ response: session.dialogData.form });
        } else {
            // Next field
            session.replaceDialog('/q&a', session.dialogData);
        }
    }
]);

var questions = [
    { field: 'name', prompt: "What's your name?" },
    { field: 'age', prompt: "How old are you?" },
    { field: 'state', prompt: "What state are you in?" }
];