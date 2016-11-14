/*-----------------------------------------------------------------------------
This Bot demonstrates how to intercept an action before it runs using  a custom 
onSelectAction() function. In this specific example we have two tasks for 
either flipping a coin and rolling some dice. The onSelectAction() function for
these two tasks lets us first prevent the user from starting the same task twice
and then ensures that the current task is ended before a new one is started.

You can use this function to do other more advanced things like prompt a user to
confirm they want to cancel an order before swtching tasks or use it save the 
users current task and return to it later.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector);

// Add default dialog
bot.dialog('/', function (session) {
    session.send("Ask me to flip a coin or roll some dice.");
});

// Add flipCoin task
bot.dialog('/flipCoin', [
    function (session, args) {
        builder.Prompts.choice(session, "Choose heads or tails.", "heads|tails", { listStyle: builder.ListStyle.none })
    },
    function (session, results) {
        var flip = Math.random() > 0.5 ? 'heads' : 'tails';
        if (flip == results.response.entity) {
            session.endDialog("It's %s. YOU WIN!", flip);
        } else {
            session.endDialog("Sorry... It was %s. you lost :(", flip);
        }
    }
]).triggerAction({ 
    matches: /flip/i,
    onSelectAction: function (session, args, next) {
        switchTasks(session, args, next, "We're already flipping a coin.");
    }
});

// Add rollDice task
bot.dialog('/rollDice', [
    function (session, args) {
        builder.Prompts.number(session, "How many dice should I roll?");
    },
    function (session, results) {
        if (results.response > 0) {
            var msg = "I rolled:";
            for (var i = 0; i < results.response; i++) {
                var roll = Math.floor(Math.random() * 6) + 1;
                msg += ' ' + roll.toString(); 
            }
            session.endDialog(msg);
        } else {
            session.endDialog("Ummm... Ok... I rolled air.");
        }
    }
]).triggerAction({
    matches: /roll/i,
    onSelectAction: function (session, args, next) {
        switchTasks(session, args, next, "We're already rolling some dice.");
    }
});

function switchTasks(session, args, next, alreadyActiveMessage) {
    // Check to see if we're already active.
    // - We're assuming that we're being called from a triggerAction() some
    //   args.action is the fully qualified dialog ID.
    var stack = session.dialogStack();
    if (builder.Session.findDialogStackEntry(stack, args.action) >= 0) {
        session.send(alreadyActiveMessage);
    } else {
        // Clear stack and switch tasks
        session.clearDialogStack();
        next();
    }
}
