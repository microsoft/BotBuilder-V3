/*-----------------------------------------------------------------------------
This Bot demonstrates how the default localizer works.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.

-----------------------------------------------------------------------------*/

var builder = require('../../core/');
var fs = require("fs");

var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector, {localizerSettings: { botLocalePath: "./locale", defaultLocale: "es" }});


bot.dialog('/', [function (session, args, next) {
    session.message.textLocale = "en-us";

    // key in es, should see es
    session.send("Hello World");

    session.message.textLocale = "en-us";

    // key in en-us, should see en-us
    session.send("Hello World2");
    
    // key in only en, should see en
    session.send("Hello World3");

    // key doesn't exist -- should fall back to the key
    session.send("Hello World4");    
}]);
