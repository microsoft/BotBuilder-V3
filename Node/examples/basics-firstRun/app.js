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

var builder = require('../../core/');

var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector);
bot.dialog('/', function (session) {
    session.send("%s, I heard: %s", session.userData.name, session.message.text);
    session.send("Say something else...");
});

// Install First Run middleware and dialog
bot.use(builder.Middleware.firstRun({ version: 1.0, dialogId: '*:/firstRun' }));
bot.dialog('/firstRun', [
    function (session) {
        builder.Prompts.text(session, "Hello... What's your name?");
    },
    function (session, results) {
        // We'll save the users name and send them an initial greeting. All 
        // future messages from the user will be routed to the root dialog.
        session.userData.name = results.response;
        session.endDialog("Hi %s, say something to me and I'll echo it back.", session.userData.name); 
    }
]);
