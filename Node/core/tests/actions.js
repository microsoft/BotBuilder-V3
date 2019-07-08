var assert = require('assert');
var builder = require('../');
const sinon = require('sinon');

describe('actions', function() {
    this.timeout(5000);
    it('should launch dialog using a triggerAction()', function (done) { 
        var connector = new builder.ConsoleConnector();       
        var bot = new builder.UniversalBot(connector);
        bot.dialog('/test', function (session, args) { 
            assert(session !== null);
            assert(args !== null);
            done();
        }).triggerAction({ matches: /test/i });
        connector.processMessage('test');
    });

    it('should launch dialog using a triggerAction() with a intent', function (done) { 
        var connector = new builder.ConsoleConnector();       
        var bot = new builder.UniversalBot(connector);
        bot.recognizer(new builder.RegExpRecognizer('test', /test/i));
        bot.dialog('/test', function (session, args) { 
            done();
        }).triggerAction({ matches: 'test' });
        connector.processMessage('test');
    });

    it('should launch dialog using a triggerAction() with custom onFindAction.', function (done) { 
        var connector = new builder.ConsoleConnector();       
        var bot = new builder.UniversalBot(connector);
        bot.dialog('/test', function (session, args) { 
            done();
        }).triggerAction({ 
            onFindAction: function (context, callback) {
                assert(context !== null);
                assert(callback !== null);
                callback(null, 1.0);
            }
        });
        connector.processMessage('test');
    });

    it('should allow passing of custom data to dialog from onFindAction.', function (done) { 
        var connector = new builder.ConsoleConnector();       
        var bot = new builder.UniversalBot(connector);
        bot.dialog('/test', function (session, args) {
            assert(args && args.data);
            assert(args.data == 'test'); 
            done();
        }).triggerAction({ 
            onFindAction: function (context, callback) {
                callback(null, 1.0, { data: 'test' });
            }
        });
        connector.processMessage('test');
    });

    it('should allow interception of a triggered action using onSelectAction.', function (done) { 
        var connector = new builder.ConsoleConnector();       
        var bot = new builder.UniversalBot(connector);
        bot.dialog('/', function (session) {
            session.send('Hi');
        }).triggerAction({ 
            matches: /test/i,
            onSelectAction: function (session, args, next) {
                assert(session !== null);
                assert(args !== null);
                assert(args.action === '*:/');
                assert(next !== null);
                done();
            }
        });
        connector.processMessage('test');
    });

    it('should reload the same dialog using a reloadAction().', function (done) { 
        var menuLoaded = false;
        var connector = new builder.ConsoleConnector();       
        var bot = new builder.UniversalBot(connector);
        bot.dialog('/', function (session) {
            session.beginDialog('/menu');
        });
        bot.dialog('/menu', function (session, args) {
            builder.Prompts.text(session, "ChooseOption");
        }).reloadAction('showMenu', null, { matches: /show menu/i });
        bot.on('send', function (message) {
            switch (message.text) {
                case 'ChooseOption':
                    if (!menuLoaded) {
                        menuLoaded = true;
                        connector.processMessage("show menu");
                    } else {
                        done();
                    }
                    break;
                default:
                    assert(false);
                    break;
            }
        });
        connector.processMessage('test');
    });

    it('should reload the same dialog using a reloadAction() but pass additional args.', function (done) { 
        var connector = new builder.ConsoleConnector();       
        var bot = new builder.UniversalBot(connector);
        bot.dialog('/', function (session) {
            session.beginDialog('/menu');
        });
        bot.dialog('/menu', function (session, args) {
            if (args && args.reloaded) {
                builder.Prompts.text(session, "ReloadedChooseOption");
            } else {
                builder.Prompts.text(session, "ChooseOption");
            }
        }).reloadAction('showMenu', null, { 
            matches: /show menu/i,
            dialogArgs: { reloaded: true } 
        });
        bot.on('send', function (message) {
            switch (message.text) {
                case 'ChooseOption':
                    connector.processMessage("show menu");
                    break;
                case 'ReloadedChooseOption':
                    done();
                    break;
                default:
                    assert(false);
                    break;
            }
        });
        connector.processMessage('test');
    });

    it('should load a diffierent dialog using beginDialogAction().', function (done) { 
        var connector = new builder.ConsoleConnector();       
        var bot = new builder.UniversalBot(connector);
        bot.dialog('/', function (session) {
            builder.Prompts.text(session, "ChooseFood");
        }).beginDialogAction('foodMenu', '/menu', {
            matches: /show menu/i,
            dialogArgs: { title: 'FoodOptions' } 
        });
        bot.dialog('/menu', function (session, args) {
            var title = args && args.title ? args.title : 'NoTitle';
            session.send(title);
        });
        bot.on('send', function (message) {
            switch (message.text) {
                case 'ChooseFood':
                    connector.processMessage("show menu");
                    break;
                case 'FoodOptions':
                    done();
                    break;
                default:
                    assert(false);
                    break;
            }
        });
        connector.processMessage('test');
    });

    it('should end the current conversation using endConversationAction().', function (done) { 
        var connector = new builder.ConsoleConnector();       
        var bot = new builder.UniversalBot(connector);
        bot.dialog('/', function (session) {
            builder.Prompts.text(session, "ChooseFood");
        }).endConversationAction('quit', "goodbye", { matches: /goodbye/i });
        bot.on('send', function (message) {
            if (message.text) {
                switch (message.text) {
                    case 'ChooseFood':
                        connector.processMessage("goodbye");
                        break;
                    case 'goodbye':
                        done();
                        break;
                    default:
                        assert(false);
                        break;
                }
            }
        });
        connector.processMessage('test');
    });

    it('should allow a semanticAction property specifiying an optional programmatic action to be added to the message object', done => { 
        const connector = new builder.ConsoleConnector();       
        const bot = new builder.UniversalBot(connector);
        bot.dialog('/', [
            session => {
                builder.Prompts.text(session, 'enter text');
            }
        ]);
        bot.on('send', message => {
            if (message.text == 'enter text') {
                const semanticAction = {
                    id: 'foo',
                    state: 'continue',
                    entities: 'bar'
                };
                const msg = new builder.Message().address(message.address).semanticAction(semanticAction).text('my reply');
                assert(msg.data.semanticAction.state === 'continue');
                bot.send(msg, err => {
                    if (err) return done(err);
                    assert(err === null);
                    done();
                });
            }
        });
        connector.processMessage('start');
    });

    it('should allow a callerId to be added to the message object', done => { 
        const connector = new builder.ConsoleConnector();       
        const bot = new builder.UniversalBot(connector);
        bot.dialog('/', [
            session => {
                builder.Prompts.text(session, 'enter text');
            }
        ]);
        bot.on('send', message => {
            if (message.text == 'enter text') {
                const callerId = 'foo';
                const msg = new builder.Message().address(message.address).callerId(callerId).text('my reply');
                assert(msg.data.callerId === 'foo');
                bot.send(msg, err => {
                    if (err) return done(err);
                    assert(err === null);
                    done();
                });
            }
        });
        connector.processMessage('start');
    });

    it('should return a user Token with a channelId property', done => {
        const address = {
            user: {
                id: "123"
            }
        };
        const name = "bar";
        const magicCode = "baz";
        const tokenResponse = {
            connectionName: "abc",
            token: "foo",
            expiration: "bar",
            channelId: "123"
        }
        const connector = new builder.ChatConnector();

        // stub authenticatedRequest function and make it return the test token object
        const authenticatedRequestStub = (options, callback) => {
            callback(null, null, tokenResponse);
        }

        const stub = sinon.stub(connector, "authenticatedRequest");
        stub.callsFake(authenticatedRequestStub);

        connector.getUserToken(address, name, magicCode, (err, tokenResp) => {
            stub.restore();
            if (err) return done(err);
            assert(tokenResp.channelId === "123");
            done();
        });
        
    });

    it('should allow a tenantId property to be added to the address object when starting a conversation', done => { 
        const connector = new builder.ChatConnector();
        const address = {
            id: "123",
            user: "foo",
            bot: "bar",
            serviceUrl: "baz",
            tenantId: "foo"
        };

        // stub authenticatedRequest function and make it return the test token object
        const authenticatedRequestStub = (options, callback) => {
            callback(null, null, address);
        }

        const stub = sinon.stub(connector, "authenticatedRequest");
        stub.callsFake(authenticatedRequestStub);
        
        connector.startConversation(address, (err, resp) => {
            stub.restore();
            if (err) return done(err);
            assert(resp.tenantId === "foo");
            done();
        });
    });
});