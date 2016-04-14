/*-----------------------------------------------------------------------------
This example demonstrates how to add logging/filtering of incoming messages 
using a piece of middleware. Users can turn logging on and off individually by 
sending a "/log on" or "/log off" message.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.

-----------------------------------------------------------------------------*/

var builder = require('../../');

var bot = new builder.TextBot();
bot.add('/', function (session) {
    session.send("Tell me about it...");
});

// Install logging middleware
bot.use(function (session, next) {
    if (/^\/log on/i.test(session.message.text)) {
        session.userData.isLogging = true;
        session.send('Logging is now turned on');
    } else if (/^\/log off/i.test(session.message.text)) {
        session.userData.isLogging = false;
        session.send('Logging is now turned off');
    } else {
        if (session.userData.isLogging) {
            console.log('Message Received: ', session.message.text);
        }
        next();
    }
});

bot.listenStdin();