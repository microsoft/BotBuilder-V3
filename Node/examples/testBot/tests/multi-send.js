var builder = require('../../../');

module.exports = {
    description: "Says 3 things. ['Say this', 'And this', 'Finally this']",
    async: true,
    addDialogs: addDialogs,
    run: run 
};

function addDialogs(bot) {
    bot.add('/tests/multi-send', function (session) {
        session.send('Say this');
        session.send('And this');
        session.endDialog('Finally this');
    });
}

function run(session) {
    session.beginDialog('/tests/multi-send');
}
