/*-----------------------------------------------------------------------------
This Bot demonstrates how to create a First Run experience using a piece of
middleware. 

The middleware function will be run for every incoming message and its simply
using a flag persisted off userData to know if the user been sent to the 
/firstRun dialog. The first run experience can be as simple or as complex as
you'd like. In our example we're prompting the user for their name but if you
just wanted to show a simple message you could have called session.send() 
instead of session.beginDialog().

You can also use a version number instead of a flag if say you need to 
periodically update your Terms Of Use and want to re-show an existing user the 
new TOU on their next interaction with the bot.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.

-----------------------------------------------------------------------------*/

var builder = require('../../');

var bot = new builder.TextBot();
bot.add('/', function (session) {
    session.send("Hi %s, what can I help you with?", session.userData.name);
});

// Install First Run middleware and dialog
bot.use(function (session, next) {
    if (!session.userData.firstRun) {
        session.userData.firstRun = true;
        session.beginDialog('/firstRun');
    } else {
        next();
    }
});
bot.add('/firstRun', [
    function (session) {
        builder.Prompts.text(session, "Hello... What's your name?");
    },
    function (session, results) {
        // We'll save the prompts result and return control to main through
        // a call to replaceDialog(). We need to use replaceDialog() because
        // we intercepted the original call to main and we want to remove the
        // /firstRun dialog from the callstack. If we called endDialog() here
        // the conversation would end since the /firstRun dialog is the only 
        // dialog on the stack.
        session.userData.name = results.response;
        session.replaceDialog('/'); 
    }
]);

bot.listenStdin();