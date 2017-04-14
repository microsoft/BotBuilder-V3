/*-----------------------------------------------------------------------------
Roller is a dice rolling bot that's been optimized for speech.
-----------------------------------------------------------------------------*/

var restify = require('restify');
var builder = require('../../core/');
var ssml = require('./ssml');

// Setup Restify Server
var server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3978, function () {
   console.log('%s listening to %s', server.name, server.url); 
});
  
// Create chat connector for communicating with the Bot Framework Service
var connector = new builder.ChatConnector({
    appId: process.env.MICROSOFT_APP_ID,
    appPassword: process.env.MICROSOFT_APP_PASSWORD
});

// Listen for messages from users 
server.post('/api/messages', connector.listen());

// Create your bot with a function to receive messages from the user
var bot = new builder.UniversalBot(connector, function (session) {
    session.say("Hello... I'm a decision making bot.'.", 
        ssml.speak("Hello. I can help you answer all of lifes tough questions."), { inputHint: builder.InputHint.ignoringInput });
    session.replaceDialog('rootMenu');
});

// Add root menu dialog
bot.dialog('rootMenu', [
    function (session) {
        // List of menu choices plus synonyms.
        var choices = [
            { 
                value: 'flipCoinDialog',
                action: { title: "Flip A Coin" },
                synonyms: 'toss coin|flip quarter|toss quarter'
            },
            {
                value: 'rollDiceDialog',
                action: { title: "Roll Dice" },
                synonyms: 'roll die|shoot dice|shoot die'
            },
            {
                value: 'magicBallDialog',
                action: { title: "Magic 8-Ball" },
                synonyms: 'shake ball'
            },
            {
                value: 'quit',
                action: { title: "Quit" },
                synonyms: 'exit|stop|end'
            }
        ];
        builder.Prompts.choice(session, "Decision Options", choices, {
            listStyle: builder.ListStyle.button,
            speak: ssml.speak("How would you like me to decide?")
        });
    },
    function (session, results) {
        switch (results.response.entity) {
            case 'quit':
                session.say("Thank You", ssml.speak("It might take a while but eventually you will find the %s in %s.", [
                    ssml.emphasis("good"),
                    ssml.emphasis("goodbye")
                ]));
                session.endConversation();
                break;
            default:
                session.beginDialog(results.response.entity);
                break;
        }
    },
    function (session) {
        // Reload menu
        session.replaceDialog('rootMenu');
    }
]).reloadAction('showMenu', null, { matches: /(menu|back)/i });

// Flip a coin
bot.dialog('flipCoinDialog', [
    function (session, args) {
        builder.Prompts.choice(session, "Choose heads or tails.", "heads|tails", { 
            listStyle: builder.ListStyle.none,
            speak: ssml.speak("heads or tails?") 
        })
    },
    function (session, results) {
        var flip = Math.random() > 0.5 ? 'heads' : 'tails';
        if (flip == results.response.entity) {
            session.say("It's " + flip + ". YOU WIN!", 
                ssml.speak(ssml.emphasis("You won.")));
        } else {
            session.say("Sorry... It was " + flip + ". you lost :(",
                ssml.speak("Oh no. You lost."));
        }
        session.endDialog();
    }
]);

// Roll some dice
bot.dialog('rollDiceDialog', [
    function (session, args) {
        builder.Prompts.number(session, "How many dice should I roll?");
    },
    function (session, results) {
        if (results.response > 0) {
            var text = "I rolled:";
            var speak = "I rolled. a "
            for (var i = 0; i < results.response; i++) {
                var roll = Math.floor(Math.random() * 6) + 1;
                text += ' ' + roll.toString();
                if (i == (results.response - 1)) {
                    speak += ' and a ';
                } else if (i > 0) {
                    speak += ', ';
                }
                speak += ssml.sayAs('cardinal', roll);
            }
            session.say(text, speak);
        } else {
            session.say("ok", ssml.speak("Ummm... Ok... I rolled air."));
        }
        session.endDialog();
    }
]);

// Magic 8-Ball
bot.dialog('magicBallDialog', [
    function (session, args) {
        builder.Prompts.text(session, "What is your question?", {
            speak: ssml.speak("Ask your question and I will decide.")
        });
    },
    function (session, results) {
        // Use the SDK's built-in ability to pick a response at random.
        session.endDialog(magicAnswers);
    }
]);

var magicAnswers = [
    "It is certain",
    "It is decidedly so",
    "Without a doubt",
    "Yes, definitely",
    "You may rely on it",
    "As I see it, yes",
    "Most likely",
    "Outlook good",
    "Yes",
    "Signs point to yes",
    "Reply hazy try again",
    "Ask again later",
    "Better not tell you now",
    "Cannot predict now",
    "Concentrate and ask again",
    "Don't count on it",
    "My reply is no",
    "My sources say no",
    "Outlook not so good",
    "Very doubtful"
];
