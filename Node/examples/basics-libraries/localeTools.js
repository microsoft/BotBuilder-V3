/*-----------------------------------------------------------------------------
Basic pattern for exposing a library. The create() function should be
called once at startup and then the library exposes wrapper methods for 
invoking its various prompts.

The primary roll of a library is to move all of the libraries dialogs into a
seperate namespace that won't collide with the bots dialogs.  That means there
is an extra step that on the library side of things you need to include your
libraries namespace when calling session.beginDialog(). You technically only 
need to do this when you're being called into from a different namespace but
it doesn't hurt to allways include the namespace.
-----------------------------------------------------------------------------*/

var builder = require('../../core/');
var request = require('request');

//=========================================================
// Library creation
//=========================================================

var lib = new builder.Library('localeTools');

exports.createLibrary = function () {
    return lib;
}

//=========================================================
// Locale Picker Prompt
//=========================================================

exports.chooseLocale = function (session, options) {
    // Start dialog in libraries namespace
    session.beginDialog('localeTools:chooseLocale', options || {});
}

lib.dialog('chooseLocale', [
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


//=========================================================
// Language Detection Middleware
//=========================================================

exports.languageDetection = function (apiKey) {
    if (!apiKey) {
        console.warn('No API Key passed to localeTools.languageDetection().');
    }
    return {
        receive: function (event, next) {
            if (apiKey && event.text && !event.textLocale) {
                var options = {
                    method: 'POST',
                    url: 'https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/languages?numberOfLanguagesToDetect=1',
                    body: { documents: [{ id: 'message', text: event.text }]},
                    json: true,
                    headers: {
                        'Ocp-Apim-Subscription-Key': apiKey
                    }
                };
                request(options, function (error, response, body) {
                    if (!error) {
                        if (body && body.documents && body.documents.length > 0) {
                            var languages = body.documents[0].detectedLanguages;
                            if (languages && languages.length > 0) {
                                event.textLocale = languages[0].iso6391Name;
                            }
                        }
                    }
                    next();
                });
            } else {
                next();
            }
        }
    };
}

