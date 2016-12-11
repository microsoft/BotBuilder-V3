/*-----------------------------------------------------------------------------
This Bot demonstrates basic localization support for a bot. It shows how to:

* Configure the bots default language and localization file path.
* Prompt the user for their preferred language.
* Localize all of a bots prompts across multiple languages.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.

-----------------------------------------------------------------------------*/

var builder = require('../../core/');

// Setup bot and root waterfall
var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector, [
    function (session) {
        session.send("greeting");
        session.send("instructions");
        session.beginDialog('localePickerDialog');
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

// Configure bots default locale and locale folder path.
bot.set('localizerSettings', {
    botLocalePath: "./customLocale", 
    defaultLocale: "en" 
});


// Add locale picker dialog 
bot.dialog('localePickerDialog', [
    function (session) {
        // Prompt the user to select their preferred locale
        builder.Prompts.choice(session, "locale_prompt", 'English|Español|Italiano');
    },
    function (session, results) {
        // Update preferred locale
        var locale;
        switch (results.response.entity) {
            case 'English':
                locale = 'en';
                break;
            case 'Español':
                locale = 'es';
                break;
            case 'Italiano':
                locale = 'it';
                break;
        }
        session.preferredLocale(locale, function (err) {
            if (!err) {
                // Locale files loaded
                session.endDialog('locale_updated');
            } else {
                // Problem loading the selected locale
                session.error(err);
            }
        });
    }
]);
