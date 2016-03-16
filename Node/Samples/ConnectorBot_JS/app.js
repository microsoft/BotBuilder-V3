var restify = require('restify');

// Setup server
// - Technically all we need is a bodyParser(). If you want to lock your bot down
//   to only be callable from the Bot Framework Service you'll need to add an
//   authorization parser and then verify the Basic Auth credentials passed in by
//   the framework.
//
//      server.use(restify.authorizationParser());
//
var server = restify.createServer();
server.use(restify.bodyParser());

// Add a rout to recieve messages from the bot framework.
// - This bot simply echo's back anything said to it. It also prefixes the response
//   with an incrementing counter stored in the botPerUserInConversationData field.
server.post('/api/messages', function (req, res) {
    // Increment count
    var data = req.body.botPerUserInConversationData || { count: 0 };
    data.count++;
    
    // Send reply
    res.send({
        text: data.count + ': I heard "' + req.body.text + '"',
        botPerUserInConversationData: data
    });
});

// Start server
server.listen(8080, function () {
    console.log('%s listening at %s', server.name, server.url);
});