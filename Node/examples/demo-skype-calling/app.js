/*-----------------------------------------------------------------------------
This Bot is a sample calling bot for Skype.  It's designed to showcase whats 
possible on Skype using the BotBuilder SDK. The demo shows how to create a 
looping menu, recognize speech & DTMF, record messages, play multiple prompts,
and even send a caller a chat message.

# RUN THE BOT:

    You can run the bot locally using ngrok found at https://ngrok.com/.

    * Install and run ngrok in a console window using "ngrok http 3978".
    * Create a bot on https://dev.botframework.com and follow the steps to setup
      a Skype channel. Ensure that you enable calling support for your bots skype
      channel. 
    * For the messaging endpoint in the Details for your Bot at dev.botframework.com,
      ensure you enter the https link from ngrok setup and set
      "<ngrok link>/api/messages" as your bots calling endpoint.
    * For the calling endpoint you setup on dev.botframework.com, copy the https 
      link from ngrok setup and set "<ngrok link>/api/calls" as your bots calling 
      endpoint.
    * Next you need to configure your bots CALLBACK_URL, MICROSOFT_APP_ID, and
      MICROSOFT_APP_PASSWORD environment variables. If you're running VSCode you 
      can add these variables to your the bots launch.json file. If you're not 
      using VSCode you'll need to setup these variables in a console window.
      - CALLBACK_URL: This should be the same endpoint you set as your calling
             endpoint in the developer portal.
      - MICROSOFT_APP_ID: This is the App ID assigned when you created your bot.
      - MICROSOFT_APP_PASSWORD: This was also assigned when you created your bot.
    * To use the bot you'll need to click the join link in the portal which will
      add it as a contact to your skype account. When you click on the bot in 
      your skype client you should see an option to call your bot. If you're 
      adding calling to an existing bot can take up to 24 hours for the calling 
      option to show up.
    * You can run the bot by launching it from VSCode or running "node app.js"
      from a console window.  Then call your bot from a skype client to start
      the demo. 

-----------------------------------------------------------------------------*/

var restify = require('restify');
var builder = require('../../core/');
var calling = require('../../calling/');
var prompts = require('./prompts');

//=========================================================
// Bot Setup
//=========================================================

// Setup Restify Server
var server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});

// Create chat bot
var chatConnector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});
var chatBot = new builder.UniversalBot(chatConnector);
server.post('/api/messages', chatConnector.listen());

// Create calling bot
var connector = new calling.CallConnector({
    callbackUrl: process.env.CALLBACK_URL,
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});
var bot = new calling.UniversalCallBot(connector);
server.post('/api/calls', connector.listen());

//=========================================================
// Chat Dialogs
//=========================================================

chatBot.dialog('/', function (session) {
    session.send(prompts.chatGreeting); 
});

//=========================================================
// Calling Dialogs
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
            { name: 'dtmf', speechVariation: ['dtmf'] },
            { name: 'digits', speechVariation: ['digits'] },
            { name: 'record', speechVariation: ['record', 'recordings'] },
            { name: 'chat', speechVariation: ['chat', 'chat message'] },
            { name: 'choices', speechVariation: ['choices', 'options', 'list'] },
            { name: 'help', speechVariation: ['help', 'repeat'] },
            { name: 'quit', speechVariation: ['quit', 'end call', 'hangup', 'goodbye'] }
        ]);
    },
    function (session, results) {
        if (results.response) {
            switch (results.response.entity) {
                case 'choices':
                    session.send(prompts.demoMenu.choices);
                    session.replaceDialog('/demoMenu', { full: false });
                    break;
                case 'help':
                    session.replaceDialog('/demoMenu', { full: true });
                    break;
                case 'quit':
                    session.endDialog();
                    break;
                default:
                    // Start demo
                    session.beginDialog('/' + results.response.entity);
                    break;
            }
        } else {
            // Exit the menu
            session.endDialog(prompts.canceled);
        }
    },
    function (session, results) {
        // The menu runs a loop until the user chooses to (quit).
        session.replaceDialog('/demoMenu', { full: false });
    }
]);

bot.dialog('/dtmf', [
    function (session) {
        session.send(prompts.dtmf.intro);
        calling.Prompts.choice(session, prompts.dtmf.prompt, [
            { name: 'option A', dtmfVariation: '1' },
            { name: 'option B', dtmfVariation: '2' },
            { name: 'option C', dtmfVariation: '3' }
        ]);
    },
    function (session, results) {
        if (results.response) {
            session.endDialog(prompts.dtmf.result, results.response.entity);
        } else {
            session.endDialog(prompts.canceled);
        }
    }
]);

bot.dialog('/digits', [
    function (session, args) {
        if (!args || args.full) {
            session.send(prompts.digits.intro);
        }
        calling.Prompts.digits(session, prompts.digits.prompt, 10, { stopTones: '#' });
    },
    function (session, results) {
        if (results.response) {
            // Confirm the users account is valid length otherwise reprompt.
            if (results.response.length >= 5) {
                var prompt = calling.PlayPromptAction.text(session, prompts.digits.confirm, results.response);
                calling.Prompts.confirm(session, prompt, results.response);
            } else {
                session.send(prompts.digits.inavlid);
                session.replaceDialog('/digits', { full: false });
            }
        } else {
            session.endDialog(prompts.canceled);
        }
    },
    function (session, results) {
        if (results.resumed == calling.ResumeReason.completed) {
            if (results.response) {
                session.endDialog();
            } else {
                session.replaceDialog('/digits', { full: false });
            }
        } else {
            session.endDialog(prompts.canceled);
        }
    }
]);

bot.dialog('/record', [
    function (session) {
        session.send(prompts.record.intro);
        calling.Prompts.record(session, prompts.record.prompt, { playBeep: true });
    },
    function (session, results) {
        if (results.response) {
            session.endDialog(prompts.record.result, results.response.lengthOfRecordingInSecs);
        } else {
            session.endDialog(prompts.canceled);
        }
    }
]);

// Import botbuilder core library and setup chat bot

bot.dialog('/chat', [
    function (session) {
        session.send(prompts.chat.intro);
        calling.Prompts.confirm(session, prompts.chat.confirm);        
    },
    function (session, results) {
        if (results.response) {
            // Delete conversation field from address to trigger starting a new conversation.
            var address = session.message.address;
            delete address.conversation;

            // Create a new chat message and pass it callers address
            var msg = new builder.Message()
                .address(address)
                .attachments([
                    new builder.HeroCard(session)
                        .title("Hero Card")
                        .subtitle("Space Needle")
                        .text("The <b>Space Needle</b> is an observation tower in Seattle, Washington, a landmark of the Pacific Northwest, and an icon of Seattle.")
                        .images([
                            builder.CardImage.create(session, "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7c/Seattlenighttimequeenanne.jpg/320px-Seattlenighttimequeenanne.jpg")
                        ])
                        .tap(builder.CardAction.openUrl(session, "https://en.wikipedia.org/wiki/Space_Needle"))
                ]);

            // Send message through chat bot
            chatBot.send(msg, function (err) {
                session.endDialog(err ? prompts.chat.failed : prompts.chat.sent);
            });
        } else {
            session.endDialog(prompts.canceled);
        }
    }
]);
