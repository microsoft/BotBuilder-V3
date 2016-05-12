
var builder = require('../../../');

module.exports = {
    description: "Lets you manually test each built-in prompt.",
    async: true,
    addDialogs: addDialogs,
    run: run 
};

function addDialogs(bot) {
    bot.add('/tests/prompts', [
        function (session) {
            session.send("Starting test. Just answer each prompt and your answer will be echoed back to you.");
            builder.Prompts.text(session, "text: enter some text.");
        },
        function (session, results) {
            if (results && results.response) {
                session.send("You entered '%s'", results.response);
                builder.Prompts.number(session, "number: enter a number.");
            } else {
                session.endDialog("You canceled.");
            }
        },
        function (session, results) {
            if (results && results.response) {
                session.send("You entered '%s'", results.response);
                builder.Prompts.choice(session, "choice: choose a list style.", "auto|inline|list|button|none");
            } else {
                session.endDialog("You canceled.");
            }
        },
        function (session, results) {
            if (results && results.response) {
                var style = builder.ListStyle[results.response.entity];
                builder.Prompts.choice(session, "choice: now pick an option.", "option A|option B|option C", { listStyle: style });
            } else {
                session.endDialog("You canceled.");
            }
        },
        function (session, results) {
            if (results && results.response) {
                session.send("You chose '%s'", results.response.entity);
                builder.Prompts.confirm(session, "confirm: is this test going well?");
            } else {
                session.endDialog("You canceled.");
            }
        },
        function (session, results) {
            if (results && results.response) {
                session.send("You chose '%s'", results.response ? 'yes' : 'no');
                builder.Prompts.time(session, "time: enter a time.");
            } else {
                session.endDialog("You canceled.");
            }
        },
        function (session, results) {
            if (results && results.response) {
                session.endDialog("Recognized Entity: %s", JSON.stringify(results.response));
            } else {
                session.endDialog("You canceled.");
            }
        }
    ]);
}

function run(session) {
    session.beginDialog('/tests/prompts');
}
