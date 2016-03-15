---
layout: page
title: Getting started with the Bot Connector
permalink: /bot-connector-sdk-getstarted/
weight: 250
parent1: Bot Connector SDK
---


* TOC
{:toc}


## Overview

// TODO: add a basic architecture diagram to include names of 'things' - showing what the parts are, and what a developer would need to write - this will then ground the rest of the document (where is the agent/bot (are agents and bots the same thing?)), how does Agent/Bot fit in with "Bot Framework Connector" and is this the same as Intercom?. 

The Microsoft Bot Framework Connector is a communication service that helps you connect your bot with many different communication channels (GroupMe, SMS, email, and others). If you write a conversational bot or agent and expose a Microsoft Bot Framework-compatible API on the internet, the Connector will forward messages from your bot to a user, and will send user messages back to your bot.  In addition the Bot Framework Connector can provide additional value by translating a message's language on behalf of the user or bot, providing per user and per conversation storage and assist with layout by translating markdown to the appropriate layout for the communication channel you've chosen.

To use the Microsoft Bot Framework Connector, you must have:

1. A Microsoft Account (Hotmail, Live, Outlook.Com) to log into the Bot Framework developer portal, which you will use to register your bot.
2. An Azure-accessible REST endpoint exposing a callback for the Connector service.
3. Developer accounts on one or more communication services (such as Skype) where your bot will communicate. 
	
In addition you may wish to have an Azure App Insights account so you can capture telemetry from your bot.  There are additionally different ways to go about building a bot; from scratch, coded directly to the Bot Connector REST API, the Bot Builder SDK's for Node.JS & .Net, and the Bot Connector SDK .Net template which is what this QuickStart guide demonstrates.
	
## Getting started in C\#
This is a step-by-step guide to writing an bot in C\# using the Bot Connector SDK .Net template.

1. Install prerequisite software

	* Visual Studio 2015 Update 1
	
		\\\products\public\PRODUCTS\Developers\Visual Studio 2015\Enterprise
		
	* Important: Please update all VS extensions to their latest versions
		Tools->Extensions and Updates->Updates

2. Download and install the Microsoft Bot Framework connector service template
	* From the MSDN Visual Studio Gallery:  [INSERT DOWNLOAD LINK HERE]()
	* Download the file from the direct download link here: [INSERT DOWNLOAD LINK HERE]()
	* Unpack the downloaded file into a folder
	* If Visual Studio is installed in the default path, Double-click install.cmd, which copies the ZIP file to your VS templates directory
		* If not, copy to "%USERPROFILE%\Documents\Visual Studio 2015\Templates\ProjectTemplates\Visual C\#\"

3. Open Visual Studio

4. Create a new C\# project using the new Bot Application template.

![Create a new C\# project using the new Bot Application template.](/images/connector-getstarted-create-project.png)

Question: Why is the project type called "Bot Connector Service"? - Why isn't this "Conversational Bot" (or something similar)?

5. The template is a fully functional Echo Bot that takes the user's text utterance as input and returns it as output.  In order to run however, 
	* The bot has to be registered with Bot Connector
	* The appid & appsecret from the BotFramework.net registration page have to be recorded in the web.config
	* The project needs to be published to the web

## Registering your bot with the Microsoft Bot Framework 

Registering your bot tells the Connector how to call your bot's web service. Note that the AppID and AppSecret are generated when your Bot is registered with the Microsoft Bot Framework, the AppID and AppSecret are used to authenticate the conversation, and allows the developer to configure their bot with the Channels they'd like to be visible on.

1. Go to the Microsoft Bot Framework portal at [https://www.botframework.com](https://www.botframework.com) and sign in with your Microsoft Account.
	
2. Register an agent
	
3. Click the "Register a bot" button and fill out the form. Many of the fields on this form can be changed later. Use a dummy endpoint for now.  Save your changes by hitting "Create" at the bottom of the form, and don't worry about the other settings for now.

![Register a bot](/images/connector-getstarted-register-agent.png)

4. Once your registration was created, Microsoft Bot Framework will have generated your AppId and AppSecrets. These are used to authenticate your bot with the Microsoft Bot Framework, and will be used in the next step.

![Microsoft Bot Framework will have generated your AppId and Subscription Keys](/images/connector-getstarted-subscription-keys.png)

## Compiling the bot and publishing the bot to Microsoft Azure

1. Now that the bot is configured, you need to update the keys in the web.config file in your Visual Studio project.  Change the following keys in the web.config file to match the ones generated when you saved your registration, and you're ready to build.  You need only the primary AppSecret, the secondary is used when you wish to regenerate your primary key without downtime.  Regenerate is shown when you view the key.

![Update the keys in the web.config file in your Visual Studio project](/images/connector-getstarted-configure-vs-keys.png)

2. The core functionality of the EchoBot is all in one function.  In this case, the code takes the message text for the user, increments a counter that it pulled from the Message, and then replies using the CreateReplyMessage function, which can be found in Controllers\MessagesController.cs 

// Where's the documentation that shows how to save/restore "per-user" settings (for example, if writing an MTBot [Machine Translation Bot] the Bot/Agent will need to track the users default/current language.

	    [BotAuthentication]
	    public class MessagesController : ApiController
	    {
	        /// <summary>
	        /// POST: api/Messages
	        /// receive a message from a user and reply to it
	        /// </summary>
	        public Message Post([FromBody]Message message)
	        {
	            if (message.Type == "Message")
	            {
	                // fetch our state associated with a user in a conversation. If we don't have state, we get default(T)
	                var counter = message.GetBotPerUserInConversationData<int>();
	
	                // create a reply message   
	                Message replyMessage = message.CreateReplyMessage($"{++counter} You said:{message.Text}");
	
	                // save our new counter by adding it to the outgoing message
	                replyMessage.SetBotPerUserInConversationData(counter);
	
	                // return our reply to the user
	                return replyMessage;
	            }
	            else
	            {
	                return HandleSystemMessage(message);
	            }
	        }
3. Use the Bot Framework Emulator to test your Bot application

The Bot Framework provides an Emulator lets you test calls to your bot as if it were being called by the Bot Framework cloud service.  To install the Bot Framework Emulator, download it from <//TODO: Insert install path for Bot Framework Emulator here>.

One installed, you're ready to test.  First, start your Bot in Visual Studio 2015 using a browser as the application host.  The image below uses Microsoft Edge.

![Start your Bot in VS2015 targeting the browser](/images/connector-getstarted-start-bot-locally.png)

When the application is built and deployed the web browser will open and display the application Default.htm file (which is part of the Bot Application project). Feel free to modify the Default.html file to match the name and description of your Bot Application.

Here's the Bot Application Default.htm file in Microsoft Edge

![Bot running the browser targeting localhost](/images/connector-getstarted-bot-running-localhost)

Make note of the port that the application is running on, which in this example is port 3978, you will need this information to run the Bot Framework Emulator.

Now open the Bot Framework Emulator, there are a few items that you will need to configure in the tool before you can interact with your Bot Application.

The three items you will need to enter are:
	1. Url, this should match the URL displayed in your web browser that is displaying the Default.htm file. Note that you will need to add  "/api/messages" to the URL.
	2. The AppId from your Web.Config file.
	3. The AppSecret from your Web.Config file.

![Configure the emulator with your locahost URL, AppId & AppSecret](/images/connector-getstarted-configure-emulator)

Now that everything is configured you can interact with your service. The bottom of the Bot Framework Emulator application has a Text Box that you can use to enter a message, this message will be echoed back to you, like below.

![Testing the interaction with the bot via the emulator](/images/connector-getstarted-test-conversation-emulator)

If we take a look at the code in the Bot Application that was generated by the Visual Studio 2015 Bot Application Template, specifically the file called MessageController.cs we can see how the message entered by a user is converted into the response message by adding "You said:" to the front of the message.

        if (message.Type == "Message")
        {
            // fetch our state associated with a user in a conversation. If we don't have state, we get default(T)
            var counter = message.GetBotPerUserInConversationData<int>();

            // create a reply message   
            Message replyMessage = message.CreateReplyMessage($"{++counter} You said:{message.Text}");

            // save our new counter by adding it to the outgoing message
            replyMessage.SetBotPerUserInConversationData(counter);

            // return our reply to the user
            return replyMessage;
        }

Publishing your Bot Application to Azure.

To publish your Bot Application you will need an Azure subscription, get a free trial from here: https://azure.microsoft.com/en-us/ 

Make what changes you like, and now you're ready to publish.  Right click on the project and choose "Publish", and then your appropriate Azure subscription information.  By default, the bot should be published as an Microsoft Azure App Service.  When publishing, keep track of the URL you chose because we'll need it to update the Bot Framework registration endpoint.

//TODO: Walk through the publish process on Azure - Selecting "Publish" (Build | Publish) displays the following dialog:
![Right click on the project and choose "Publish", and then your appropriate Azure subscription information](/images/connector-getstarted-publish-dialog.png)
![Right click on the project and choose "Publish", and then your appropriate Azure subscription information](/images/connector-getstarted-publish.png)
![](/images/connector-getstarted-publish-app-service.png)
![](/images/connector-getstarted-publish-app-service-create.png)
![](/images/connector-getstarted-publish-app-service-create-spinner.png)
![](/images/connector-getstarted-publish-destination.png)
![](/images/connector-getstarted-publish-configuration.png)
![](/images/connector-getstarted-publish-preview.png)
![](/images/connector-getstarted-publish-output.png)





Assume that the developer doesn't know what options to choose, or hasn't been through this process before.

4. Go back to the dev portal, and update the Endpoint with the URL of your bot.  If you're using the Bot Framework Connector template still, you'll need to extend it with the path to the endpoint at /API/Messages as shown below. (Question: the walkthrough uses the BotFramework connector template, Step4 states "if you are still using the bot framework template" - assuming that the developer is following along, then they are using the template, right?

![Go back to the dev portal, and update the Endpoint with the URL of your bot](/images/connector-getstarted-portal-endpoint.png)

## Testing the connection to your bot

Back in the My bots dashboard for your bot there's a test chat window that you can use to interact with your bot without further configuration, and verify that the Bot Framework Connector can communicate with your bot's web service.  

Note that the first request after your bot starts up can take 20-30s as Azure starts up the web service for the first time. Subsequent requests will be quick.  This simple viewer will let you see the JSON object returned by your bot.
	
## Configuring Channels

Now that you have a bot up and running, you'll want to configure it for one or more channels your users are using.  Configuring channels is a combination of Microsoft Bot Framework workflow and conversation service workflow, and is unique for each channel you wish to configure.  

1. To configure a channel, go back to the Bot Framework dev portal at https://www.botframework.com.  Sign in, select your bot, and go to the Channels panel.

![Sign in, select your bot, and go to the Channels panel.](/images/connector-getstarted-configure-channels.png)

2. Pick the channel you wish to configure, and click edit.  You'll be taken to a page of instructions for registering a bot.  In the end in most cases you're configuring your credentials as a developer on the target service, registering your app, and getting a set of Oauth keys that Microsoft Bot Framework can use on your behalf.

3. Once you've gone through the steps here, return to the channel page on the dev portal, click the checkbox for the channel you chose (if you haven't already), and hit "save changes".  

That's the end of configuration - your bot is ready for your users.  They will have their own steps to follow to give the bot permission to participate in their group/channel or get connection details like the SMS phone number or e-mail.  They can do this in the Bot Directory page for your bot.  The link to this is at the top of the Bot Details page in the dev portal.  They also can find your bot by searching by name in the Bot Directory.

The next section will cover adding a bot to a conversation.
