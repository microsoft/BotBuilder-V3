/*-----------------------------------------------------------------------------
A bot for testing various features of the framework.  See the README.md file 
for usage instructions.
-----------------------------------------------------------------------------*/

var builder = require('../../');
var index = require('./dialogs/index')

var textBot = new builder.TextBot();
index.addDialogs(textBot, function (message) {
    return {
        to: message.from,
        from: message.to    
    };
});

textBot.listenStdin();
