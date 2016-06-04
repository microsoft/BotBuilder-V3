/*-----------------------------------------------------------------------------
This Bot uses the Bot Connector Service but is designed to showcase whats 
possible on Facebook using the framework. The demo shows how to create a looping 
menu how send things like Pictures, Bubbles, Receipts, and use Carousels. It also
shows all of the prompts supported by Bot Builder and how to recieve uploaded
photos, videos, and location.

# RUN THE BOT:

    You can run the bot locally using the Bot Framework Emulator but for the best
    experience you should register a new bot on Facebook and bind it to the demo 
    bot. You can run the bot locally using ngrok found at https://ngrok.com/.

    * Install and run ngrok in a console window using "ngrok http 3978".
    * Create a bot on https://dev.botframework.com and follow the steps to setup
      a Facebook channel. The Facebook channel config page will walk you through 
      creating a Facebook page & app for your bot.
    * For the endpoint you setup on dev.botframework.com, copy the https link 
      ngrok setup and set "<ngrok link>/api/messages" as your bots endpoint.
    * In a separate console window set BOTFRAMEWORK_APPID and BOTFRAMEWORK_APPSECRET
      and run "node app.js" from the example directory
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
server.listen(process.env.port || process.env.PORT || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});

// Add dialogs to your bot
bot.add('/', [
    function (session) {
        // Send a greeting and start the menu.
        var msg = new builder.Message()
            .addAttachment({
                thumbnailUrl: "http://docs.botframework.com/images/demo_bot_image.png",
                title: "Microsoft Bot Framework",
                text: "Your bots — wherever your users are talking.",
                titleLink: "https://dev.botframework.com/",
            });
        session.send(msg);
        session.send("Hi... I'm the Microsoft Bot Framework demo bot for Facebook. I can show you everything you can use our Bot Builder SDK to do on Facebook.");
        session.beginDialog('/menu');
    },
    function (session, results) {
        // Always say goodbye
        session.send("Ok... See you later!");
    }
]);

bot.add('/menu', [
    function (session) {
        builder.Prompts.choice(session, "What demo would you like to run?", "prompts|picture|video|bubble|carousel|receipt|location|(quit)");
    },
    function (session, results) {
        if (results.response && results.response.entity != '(quit)') {
            switch (results.response.entity) {
                case 'prompts':
                    session.beginDialog('/prompts');
                    break;
                case 'picture':
                    session.beginDialog('/picture');
                    break;
                case 'video':
                    session.beginDialog('/video');
                    break;
                case 'bubble':
                    session.beginDialog('/bubble');
                    break;
                case 'carousel':
                    session.beginDialog('/carousel');
                    break;
                case 'receipt':
                    session.beginDialog('/receipt');
                    break;
                case 'location':
                    session.beginDialog('/location');
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

bot.add('/picture', [
    function (session) {
        session.send("You can easily send pictures to a user...");
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
        session.send("Facebook bots can recieve videos but Facebook doesn't currently support sending videos. You can however share links to videos using bubbles.");
        builder.Prompts.attachment(session, "Send me a video (or any type of attachment) and I'll send it back to you as a bubble.");
    },
    function (session, results) {
        if (results.response && results.response.length) {
            var first = results.response[0];
            var bubble = {};
            bubble.title = "Attachment Received";
            bubble.titleLink = first.contentUrl;
            bubble.text = "Attachment Type: " + first.contentType;
            if (first.contentType.indexOf('image') == 0) {
                bubble.thumbnailUrl = bubble.titleLink;
            } 
            var msg = new builder.Message()
                .addAttachment(bubble);
            session.endDialog(msg);
        } else {
            session.endDialog("I didn't get any attachments :(");
        }
    }
]);

bot.add('/bubble', [
    function (session) {
        session.send("You can use bubbles to send the user a rich card...");
        var msg = new builder.Message()
            .addAttachment({
                thumbnailUrl: "http://petersapparel.parseapp.com/img/item101-thumb.png",
                title: "Classic Grey T-Shirt",
                titleLink: "https://petersapparel.parseapp.com/view_item?item_id=101",
                text: "Soft white cotton t-shirt is back in style"
            });
        session.endDialog(msg);
    }
]);

bot.add('/carousel', [
    function (session) {
        session.send("You can pass a custom message to Prompts.choice() that will present the user with a carousel of cards to select from. Each card can even support multiple actions.");
        
        // Ask the user to select an item from a carousel.
        var msg = new builder.Message();
        msg.addAttachment({
            title: "Classic White T-Shirt",
            text: "Soft white cotton t-shirt is back in style",
            thumbnailUrl: "http://petersapparel.parseapp.com/img/item100-thumb.png",
            actions: [
                { title: "View Item", url: "https://petersapparel.parseapp.com/view_item?item_id=100" },
                { title: "Buy Item", message: "buy:100" },
                { title: "Bookmark Item", message: "bookmark:100" }
            ]
        });
        msg.addAttachment({
            title: "Classic Grey T-Shirt",
            text: "Soft gray cotton t-shirt is back in style",
            thumbnailUrl: "http://petersapparel.parseapp.com/img/item101-thumb.png",
            actions: [
                { title: "View Item", url: "https://petersapparel.parseapp.com/view_item?item_id=101" },
                { title: "Buy Item", message: "buy:101" },
                { title: "Bookmark Item", message: "bookmark:101" }
            ]
        });
        builder.Prompts.choice(session, msg, "buy:100|bookmark:100|buy:101|bookmark:101");
    },
    function (session, results) {
        if (results.response) {
            var action, item;
            var kvPair = results.response.entity.split(':');
            switch (kvPair[0]) {
                case 'buy':
                    action = 'purchased';
                    break;
                case 'bookmark':
                    action = 'bookmarked';
                    break;
            }
            switch (kvPair[1]) {
                case '100':
                    item = "Classic White T-Shirt";
                    break;
                case '101':
                    item = "Classic Grey T-Shirt";
                    break;
            }
            session.endDialog('You %s the "%s"', action, item);
        } else {
            session.endDialog("You canceled.");
        }
    }    
]);

bot.add('/receipt', [
    function (session) {
        session.send("You can send a receipt using ChannelData field...");
        var msg = new builder.Message();
        msg.setChannelData({
            "attachment":{
                "type":"template",
                "payload":{
                    "template_type":"receipt",
                    "recipient_name":"Stephane Crozatier",
                    "order_number":"12345678902",
                    "currency":"USD",
                    "payment_method":"Visa 2345",        
                    "order_url":"http://petersapparel.parseapp.com/order?order_id=123456",
                    "timestamp":"1428444852", 
                    "elements":[
                        {
                            "title":"Classic White T-Shirt",
                            "subtitle":"100% Soft and Luxurious Cotton",
                            "quantity":2,
                            "price":50,
                            "currency":"USD",
                            "image_url":"http://petersapparel.parseapp.com/img/whiteshirt.png"
                        },
                        {
                            "title":"Classic Gray T-Shirt",
                            "subtitle":"100% Soft and Luxurious Cotton",
                            "quantity":1,
                            "price":25,
                            "currency":"USD",
                            "image_url":"http://petersapparel.parseapp.com/img/grayshirt.png"
                        }
                    ],
                    "address":{
                        "street_1":"1 Hacker Way",
                        "street_2":"",
                        "city":"Menlo Park",
                        "postal_code":"94025",
                        "state":"CA",
                        "country":"US"
                    },
                    "summary":{
                        "subtotal":75.00,
                        "shipping_cost":4.95,
                        "total_tax":6.19,
                        "total_cost":56.14
                    },
                    "adjustments":[
                        {
                            "name":"New Customer Discount",
                            "amount":20
                        },
                        {
                            "name":"$10 Off Coupon",
                            "amount":10
                        }
                    ]
                }
            }
        });
        session.endDialog(msg);
    }
]);

bot.add('/location', [
    function (session) {
        session.send("A user can share their location with a bot using the location pin in Messenger.  Bot Builder lets you easily build a custom prompt to ask the user for their location.");
        session.beginDialog('/locationPrompt', { prompt: 'Please send me your current location.' });        
    }, 
    function (session, results) {
        if (results.response) {
            session.endDialog("Here's the location object I received: %s", JSON.stringify(results.response));
        } else {
            session.endDialog("You canceled.");
        }
    }
]);

bot.add('/locationPrompt', [
    function (session, args) {
        if (typeof args.maxRetries !== 'number') {
            args.maxRetries = 2;
        }
        session.dialogData.args = args;
        builder.Prompts.text(session, args.prompt);
    },
    function (session, results) {
        if (results.resumed == builder.ResumeReason.completed) {
            // Validate response
            if (session.message.location) {
                // Return location
                session.endDialog({ response: session.message.location });
            } else if (session.dialogData.args.maxRetries > 0) {
                // Reprompt for location
                var args = session.dialogData.args;
                args.maxRetries--;
                args.prompt = args.retryPrompt || "I didn't receive a location. Please try again.";
                session.replaceDialog('/locationPrompt', args);
            } else {
                // Out of retries
                sessions.endDialog({ resumed: builder.ResumeReason.notCompleted });
            }
        } else {
            // User canceled prompt or an error occured
            session.endDialog(results);
        }
    }
]);