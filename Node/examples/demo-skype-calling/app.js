/*-----------------------------------------------------------------------------
This Bot uses the Bot Connector Service but is designed to showcase whats 
possible on Skype using the framework. The demo shows how to create a looping 
menu, how send things like Pictures, Hero & Thumbnail Cards, Receipts, and use 
Carousels. It alsoshows all of the prompts supported by Bot Builder and how to 
recieve uploaded photos and videos.

# RUN THE BOT:

    You can run the bot locally using the Bot Framework Emulator but for the best
    experience you should register a new bot on Facebook and bind it to the demo 
    bot. You can run the bot locally using ngrok found at https://ngrok.com/.

    * Install and run ngrok in a console window using "ngrok http 3978".
    * Create a bot on https://dev.botframework.com and follow the steps to setup
      a Skype channel.
    * For the endpoint you setup on dev.botframework.com, copy the https link 
      ngrok setup and set "<ngrok link>/api/messages" as your bots endpoint.
    * In a separate console window set MICROSOFT_APP_ID and MICROSOFT_APP_PASSWORD
      and run "node app.js" from the example directory. You should be ready to add 
      your bot as a contact and say "hello" to start the demo.

-----------------------------------------------------------------------------*/

var restify = require('restify');
var calling = require('../../calling/');
var prompts = require('./prompts');

//=========================================================
// Bot Setup
//=========================================================
  
// Create bot and setup server
var connector = new calling.CallConnector({
    callbackUri: process.env.CALLBACK_URI,
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});
var bot = new calling.UniversalCallBot(connector);

// Setup Restify Server
var server = restify.createServer();
server.post('/api/calling', connector.verifyBotFramework(), connector.listen());
server.listen(process.env.port || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});

//=========================================================
// Bots Dialogs
//=========================================================

bot.dialog('/', [
    function (session) {
        // Send a greeting and start the menu.
        if (!session.userData.welcomed) {
            session.userData.welcomed = true;
            session.send(prompts.welcome);
            session.beginDialog('/demoMenu', { full: true });
        } else {
            session.send(prompts.welcomeBack);
            session.beginDialog('/demoMenu', { full: false });
        }
    },
    function (session, results) {
        // Always say goodbye
        session.send(prompts.goodbye);
    }
]);

bot.dialog('/demoMenu', [
    function (session, args) {
        // Build up a stack of prompts to play
        var list = [];
        list.push(calling.Prompt.text(session, prompts.demoMenu.prompt));
        if (!args || args.full) {
            list.push(calling.Prompt.text(session, prompts.demoMenu.choices));
            list.push(calling.Prompt.text(session, prompts.demoMenu.help));
        }

        // Prompt user to select a menu option
        calling.Prompts.choice(session, new calling.PlayPromptAction(session).prompts(list), [
            { name: 'digits', speechVariation: ['digits'] },
            { name: 'recordings', speechVariation: ['record', 'recordings'] },
            { name: 'chat', speechVariation: ['chat', 'chat message']  },
            { name: 'choices', speechVariation: ['choices', 'options', 'list'] },
            { name: 'help', speechVariation: ['help', 'repeat'] },
            { name: 'quit', speechVariation: ['quit', 'end call', 'hangup', 'goodbye'] }
        ]);
    },
    function (session, results) {
        if (results.response) {
            switch (results.response.entity) {
                case 'digits':
                    session.beginDialog('/digits');
                    break;
                case 'recordings':
                    session.beginDialog('/recordings');
                    break;
                case 'chat':
                    session.beginDialog('/chat');
                    break;
                case 'choices':
                    session.send(prompts.demoMenu.choices);
                    session.replaceDialog('/demoMenu', { full: false });
                    break;
                case 'help':
                    session.replaceDialog('/demoMenu', { full: true });
                    break;
                default:
                case 'quit':
                    session.endDialog();
                    break;
            }
        } else {
            // Exit the menu
            session.endDialogWithResults(results);
        }
    },
    function (session, results) {
        // The menu runs a loop until the user chooses to (quit).
        session.replaceDialog('/demoMenu', { full: false });
    }
]);
