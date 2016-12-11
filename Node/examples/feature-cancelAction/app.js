/*-----------------------------------------------------------------------------
This Bot demonstrates how to add a cancel action that will cause a dialog to 
automatically end based off a users utterance. In the sample we let the user 
create a list of items and they can say either 'end list' to finish the list or 
'cancel' to discard it.

The sample also shows how to session.save() to persist changes to one of the 
data bags when your bot doesn't send a reply to the user.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

// Setup bot and root waterfall
var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector, [
    function (session) {
        // Start a new list
        session.beginDialog('listBuilderDialog');
    },
    function (session, results) {
        // Print out results if not canceled
        if (results.response) {
            session.send(printList(results.response));
        }
    }
]);

// Add dialog for creating a list
bot.dialog('listBuilderDialog', function (session) {
    if (!session.dialogData.list) {
        // Start a new list 
        session.dialogData.list = [];
        session.send("Each message will added as a new item to the list.\nSay 'end list' when finished or 'cancel' to discard the list.\n")
    } else if (/end.*list/i.test(session.message.text)) {
        // Return current list
        session.endDialogWithResult({ response: session.dialogData.list });
    } else {
        // Add item to list and save() change to dialogData
        session.dialogData.list.push(session.message.text);
        session.save();
    }
}).cancelAction('cancelList', "List canceled", { 
    matches: /^cancel/i,
    confirmPrompt: "Are you sure?"
});

// Helper to print out list
function printList(list) {
    var msg = "\nHere's your list:";
    for (var i = 0; i < list.length; i++) {
        msg += '\n* ' + list[i];
    }
    return msg;
}