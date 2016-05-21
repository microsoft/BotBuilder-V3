/*-----------------------------------------------------------------------------
This Bot uses the Bot Connector Service but is designed to showcase whats 
possible on Kik using the framework. The demo shows how to create a menu using
buttons and how send things like Kiks Link, Picture, and Video messages. It also
shows all of the prompts supported by Bit Builder and how to recieve uploaded
photos & videos. 

# RUN THE BOT:

    You can run the bot locally using the Bot Framework Emulator but for the best
    experience you should register a new bot on Kik and bind it to the demo bot.
    You can run the bot locally using ngrok found at https://ngrok.com/.

    * Install and run ngrok in a console window using "ngrok http 3978".
    * Create a bot on https://dev.botframework.com and follow the steps to setup
      a Kik channel. The Kik channel config page will walk you through creating
      a bot on Kik.
    * For the endpoint you setup on dev.botframework.com, copy the https link 
      ngrok setup and set "<ngrok link>/api/messages" as your bots endpoint.
    * In a seperate console window run "node app.js" from the example directory
      and you should be ready to add your bot as a contact and say "hello" to 
      start the demo.
    
-----------------------------------------------------------------------------*/

var restify = require('restify');
var builder = require('../../');

// Create bot and setup server
var bot = new builder.BotConnectorBot({ 
    appId: process.env.BOTFRAMEWORK_APPID, 
    appSecret: process.env.BOTFRAMEWORK_APPSECRET
});
var server = restify.createServer();
server.post('/api/messages', bot.verifyBotFramework(), bot.listen());
server.listen(process.env.port || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});

// Add dialogs to your bot
bot.add('/', [
    function (session) {
        // Send a greeting and start the menu.
        var msg = new builder.Message()
            .addAttachment({
                thumbnailUrl: "http://propakistani.pk/wp-content/uploads/2016/03/chrome_2016-03-30_18-02-211-700x316.png",
                title: "Microsoft Bot Framework",
                text: "Your bots — wherever your users are talking.",
                titleLink: "https://dev.botframework.com/",
            });
        session.send(msg);
        session.send("Hi... I'm the Microsoft Bot Framework demo bot for Kik. I can show you everything you can use our Bot Builder SDK to do on Kik.");
        session.beginDialog('/menu');
    },
    function (session, results) {
        // Always say goodbye
        session.send("Ok... See you later!");
    }
]);

bot.add('/menu', [
    function (session) {
        // Ask the user to pick from the menu.
        builder.Prompts.choice(session, "What demo would you like to run? On Kik you can press a button or type the name of the button.", "prompts|links|picture|video|(quit)");
    },
    function (session, results) {
        if (results.response && results.response.entity != '(quit)') {
            switch (results.response.entity) {
                case 'prompts':
                    session.beginDialog('/prompts');
                    break;
                case 'links':
                    session.beginDialog('/links');
                    break;
                case 'picture':
                    session.beginDialog('/picture');
                    break;
                case 'video':
                    session.beginDialog('/video');
                    break;
            }
        } else {
            // Exit the menu
            session.endDialog();
        }
    },
    function (session, results) {
        // The menu runs a loop until the user chooses to (quit).
        session.replaceDialog('/menu');
    }
])

bot.add('/prompts', [
    function (session) {
        session.send("Our Bot Builder SDK has a rich set of built-in prompts that simplify asking the user a series of questions. This demo will walk you through using each prompt. Just follow the prompts and you can quit at any time by saying 'cancel'.");
        builder.Prompts.text(session, "Prompts.text()\n\nEnter some text and I'll say it back.");
    },
    function (session, results) {
        if (results && results.response) {
            session.send("You entered '%s'", results.response);
            builder.Prompts.number(session, "Prompts.number()\n\nNow enter a number.");
        } else {
            session.endDialog("You canceled.");
        }
    },
    function (session, results) {
        if (results && results.response) {
            session.send("You entered '%s'", results.response);
            session.send("Bot Builder includes a rich choice() prompt that lets you offer a user a list choices to pick from. On Kik these choices by default surface as buttons but you can control how they're presented.");
            builder.Prompts.choice(session, "Prompts.choice()\n\nChoose a list style (the default is auto.)", "auto|inline|list|button|none");
        } else {
            session.endDialog("You canceled.");
        }
    },
    function (session, results) {
        if (results && results.response) {
            var style = builder.ListStyle[results.response.entity];
            builder.Prompts.choice(session, "Prompts.choice()\n\nNow pick an option.", "option A|option B|option C", { listStyle: style });
        } else {
            session.endDialog("You canceled.");
        }
    },
    function (session, results) {
        if (results && results.response) {
            session.send("You chose '%s'", results.response.entity);
            builder.Prompts.confirm(session, "Prompts.confirm()\n\nSimple yes/no questions are possible. Answer yes or no now.");
        } else {
            session.endDialog("You canceled.");
        }
    },
    function (session, results) {
        if (results && results.response) {
            session.send("You chose '%s'", results.response ? 'yes' : 'no');
            builder.Prompts.time(session, "Prompts.time()\n\nThe framework can recognize a range of times expressed as natural language. Enter a time like 'Monday at 7am' and I'll show you the JSON we return.");
        } else {
            session.endDialog("You canceled.");
        }
    },
    function (session, results) {
        if (results && results.response) {
            session.send("Recognized Entity: %s", JSON.stringify(results.response));
            builder.Prompts.attachment(session, "Prompts.attachment()\n\nYour bot can wait on the user to upload an image or video. Send me an image and I'll send it back to you.");
        } else {
            session.endDialog("You canceled.");
        }
    },
    function (session, results) {
        if (results && results.response) {
            var msg = new builder.Message()
                .setNText(session, "I got %d attachment.", "I got %d attachments.", results.response.length);
            results.response.forEach(function (attachment) {
                msg.addAttachment(attachment);    
            });
            session.endDialog(msg);
        } else {
            session.endDialog("You canceled.");
        }
    }
]);

bot.add('/links', [
    function (session) {
        session.send("Here's how to send a link message...");
        var msg = new builder.Message()
            .addAttachment({
                thumbnailUrl: "http://www.theoldrobots.com/images62/Bender-18.JPG",
                title: "Bender",
                titleLink: "https://en.wikipedia.org/wiki/Bender_(Futurama)",
                text: "Bender Bending Rodríguez, commonly known as Bender, is a main character in the animated television series Futurama."
            });
        session.endDialog(msg);
    }
]);

bot.add('/picture', [
    function (session) {
        session.send("Here's how to send a picture message...");
        var msg = new builder.Message()
            .addAttachment({
                contentUrl: "http://www.theoldrobots.com/images62/Bender-18.JPG",
                contentType: "image/jpeg"
            });
        session.endDialog(msg);
    }
]);

bot.add('/video', [
    function (session) {
        builder.Prompts.attachment(session, "Send me a video and I'll send it back to you.");
    },
    function (session, results) {
        if (results.response) {
            var msg = new builder.Message()
                .setNText(session, "I got %d attachment.", "I got %d attachments.", results.response.length);
            results.response.forEach(function (attachment) {
                msg.addAttachment(attachment);    
            });
            session.endDialog(msg);
        } else {
            session.endDialog("I didn't get any video :(");
        }
    }
]);
