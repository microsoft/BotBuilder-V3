/*-----------------------------------------------------------------------------
This Bot demonstrates how to create a custom library.  It takes the locale
picker from the "./basics-localization" example and moves it into a seperate
library.  This library could be packaged up into it's own NPM and shared across
multiple bots. 

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');
var localeTools = require('./localeTools');

var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector);

// Initialize prompts and libraries
// - For languageDetectionKey follow instructions at:
//
//      https://azure.microsoft.com/en-us/documentation/articles/cognitive-services-text-analytics-quick-start/
// 
localeTools.create(bot, { languageDetectionKey: process.env.LANGUAGE_DETECTION_KEY });

bot.dialog("/", [
    function (session) {
        session.send("greeting");
        session.send("instructions");
        localeTools.chooseLocale(session);
    },
    function (session) {
        builder.Prompts.text(session, "text_prompt");
    },
    function (session, results) {
        session.send("input_response", results.response);
        builder.Prompts.number(session, "number_prompt");
    },
    function (session, results) {
        session.send("input_response", results.response);
        builder.Prompts.choice(session, "listStyle_prompt", "auto|inline|list|button|none");
    },
    function (session, results) {
        // You can use the localizer manually to load a localized list of options.
        var style = builder.ListStyle[results.response.entity];
        var options = session.localizer.gettext(session.preferredLocale(), "choice_options");
        builder.Prompts.choice(session, "choice_prompt", options, { listStyle: style });
    },
    function (session, results) {
        session.send("choice_response", results.response.entity);
        builder.Prompts.confirm(session, "confirm_prompt");
    },
    function (session, results) {
        // You can use the localizer manually to load prompts from another namespace.
        var choice = results.response ? 'confirm_yes' : 'confirm_no';
        session.send("choice_response", session.localizer.gettext(session.preferredLocale(), choice, 'BotBuilder'));
        builder.Prompts.time(session, "time_prompt");
    },
    function (session, results) {
        session.send("time_response", JSON.stringify(results.response));
        session.endDialog("demo_finished");
    }
]);
