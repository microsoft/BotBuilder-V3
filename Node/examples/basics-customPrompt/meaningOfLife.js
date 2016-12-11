/*-----------------------------------------------------------------------------
Basic pattern for exposing a custom prompt. The create() function should be
called once at startup and then beginDialog() can be called everytime you
wish to invoke the prompt.
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

exports.beginDialog = function (session, options) {
    session.beginDialog('meaningOfLife', options || {});
}

exports.create = function (bot) {
    var prompt = new builder.IntentDialog()
        .onBegin(function (session, args) {
            // Save args passed to prompt
            session.dialogData.retryPrompt = args.retryPrompt || "Sorry that's incorrect. Guess again. Or do you give up?";

            // Send initial prompt
            // - This isn't a waterfall so you shouldn't call any of the built-in Prompts.
            session.send(args.prompt || "What's the meaning of life?");
        })
        .matches(/(give up|quit|skip|yes)/i, function (session) {
            // Return 'false' to indicate they gave up
            session.endDialogWithResult({ response: false });
        })
        .onDefault(function (session) {
            // Validate users reply.
            if (session.message.text == '42') {
                // Return 'true' to indicate success
                session.endDialogWithResult({ response: true });
            } else {
                // Re-prompt user
                session.send(session.dialogData.retryPrompt);
            }
        });
    bot.dialog('meaningOfLife', prompt);
}
