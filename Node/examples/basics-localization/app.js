/*-----------------------------------------------------------------------------
This Bot demonstrates how the default localizer can be used to localize prompts. 

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.

-----------------------------------------------------------------------------*/

var builder = require('../../core/');
var fs = require("fs");

var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector, {localizerSettings: { botLocalePath: "./locale", defaultLocale: "en" }});

// Locale detection middleware
bot.use({
    botbuilder: function (session, next) {
        // Use proper method to detect locale -- typing es triggers the es locale to be set
        if (/^es/i.test(session.message.text)) {
            // Set the locale for the session once its detected
            session.preferredLocale("es", (err) => {
                next();
            });

        // Use proper method to detect locale -- typing us triggers the en-us locale to be set            
        } else if (/^us/i.test(session.message.text)) {
            // Set the locale for the session once its detected
            session.preferredLocale("en-us", (err) => {
                next();
            });
        } else {
            // By not setting the preferred locale, we will fallback to the default (en in this case) 
            next();
        }
    }
});


bot.dialog('/', [
    function (session, args, next) {
        // This key is present in all the locale/*/index.json files, so whichever locale is set on the session should 
        // dictate what's output to the user
        session.send("Hello World");

        // This key is only in the locale/en/index.json file, so en-us will fallback to this because its a child of en.
        // es will also fallback to this, but only because our default locale is en.  
        session.send("Hello World2");

        // This key is not present in any of the locale/*/index.json files, so we will just end up showing 'Hello World3'
        session.send("Hello World3");

        // Supply the localized key to the prompt.
        // Note that our locale/en/botbuilder.json file overrides the system's default prompt when a number is not supplied
        builder.Prompts.choice(session, "age", "y|n|idk")   
    },
    function (session, results) {
        if (results.response) {
            session.send("Thanks");
        } else {
            session.send("Sorry!");
        }
    }
]);
