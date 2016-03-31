/*-----------------------------------------------------------------------------
This Bot demonstrates how to implement simple multi-turns using waterfalls. By
multi-turn we mean supporting scenarios where a user asks a question about 
something and then wants to ask a series of follow-up questions. To support this
the bot needs to track the current context or topic of the conversation. This
sample shows a simple way to use session.dialogState to do just that.

In this specific sample we're using a LuisDialog to to give the bot a more natural
language interface but there's nothing specific about multi-turn that requires the
use of LUIS.

The basic idea is that before we can answer a question we need to know the company 
to answer the question for. This is the “context” of the question. We’re using a 
LUIS model to identify the question the user would like asked and so for every 
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

var builder = require('../../');
var prompts = require('./prompts');

/** Use CrunchBot LUIS model for the root dialog. */
var model = process.env.model || 'https://api.projectoxford.ai/luis/v1/application?id=56c73d36-e6de-441f-b2c2-6ba7ea73a1bf&subscription-key=6d0966209c6e4f6b835ce34492f3e6d9&q=';
var dialog = new builder.LuisDialog(model);
var crunchBot = new builder.TextBot();
crunchBot.add('/', dialog);

crunchBot.listenStdin();

/** Answer help related questions like "what can I say?" */
dialog.on('Help', builder.DialogAction.send(prompts.helpMessage));

/** Answer acquisition related questions like "how many companies has microsoft bought?" */
dialog.on('Acquisitions', [askCompany, answerQuestion('acquisitions', prompts.answerAcquisitions)]);

/** Answer IPO date related questions like "when did microsoft go public?" */
dialog.on('IpoDate', [askCompany, answerQuestion('ipoDate', prompts.answerIpoDate)]);

/** Answer headquarters related questions like "where is microsoft located?" */
dialog.on('Headquarters', [askCompany, answerQuestion('headquarters', prompts.answerHeadquarters)]);

/** Answer description related questions like "tell me about microsoft" */
dialog.on('Description', [askCompany, answerQuestion('description', prompts.answerDescription)]);

/** Answer founder related questions like "who started microsoft?" */
dialog.on('Founders', [askCompany, answerQuestion('founders', prompts.answerFounders)]);

/** Answer website related questions like "how can I contact microsoft?" */
dialog.on('website', [askCompany, answerQuestion('website', prompts.answerWebsite)]);

/** 
 * This function the first step in the waterfall for intent handlers. It will use the company mentioned
 * in the users question if specified and valid. Otherwise it will use the last company a user asked 
 * about. If it the company is missing it will prompt the user to pick one. 
 */
function askCompany(session, args, next) {
    // First check to see if we either got a company from LUIS or have a an existing company
    // that we can multi-turn over.
    var company;
    var entity = builder.EntityRecognizer.findEntity(args.entities, 'CompanyName');
    if (entity) {
        // The user specified a company so lets look it up to make sure its valid.
        // * This calls the underlying function Prompts.choice() uses to match a users response
        //   to a list of choices. When you pass it an object it will use the field names as the
        //   list of choices to match against. 
        company = builder.EntityRecognizer.findBestMatch(data, entity.entity);
    } else if (session.dialogData.company) {
        // Just multi-turn over the existing company
        company = session.dialogData.company;
    }
    
    // Prompt the user to pick a ocmpany if they didn't specify a valid one.
    if (!company) {
        // Lets see if the user just asked for a company we don't know about.
        var txt = entity ? session.gettext(prompts.companyUnknown, { company: entity.entity }) : prompts.companyUnknown;
        
        // Prompt the user to pick a company from the list. They can also ask to cancel the operation.
        builder.Prompts.choice(session, txt, data);
    } else {
        // Great! pass the company to the next step in the waterfall which will answer the question.
        // * This will match the format of the response returned from Prompts.choice().
        next({ response: company })
    }
}

/**
 * This function generates a generic answer step for an intent handlers waterfall. The company to answer
 * a question about will be passed into the step and the specified field from the data will be returned to 
 * the user using the specified answer template. 
 */
function answerQuestion(field, answerTemplate) {
    return function (session, results) {
        // Check to see if we have a company. The user can cancel picking a company so IPromptResult.response
        // can be null. 
        if (results.response) {
            // Save company for multi-turn case and compose answer            
            var company = session.dialogData.company = results.response;
            var answer = { company: company.entity, value: data[company.entity][field] };
            session.send(answerTemplate, answer);
        } else {
            session.send(prompts.cancel);
        }
    };
}


/** 
 * Sample data sourced from http://crunchbase.com on 3/18/2016 
 */
var data = {
  'Microsoft': {
      acquisitions: 170,
      ipoDate: 'Mar 13, 1986',
      headquarters: 'Redmond, WA',
      description: 'Microsoft, a software corporation, develops licensed and support products and services ranging from personal use to enterprise application.',
      founders: 'Bill Gates and Paul Allen',
      website: 'http://www.microsoft.com'
  },
  'Apple': {
      acquisitions: 72,
      ipoDate: 'Dec 19, 1980',
      headquarters: 'Cupertino, CA',
      description: 'Apple is a multinational corporation that designs, manufactures, and markets consumer electronics, personal computers, and software.',
      founders: 'Kevin Harvey, Steve Wozniak, Steve Jobs, and Ron Wayne',
      website: 'http://www.apple.com'
  },
  'Google': {
      acquisitions: 39,
      ipoDate: 'Aug 19, 2004',
      headquarters: 'Mountain View, CA',
      description: 'Google is a multinational corporation that is specialized in internet-related services and products.',
      founders: 'Baris Gultekin, Michoel Ogince, Sergey Brin, and Larry Page',
      website: 'http://www.google.com'
  },
  'Amazon': {
      acquisitions: 58,
      ipoDate: 'May 15, 1997',
      headquarters: 'Seattle, WA',
      description: 'Amazon.com is an international e-commerce website for consumers, sellers, and content creators.',
      founders: 'Sachin Agarwal and Jeff Bezos',
      website: 'http://amazon.com'
  }
};
