/*-----------------------------------------------------------------------------
A simple "Hello World" bot that can be run from a console window.

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then type 
    "hello" to wake the bot up.

-----------------------------------------------------------------------------*/

var builder = require('../../');

var bot = new builder.TextBot();
bot.add('/', function (session) {
   session.send('Hello World'); 
});

bot.listenStdin();
