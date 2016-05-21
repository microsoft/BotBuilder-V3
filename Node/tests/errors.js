var assert = require('assert');
var builder = require('../');

describe('errors', function() {
    this.timeout(10000);

//=============================================================================
// Basic Dialogs
//=============================================================================

    it('should catch an exception from a Dialog based on a closure.', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.on('error', function (err) {
            assert(err && err.message == 'test');
            done();
        });
        bot.add('/', function (session) {
            throw "test";
        });
        bot.processMessage({ text: 'hello' });
    });

    it('should catch an exception from a Dialog based on a waterfall.', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.on('error', function (err) {
            assert(err && err.message == 'test');
            done();
        });
        bot.add('/', [
            function (session) {
                throw "test";
            }
        ]);
        bot.processMessage({ text: 'hello' });
    });

//=============================================================================
// CommandDialog
//=============================================================================

    it('should catch an exception from a CommandDialog.onBegin() handler.', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.on('error', function (err) {
            assert(err && err.message == 'test');
            done();
        });
        var dialog = new builder.CommandDialog();
        bot.add('/', dialog);
        dialog.onBegin(function (session, args, next) {
            throw "test";
        });
        bot.processMessage({ text: 'hello' });
    });

    it('should catch an exception from a CommandDialog.matches() handler based on a closure.', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.on('error', function (err) {
            assert(err && err.message == 'test');
            done();
        });
        var dialog = new builder.CommandDialog();
        bot.add('/', dialog);
        dialog.matches('hello', function (session) {
            throw "test";
        });
        bot.processMessage({ text: 'hello' });
    });

    it('should catch an exception from a CommandDialog.matches() handler based on a waterfall.', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.on('error', function (err) {
            assert(err && err.message == 'test');
            done();
        });
        var dialog = new builder.CommandDialog();
        bot.add('/', dialog);
        dialog.matches('hello', [
            function (session) {
                throw "test";
            }
        ]);
        bot.processMessage({ text: 'hello' });
    });
    
    it('should catch an exception from a CommandDialog.onDefault() handler based on a closure.', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.on('error', function (err) {
            assert(err && err.message == 'test');
            done();
        });
        var dialog = new builder.CommandDialog();
        bot.add('/', dialog);
        dialog.onDefault(function (session) {
            throw "test";
        });
        bot.processMessage({ text: 'hello' });
    });

    it('should catch an exception from a CommandDialog.onDefault() handler based on a waterfall.', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.on('error', function (err) {
            assert(err && err.message == 'test');
            done();
        });
        var dialog = new builder.CommandDialog();
        bot.add('/', dialog);
        dialog.onDefault([
            function (session) {
                throw "test";
            }
        ]);
        bot.processMessage({ text: 'hello' });
    });
    
//=============================================================================
// LuisDialog
//=============================================================================

    it('should catch an exception from a LuisDialog.onBegin() handler.', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.on('error', function (err) {
            assert(err && err.message == 'test');
            done();
        });
        var dialog = new builder.LuisDialog('https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=6d0966209c6e4f6b835ce34492f3e6d9&q=');
        bot.add('/', dialog);
        dialog.onBegin(function (session, args, next) {
            throw "test";
        });
        bot.processMessage({ text: 'set alarm' });
    });

    it('should catch an exception from a LuisDialog.on() handler based on a closure.', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.on('error', function (err) {
            assert(err && err.message == 'test');
            done();
        });
        var dialog = new builder.LuisDialog('https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=6d0966209c6e4f6b835ce34492f3e6d9&q=');
        bot.add('/', dialog);
        dialog.on('builtin.intent.alarm.set_alarm', function (session) {
            throw "test";
        });
        bot.processMessage({ text: 'set alarm' });
    });

    it('should catch an exception from a LuisDialog.on() handler based on a waterfall.', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.on('error', function (err) {
            assert(err && err.message == 'test');
            done();
        });
        var dialog = new builder.LuisDialog('https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=6d0966209c6e4f6b835ce34492f3e6d9&q=');
        bot.add('/', dialog);
        dialog.on('builtin.intent.alarm.set_alarm', [
            function (session) {
                throw "test";
            }
        ]);
        bot.processMessage({ text: 'set alarm' });
    });
    
    it('should catch an exception from a CommandDialog.onDefault() handler based on a closure.', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.on('error', function (err) {
            assert(err && err.message == 'test');
            done();
        });
        var dialog = new builder.LuisDialog('https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=6d0966209c6e4f6b835ce34492f3e6d9&q=');
        bot.add('/', dialog);
        dialog.onDefault(function (session) {
            throw "test";
        });
        bot.processMessage({ text: 'set alarm' });
    });

    it('should catch an exception from a LuisDialog.onDefault() handler based on a waterfall.', function (done) {
        var bot = new builder.TextBot({ minSendDelay: 0 });
        bot.on('error', function (err) {
            assert(err && err.message == 'test');
            done();
        });
        var dialog = new builder.LuisDialog('https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=6d0966209c6e4f6b835ce34492f3e6d9&q=');
        bot.add('/', dialog);
        dialog.onDefault([
            function (session) {
                throw "test";
            }
        ]);
        bot.processMessage({ text: 'set alarm' });
    });
});