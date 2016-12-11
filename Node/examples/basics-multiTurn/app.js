/*-----------------------------------------------------------------------------
This Bot demonstrates how to implement simple multi-turns using waterfalls. By
multi-turn we mean supporting scenarios where a user asks a question about 
something and then wants to ask a series of follow-up questions. To support this
the bot needs to track the current context or topic of the conversation. This
sample shows a simple way to use session.dialogState to do just that.

In this specific sample we're using a global LuisRecognizer to to give 
the bot a more natural language interface but there's nothing specific about 
multi-turn that requires the use of LUIS.

The basic idea is that before we can answer a question we need to know the company 
to answer the question for. This is the “context” of the question. We’re using a 
LUIS model to identify the question the user would like asked and so for every 
message we'll first run the users utterance through LUIS to identify their intent
and then we'll launch the appropriate dialog to answer the user question using a
triggerAction() bound to each dialog. When invoked we'll run through '
intent handler we have the same two basic steps which we’re representing using a 
waterfall. 

The first step of our waterfall is a function called askCompany(). This function 
determines the current company in one of 3 ways. First it looks to see if the 
company was passed to us from an LUIS as an entity. This would be the case for a 
query like “when did microsoft ipo?”. If there was no company passed from LUIS 
then we check to see if we just answered a question about a company, if so we’ll 
use that as the current context. Finally, if the company wasn’t passed in and 
there is a current context then we’ll ask the user the name of the company to use.

In any case the output of askCompany() is passed to the second step of the 
waterfall which is a function called answerQuestion(). This function checks to see 
if the company was passed in (the only way it wouldn’t be is if the user said 
‘nevermind’ when asked for the company) and then sets that company to be the 
context for future questions and returns an answer for data that was asked for. 

# INSTALL THE MODEL

    The sample is coded to use a version of the LUIS models deployed to our 
    LUIS account. This model is rate limited and intended for sample use only so
    if you would like to deploy your own copy of the model we've included it in 
    the models folder. 
    
    Import the model as an Appliction into your LUIS account (http://luis.ai) and 
    assign the models service url to an environment variable called model.
    
         set model="MODEL_URL"

# RUN THE BOT:

    Run the bot from the command line using "node app.js" and then try saying the 
    following.

    Say: who founded google?
    
    Say: where are they located?
    
    Say: how can I contact microsoft?
    
    Say: when did they ipo?

-----------------------------------------------------------------------------*/

var builder = require('../../core/');

// Load company data (Sample data sourced from http://crunchbase.com on 3/18/2016)
var companyData = require('./companyData.json');

// Setup bot and root message handler
var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector, function (session) {
    // Simply defer to help dialog for un-recognized intents
    session.beginDialog('helpDialog');
});

// Add global recognizer for LUIS model (run for every message)
var model = process.env.model || 'https://api.projectoxford.ai/luis/v1/application?id=56c73d36-e6de-441f-b2c2-6ba7ea73a1bf&subscription-key=6d0966209c6e4f6b835ce34492f3e6d9&q=';
bot.recognizer(new builder.LuisRecognizer(model));

// Answer help related questions like "what can I say?"
bot.dialog('helpDialog', function (session) {
    // Send help message and end dialog.
    session.endDialog('helpMessage');
}).triggerAction({ matches: 'Help' });

// Answer acquisition related questions like "how many companies has microsoft bought?"
bot.dialog('acquisitionsDialog', function (session, args) {
    // Any entities recognized by LUIS will be passed in via the args.
    var entities = args.intent.entities;

    // Call common answer dialog
    session.beginDialog('answerDialog', {
        company: builder.EntityRecognizer.findEntity(entities, 'CompanyName'),
        field: 'acquisitions',
        template: 'answerAcquisitions'
    });
}).triggerAction({ matches: 'Acquisitions' });

// Answer IPO date related questions like "when did microsoft go public?"
bot.dialog('ipoDateDialog', function (session, args) {
    var entities = args.intent.entities;
    session.beginDialog('answerDialog', {
        company: builder.EntityRecognizer.findEntity(entities, 'CompanyName'),
        field: 'ipoDate',
        template: 'answerIpoDate'
    });
}).triggerAction({ matches: 'IpoDate' });

// Answer headquarters related questions like "where is microsoft located?"
bot.dialog('headquartersDialog', function (session, args) {
    var entities = args.intent.entities;
    session.beginDialog('answerDialog', {
        company: builder.EntityRecognizer.findEntity(entities, 'CompanyName'),
        field: 'headquarters',
        template: 'answerHeadquarters'
    });
}).triggerAction({ matches: 'Headquarters' });

// Answer description related questions like "tell me about microsoft"
bot.dialog('descriptionDialog', function (session, args) {
    var entities = args.intent.entities;
    session.beginDialog('answerDialog', {
        company: builder.EntityRecognizer.findEntity(entities, 'CompanyName'),
        field: 'description',
        template: 'answerDescription'
    });
}).triggerAction({ matches: 'Description' });

// Answer founder related questions like "who started microsoft?"
bot.dialog('foundersDialog', function (session, args) {
    var entities = args.intent.entities;
    session.beginDialog('answerDialog', {
        company: builder.EntityRecognizer.findEntity(entities, 'CompanyName'),
        field: 'founders',
        template: 'answerFounders'
    });
}).triggerAction({ matches: 'Founders' });

// Answer website related questions like "how can I contact microsoft?"
bot.dialog('websiteDialog', function (session, args) {
    var entities = args.intent.entities;
    session.beginDialog('answerDialog', {
        company: builder.EntityRecognizer.findEntity(entities, 'CompanyName'),
        field: 'website',
        template: 'answerWebsite'
    });
}).triggerAction({ matches: 'Website' });

// Common answer dialog. It expects the following args:
// {
//      field: string;
//      template: string;   
//      company?: IEntity;
// }
bot.dialog('answerDialog', [
    function askCompany(session, args, next) {
        // Save args passed to dialogData which remembers them for just this answer.
        session.dialogData.args = args;

        // Validate company passed in
        var company, isValid;
        if (args.company) {
            company = args.company.entity.toLowerCase();
            isValid = companyData.hasOwnProperty(company);
        } else if (session.privateConversationData.company) {
            // Use valid company selected in previous turn
            company = session.privateConversationData.company;
            isValid = true;
        }
       
        // Prompt the user to pick a company if they didn't specify a valid one.
        if (!isValid) {
            // Lets see if the user just asked for a company we don't know about.
            var txt = args.company ? session.gettext('companyUnknown', { company: args.company }) : 'companyMissing';
            
            // Prompt the user to pick a company from the list. This will use the
            // keys of the companyData map for the choices.
            builder.Prompts.choice(session, txt, companyData);
        } else {
            // Great! pass the company to the next step in the waterfall which will answer the question.
            // * This will match the format of the response returned from Prompts.choice().
            next({ response: { entity: company } });
        }
    },
    function answerQuestion(session, results) {
        // Get args we saved away
        var args = session.dialogData.args;

        // Remember company for future turns with the user
        var company = session.privateConversationData.company = results.response.entity;

        // Reply to user with answer
        var answer = { company: company, value: companyData[company][args.field] };
        session.endDialog(args.template, answer);
    }
]).cancelAction('cancelAnswer', "cancelMessage", { matches: /^cancel/i });
