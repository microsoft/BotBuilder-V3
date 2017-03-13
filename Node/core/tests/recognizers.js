var assert = require('assert');
var builder = require('../');

describe('recognizers', function() {
    this.timeout(5000);
    it('should match a RegExpRecognizer', function (done) { 
        var connector = new builder.ConsoleConnector();       
        var bot = new builder.UniversalBot(connector, function (session, args) {
            session.send('Not Matched');
        });
        bot.recognizer(new builder.RegExpRecognizer('HelpIntent', /^help/i));
        bot.dialog('testDialog', function (session) {
            session.send('Matched');
        }).triggerAction({ matches: 'HelpIntent' });
        bot.on('send', function (message) {
            assert(message.text === 'Matched');
            done();
        });
        connector.processMessage('help');
    });

    it('should NOT match a RegExpRecognizer', function (done) { 
        var connector = new builder.ConsoleConnector();       
        var bot = new builder.UniversalBot(connector, function (session, args) {
            session.send('Not Matched');
        });
        bot.recognizer(new builder.RegExpRecognizer('HelpIntent', /^help/i));
        bot.dialog('testDialog', function (session) {
            session.send('Matched');
        }).triggerAction({ matches: 'HelpIntent' });
        bot.on('send', function (message) {
            assert(message.text === 'Not Matched');
            done();
        });
        connector.processMessage('hello');
    });

    it('should match a LocalizedRegExpRecognizer', function (done) { 
        var connector = new builder.ConsoleConnector();       
        var bot = new builder.UniversalBot(connector, function (session, args) {
            session.send('Not Matched');
        });
        bot.recognizer(new builder.LocalizedRegExpRecognizer('HelpIntent', "exp1"));
        bot.dialog('testDialog', function (session) {
            session.send('Matched');
        }).triggerAction({ matches: 'HelpIntent' });
        bot.on('send', function (message) {
            assert(message.text === 'Matched');
            done();
        });
        connector.processMessage('help');
    });

    it('should NOT match a LocalizedRegExpRecognizer', function (done) { 
        var connector = new builder.ConsoleConnector();       
        var bot = new builder.UniversalBot(connector, function (session, args) {
            session.send('Not Matched');
        });
        bot.recognizer(new builder.LocalizedRegExpRecognizer('HelpIntent', "exp1"));
        bot.dialog('testDialog', function (session) {
            session.send('Matched');
        }).triggerAction({ matches: 'HelpIntent' });
        bot.on('send', function (message) {
            assert(message.text === 'Not Matched');
            done();
        });
        connector.processMessage('hello');
    });
});