/*-----------------------------------------------------------------------------
This Bot demonstrates how to add cancel actions which will cause the dialog to 
end based off a users utterance. In the sample we let the user create a list
of items and they can say either 'end list' to finish the list or 'cancel' to
discard it. Both commands are achieved using cancel actions and the 'endList'
action contains a custom onSelectAction() handler to return the list being 
built.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector);

// Add default dialog
bot.dialog('/', [
    function (session) {
        session.beginDialog('/listBuilder');
    },
    function (session, results) {
        if (results.response) {
            session.send(printList(results.response));
        }
    }
]);

bot.dialog('/listBuilder', function (session) {
    if (!session.dialogData.list) {
        session.dialogData.list = [];
        session.send("Each message will added as a new item to the list.\nSay 'end list' when finished or 'cancel' to discard the list.\n")
    } else {
        session.dialogData.list.push(session.message.text);
        session.save();
    }
}).cancelAction('cancelList', "List canceled", { 
    matches: /^cancel/i,
    confirmPrompt: "Are you sure?"
}).cancelAction('endList', null, {
    matches: /^end list/i,
    onSelectAction: function (session) {
        session.endDialogWithResult({ response: session.dialogData.list });
    }
});

function printList(list) {
    var msg = "\nHere's your list:";
    for (var i = 0; i < list.length; i++) {
        msg += '\n* ' + list[i];
    }
    return msg;
}