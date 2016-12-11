/*-----------------------------------------------------------------------------
This Bot demonstrates how to completely customize a libraries message routing 
logic. Most bot developers shouldn't have to do this but it does give you 
ultimate control over how things are routed within your bot.

In this particular example we're adding a custom 'LanguageFilter' route type 
which will detect the user saying bad words and then capture those calls away
from the rest of the bots dialog system.

This isn't really the best use of custom routing as you could implement the same 
feature using middleware and it would be significantly less code.  A better use
for this feature would be a conversation control library that needs some custom
routing logic. Routing is customizable on a per/library basis and child 
libraries aren't allowed to directly install middleware so this might be a 
good option. 

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');
var async = require('async');

// Setup bot and default message handler
var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector, function (session) {
    session.send("You said: '%s'.", session.message.text);
});

// Override Library.findRoutes() function with custom implementation. The default logic has
// been extended here to add a custom 'LanguageFilter' route type that looks for bad words.
var stopWords = ['dang', 'hell', 'shoot'];
bot.onFindRoutes(function (session, callback) {
    var results = builder.Library.addRouteResult({ score: 0.0, libraryName: bot.name });
    bot.recognize(session, (err, topIntent) => {
        if (!err) {
            async.parallel([
                // >>>> BEGIN CUSTOM ROUTE
                (cb) => {
                    // Check users utterance for bad words
                    var utterance = session.message.text.toLowerCase();
                    for (var i = 0; i < stopWords.length; i++) {
                        if (utterance.indexOf(stopWords[i]) >= 0) {
                            // Route triggered
                            results = builder.Library.addRouteResult({
                                score: 1.0,
                                libraryName: bot.name,
                                routeType: 'LanguageFilter',
                                routeData: {
                                    badWord: stopWords[i]
                                }
                            }, results);
                            break;
                        }
                    }
                    cb(null);
                },
                // <<<< END CUSTOM ROUTE
                (cb) => {
                    // Check the active dialogs score
                    bot.findActiveDialogRoutes(session, topIntent, (err, routes) => {
                        if (!err && routes) {
                            routes.forEach((r) => results = builder.Library.addRouteResult(r, results));
                        }
                        cb(err);
                    });
                },
                (cb) => {
                    // Search for triggered stack actions.
                    bot.findStackActionRoutes(session, topIntent, (err, routes) => {
                        if (!err && routes) {
                            routes.forEach((r) => results = builder.Library.addRouteResult(r, results));
                        }
                        cb(err);
                    });
                },
                (cb) => {
                    // Search for global actions.
                    bot.findGlobalActionRoutes(session, topIntent, (err, routes) => {
                        if (!err && routes) {
                            routes.forEach((r) => results = builder.Library.addRouteResult(r, results));
                        }
                        cb(err);
                    });
                }
            ], (err) => {
                if (!err) {
                    callback(null, results);
                } else {
                    callback(err, null);
                }
            });
        } else {
            callback(err, null);
        }
    });
});

// Override Library.selectRoute() with custom logic. The default behaviour has been extended
// to invoke the new 'LanguageFilter' route type when it wins.
bot.onSelectRoute(function (session, route) {
    switch (route.routeType || '') {
        // >>>> BEGIN CUSTOM ROUTE
        case 'LanguageFilter':
            session.send("You really shouldn't say words like '%s'...", route.routeData.badWord);
            break;
        // <<<< END CUSTOME ROUTE
        case builder.Library.RouteTypes.ActiveDialog:
            bot.selectActiveDialogRoute(session, route);
            break;
        case builder.Library.RouteTypes.StackAction:
            bot.selectStackActionRoute(session, route);
            break;
        case builder.Library.RouteTypes.GlobalAction:
            bot.selectGlobalActionRoute(session, route);
            break;
        default:
            throw new Error('Invalid route type passed to Library.selectRoute().');
    }
});
