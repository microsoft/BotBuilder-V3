var assert = require('assert');
var builder = require('../');

describe('Dialogs', function() {
    this.timeout(5000);
    it('should redirect to another dialog with arguments', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 }); 
        bot.add('/', [
            function (session) {
                session.beginDialog('/child', { foo: 'bar' }) 
            },
            function (session, results) {
                assert(results.response.bar === 'foo');
                session.send('done');
            }
        ]);
        bot.add('/child', function (session, args) {
            assert(args.foo === 'bar');
            session.endDialog({ response: { bar: 'foo' }});
        });
        bot.on('reply', function (message) {
            assert(message.text == 'done');
            done();
        });
        bot.processMessage({ text: 'start' });
    });

    it('should process a waterfall of all built-in prompt types', function (done) {
        var step = 0;
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.add('/', [
            function (session) {
                assert(session.message.text == 'start');
                builder.Prompts.text(session, 'enter text');
            },
            function (session, results) {
                assert(results.response === 'some text');
                builder.Prompts.number(session, 'enter a number');
            },
            function (session, results) {
                assert(results.response === 42);
                builder.Prompts.choice(session, 'pick a color', 'red|green|blue');
            },
            function (session, results) {
                assert(results.response && results.response.entity === 'green');
                builder.Prompts.confirm(session, 'Is green your choice?');
            },
            function (session, results) {
                assert(results.response && results.response === true);
                builder.Prompts.time(session, 'enter a time');
            },
            function (session, results) {
                assert(results.response);
                var date = builder.EntityRecognizer.resolveTime([results.response]);
                assert(date);
                session.send('done');
            }
        ]);
        bot.on('reply', function (message) {
            switch (++step) {
                case 1:
                    assert(message.text == 'enter text');
                    bot.processMessage({ text: 'some text' });
                    break;
                case 2:
                    bot.processMessage({ text: '42' });
                    break;
                case 3:
                    bot.processMessage({ text: 'green' });
                    break;
                case 4:
                    bot.processMessage({ text: 'yes' });
                    break;
                case 5:
                    bot.processMessage({ text: 'in 5 minutes' });
                    break;
                case 6:
                    assert(message.text == 'done');
                    done();
                    break;
            }
        });
        bot.processMessage({ text: 'start' });
    });
});