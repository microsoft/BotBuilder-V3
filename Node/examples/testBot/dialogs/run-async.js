var builder = require('../../../');
var prompts = require('../prompts');
var tests = require('../tests/index');

// Filter to only list of async tests
var asyncTests = {};
for (var key in tests) {
    var test = tests[key];
    if (test.async) {
        asyncTests[key] = test;
    }
}

module.exports = {
    addDialogs: addDialogs  
};

function addDialogs(bot, addressConverter) {
    bot.add('/run-async', [
        function (session, args, next) {
            // See if the user specified a valid test to run by name.
            var match;
            var utterance = session.message.text;
            var brk = utterance.indexOf(' ');
            var name = brk > 0 && brk < utterance.length ? utterance.substr(brk + 1).trim() : null;
            if (name) {
                match = builder.EntityRecognizer.findBestMatch(asyncTests, name);        
            }
            
            // Prompt for test if no valid name specified.
            if (!match) {
                builder.Prompts.choice(session, prompts.runPrompt, asyncTests);
            } else {
                next({ response: match });
            }
        },
        function (session, results) {
            // Run the test unless the user canceled.
            if (results.response) {
                var name = results.response.entity;
                var address = addressConverter(session.message);
                setTimeout(function () {
                    bot.beginDialog(address, '/run-async/runner', { name: name });
                }, 4000);
                session.endDialog(prompts.runAsyncTest, name);
            } else {
                session.endDialog(prompts.canceled);
            }
        }
    ]);
    
    bot.add('/run-async/runner', [
        function (session, args) {
            // Execute test
            session.dialogData.name = args.name;
            var test = tests[session.dialogData.name];
            test.run(session);
        },
        function (session) {
            // Test completed execution.
            session.endDialog(prompts.testCompleted, session.dialogData.name);
        }
    ])
}

