/*-----------------------------------------------------------------------------
Creates a prompt that lets a user save their responses for future use. The list
will be saved to a field off session.userData. 
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

exports.beginDialog = function (session, options) {
    session.beginDialog('savedListPrompt', options);
}

exports.create = function (bot) {
    bot.dialog('savedListPrompt', [
        function (session, args) {
            // Check to see if there are any saved values.
            session.dialogData.args = args;
            var choices = session.userData[args.field];
            if (choices) {
                builder.Prompts.choice(session, args.choicesPrompt, choices, { maxRetries: 0 });
            } else {
                builder.Prompts.text(session, args.noChoicesPrompt);
            }
        },
        function (session, results) {
            var args = session.dialogData.args;
            if (results.response && results.response.entity) {
                // Return saved value
                var choices = session.userData[args.field];
                session.endDialogWithResult({ response: choices[results.response.entity] });
            } else {
                // Store and prompt user to save to list.
                session.dialogData.value = session.message.text;
                builder.Prompts.confirm(session, args.savePrompt || 'Would you like to save this for future use?');
            }
        },
        function (session, results) {
            var value = session.dialogData.value;
            if (results.response) {
                // Add value to list.
                var args = session.dialogData.args;
                session.replaceDialog('savedListPrompt/add', {
                    field: args.field,
                    addPrompt: args.addPrompt,
                    value: value
                });
            } else {
                // Return entered value
                session.endDialogWithResult({ response: value });
            }
        }
    ]);

    bot.dialog('savedListPrompt/add', [
        function (session, args) {
            // Prompt user for the name
            session.dialogData.args = args;
            builder.Prompts.text(session, args.addPrompt || 'What would you like to name it?');
        },
        function (session, results) {
            // Save the value to the list and return it
            var args = session.dialogData.args;
            var choices = session.userData[args.field] || {};
            choices[results.response] = args.value;
            session.userData[args.field] = choices;
            session.endDialogWithResult({ response: args.value });
        }
    ]);
}
