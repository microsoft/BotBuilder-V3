var restify = require('restify');
var builder = require('../../');

var server = restify.createServer();

var bot = new builder.BotConnectorBot();
bot.add('/', function (session) {
   session.send('Hello World'); 
});

server.post('/v1/messages', bot.listen());

server.listen(8080, function () {
   console.log('%s listening to %s', server.name, server.url); 
});