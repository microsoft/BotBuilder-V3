---
layout: 'redirect'
permalink: /en-us/node/builder/chat/localization/
redir_to: 'https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-localization'
sitemap: false
---

* TOC
{:toc}

## Overview
Bot Builder includes a rich localization system for building bots that can communicate with the user in multiple languages.  All of your bots prompts can be localized using JSON files stored in your bots directory structure and if you’re using a system like [LUIS](https://luis.ai) to perform natural language processing you can configure your [LuisRecognizer](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.luisrecognizer) with a separate model for each language your bot supports and the SDK will automatically select the model matching the users preferred locale.

## Determining Locale
The first step to localizing your bot for the user is adding the ability to identify the users preferred language.  The SDK provides a [session.preferredLocale()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#preferredlocale) method to both save and retrieve this preference on a per user basis.  Below is short example dialog to prompt the user for their preferred language and then persist their choice:

{% highlight JavaScript %}
bot.dialog('/localePicker', [
    function (session) {
        // Prompt the user to select their preferred locale
        builder.Prompts.choice(session, "What's your preferred language?", 'English|Español|Italiano');
    },
    function (session, results) {
        // Update preferred locale
        var locale;
        switch (results.response.entity) {
            case 'English':
                locale = 'en';
            case 'Español':
                locale = 'es';
            case 'Italiano':
                locale = 'it';
                break;
        }
        session.preferredLocale(locale, function (err) {
            if (!err) {
                // Locale files loaded
                session.endDialog("Your preferred language is now %s.", results.response.entity);
            } else {
                // Problem loading the selected locale
                session.error(err);
            }
        });
    }
]);
{% endhighlight %}

Another option is to install a piece of middleware that uses a service like Microsofts [Text Analytics API](https://www.microsoft.com/cognitive-services/en-us/text-analytics-api) to automatically detect the users language based upon the text of the message they sent:

{% highlight JavaScript %}
var request = require('request');

bot.use({
    receive: function (event, next) {
        if (event.text && !event.textLocale) {
            var options = {
                method: 'POST',
                url: 'https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/languages?numberOfLanguagesToDetect=1',
                body: { documents: [{ id: 'message', text: event.text }]},
                json: true,
                headers: {
                    'Ocp-Apim-Subscription-Key': '<YOUR API KEY>'
                }
            };
            request(options, function (error, response, body) {
                if (!error && body) {
                    if (body.documents && body.documents.length > 0) {
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
});
{% endhighlight %}

Calling `session.preferredLocale()` will automatically return the detected language if a user selected locale hasn’t been assigned.  The exact search order for `preferredLocale()` is:

* Locale saved by calling `session.preferredLocale()`. This value is stored in `session.userData['BotBuilder.Data.PreferredLocale']`.
* Detected locale assigned to `session.message.textLocale`.
* Bots configured [default locale](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.iuniversalbotsettings#localizersettings).
* English ('en').

You can configure the bots default locale during construction:

{% highlight JavaScript %}
var bot = new builder.UniversalBot(connector, {
    localizerSettings: { 
        defaultLocale: "es" 
    }
});
{% endhighlight %}

## Localizing Prompts
The default localization system for Bot Builder is file based and lets a bot support multiple languages using JSON files stored on disk.  By default, the localization system will search for the bots prompts in the `./locale/<IETF TAG>/index.json` file where `<IETF TAG>` is a valid [IETF language tag](https://en.wikipedia.org/wiki/IETF_language_tag) representing the preferred locale to use the prompts for.  Below is a screenshot of the directory structure for a bot that supports three languages, English, Italian, and Spanish:   

![Locale Directory Structure](/en-us/images/builder/locale-dir.png)

The structure of the file is straight forward. It’s a simple JSON map of message ID’s to localized text strings.  If the value is an `array` instead of a `string` a prompt will be chosen at random anytime that value is retrieved using [session.localizer.gettext()](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ilocalizer#gettext). Returning the localized version of a message generally happens automatically by simply passing the message ID in a call to [session.send()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#send) instead of language specific text:

{% highlight JavaScript %}
bot.dialog("/", [
    function (session) {
        session.send("greeting");
        session.send("instructions");
        session.beginDialog('/localePicker');
    },
    function (session) {
        builder.Prompts.text(session, "text_prompt");
    },
{% endhighlight %}

Internally, the SDK will call `session.preferredLocale()` to get the users preferred locale and will then use that in a call to `session.localizer.gettext()` to map the message ID to its localized text string.  There are times where you may need to manually call the localizer. For instance, the enum values passed to [Prompts.choice()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.prompts#choice) are never automatically localized so you may need to manually retrieve a localized list prior to calling the prompt:

{% highlight JavaScript %}
    var options = session.localizer.gettext(session.preferredLocale(), "choice_options");
    builder.Prompts.choice(session, "choice_prompt", options);
{% endhighlight %}

The default localizer will search for a message ID across multiple files and if it can’t find an ID (or if no localization files were provided) it will simply return the text of ID, making the use of localization files transparent and optional.  Files are searched in teh following order:

* First the `index.json` file under the locale returned by `session.preferredLocale()` will be searched.
* Next, if the locale included and optional subtag like `en-US` then the root tag of `en` will be searched.
* Finally, the bots configured default locale will be searched.

## Namespaced Prompts
The default localizer supports the namespacing of prompts to avoid collisions between message ID’s.  Name spaced prompts can also be overridden by the bot to essentially let a bot customize or re-skin the prompts from another namespace.  Today, you can leverage this capability to customize the SDK’s built-in messages, letting you either add support for additional languages or to simply re-word the SDK’s current messages.  For instance, you can change the SDK’s default error message by simply adding a file called `BotBuilder.json` to your bots locale directory and then adding an entry for the `default_error` message ID:
 
![Locale Namespacing](/en-us/images/builder/locale-namespacing.png)
