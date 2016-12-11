/*-----------------------------------------------------------------------------
This Bot uses the Bot Connector Service but is designed to showcase whats 
possible on Slack using the framework. The demo shows how to create a looping 
menu, use the built-in prompts, send Pictures, send Hero & Thumbnail Cards, 
send Receipts, and use Carousels. 

# RUN THE BOT:

    You can run the bot locally using the Bot Framework Emulator but for the best
    experience you should register a new bot on Slack and bind it to the demo 
    bot. You can then run the bot locally using ngrok found at https://ngrok.com/.

    * Install and run ngrok in a console window using "ngrok http 3978".
    * Create a bot on https://dev.botframework.com and follow the steps to setup
      a Slack channel.
    * For the endpoint you setup on dev.botframework.com, copy the https link 
      ngrok setup and set "<ngrok link>/api/messages" as your bots endpoint.
    * Next you need to configure your bots MICROSOFT_APP_ID, and
      MICROSOFT_APP_PASSWORD environment variables. If you're running VSCode you 
      can add these variables to your the bots launch.json file. If you're not 
      using VSCode you'll need to setup these variables in a console window.
      - MICROSOFT_APP_ID: This is the App ID assigned when you created your bot.
      - MICROSOFT_APP_PASSWORD: This was also assigned when you created your bot.
    * To use the bot you'll need to click the join link in the portal which will
      add it as a contact to your Slack account. 
    * To run the bot you can launch it from VSCode or run "node app.js" from a 
      console window. 

-----------------------------------------------------------------------------*/

var restify = require('restify');
var builder = require('../../core/');

//=========================================================
// Bot Setup
//=========================================================

// Setup Restify Server
var server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});
  
// Create chat bot
var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});
var bot = new builder.UniversalBot(connector);
server.post('/api/messages', connector.listen());


//=========================================================
// Activity Events
//=========================================================

bot.on('conversationUpdate', function (message) {
   // Check for group conversations
    if (message.address.conversation.isGroup) {
        // Send a hello message when bot is added
        if (message.membersAdded) {
            message.membersAdded.forEach(function (identity) {
                if (identity.id === message.address.bot.id) {
                    var reply = new builder.Message()
                            .address(message.address)
                            .text("Hello everyone!");
                    bot.send(reply);
                }
            });
        }

        // Send a goodbye message when bot is removed
        if (message.membersRemoved) {
            message.membersRemoved.forEach(function (identity) {
                if (identity.id === message.address.bot.id) {
                    var reply = new builder.Message()
                        .address(message.address)
                        .text("Goodbye");
                    bot.send(reply);
                }
            });
        }
    }
});

bot.on('contactRelationUpdate', function (message) {
    if (message.action === 'add') {
        var name = message.user ? message.user.name : null;
        var reply = new builder.Message()
                .address(message.address)
                .text("Hello %s... Thanks for adding me. Say 'hello' to see some great demos.", name || 'there');
        bot.send(reply);
    } else {
        // delete their data
    }
});

bot.on('typing', function (message) {
    // User is typing
});

bot.on('deleteUserData', function (message) {
    // User asked to delete their data
});


//=========================================================
// Bots Middleware
//=========================================================

// Anytime the major version is incremented any existing conversations will be restarted.
bot.use(builder.Middleware.dialogVersion({ version: 1.0, resetCommand: /^reset/i }));

//=========================================================
// Bots Dialogs
//=========================================================

bot.dialog('/', [
    function (session) {
        // Send a greeting and start the menu.
        var card = new builder.HeroCard(session)
            .title("Microsoft Bot Framework")
            .text("Your bots - wherever your users are talking.")
            .images([
                 builder.CardImage.create(session, "http://docs.botframework.com/images/demo_bot_image.png")
            ]);
        var msg = new builder.Message(session).attachments([card]);
        session.send(msg);
        session.send("Hi... I'm the Microsoft Bot Framework demo bot for Slack. I can show you everything you can use our Bot Builder SDK to do on Slack.");
        session.beginDialog('/menu');
    },
    function (session, results) {
        // Always say goodbye
        session.send("Ok... See you later!");
    }
]);

bot.dialog('/menu', [
    function (session) {
        builder.Prompts.choice(session, "What demo would you like to run?", "prompts|picture|cards|list|carousel|receipt|(quit)", { listStyle: builder.ListStyle.button });
    },
    function (session, results) {
        if (results.response && results.response.entity != '(quit)') {
            // Launch demo dialog
            session.beginDialog('/' + results.response.entity);
        } else {
            // Exit the menu
            session.endDialog();
        }
    },
    function (session, results) {
        // The menu runs a loop until the user chooses to (quit).
        session.replaceDialog('/menu');
    }
]);

bot.dialog('/prompts', [
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
            session.send("Bot Builder includes a rich choice() prompt that lets you offer a user a list choices to pick from. On Facebook these choices by default surface using buttons if there are 3 or less choices. If there are more than 3 choices a numbered list will be used but you can specify the exact type of list to show using the ListStyle property.");
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
        if (results && results.resumed == builder.ResumeReason.completed) {
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
            var msg = new builder.Message(session)
                .ntext("I got %d attachment.", "I got %d attachments.", results.response.length);
            results.response.forEach(function (attachment) {
                msg.addAttachment(attachment);    
            });
            session.endDialog(msg);
        } else {
            session.endDialog("You canceled.");
        }
    }
]);

bot.dialog('/picture', [
    function (session) {
        session.send("You can easily send pictures to a user...");
        var msg = new builder.Message(session)
            .attachments([{
                contentType: "image/jpeg",
                contentUrl: "http://www.theoldrobots.com/images62/Bender-18.JPG"
            }]);
        session.endDialog(msg);
    }
]);

bot.dialog('/cards', [
    function (session) {
        session.send("You can use Hero & Thumbnail cards to send the user a visually rich information...");

        var msg = new builder.Message(session)
            .textFormat(builder.TextFormat.xml)
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
        session.send(msg);

        msg = new builder.Message(session)
            .textFormat(builder.TextFormat.xml)
            .attachments([
                new builder.ThumbnailCard(session)
                    .title("Thumbnail Card")
                    .subtitle("Pikes Place Market")
                    .text("<b>Pike Place Market</b> is a public market overlooking the Elliott Bay waterfront in Seattle, Washington, United States.")
                    .images([
                        builder.CardImage.create(session, "https://upload.wikimedia.org/wikipedia/en/thumb/2/2a/PikePlaceMarket.jpg/320px-PikePlaceMarket.jpg")
                    ])
                    .tap(builder.CardAction.openUrl(session, "https://en.wikipedia.org/wiki/Pike_Place_Market"))
            ]);
        session.endDialog(msg);
    }
]);

bot.dialog('/list', [
    function (session) {
        session.send("You can send the user a list of cards...");

        var msg = new builder.Message(session)
            .textFormat(builder.TextFormat.xml)
            .attachments([
                new builder.HeroCard(session)
                    .title("Hero Card")
                    .subtitle("Space Needle")
                    .text("The <b>Space Needle</b> is an observation tower in Seattle, Washington, a landmark of the Pacific Northwest, and an icon of Seattle.")
                    .images([
                        builder.CardImage.create(session, "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7c/Seattlenighttimequeenanne.jpg/320px-Seattlenighttimequeenanne.jpg")
                    ]),
                new builder.ThumbnailCard(session)
                    .title("Thumbnail Card")
                    .subtitle("Pikes Place Market")
                    .text("<b>Pike Place Market</b> is a public market overlooking the Elliott Bay waterfront in Seattle, Washington, United States.")
                    .images([
                        builder.CardImage.create(session, "https://upload.wikimedia.org/wikipedia/en/thumb/2/2a/PikePlaceMarket.jpg/320px-PikePlaceMarket.jpg")
                    ])
            ]);
        session.endDialog(msg);
    }
]);

bot.dialog('/carousel', [
    function (session) {
        session.send("You can pass a custom message to Prompts.choice() that will present the user with a carousel of cards to select from. Each card can even support multiple actions.");
        
        // Ask the user to select an item from a carousel.
        var msg = new builder.Message(session)
            .textFormat(builder.TextFormat.xml)
            .attachmentLayout(builder.AttachmentLayout.carousel)
            .attachments([
                new builder.HeroCard(session)
                    .title("Space Needle")
                    .text("The <b>Space Needle</b> is an observation tower in Seattle, Washington, a landmark of the Pacific Northwest, and an icon of Seattle.")
                    .images([
                        builder.CardImage.create(session, "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7c/Seattlenighttimequeenanne.jpg/320px-Seattlenighttimequeenanne.jpg")
                            .tap(builder.CardAction.showImage(session, "https://upload.wikimedia.org/wikipedia/commons/thumb/7/7c/Seattlenighttimequeenanne.jpg/800px-Seattlenighttimequeenanne.jpg")),
                    ])
                    .buttons([
                        builder.CardAction.openUrl(session, "https://en.wikipedia.org/wiki/Space_Needle", "Wikipedia"),
                        builder.CardAction.imBack(session, "select:100", "Select")
                    ]),
                new builder.HeroCard(session)
                    .title("Pikes Place Market")
                    .text("<b>Pike Place Market</b> is a public market overlooking the Elliott Bay waterfront in Seattle, Washington, United States.")
                    .images([
                        builder.CardImage.create(session, "https://upload.wikimedia.org/wikipedia/en/thumb/2/2a/PikePlaceMarket.jpg/320px-PikePlaceMarket.jpg")
                            .tap(builder.CardAction.showImage(session, "https://upload.wikimedia.org/wikipedia/en/thumb/2/2a/PikePlaceMarket.jpg/800px-PikePlaceMarket.jpg")),
                    ])
                    .buttons([
                        builder.CardAction.openUrl(session, "https://en.wikipedia.org/wiki/Pike_Place_Market", "Wikipedia"),
                        builder.CardAction.imBack(session, "select:101", "Select")
                    ]),
                new builder.HeroCard(session)
                    .title("EMP Museum")
                    .text("<b>EMP Musem</b> is a leading-edge nonprofit museum, dedicated to the ideas and risk-taking that fuel contemporary popular culture.")
                    .images([
                        builder.CardImage.create(session, "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/Night_Exterior_EMP.jpg/320px-Night_Exterior_EMP.jpg")
                            .tap(builder.CardAction.showImage(session, "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/Night_Exterior_EMP.jpg/800px-Night_Exterior_EMP.jpg"))
                    ])
                    .buttons([
                        builder.CardAction.openUrl(session, "https://en.wikipedia.org/wiki/EMP_Museum", "Wikipedia"),
                        builder.CardAction.imBack(session, "select:102", "Select")
                    ])
            ]);
        builder.Prompts.choice(session, msg, "select:100|select:101|select:102");
    },
    function (session, results) {
        if (results.response) {
            var action, item;
            var kvPair = results.response.entity.split(':');
            switch (kvPair[0]) {
                case 'select':
                    action = 'selected';
                    break;
            }
            switch (kvPair[1]) {
                case '100':
                    item = "the <b>Space Needle</b>";
                    break;
                case '101':
                    item = "<b>Pikes Place Market</b>";
                    break;
                case '101':
                    item = "the <b>EMP Museum</b>";
                    break;
            }
            session.endDialog('You %s "%s"', action, item);
        } else {
            session.endDialog("You canceled.");
        }
    }    
]);

bot.dialog('/receipt', [
    function (session) {
        session.send("You can send a receipts for purchased good with both images and without...");
        
        // Send a receipt with images
        var msg = new builder.Message(session)
            .attachments([
                new builder.ReceiptCard(session)
                    .title("Recipient's Name")
                    .items([
                        builder.ReceiptItem.create(session, "$22.00", "EMP Museum").image(builder.CardImage.create(session, "https://upload.wikimedia.org/wikipedia/commons/a/a0/Night_Exterior_EMP.jpg")),
                        builder.ReceiptItem.create(session, "$22.00", "Space Needle").image(builder.CardImage.create(session, "https://upload.wikimedia.org/wikipedia/commons/7/7c/Seattlenighttimequeenanne.jpg"))
                    ])
                    .facts([
                        builder.Fact.create(session, "1234567898", "Order Number"),
                        builder.Fact.create(session, "VISA 4076", "Payment Method"),
                        builder.Fact.create(session, "WILLCALL", "Delivery Method")
                    ])
                    .tax("$4.40")
                    .total("$48.40")
            ]);
        session.send(msg);

        // Send a receipt without images
        msg = new builder.Message(session)
            .attachments([
                new builder.ReceiptCard(session)
                    .title("Recipient's Name")
                    .items([
                        builder.ReceiptItem.create(session, "$22.00", "EMP Museum"),
                        builder.ReceiptItem.create(session, "$22.00", "Space Needle")
                    ])
                    .facts([
                        builder.Fact.create(session, "1234567898", "Order Number"),
                        builder.Fact.create(session, "VISA 4076", "Payment Method"),
                        builder.Fact.create(session, "WILLCALL", "Delivery Method")
                    ])
                    .tax("$4.40")
                    .total("$48.40")
            ]);
        session.endDialog(msg);
    }
]);

bot.dialog('/signin', [ 
    function (session) { 
        // Send a signin 
        var msg = new builder.Message(session) 
            .attachments([ 
                new builder.SigninCard(session) 
                    .title("You must first signin to your account.") 
                    .button("signin", "http://example.com/") 
            ]); 
        session.endDialog(msg); 
    } 
]); 
