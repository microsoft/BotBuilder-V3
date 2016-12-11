var builder = require('../../core/');

// For federation you don't need to provide a connector but you should
// ensure that each bot being federated over has a unique library name.
var bot = new builder.UniversalBot(null, null, 'bot2');

// Export createLibrary() function
exports.createLibrary = function () {
    return bot.clone();
}

// Add a dialog with a trigger action like you normally would.
bot.dialog('/greeting', function (session) {
    session.endDialog("Hello %s... I'm Bot2", session.userData.name || 'there');
}).triggerAction({ matches: /(hello|hi).*bot2/i });
