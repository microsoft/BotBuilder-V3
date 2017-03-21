// Create bot and default message handler
var bot = new builder.UniversalBot(connector, function (session) {
    session.send("Hi... We sell shirts. Say 'show shirts' to see our products.");
}).endConversationAction('goodbyeAction', "Ok... See you next time.", { 
    matches: /^goodbye/i 
});
