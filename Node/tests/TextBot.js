var assert = require('assert');
var builder = require('../');

describe('TextBot', function() {
    this.timeout(5000);
    it('should reply inline with "Hello World"', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.add('/', function (session) {
            assert(session.message.text == 'hello');
            session.send('Hello World');
        });
        bot.processMessage({ text: 'hello' }, function (err, reply) {
            assert(reply && reply.text == 'Hello World');
            done();
        });
    });

    it('should reply via event with "Hello World"', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.add('/', function (session) {
            session.send('Hello World');
        });
        bot.on('reply', function (reply) {
            assert(reply && reply.text == 'Hello World');
            done();
        });
        bot.processMessage({ text: 'hello' });
    });
    
    it('should reply inline with "msg1" and then reply via event with "msg2"', function (done) {
        var inlineReply = false;
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.add('/', function (session) {
            session.send('msg1');
            session.send('msg2');
        });
        bot.on('reply', function (reply) {
            assert(reply && reply.text == 'msg2');
            assert(inlineReply);
            done();
        });
        bot.processMessage({ text: 'hello' }, function (err, reply) {
            assert(reply && reply.text == 'msg1');
            inlineReply = true;
        });
        
    });
    
    it('should reply inline with "msg1" and then quit', function (done) {
        var inlineReply = false;
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.add('/', function (session) {
            session.send('msg1');
            session.endDialog();
        });
        bot.on('quit', function () {
            assert(inlineReply);
            done();
        });
        bot.processMessage({ text: 'hello' }, function (err, reply) {
            assert(reply && reply.text == 'msg1');
            inlineReply = true;
        });
    });
});