import express = require('express');
import routes = require('./routes/index');
import user = require('./routes/user');
import http = require('http');
import path = require('path');

var app = express();

// all environments
app.set('port', process.env.PORT || 3000);
app.set('views', path.join(__dirname, 'views'));
app.set('view engine', 'jade');
app.use(express.favicon());
app.use(express.logger('dev'));
app.use(express.json());
app.use(express.urlencoded());
app.use(express.methodOverride());
app.use(express.bodyParser());      // <-- add body parser
app.use(app.router);

import stylus = require('stylus');
app.use(stylus.middleware(path.join(__dirname, 'public')));
app.use(express.static(path.join(__dirname, 'public')));

// development only
if ('development' == app.get('env')) {
    app.use(express.errorHandler());
}

app.get('/', routes.index);
app.get('/users', user.list);

// Import BotBuilder
import builder = require('botbuilder');

// Define dialog
builder.CommandDialog.create('echo')
    .matches('reset', onResetStart, onResetContinue)
    .onDefault((session) => {
        var count = session.data.getDialogState('count', 0);
        session.data.setDialogState('count', ++count);
        session.send('%d: I heard "%s"', count, session.getMessage().text);
    });

// Configure bot framework and listen for incoming messages 
builder.ConnectorSession.configure({
    appId: 'foo',
    appSecret: 'bar'
});
builder.ConnectorSession.listen(app, '/api/messages', 'echo');

http.createServer(app).listen(app.get('port'), function () {
    console.log('Express server listening on port ' + app.get('port'));
});



/*
    .matches('reset', onResetStart, onResetContinue)
*/

function onResetStart(session: builder.ISession) {
    // Ask user to confirm their intentions
    builder.Prompts.confirm(session, 'Are you sure?');
}

function onResetContinue(session: builder.ISession, result: builder.IPromptConfirmResult) {
    // Did they say yes or no?
    if (result.completed && result.response) {
        session.data.setDialogState('count', 0);
        session.send('Count reset.');
    } else {
        session.send('Ok');
    }
}
