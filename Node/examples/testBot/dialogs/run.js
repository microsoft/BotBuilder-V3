var builder = require('../../../');
var prompts = require('../prompts');
var tests = require('../tests/index');

module.exports = {
    addDialogs: addDialogs  
};

function addDialogs(bot) {
    bot.add('/run', [
        function (session, args, next) {
            // See if the user specified a valid test to run by name.
            var match;
            var utterance = session.message.text;
            var brk = utterance.indexOf(' ');
            var name = brk > 0 && brk < utterance.length ? utterance.substr(brk + 1).trim() : null;
            if (name) {
                match = builder.EntityRecognizer.findBestMatch(tests, name);        
            }
            
            // Prompt for test if no valid name specified.
            if (!match) {
                builder.Prompts.choice(session, prompts.runPrompt, tests);
            } else {
                next({ response: match });
            }
        },
        function (session, results) {
            // Run the test unless the user canceled.
            if (results.response) {
                session.dialogData.name = results.response.entity;
                var test = tests[session.dialogData.name];
                test.run(session);
            } else {
                session.endDialog(prompts.canceled);
            }
        },
        function (session) {
            // Test completed execution.
            session.endDialog(prompts.testCompleted, session.dialogData.name);
        }
    ]);
}
