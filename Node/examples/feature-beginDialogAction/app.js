/*-----------------------------------------------------------------------------
This Bot demonstrates how to use beginDialogAction() to create actions that are
only in scope when a particular dialog is on teh stack. The '/orderPizza' adds
actions that let the user view their cart and checkout but those actions can 
only be taken while the user is actually ordering a pizza.

This sample also shows how support multi-level cancel within a bot. When 
ordering a pizza you can cancel either an item you're adding or the entire 
order.  The user can say "cancel order" at anytime to cancel the order but 
saying just "cancel" will intelligently cancel either the current item being 
added or the order depending on where the user is in the flow.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector);

// Add default dialog
bot.dialog('/', function (session) {
    session.send("Say 'order pizza' to start a new order. ")
});

bot.dialog('/orderPizza', [
    function (session) {
        if (!session.userData.cart) {
            session.userData.cart = [];
            session.send("At anytime you can say 'cancel order', 'view cart', or 'checkout'.")
        }
        builder.Prompts.choice(session, "What would you like to add?", "Pizza|Drinks|Extras");
    },
    function (session, results) {
        session.beginDialog('/add' + results.response.entity);
    },
    function (session, results) {
        if (results.response) {
            session.userData.cart.push(results.response);
        }
        session.replaceDialog('/orderPizza');
    }
]).triggerAction({ 
        matches: /^order pizza/i,
        onSelectAction: function (session, args, next) {
            switchTasks(session, args, next, "You're already ordering a pizza.");
        }
  })
  .cancelAction('cancelOrder', "Order canceled.", { matches: /^(cancel order|cancel)/i })
  .beginDialogAction('viewCart', '/viewCart', { matches: /^view cart/i })
  .beginDialogAction('checkout', '/checkout', { matches: /^checkout/i });

bot.dialog('/addPizza', [
    function (session) {
        builder.Prompts.choice(session, "What kind of pizza?", "Hawaiian|Meat Lovers|Supreme");
    },
    function (session, results) {
        session.dialogData.pizza = results.response.entity;
        builder.Prompts.choice(session, "What size?", 'Small 8"|Medium 10"|Large 12"');
    },
    function (session, results) {
        var item = results.response.entity + ' ' + session.dialogData.pizza + ' Pizza';
        session.endDialogWithResult({ response: item });
    }
]).cancelAction('cancelItem', "Item canceled.", { matches: /^(cancel item|cancel)/i });

bot.dialog('/addDrinks', [
    function (session) {
        builder.Prompts.choice(session, "What kind of 2 Liter drink?", "Coke|Sprite|Pepsi");
    },
    function (session, results) {
        session.endDialogWithResult({ response: '2 Liter ' + results.response.entity });
    }
]).cancelAction('cancelItem', "Item canceled.", { matches: /^(cancel item|cancel)/i });

bot.dialog('/addExtras', [
    function (session) {
        builder.Prompts.choice(session, "What kind of extra?", "Salad|Breadsticks|Wings");
    },
    function (session, results) {
        session.endDialogWithResult({ response: results.response.entity });
    }
]).cancelAction('cancelItem', "Item canceled.", { matches: /^(cancel item|cancel)/i });

bot.dialog('/viewCart', function (session) {
    var msg;
    var cart = session.userData.cart;
    if (cart.length > 0) {
        msg = "Items in your cart:";
        for (var i = 0; i < cart.length; i++) {
            msg += "\n* " + cart[i];
        }
    } else {
        msg = "Your cart is empty.";
    }
    session.endDialog(msg);
});

bot.dialog('/checkout', function (session) {
    var msg;
    var cart = session.userData.cart;
    if (cart.length > 0) {
        msg = "Your order is on its way.";
    } else {
        msg = "Your cart is empty.";
    }
    delete session.userData.cart;
    session.endConversation(msg);
});

function switchTasks(session, args, next, alreadyActiveMessage) {
    // Check to see if we're already active.
    // - We're assuming that we're being called from a triggerAction() some
    //   args.action is the fully qualified dialog ID.
    var stack = session.dialogStack();
    if (builder.Session.findDialogStackEntry(stack, args.action) >= 0) {
        session.send(alreadyActiveMessage);
    } else {
        // Clear stack and switch tasks
        session.clearDialogStack();
        next();
    }
}
