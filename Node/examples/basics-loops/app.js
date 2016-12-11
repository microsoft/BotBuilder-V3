/*-----------------------------------------------------------------------------
This Bot demonstrates how to use session.replaceDialog() to create a simple
loop that dyanmically populates a form.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

// Setup bot and root waterfall
var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector, [
    function (session) {
        session.beginDialog('q&aDialog');
    },
    function (session, results) {
        session.send("Thanks %(name)s... You're %(age)s and located in %(state)s.", results.response);
    }
]);

// Add Q&A dialog
bot.dialog('q&aDialog', [
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
            session.replaceDialog('q&aDialog', session.dialogData);
        }
    }
]);

var questions = [
    { field: 'name', prompt: "What's your name?" },
    { field: 'age', prompt: "How old are you?" },
    { field: 'state', prompt: "What state are you in?" }
];