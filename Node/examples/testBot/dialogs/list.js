var builder = require('../../../');
var prompts = require('../prompts');
var tests = require('../tests/index');

module.exports = {
    addDialogs: addDialogs  
};

function addDialogs(bot) {
    bot.add('/list', function (session) {
        // Render the lists of tests
        var items = '';
        for (var key in tests) {
            var test = tests[key];
            var description = test.description || (key + ' test');
            if (!test.async) {
                description += ' [NO ASYNC]';
            }
            items += session.gettext(prompts.testListItem, { name: key, description: description});
        }
        session.endDialog(prompts.testList, items);
    });
}
