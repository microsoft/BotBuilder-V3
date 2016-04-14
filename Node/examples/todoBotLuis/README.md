# TodoBot
A bot for managing a users to-do list using a natural language interface. Created tasks are saved to the session.userData object so are tracked per/user. For Slack you could easily change this to the session.channelData object and create a team wide task list.

# Install LUIS Model
The sample is coded to use a version of the LUIS models deployed to our LUIS account. This model is rate limited and intended for sample use only so if you would like to deploy your own copy of the model we've included it in the models folder. 
    
Import the model as an Appliction into your LUIS account (http://luis.ai) and assign the models service url to an environment variable called model.
    
    set model="MODEL_URL"

# TextBot Usage
To run the bot from a console window execute "node textBot.js" and type “add test task”.

# BotConnectorBot Usage
To run the bot using the Bot Framework Emulator open a console window and execute:

    set appId=YourAppId
    set appSecret=YourAppSecret
    node botConnectorBot.js

Then launch the Bot Framework Emulator, connect to http://localhost:8080/v1/messages, and say "add test task".

To publish the bot to the Bot Connector Service follow the steps outlined in the article below.

    http://docs.botframework.com/builder/node/bots/BotConnectorBot/#publishing

# SkypeBot Usage
To run the bot using Skype you'll need to follow Skypes Gettign Started guide and register a new bot with the Skype Developer Portal.

    http://docs.botframework.com/builder/node/bots/SkypeBot/#usage

To test the bot using ngrok open a console window and execute:

    ngrok 8080

Then update your bots callback address in the Skype Developer Portal to match the ngrok address. 

Next open a second console window and execute:

    set APP_ID=YourAppId
    set APP_SECRET=YourAppSecret
    node skypeBot.js

Then add the bot to your contacts list using the join link in the portal and say "add test task".

# SlackBot Usage
To run the bot in Slack for follow BotKits Getting Started guide and create an integration for the bot.

    http://howdy.ai/botkit/docs/#getting-started

Next open a console window and execute:

    set token=YourToken
    node slackBot.js

Then find the bot slack and say "add test task".

