---
layout: 'redirect'
permalink: /en-us/node/builder/chat/session/
redir_to: 'https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-use-default-message-handler'
sitemap: false
---
* TOC
{:toc}

## Overview
The [session](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session) object is passed to your [dialog handlers](/en-us/node/builder/chat/dialogs/#dialog-handlers) anytime your bot receives a message from the user. The session object is the primary mechanism you’ll use to [send messages](#sending-message) to the user and to manipulate the bots [dialog stack](#dialog-stack).

## Sending Messages
The [session.send()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#send) method can be used to easily send messages, attachments, and rich cards to the user. Your bot is free to call `send()` as many times as it likes in response to a message from the user.  When sending multiple replies, the individual replies will be automatically grouped into a batch and delivered to the user as a set in an effort to preserve the original order of the messages. 

> __Auto Batching__
> 
> When a bot sends multiple replies to a user using `session.send()`, those replies will automatically be grouped into a batch using a feature called “auto batching.”  Auto batching works by waiting a default of 250ms after every call to `send()` for an additional call to `send()`.  
> To avoid a 250ms pause after the last call to `send()` you can manually trigger delivery of the batch by calling [session.sendBatch()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#sendbatch). In practice it’s rare that you actually need to call sendBatch() as the 
> built-in [prompts](/en-us/node/builder/chat/prompts/) and [session.endConversation()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#endconversation) automatically call sendBatch() for you.
>
> The goal of batching is to try and avoid multiple messages from the bot being displayed out of order. Unfortunately, not all chat clients can guarantee this. In particular, the clients tend to want to download images before displaying a message to the user so if you send a message 
> containing an image followed immediately by a message without images you’ll sometimes see the messages flipped in the users feed. To minimize the chance of this you can try to insure that your images are coming from CDNs and try to avoid the use of overly large images. In extreme cases 
> you may even need to insert a 1-2 second delay between the message with the image and the one without.  You can make this delay feel a bit more natural to the user by calling [session.sendTyping()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#sendtyping) before starting 
> your delay.
>
> The SDKs auto batching delay is [configurable](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.iuniversalbotsettings.html#autobatchdelay) so If you’d like to disable the SDK’s auto-batching logic all together you can set the default delay to a large number and then manually 
> call sendBatch() with a callback that will be invoked after the batch has been delivered.

### Text Messages
To send a simple text message to the user you can simply call `session.send("hello there")`. The message can also contain template parameters which can be expanded using `session.send("hello there %s", name)`.  The SDK currently uses a library called [node-sprintf](https://github.com/maritz/node-sprintf) to implement the template functionality so for a full list of what’s possible consult the [sprintf documentation](http://www.diveintojavascript.com/projects/javascript-sprintf).

> __NOTE:__ One word of caution about using named parameters with sprintf. It’s a common mistake to forget the trailing format specifier (i.e. the ‘s’) when using named parameters `session.send("Hello there %(name)s", user)`. If that happens you’ll get an invalid template exception at runtime. 
> Use that as an indicator that you should verify your templates are correct.

### Attachments
Many chat services support sending image, video, and file attachments to the user. You can use `session.send()` for this as well but you’ll need to use it in conjunction with the SDK’s [Message](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.message) builder class. You can use either the [attachments()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.message#attachments) or [addAttachment()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.message#addattachment) methods to create a message containing an image:

{% highlight JavaScript %}
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
{% endhighlight %}

### Cards
Several chat services are starting to support the sending of rich cards to the user containing text, images, and even buttons. Not all chat services support cards or have the same level of richness so you’ll need to consult the individual services documentation to determine what’s currently supported. 

The BotBuilder SDK contains a set of helper classes that can be used to render cards to the user in a cross platform way.  The [HeroCard](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.herocard) and [ThumbnailCard](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.thumbnailcard) classes can be used to render a card with some text, an image, and optional buttons.  On most channels you’ll notice no difference between these two cards but on skype you can use them to control the presentation of the image to the user.

{% highlight JavaScript %}
bot.dialog('/cards', [
    function (session) {
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
        session.endDialog(msg);
    }
]);
{% endhighlight %}

Cards can typically have tap actions which let you specify what should happen when the user taps on the card (open a link, send a message, etc.)  You can use the [CardAction](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.cardaction) class to customize this behavior. There are several different types of actions you can specify but most channels only support a handful of actions.  You’ll generally want to stick with using the [openUrl()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.cardaction#openurl), [postBack()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.cardaction#postback), and [dialogAction()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.cardaction#dialogaction) actions for maximum compatibility.

> __NOTE:__ The differences between [postBack()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.cardaction#postback) and [imBack()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.cardaction#imback) actions is subtle. The intention is that imBack() will show the message beign sent to the bot in the users feed where postBack() will hide the sent message from the user. Not 
> all channels (like Skype) currently support postBack() so those channels will simply fall back to using imBack(). This generally won't change how your bot behaves but it does mean that if you're including data like an order id in your postBack() it may be visible on certain channels when you
> didn't expect it to be.

The SDK also supports more specialized card types like [ReceiptCard](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.receiptcard) and [SigninCard](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.signincard).  It’s not practical for the SDK to support every card or attachment format supported by the underlying chat service so we have a [Message.sourceEvent()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.message#sourceevent) method that you can use to send custom messages & attachments in the channel native schema:

{% highlight JavaScript %}
bot.dialog('/receipt', [
    function (session) {
        session.send("You can send a receipts for facebook using Bot Builders ReceiptCard...");
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
                        builder.Fact.create(session, "VISA 4076", "Payment Method")
                    ])
                    .tax("$4.40")
                    .total("$48.40")
            ]);
        session.send(msg);

        session.send("Or using facebooks native attachment schema...");
        msg = new builder.Message(session)
            .sourceEvent({
                facebook: {
                    attachment: {
                        type: "template",
                        payload: {
                            template_type: "receipt",
                            recipient_name: "Stephane Crozatier",
                            order_number: "12345678902",
                            currency: "USD",
                            payment_method: "Visa 2345",        
                            order_url: "http://petersapparel.parseapp.com/order?order_id=123456",
                            timestamp: "1428444852", 
                            elements: [
                                {
                                    title: "Classic White T-Shirt",
                                    subtitle: "100% Soft and Luxurious Cotton",
                                    quantity: 2,
                                    price: 50,
                                    currency: "USD",
                                    image_url: "http://petersapparel.parseapp.com/img/whiteshirt.png"
                                },
                                {
                                    title: "Classic Gray T-Shirt",
                                    subtitle: "100% Soft and Luxurious Cotton",
                                    quantity: 1,
                                    price: 25,
                                    currency: "USD",
                                    image_url: "http://petersapparel.parseapp.com/img/grayshirt.png"
                                }
                            ],
                            address: {
                                street_1: "1 Hacker Way",
                                street_2: "",
                                city: "Menlo Park",
                                postal_code: "94025",
                                state: "CA",
                                country: "US"
                            },
                            summary: {
                                subtotal: 75.00,
                                shipping_cost: 4.95,
                                total_tax: 6.19,
                                total_cost: 56.14
                            },
                            adjustments: [
                                { name: "New Customer Discount", amount: 20 },
                                { name: "$10 Off Coupon", amount: 10 }
                            ]
                        }
                    }
                }
            });
        session.endDialog(msg);
    }
]);
{% endhighlight %}
 
### Typing Indicator
Not all chat services support sending typing events, but for those that do you can use [session.sendTyping()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#sendtyping) to tell the user that the bot is actively composing a reply.  This is particular useful if you’re about to begin an asynchronous operation that could take a few seconds to complete.  The amount of time the indicator stays on varies by service (Slack is 3 seconds and Facebook is 20 seconds) so as a general rule, if you need the indicator to stay on for more than a few seconds you should add logic to call sendTyping() periodically.

{% highlight JavaScript %}
bot.dialog('/countItems', function (session, args) {
    session.sendTyping();
    lookupItemsAsync(args, function (err, items) {
        if (!err) {
            session.send("%d items found", items.length);
        } else {
            session.error(err);
        }
    });
});
{% endhighlight %}

## Dialog Stack
With the Bot Builder SDK you’ll use [dialogs](/en-us/node/builder/chat/dialogs/) to organize your bots conversations with the user. The bot tracks where it is in the conversation with a user using a stack that’s persisted to the bots [storage system](#bot-storage).  When the bot receives the first message from a user it will push the bots [default dialog](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.iuniversalbotsettings.html#defaultdialogid) onto the stack and pass that dialog the users message. The dialog can either process the incoming message and send a reply directly to the user or it can start other dialogs which will guide the user through a series of questions that collect input from the user needed to complete some task. 

The session includes several methods for managing the bots dialog stack and therefore manipulate where the bot is conversationally with the user. Once you get the hang of working with the dialog stack you can use a combination of dialogs and the sessions stack manipulation methods to achieve just about any conversational flow you can dream of.

### Starting and Ending dialogs
You can use [session.beginDialog()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#begindialog) to call a dialog (pushing it onto the stack) and then either [session.endDialog()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#enddialog) or [session.endDialogWithResults()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#enddialogwithresults) to return control back to the caller (popping off the stack.) When paired with [waterfalls](/en-us/node/builder/chat/dialogs/#waterfall) you have a simple mechanism for driving conversations forward. The example below uses two waterfalls to prompt the user for their name and then provide them with a custom greeting: 

{% highlight JavaScript %}
bot.dialog('/', [
    function (session) {
        session.beginDialog('/askName');
    },
    function (session, results) {
        session.send('Hello %s!', results.response);
    }
]);
bot.dialog('/askName', [
    function (session) {
        builder.Prompts.text(session, 'Hi! What is your name?');
    },
    function (session, results) {
        session.endDialogWithResult(results);
    }
]);
{% endhighlight %}

If you were to run through this sample using the emulator you would see a console output like below:

```
restify listening to http://[::]:3978
ChatConnector: message received.
session.beginDialog(/)
/ - waterfall() step 1 of 2
/ - session.beginDialog(/askName)
./askName - waterfall() step 1 of 2
./askName - session.beginDialog(BotBuilder:Prompts)
..Prompts.text - session.send()
..Prompts.text - session.sendBatch() sending 1 messages

ChatConnector: message received.
..Prompts.text - session.endDialogWithResult()
./askName - waterfall() step 2 of 2
./askName - session.endDialogWithResult()
/ - waterfall() step 2 of 2
/ - session.send()
/ - session.sendBatch() sending 1 messages
```

We can see that the user sent two messages to the bot.  The first message of "hi" resulted in the default “/” dialog being pushed onto the stack, entering step 1 of the first waterfall. That step called `beginDialog()` and pushed “/askName” onto the stack, entering step 1 of the second waterfall. That step then called [Prompts.text()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.prompts#text) to ask the user their name. Prompts are themselves dialogs so we see that going onto the stack (notice that you can use the dots “.” prefixing each line of console output to tell your current stack depth.)

When the user replies with their name we can see the text() prompt return their input to the second waterfall using `endDialogWithResults()`. The waterfall passes this value to the step 2 which itself calls `endDialogWithResult()` to pass it back to the first waterfall. The first waterfall passes that result to step 2 which is where we generate the actual personalized greeting that’s sent to the user.

In the example we used `session.endDialogWithResults()` to return control back to the caller and pass them a value (the users input.) You can pass your own complex values back to the caller using `session.endDialogWithResults({ response: { name: 'joe smith', age: 37 } })`.  We can also return control a send the user a message using `session.endDialog("ok… operation canceled")` or just simply return control to the caller using `session.endDialog()`.

When calling a dialog with `session.beginDialog()` you can optionally pass in a set of arguments which lets you truly call dialogs in much the same way you would a function.  We can update our previous example to prompt the user to provide their profile information and then remember it for future conversations:

{% highlight JavaScript %}
bot.dialog('/', [
    function (session) {
        session.beginDialog('/ensureProfile', session.userData.profile);
    },
    function (session, results) {
        session.userData.profile = results.profile;
        session.send('Hello %s!', session.userData.profile.name);
    }
]);
bot.dialog('/ensureProfile', [
    function (session, args, next) {
        session.dialogData.profile = args || {};
        if (!args.profile.name) {
            builder.Prompts.text(session, "Hi! What is your name?");
        } else {
            next();
        }
    },
    function (session, results, next) {
        if (results.response) {
            session.dialogData.profile.name = results.response;
        }
        if (!args.profile.email) {
            builder.Prompts.text(session, "What's your email address?");
        } else {
            next();
        }
    },
    function (session, results) {
        if (results.response) {
            session.dialogData.profile.email = results.response;
        }
        session.endDialogWithResults({ repsonse: session.dialogData.profile })
    }
]);
{% endhighlight %}

We’re using [session.userData](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#userdata) to remember the users `profile` so we’ll pass that to our “/ensureProfile” dialog in the call to `beginDialog()`.  Dialogs can use [session.dialogData](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#dialogdata) to temporarily hold values they’re working on. We’ll use that to hold the `profile` object we were passed. On the first call this will be undefined so we’ll initialize a new profile that we’ll fill in. We can use the `next()` function passed to every waterfall step to essentially skip any fields that are already filled in.

### Replacing Dialogs
The [session.replaceDialog()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#replacedialog) method lets you end the current dialog and replace it with a new one without returning to the caller. This method can be used to create a number of interesting flows. One of the most useful being the creation of loops.

The SDK includes a useful set of built-in prompts but there will be times when you’ll want to create your own custom prompts that either add some custom validation logic. Using a combination of `Prompts.text()` and `session.replaceDialog()` you can easily build new prompts.  The example below shows how to build a fairly flexible phone number prompt:

{% highlight JavaScript %}
bot.dialog('/', [
    function (session) {
        session.beginDialog('/phonePrompt');
    },
    function (session, results) {
        session.send('Got it... Setting number to %s', results.response);
    }
]);
bot.dialog('/phonePrompt', [
    function (session, args) {
        if (args && args.reprompt) {
            builder.Prompts.text(session, "Enter the number using a format of either: '(555) 123-4567' or '555-123-4567' or '5551234567'")
        } else {
            builder.Prompts.text(session, "What's your phone number?");
        }
    },
    function (session, results) {
        var matched = results.response.match(/\d+/g);
        var number = matched ? matched.join('') : '';
        if (number.length == 10 || number.length == 11) {
            session.endDialogWithResult({ response: number });
        } else {
            session.replaceDialog('/phonePrompt', { reprompt: true });
        }
    }
]);
{% endhighlight %}

The SDK’s stack based model for managing the conversation with a user is useful but not always the best approach for every bot. Some bots, like a text adventure bot, might be better served using a more state machine like model where the user is moved from one state or location to the next. This pattern can easily be achieved using `replaceDialog()` to transition between the states:

{% highlight JavaScript %}
bot.dialog('/', [
    function (session) {
        session.send("You're in a large clearing. There's a path to the north.");
        builder.Prompts.choice(session, "command?", ["north", "look"]);
    },
    function (session, results) {
        switch (results.repsonse.entity) {
            case "north":
                session.replaceDialog("/room1");
                break;
            default:
                session.replaceDialog("/");
                break;
        }
    }
]);
bot.dialog('/room1', [
    function (session) {
        session.send("There's a small house here surrounded by a white fence with a gate. There's a path to the south and west.");
        builder.Prompts.choice(session, "command?", ["open gate", "south", "west", "look"]);
    },
    function (session, results) {
        switch (results.repsonse.entity) {
            case "open gate":
                session.replaceDialog("/room2");
                break;
            case "south":
                session.replaceDialog("/");
                break;
            case "west":
                session.replaceDialog("/room3");
                break;
            default:
                session.replaceDialog("/room1");
                break;
        }
    }
]);
{% endhighlight %}

This example creates a seperate dialog for every location and moves from location-to-location using `replaceDialog()`. We can make things more data driven using something like:

{% highlight JavaScript %}
var world = {
    "room0": { 
        description: "You're in a large clearing. There's a path to the north.",
        commands: { north: "room1", look: "room0" }
    },
    "room1": {
        description: "There's a small house here surrounded by a white fence with a gate. There's a path to the south and west.",
        commands: { "open gate": "room2", south: "room0", west: "room3", look: "room1" }
    }
}

bot.dialog('/', [
    function (session, args) {
        session.beginDialog("/location", { location: "room0" });
    },
    function (session, results) {
        session.send("Congratulations! You made it out!");
    }
]);
bot.dialog('/location', [
    function (session, args) {
        var location = world[args.location];
        session.dialogData.commands = location.commands;
        builder.Prompts.choice(session, location.description, location.commands);
    },
    function (session, results) {
        var destination = session.dialogData.commands[results.response.entity];
        session.replaceDialog("/location", { location: destination });
    }
]);
{% endhighlight %}

### Canceling Dialogs
Sometimes you may want to do more extensive stack manipulation.  For that you can use the [session.cancelDialog()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#canceldialog) to end a dialog at any arbitrary point in the dialog stack and optionally start a new dialog in its place.  You can call `session.cancelDialog('/placeOrder')` with the ID of a dialog to cancel. The stack will be searched backwards and the first occurrence of that dialog will be canceled causing that dialog plus all of its children to be removed from the stack.  Control will be returned to the original caller and they can check for a [results.resumed](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.ipromptresult.html#resumed) code equal to [ResumeReason.notCompleted](/en-us/node/builder/chat-reference/enums/_botbuilder_d_.resumereason.html#notcompleted) to detect the cancelation.

You can also pass the zero-based index of the dialog to cancel.  Calling `session.cancelDialog(0, '/storeHours')` with an index of 0 and the ID of a new dialog to start lets you easily terminate any active task and start a new one in its place.

### Ending Conversations
The [session.endConversation()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#endconversation) method provides a convenient method for quickly terminating a conversation with a user.  This could be in response to the user saying “goodbye” or because you’ve simply completed the users task.  While you can technically end the conversation using `session.cancelDialog(0)` there are a few advantages to using `endConversation()` instead.

The `endConversation()` method not only clears the dialog stack, it also clears the entire [session.privateConversationData](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session#privateconversationdata) variable that gets persisted to storage. That means you can use `privateConversationData` to cache state relative to the current task and so long as you call `endConversation()` when the task is completed all of this state will be automatically cleaned up.

You can pass a message to `session.endConversation("Ok… Goodbye.")` and `session.sendBatch()` will be automatically called causing the message to be immediately sent to the user. It’s also worth noting that anytime your bot throws an exception, `endConversation()` gets called with a [configurable](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.iuniversalbotsettings.html#dialogerrormessage) error message in an effort to return the bot to a consistent state.

## Using Session in Callbacks
Inevitably you're going to want to make some asynchronous network call to retrieve data and then send those results to the user using the session object. This is completely fine but there are a few best practices you’ll want to follow.

__ok to use session__

If you’re making an async call in the context of a message you received from the user you’re generally ok calling `session.send()`:

{% highlight JavaScript %}
bot.dialog('listItems', function (session) {
    session.sendTyping();
    lookupItemsAsync(function (results) {
        // OK to call session.send() here.
        session.send(results.message);
    });
});
{% endhighlight %}

__not ok to use session__

Here’s a common mistake we see developers make. They start an asynchronous call and then immediately call something like `session.endDialog()` that changes their bots conversation state:

{% highlight JavaScript %}
bot.dialog('listItems', function (session) {
    session.sendTyping();
    lookupItemsAsync(function (results) {
        // Calling session.send() here is dangerous because you've done an action that's
        // triggered a change in your bots conversation state.
        session.send(results.message);
    });
    session.endDialog();
});
{% endhighlight %}

In general this is a pattern you should avoid. The correct way to achieve the above behavior is to move the `endDialog()` call into your callback:

{% highlight JavaScript %}
bot.dialog('listItems', function (session) {
    session.sendTyping();
    lookupItemsAsync(function (results) {
        // Calling session.send() here is dangerous because you've done an action that's
        // triggered a change in your bots conversation state.
        session.endDialog(results.message);
    });
});
{% endhighlight %}

__dangerous to use session__

The other case where you probably shouldn’t use session (or at least should be careful) is when you’re doing some long running task and you wish to communicate with the user at the beginning and end of the task:

{% highlight JavaScript %}
bot.dialog('orderPizza', function (session, args) {
    session.send("Starting your pizza order...");
    queue.startOrder({ session: session, order: args.order });
});

queue.orderReady(function (session, order) {
    session.send("Your pizza is on its way!");
});
{% endhighlight %}

This can be dangerous because the bots server could crash or the user could send other messages while the bot is doing the task and that could leave the bot in a bad conversation state. The better approach is to persist the users [session.message.address](/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.imessage.html#address) object and then send them a proactive message using [bot.send()](/en-us/node/builder/chat-reference/classes/_botbuilder_d_.universalbot.html#send) once their order is ready: 

{% highlight JavaScript %}
bot.dialog('orderPizza', function (session, args) {
    session.send("Starting your pizza order...");
    queue.startOrder({ address: session.message.address, order: args.order });
});

queue.orderReady(function (address, order) {
    var msg = new builder.Message()
        .address(address)
        .text("Your pizza is on its way!");
    bot.send(msg);
});
{% endhighlight %}
