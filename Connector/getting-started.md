---
layout: page
title: Getting started
permalink: /connector/getstarted/
weight: 201
parent1: Bot Connector
---


* TOC
{:toc}


## Overview

The Microsoft Bot Framework Connector is a communication service that helps you connect your Bot with many different communication channels (GroupMe, SMS, email, and others). If you write a conversational Bot or agent and expose a Microsoft Bot Framework-compatible API on the internet, the Connector will forward messages from your Bot to a user, and will send user messages back to your Bot.  

![System Overview of the Bot Framework](/images/connector-getstarted-system-diagram.png)

To use the Microsoft Bot Framework Connector, you must have:

1. A Microsoft Account (Hotmail, Live, Outlook.Com) to log into the Bot Framework developer portal, which you will use to register your Bot.
2. An Azure-accessible REST endpoint exposing a callback for the Connector service.
3. Developer accounts on one or more communication services (such as Skype) where your Bot will communicate. 
	
In addition you may wish to have an Azure App Insights account so you can capture telemetry from your Bot. There are additionally different ways to go about building a Bot; from scratch, coded directly to the Bot Connector REST API, the Bot Builder SDK's for Node.JS & .Net, and the Bot Connector .Net template which is what this QuickStart guide demonstrates.
	
## Getting started in .Net    
This is a step-by-step guide to writing an Bot in C\# using the Bot Framework Connector SDK .Net template.

1. Install prerequisite software

	* Visual Studio 2015 Update 1 - you can downlodad the community version here for free:
        https://www.visualstudio.com/
            
	* Important: Please update all VS extensions to their latest versions
		Tools->Extensions and Updates->Updates

2. Download and install the Bot Application template
	* Download the file from the direct download link [here](http://aka.ms/bf-bc-vstemplate):
	* Unpack the downloaded file into a folder
	* If Visual Studio is installed in the default path, Double-click install.cmd, which copies the ZIP file to your VS templates directory
		* If not, copy to "%USERPROFILE%\Documents\Visual Studio 2015\Templates\ProjectTemplates\Visual C\#\"

3. Open Visual Studio

4. Create a new C\# project using the new Bot Application template.

![Create a new C\# project using the new Bot Application template.](/images/connector-getstarted-create-project.png)

5. The template is a fully functional Echo Bot that takes the user's text utterance as input and returns it as output.  In order to run however, 
	* The bot has to be registered with Bot Connector
	* The AppId and AppSecret from the Bot Framework registration page have to be recorded in the project's web.config
	* The project needs to be published to the web

## Building your Bot

The core functionality of the Bot Template is all in the Post function within Controllers\MessagesController.cs. In this case the code takes the message text for the user, increments a counter that it pulled from the Message, and then creates replyMessage using the CreateReplyMessage function. It wraps up by saving the counter in the PerUserInConversationData block, and returns the replyMessage.

{% highlight cs %}

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
            
{% endhighlight %}
            
##Use the Bot Framework Emulator to test your Bot application

The Bot Framework provides an emulator that lets you test calls to your Bot as if it were being called by the Bot Framework cloud service. To install the Bot Framework Emulator, download it from here [https://aka.ms/bf-bc-emulator](https://aka.ms/bf-bc-emulator).

One installed, you're ready to test. First, start your Bot in Visual Studio using a browser as the application host. The image below uses Microsoft Edge.

![Start your Bot in VS2015 targeting the browser](/images/connector-getstarted-start-bot-locally.png)

When the application is built and deployed the web browser will open and display the application Default.htm file (which is part of the Bot Application project). Feel free to modify the Default.html file to match the name and description of your Bot Application.

Here's the Bot Application Default.htm file in Microsoft Edge

![Bot running the browser targeting localhost](/images/connector-getstarted-bot-running-localhost.png)

When using the emulator to test your Bot application, make note of the port that the application is running on, which in this example is port 3978. You will need this information to run the Bot Framework Emulator.

Now open the Bot Framework Emulator. There are a few items that you will need to configure in the tool before you can interact with your Bot Application.

The three items you will need to enter are:
	1. Url, this should match the URL displayed in your web browser that is displaying the Default.htm file. Note that you will need to add  "/api/messages" to the URL when using the Bot Application template.
	2. The AppId from your Web.Config file.
	3. The AppSecret from your Web.Config file.

![Configure the emulator with your locahost URL, AppId & AppSecret](/images/connector-getstarted-configure-emulator.png)

Now that everything is configured you can interact with your service. The bottom of the Bot Framework Emulator application has a Text Box that you can use to enter a message, this message will be echoed back to you, like below.

![Testing the interaction with the Bot via the emulator](/images/connector-getstarted-test-conversation-emulator.png)

If we take a look at the code in the Bot Application that was generated by the Visual Studio 2015 Bot Application Template, specifically the file called MessageController.cs we can see how the message entered by a user is converted into the response message by adding "You said:" to the front of the message.


{% highlight cs %}

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
            
{% endhighlight %}
        

## Publishing your Bot Application to Microsoft Azure

In this tutorial, we use Microsoft Azure to host the Bot application. To publish your Bot Application you will need a Microsoft Azure subscription. You can get a free trial from here: https://azure.microsoft.com/en-us/ 

Make what changes you like to the project, and now you're ready to publish. Right click on the project and choose "Publish", and then your appropriate Azure subscription information. By default, the bot should be published as an Microsoft Azure App Service. When publishing, keep track of the URL you chose because we'll need it to update the Bot Framework registration endpoint. The first time you publish there are a few extra steps; but you only have to do them once.

In Visual Studio, right clicking on the project in Solution Explorer and select "Publish" - or alternately selecting "Build | Publish" displays the following dialog:

![Right click on the project and choose "Publish" to start the Azure publish wizard](/images/connector-getstarted-publish-dialog.png)

The Publish to Azure wizard will start. For this tutorial you will need to select "Microsoft Azure App Service" as your project type.

![Select Microsoft Azure App Service and click Next](/images/connector-getstarted-publish.png)

The next step in the Azure App Service publishing process is to create your App Service. Click on "New…" on the right side of the dialog to create the App Service.

![Click new to create a "New..." Azure App Service](/images/connector-getstarted-publish-app-service.png)

The Create App Service dialog will be displayed, fill in the details as appropriate

![Give your App Service a name, then click New App Service Plan to define one](/images/connector-getstarted-publish-app-service-create.png)

One final complexity on this dialog is the App Service Plan. This just lets you give a name to a combination of location and system size so you can re-use it on future deployments. Just put in any name, then choose the datacenter and size of deployment you want.

![Create your definition for an App Service Plan ](/images/connector-getstarted-publish-app-service-create-spinner.png)

Once you hit okay on the App Service Plan, you'll have defined your App Service completely. Hit Create, and you'll be taken back to the Publish Web Wizard.

![Complete the Create App Service wizard by clicking Create](/images/connector-getstarted-publish-destination.png)

Now that you've returned to the Publish Web wizard copy the destination URL to the clipboard, you'll need it in a few moments. Hit "Validate Connection" to ensure the configuration is good, and if all goes well, click "Next".

![Validate and click next to move on to the last step.](/images/connector-getstarted-publish-configuration.png)

By default your Bot will be published in a Release configuration. If you want to debug your Bt, change Configuration to Debug. Regardless, from here you'll hit "Publish" and your Bot will be published to Azure.

![Last step; click Publish to submit to Azure](/images/connector-getstarted-publish-preview.png)

You will see a number of messages displayed in the Visual Studio 2015 "Output" window. Once publishing is complete you will also see the web page for your Bot Application displayed in your browser (the browser will launch, and render your Bot Application HTML page), see below.

![Voila, your Bot has been publisehd and is running.](/images/connector-getstarted-publish-output.png)

## Registering your Bot with the Microsoft Bot Framework 

Registering your Bot tells the Connector how to call your Bot's web service. Note that the AppId and AppSecret are generated when your Bot is registered with the Microsoft Bot Framework Connector, the AppId and AppSecret are used to authenticate the conversation, and allows the developer to configure their Bot with the Channels they'd like to be visible on.

1. Go to the Microsoft Bot Framework portal at [https://www.botframework.com](https://www.botframework.com) and sign in with your Microsoft Account.
	
2. Register an agent
	
3. Click the "Register a Bot" button and fill out the form. Many of the fields on this form can be changed later. Use a the endpoint generated from your Azure deployment, and don't forget that when using the Bot Application tempalate you'll need to extend the URL you pasted in with the path to the endpoint at /API/Messages. Save your changes by hitting "Create" at the bottom of the form.

![Register a bot](/images/connector-getstarted-register-agent.png)

4. Once your registration is created, Microsoft Bot Framework will have generated your AppId and AppSecrets. These are used to authenticate your Bot with the Microsoft Bot Framework.

![Microsoft Bot Framework will have generated your AppId and Subscription Keys](/images/connector-getstarted-subscription-keys.png)

Now that the Bot is registered, you need to update the keys in the web.config file in your Visual Studio project. Change the following keys in the web.config file to match the ones generated when you saved your registration, and you're ready to build. You need only the primary AppSecret, the secondary is used when you wish to regenerate your primary key without downtime. Clicking the "show" link will show the value, along wtih exposing the regenerate link if you ever need to change your AppSecret. Update your web.config, and re-publish your bot to Azure.

{% highlight cs %}

    <?xml version="1.0" encoding="utf-8"?>
    <!--
    For more information on how to configure your ASP.NET application, please visit
    http://go.microsoft.com/fwlink/?LinkId=301879
    -->
    <configuration>
    <appSettings>
        <!-- update these with your appid and one of your appsecret keys-->
        <add key="AppId" value="gssamplebot" />
        <add key="AppSecret" value="41bdc034db134879a53ad7b605936e79" />
    </appSettings>
  
{% endhighlight %}

## Testing the connection to your bot

Back in the developer dashboard for your Bot there's a test chat window that you can use to interact with your Bot without further configuration, and verify that the Bot Framework can communicate with your Bot's web service.  

Note that the first request after your Bot starts up can take 20-30s as Azure starts up the web service for the first time. Subsequent requests will be quick. This simple viewer will let you see the JSON object returned by your Bot.

![Test communication with your now deployed bot in the test channel.](/images/connector-getstarted-test-channel-verification.png)
	
## Configuring Channels

Now that you have a Bot up and running, you'll want to configure it for one or more channels your users are using. Configuring channels is a combination of Microsoft Bot Framework workflow and conversation service workflow, and is unique for each channel you wish to configure.  

1. To configure a channel, go back to the Bot Framework portal at https://www.botframework.com. Sign in, select your Bot, and go to the channels panel.

![Sign in, select your Bot, and go to the Channels panel.](/images/connector-getstarted-configure-channels.png)

2. Pick the channel you wish to configure, and click add.  You'll be taken to a page of instructions for registering a Bot. In the end in most cases you're configuring your credentials as a developer on the target service, registering your app, and getting a set of Oauth keys that Microsoft Bot Framework can use on your behalf.

![Configuring a channel, for example, Skype.](/images/connector_channel_config_skype.png)

3. Once you've gone through the steps here, return to the channel page on the dev portal, click the checkbox for the channel you chose (if you haven't already), and hit "save changes".  

That's the end of configuration - your Bot is ready for your users.  They will have their own steps to follow to give the Bot permission to participate in their group/channel or get connection details like the SMS phone number or e-mail. They can do this in the Bot Directory page for your Bot. The link to this is at the top of the Bot Details page in the dev portal. 