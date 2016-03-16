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
app.use(express.bodyParser());
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

// Create alarm scheduler
import alarms = require('./MemoryBasedAlarmScheduler');
var scheduler = new alarms.MemoryBasedAlarmScheduler();

// Create Dialogs
import builder = require('botbuilder');
import main = require('./dialogs/main');
main.create(scheduler);

// Configure bot framework and listen for incoming messages. New conversations will
// be started using the alarms dialog.
builder.ConnectorSession.configure({
    appId: 'foo',
    appSecret: 'bar'
});
builder.ConnectorSession.listen(app, '/api/messages', main.getId());


http.createServer(app).listen(app.get('port'), function () {
    console.log('Express server listening on port ' + app.get('port'));
});
