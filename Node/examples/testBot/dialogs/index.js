var builder = require('../../../');
var prompts = require('../prompts');
var list = require('./list');
var run = require('./run');
var runAsync = require('./run-async');
var tests = require('../tests/index');

module.exports = {
    addDialogs: addDialogs  
};

/**
 * Triggers loading of all the bots dialogs.
 * 
 * For the TestBot we first load the main dialog that listens for /commands and then we load the
 * the individual dialogs for each command. We wanted new tests to be easy to add so the index.js
 * file in the ../tests directory dinamically loads all of the *.js files it finds in that directory.
 * 
 * We need to add the dialogs for each test so after we've loaded our command dialogs we then run 
 * through each test and load the tests dialogs. 
 */
function addDialogs(bot, addressConverter) {
    // Add the main dialog
    bot.add('/', new builder.CommandDialog()
        .matches('^\/help', builder.DialogAction.send(prompts.helpMessage))
        .matches('^\/list', '/list')
        .matches('^\/run-async', '/run-async')
        .matches('^\/run', '/run')
        .matches('^\/quit', builder.DialogAction.endDialog(prompts.goodbye))
        .onDefault(builder.DialogAction.send(prompts.unknown)));
    
    // Add dialogs for commands
    list.addDialogs(bot);
    run.addDialogs(bot);
    runAsync.addDialogs(bot, addressConverter);
    
    // Add tests
    for (var key in tests) {
        var test = tests[key];
        test.addDialogs(bot);
    }
}
