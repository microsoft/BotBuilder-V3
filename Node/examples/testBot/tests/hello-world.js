var builder = require('../../../');
var prompts = require('../prompts');
var tests = require('../tests/index');

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
