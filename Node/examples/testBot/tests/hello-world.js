var builder = require('../../../');

module.exports = {
    description: "Says 'Hello World'",
    async: true,
    addDialogs: addDialogs,
    run: run 
};

function addDialogs(bot) {
    bot.add('/tests/hello-world', function (session) {
        session.endDialog('Hello World');
    });
}

function run(session) {
    session.beginDialog('/tests/hello-world');
}
