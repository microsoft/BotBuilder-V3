/*-----------------------------------------------------------------------------
This Bot demonstrates how to prompt a user to disambiguate between potentially
ambiguous actions that have been triggered by the user. The sample updates the
"feature-beginDialogAction" example to include a custom bot.onDisambiguateRoute()
handler that will look for both the 'cancelOrder' and 'cancelItem' actions to be 
triggered when this happens it will call Prompt.disambiguate() to have the user
select between the two actions.  The user is also given the option to say 
"neither" which will continue the current task.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "order pizza" to start a new order. Choose an item to add then say 
    "cancel" to trigger displaying of the disambigutation prompt.
    
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

// Setup bot and default message handler
var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector, function (session){
    session.send("Say 'order pizza' to start a new order. ");
});

// Add custom disambiguation logic
bot.onDisambiguateRoute(function (session, routes) {
    var cancelOrder = findStackAction(routes, 'cancelOrderAction');
    var cancelItem = findStackAction(routes, 'cancelItemAction');
    if (cancelOrder && cancelItem) {
        // Disambiguate between conflicting actions
        builder.Prompts.disambiguate(session, "Which would you like to cancel?", {
            "Cancel Item": cancelItem,
            "Cancel Order": cancelOrder,
            "Neither": null
        });
    } else {
        // Route message as normal
        var route = builder.Library.bestRouteResult(routes, session.dialogStack(), bot.name);
        if (route) {
            bot.library(route.libraryName).selectRoute(session, route);
        } else {
            // Just let the active dialog process the message
            session.routeToActiveDialog();
        }
    }
});

// Add dialog to manage ordering a pizza
bot.dialog('orderPizzaDialog', [
    function (session, args) {
        if (!args.continueOrder) {
            session.userData.cart = [];
            session.send("At anytime you can say 'cancel order', 'view cart', or 'checkout'.")
        }
        builder.Prompts.choice(session, "What would you like to add?", "Pizza|Drinks|Extras");
    },
    function (session, results) {
        session.beginDialog('add' + results.response.entity);
    },
    function (session, results) {
        if (results.response) {
            session.userData.cart.push(results.response);
        }
        session.replaceDialog('orderPizzaDialog', { continueOrder: true });
    }
]).triggerAction({ 
        matches: /order.*pizza/i,
        confirmPrompt: "This will cancel the current order. Are you sure?"
  })
  .cancelAction('cancelOrderAction', "Order canceled.", { 
      matches: /(cancel.*order|^cancel)/i,
      confirmPrompt: "Are you sure?"
  })
  .beginDialogAction('viewCartAction', 'viewCartDialog', { matches: /view.*cart/i })
  .beginDialogAction('checkoutAction', 'checkoutDialog', { matches: /checkout/i });

// Add pizza menu option
bot.dialog('addPizza', [
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
]).cancelAction('cancelItemAction', "Item canceled.", { matches: /(cancel.*item|^cancel)/i });

// Add drink menu option
bot.dialog('addDrinks', [
    function (session) {
        builder.Prompts.choice(session, "What kind of 2 Liter drink?", "Coke|Sprite|Pepsi");
    },
    function (session, results) {
        session.endDialogWithResult({ response: '2 Liter ' + results.response.entity });
    }
]).cancelAction('cancelItemAction', "Item canceled.", { matches: /(cancel.*item|^cancel)/i });

// Add extras menu option
bot.dialog('addExtras', [
    function (session) {
        builder.Prompts.choice(session, "What kind of extra?", "Salad|Breadsticks|Wings");
    },
    function (session, results) {
        session.endDialogWithResult({ response: results.response.entity });
    }
]).cancelAction('cancelItemAction', "Item canceled.", { matches: /(cancel.*item|^cancel)/i });

// Dialog for showing the users cart
bot.dialog('viewCartDialog', function (session) {
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

// Dialog for checking out
bot.dialog('checkoutDialog', function (session) {
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

// Helper function to find a specific stack action
function findStackAction(routes, name) {
    for (var i = 0; i < routes.length; i++) {
        var r = routes[i];
        if (r.routeType === builder.Library.RouteTypes.StackAction &&
            r.routeData.action === name) {
                return r;
        }
    }
    return null;
}