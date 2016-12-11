/*-----------------------------------------------------------------------------
This Bot demonstrates how to add a simple context aware help system to your 
bot. The '/help' dialogs triggerAction() will invoke the dialog anytime 
someone asks for help. It can be made context aware by adding a 
beginDialogAction('<name>', '/help') to other dialogs throughout your bot. You
can use the name of the action that triggered the dialog to render context 
sensitive help. 

This system leverages the fact that anytime multiple actions are triggered the
framework will favor the action for the deepest dialog on the stack. The name
of the action that started the dialog is then passed in which is what we use to
determine the context.  

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

// Setup bot and root message handler.
var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector, function (session) {
    session.send("Ask me to flip a coin or roll some dice.");
});

// Setup help system
bot.recognizer(new builder.RegExpRecognizer('HelpIntent', /^(help|options)/i));
bot.dialog('helpDialog', function (session, args) {
    switch (args.action) {
        default:
            // args.action is '*:/help' indicating the triggerAction() was matched
            session.endDialog("You can say 'flip a coin' or 'roll dice'.");
            break;
        case 'flipCoinHelp':
            session.endDialog("Say 'heads' or 'tails'.");
            break;
        case 'rollDiceHelp':
            session.endDialog("Say the number of dice you'd like rolled.");
            break;
    }
}).triggerAction({ matches: 'HelpIntent' });

// Add flipCoin task
bot.dialog('flipCoinDialog', [
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
    matches: /flip/i
}).beginDialogAction('flipCoinHelp', 'helpDialog', { matches: 'HelpIntent' });

// Add rollDice task
bot.dialog('rollDiceDialog', [
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
    matches: /roll/i
}).beginDialogAction('rollDiceHelp', 'helpDialog', { matches: 'HelpIntent' });

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