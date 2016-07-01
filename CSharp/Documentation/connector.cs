namespace Microsoft.Bot.Builder.Connector
{
    /// \page connector %Connector 
    /// \tableofcontents
    /// \section Overview
    /// The %Microsoft %Bot %Builder for REST is a communication service that helps you connect your Bot with many different communication channels(GroupMe, SMS, email, and others). If you write a conversational Bot or agent and expose a Microsoft Bot Framework-compatible API on the internet, the Bot Framework connector service will forward messages from your Bot to a user, and will send user messages back to your Bot.
    /// To use the Microsoft Bot Framework Connector, you must have:
    /// 
    ///1. A Microsoft Account (Hotmail, Live, Outlook.Com) to log into the Bot Framework developer portal, which you will use to register your Bot.
    ///2. An Azure-accessible REST endpoint exposing a callback for the Connector service.
    ///3. Developer accounts on one or more communication services(such as Skype) where your Bot will communicate.
    ///In addition you may wish to have an Azure App Insights account so you can capture telemetry from your Bot.There are different ways to go about building a Bot; from scratch, coded directly to the Bot Builder for REST, the Bot Builder SDK's for Node.JS & .NET, and the Bot Connector .NET template which is what this QuickStart guide demonstrates.
    /// \subsection started Getting started in .NET    
    ///This is a step-by-step guide to writing an Bot in C\# using the Bot Framework Connector SDK .NET template.
    ///1. Install prerequisite software
    /// * Visual Studio 2015 (latest update) - you can download the community version here for free:
    ///     [www.visualstudio.com](https://www.visualstudio.com/)
    /// * Important: Please update all VS extensions to their latest versions
    ///     Tools->Extensions and Updates->Updates
    ///2. Download and install the Bot Application template
    /// * Download the file from the direct download link**[here(hackathon only)](https://aka.ms/hackathon-bf-vs-template)**:
    /// * Save the zip file to your Visual Studio 2015 templates directory which is traditionally in "%USERPROFILE%\Documents\Visual Studio 2015\Templates\ProjectTemplates\Visual C\#\"
    ///3. Open Visual Studio
    ///4. Create a new C\# project using the new Bot Application template.
    ///![Create a new C\# project using the new Bot Application template.](/en-us/images/connector/connector-getstarted-create-project.png)
    ///5. The template is a fully functional Echo Bot that takes the user's text utterance as input and returns it as output.  In order to run however, 
    /// * The bot has to be registered with Bot Connector
    /// * The AppId and AppPassword from the Bot Framework registration page have to be recorded in the project's web.config
    /// * The project needs to be published to the web
    /// \subsection building Building your Bot
    ///The core functionality of the Bot Template is all in the Post function within Controllers\MessagesController.cs.In this case the code takes the message text for the user, then creates replyMessage using the CreateReplyMessage function.The BotAuthentication decoration on the method is used to validate your Bot Connector credentials over HTTPS.
    ///\code{.cs}
    /// [BotAuthentication]
    /// public class MessagesController : ApiController
    /// {
    ///     /// <summary>
    ///     /// POST: api/Messages
    ///     /// Receive a message from a user and reply to it
    ///     /// </summary>
    ///     public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
    ///     {
    ///         ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
    ///         if (activity.Type == ActivityTypes.Message)
    ///         {
    ///             // calculate something for us to return
    ///             int length = (activity.Text ?? string.Empty).Length;
    ///
    ///             // return our reply to the user
    ///             Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
    ///             await connector.Conversations.ReplyToActivityAsync(reply);
    ///         }
    ///         else
    ///         {
    ///             HandleSystemMessage(activity);
    ///         }
    ///         var response = Request.CreateResponse(HttpStatusCode.OK);
    ///         return response;
    ///     }
    ///
    ///\endcode
    ///
    /// \subsection emulator Use the Bot Framework Emulator to test your Bot application
    ///
    /// The Bot Framework provides a a channel emulator that lets you test calls to your Bot as if it were being called by the Bot Framework cloud service.To install the Bot Framework Emulator, download it from**[here(hackathon only)](https://aka.ms/hackathon-bot-framework-emulator)**.
    ///
    /// One installed, you're ready to test. First, start your Bot in Visual Studio using a browser as the application host. The image below uses Microsoft Edge.
    ///
    ///![Start your Bot in VS2015 targeting the browser](/en-us/images/connector/connector-getstarted-start-bot-locally.png)
    ///
    ///When the application is built and deployed the web browser will open and display the application Default.htm file(which is part of the Bot Application project). Feel free to modify the Default.html file to match the name and description of your Bot Application.
    ///
    ///Here's the Bot Application Default.htm file in Microsoft Edge
    ///
    ///![Bot running the browser targeting localhost](/en-us/images/connector/connector-getstarted-bot-running-localhost.png)
    ///
    ///When using the emulator to test your Bot application, make note of the port that the application is running on, which in this example is port 3978. You will need this information to run the Bot Framework Emulator.
    ///
    ///Now open the Bot Framework Emulator. There are a few items that you will need to configure in the tool before you can interact with your Bot Application.
    ///
    ///The three items you will need to enter are:
    ///	1. Url, this should match the URL displayed in your web browser that is displaying the Default.htm file. Note that you will need to add  "/api/messages" to the URL when using the Bot Application template.
    ///	2. The MicrosoftAppId from your Web.Config file.
    ///	3. The MicrosoftAppPassword from your Web.Config file.
    ///
    ///![Configure the emulator with your locahost URL, AppId & AppPassword](/en-us/images/connector/connector-getstarted-configure-emulator.png)
    ///
    ///Now that everything is configured you can interact with your service.The bottom of the Bot Framework Emulator application has a Text Box that you can use to enter a message, this message will be echoed back to you, like below.
    ///
    ///![Testing the interaction with the Bot via the emulator](/en-us/images/connector/connector-getstarted-test-conversation-emulator.png)
    ///
    ///If we take a look at the code in the Bot Application that was generated by the Visual Studio 2015 Bot Application Template, specifically the file called MessageController.cs we can see how the message entered by a user is converted into the reply Activity, sending "You sent {activity.Text} which was {length} characters" back to the user.     
    ///
    /// \subsection publishing Publishing your Bot Application to Microsoft Azure
    ///
    ///In this tutorial, we use Microsoft Azure to host the Bot application. To publish your Bot Application you will need a Microsoft Azure subscription. You can get a free trial from here: https://azure.microsoft.com/en-us/ 
    ///
    /// Make what changes you like to the project, and now you're ready to publish. Right click on the project and choose "Publish", and then your appropriate Azure subscription information. By default, the bot should be published as an Microsoft Azure App Service. When publishing, keep track of the URL you chose because we'll need it to update the Bot Framework registration endpoint. The first time you publish there are a few extra steps; but you only have to do them once.
    ///
    ///
    /// In Visual Studio, right clicking on the project in Solution Explorer and select "Publish" - or alternately selecting "Build \| Publish" displays the following dialog:
    ///
    ///![Right click on the project and choose "Publish" to start the Azure publish wizard](/en-us/images/connector/connector-getstarted-publish-dialog.png)
    ///
    ///The Publish to Azure wizard will start.For this tutorial you will need to select "Microsoft Azure App Service" as your project type.
    ///
    ///![Select Microsoft Azure App Service and click Next](/en-us/images/connector/connector-getstarted-publish.png)
    ///
    ///The next step in the Azure App Service publishing process is to create your App Service. Click on "New…" on the right side of the dialog to create the App Service.
    ///
    ///![Click new to create a "New..." Azure App Service](/en-us/images/connector/connector-getstarted-publish-app-service.png)
    ///
    ///The Create App Service dialog will be displayed, fill in the details as appropriate.Make sure to choose "Web App" from the Change Type drop down in the top right instead of "API App"(which is the default).
    ///
    ///![Give your App Service a name, then click New App Service Plan to define one](/en-us/images/connector/connector-getstarted-publish-app-service-create.png)
    ///
    ///One final complexity on this dialog is the App Service Plan.This just lets you give a name to a combination of location and system size so you can re - use it on future deployments.Just put in any name, then choose the datacenter and size of deployment you want.
    ///
    ///![Create your definition for an App Service Plan ](/en-us/images/connector/connector-getstarted-publish-app-service-create-spinner.png)
    ///
    ///Once you hit okay on the App Service Plan, you'll have defined your App Service completely. Hit Create, and you'll be taken back to the Publish Web Wizard.
    ///
    ///![Complete the Create App Service wizard by clicking Create](/en-us/images/connector/connector-getstarted-publish-destination.png)
    ///
    ///Now that you've returned to the Publish Web wizard copy the destination URL to the clipboard, you'll need it in a few moments.Hit "Validate Connection" to ensure the configuration is good, and if all goes well, click "Next".
    ///
    ///![Validate and click next to move on to the last step.](/en-us/images/connector/connector-getstarted-publish-configuration.png)
    ///
    ///By default your Bot will be published in a Release configuration.If you want to debug your Bot, change Configuration to Debug. Regardless, from here you'll hit "Publish" and your Bot will be published to Azure.
    ///
    ///![Last step; click Publish to submit to Azure](/en-us/images/connector/connector-getstarted-publish-preview.png)
    ///
    ///You will see a number of messages displayed in the Visual Studio 2015 "Output" window.Once publishing is complete you will also see the web page for your Bot Application displayed in your browser(the browser will launch, and render your Bot Application HTML page), see below.
    ///
    ///![Voila, your Bot has been publisehd and is running.](/en-us/images/connector/connector-getstarted-publish-output.png)
    ///
    /// \subsection registering Registering your Bot with the Microsoft Bot Framework 
    ///
    ///Registering your Bot tells the Connector how to call your Bot's web service. Note that the MicrosoftAppId and MicrosoftAppPassword are generated when your Bot is registered with the Microsoft Bot Framework Connector, the MicrosoftAppId and MicrosoftAppPassword are used to authenticate the conversation, and allows the developer to configure their Bot with the Channels they'd like to be visible on.The BotId, which you specify, is used for the URL in the directory and developer portal.
    ///
    ///
    ///1.Go to the Microsoft Bot Framework portal at[https://dev.botframework.com](https://dev.botframework.com) and sign in with your Microsoft Account.
    ///
    ///2.Click the "Register a Bot" button and fill out the form.Many of the fields on this form can be changed later.Use a the endpoint generated from your Azure deployment, and don't forget that when using the Bot Application tempalate you'll need to extend the URL you pasted in with the path to the endpoint at / API / Messages.You should also prefix your URL with HTTPS instead of HTTP; Azure will take care of providing HTTPS support on your bot.Save your changes by hitting "Create" at the bottom of the form.
    ///
    ///![Register a bot](/en-us/images/connector/connector-getstarted-register-agent.png)
    ///
    ///3.Once your registration is created, Microsoft Bot Framework will have generated your MicrosoftAppId and MicrosofAppPassword. These are used to authenticate your Bot with the Microsoft Bot Framework.
    ///
    ///![Microsoft Bot Framework will have generated your MicrosoftAppId and MicrosoftAppPassword](/en-us/images/connector/connector-getstarted-subscription-keys.png)
    ///
    ///Now that the Bot is registered, you need to update the keys in the web.config file in your Visual Studio project. Change the following keys in the web.config file to match the ones generated when you saved your registration, and you're ready to build. You need only the primary AppPassword, the secondary is used when you wish to regenerate your primary key without downtime. Clicking the "show" link will show the value, along wtih exposing the regenerate link if you ever need to change your AppPassword. Update your web.config, and re-publish your bot to Azure.
    ///
    ///~~~
    ///
    /// <? xml version = "1.0" encoding = "utf-8" ?>
    ///    < !--
    ///    For more information on how to configure your ASP.NET application, please visit
    ///    http://go.microsoft.com/fwlink/?LinkId=301879
    /// -->
    /// < configuration >
    /// < appSettings >
    ///     < !--update these with your appid and one of your appsecret keys-->
    ///     < add key = "MicrosoftAppId" value = "[GUID]" />
    ///        < add key = "MicrosoftAppPassword" value = "[PASSWORD]" />
    ///       </ appSettings >
    ///~~~
    ///
    /// \subsection testing Testing the connection to your bot
    ///
    /// Back in the developer dashboard for your Bot there's a test chat window that you can use to interact with your Bot without further configuration, and verify that the Bot Framework can communicate with your Bot's web service.
    ///
    /// Note that the first request after your Bot starts up can take 10 - 15 s as Azure starts up the web service for the first time.Subsequent requests will be quick.This simple viewer will let you see the JSON object returned by your Bot.
    ///
    ///
    ///   ![Test communication with your now deployed bot in the test channel.](/en-us/images/connector/connector-getstarted-test-channel-verification.png)
    ///
    /// \subsection channels Configuring Channels
    ///
    ///Now that you have a Bot up and running, you'll want to configure it for one or more channels your users are using. Configuring channels is a combination of Microsoft Bot Framework workflow and conversation service workflow, and is unique for each channel you wish to configure.  
    ///
    ///
    ///1.To configure a channel, go back to the Bot Framework portal at https://www.botframework.com. Sign in, select your Bot, and go to the channels panel.
    ///   ![Sign in, select your Bot, and go to the Channels panel.](/en-us/images/connector/connector-getstarted-configure-channels.png)
    ///
    ///
    ///2.Pick the channel you wish to configure, and click add.You'll be taken to a page of instructions for registering a Bot. In the end in most cases you're configuring your credentials as a developer on the target service, registering your app, and getting a set of Oauth keys that Microsoft Bot Framework can use on your behalf.
    ///   ![Configuring a channel, for example, Skype.](/en-us/images/connector/connector_channel_config_skype.png)
    ///
    ///
    ///3.Once you've gone through the steps here, return to the channel page on the dev portal, click the checkbox for the channel you chose (if you haven't already), and hit "save changes".
    ///
    ///That's the end of configuration - your Bot is ready for your users.  They will have their own steps to follow to give the Bot permission to participate in their group/channel or get connection details like the SMS phone number or e-mail. They can do this in the Bot Directory page for your Bot. The link to this is at the top of the Bot Details page in the dev portal. 
    ///  
    ///\section Routing
    ///\subsection replying Replying to a message Activity
    ///When your bot receives a message Activity it most likely will want to respond. The minimum amount of information that is needed to respond is to send back the text that you want to send back to the user as a reply.
    ///
    ///To do that, you need a new Activity() with
    ///
    ///* The From and Recipient fields swapped from the original message(so that it will be routed back to where it came from)
    ///* The conversation from the original message on it(so you can send it back to the same conversation)
    ///* The new Text
    ///
    ///To make this super easy to do in C# we created an extension method on the Activity class called **CreateReply()**.
    ///
    ///To create and send a proper reply message all you have to do is:
    ///~~~
    ///var replyMessage = incomingMessage.CreateReply("Yo, I heard you.", "en-Us");
    ///var response = await connector.Conversations.ReplyToActivityAsync(replyMessage);
    ///~~~
    ///
    ///\subsubsection replyinglater Replying to the message later
    ///
    ///To reply to your user, you create a reply message and send it to the user.The difference between ReplyToActivityAsync
    ///and SendToConversationAsync is just that Reply, on channels that support it, will attempt to maintain "threading" in the conversation whereas Send will not.
    ///
    ///~~~
    ///var replyMessage =  incomingMessage.CreateReply("Yo, I heard you.", "en");
    ///return null; // no reply
    ///...
    ///
    /// send the reply later    
    ///var connector = new ConnectorClient(incomingMessage.ServiceUrl);
    ///await connector.Conversations.ReplyToActivityAsync(replyMessage); 
    ///
    ///~~~
    ///
    ///\subsubsection multiplereplies Multiple replies
    ///The reply mechanism is really a "add message to conversation", and so there is nothing wrong with sending
    ///multiple replies at a later point.
    ///
    ///~~~
    ///var connector = new ConnectorClient(incomingMessage.ServiceUrl);
    ///connector.Conversations.ReplyToActivity(incomingMessage.CreateReply("Yo, I heard you 1.", "en"));
    ///connector.Conversations.ReplyToActivity(incomingMessage.CreateReply("Yo, I heard you 2.", "en"));
    ///connector.Conversations.ReplyToActivity(incomingMessage.CreateReply("Yo, I heard you 3.", "en"));
    ///~~~
    ///
    ///
    ///\subsection conversation Starting a conversation
    ///
    ///The difference between a new conversation, and a reply to an existing conversation is the value of the Conversation property. Conversation is a required property on an Activity.
    ///
    ///* If the Conversation property is set using the value from an existing conversation, it will continue that conversation.
    ///* A new Conversation can be created with the CreateConversation or CreateDirectConversation methods.
    ///
    ///\subsubsection conversationuser Example of starting a new conversation with a user
    ///~~~{.cs}
    ///    var connector = new ConnectorClient(incomingMessage.ServiceUrl);
    ///    Activity message = new Activity();
    ///    message.From = botChannelAccount;
    ///    message.ChannelId = "slack";
    ///    message.Recipient = new ChannelAccount() { name: "Larry", "id":"@UV357341"};
    ///    message.Text = "Hey, what's up homey?";
    ///    message.Locale = "en-Us";
    ///    var ConversationId = await connector.Conversations.CreateDirectConversationAsync(incomingMessage.Recipient, incomingMessage.From);
    ///    message.Conversation = new ConversationAccount(id: ConversationId.Id);
    ///    var reply = await connector.Conversations.ReplyToActivityAsync(message);
    ///~~~
    ///
    ///
    ///\subsubsection conversationmultiple Example of starting a new conversation with a set of users
    ///~~~{.cs}
    ///    var connector = new ConnectorClient();
    ///    List<ChannelAccount> participants = new List<ChannelAccount>();
    ///    participants.Add(new ChannelAccount("joe@contoso.com", "Joe the Engineer"));
    ///    participants.Add(new ChannelAccount("sara@contso.com", "Sara in Finance"));
    ///
    ///    ConversationParameters cpMessage = new ConversationParameters(message.Recipient, participants, "Quarter End Discussion");
    ///    var ConversationId = connector.Conversations.CreateConversationAsync(cpMessage);
    ///
    ///    Activity message = new Activity();
    ///    message.From = botChannelAccount;
    ///    message.Recipient = new ChannelAccount("lydia@contoso.com", "Lydia the CFO"));
    ///    message.Conversation = ConversationId;
    ///    message.ChannelId = "email";
    ///    message.Text = "Hey, what's up everyone?";
    ///    message.Locale = "en-Us";
    ///    var reply = await connector.Conversations.ReplyToActivityAsync(message);
    ///~~~
    ///
    ///
    /// > NOTE: All of the ChannelAccounts need to be on the same Channel as the Activity has a single ChannelId
    ///
    /// 
    ///\subsection addresses Addresses in messages
    ///The Bot Framework API uses ChannelAccount records to represent an contact address for a user or bot
    /// on a communication channel.Numerous fields in the Activity object have ChannelAccount
    /// references in them to represent the relationships between the users and bots that are participating in
    ///a conversation. 
    ///
    ///\subsubsection ChannelAccount
    ///The ChannelAccount object is a core object which describes an alias for a user. 
    ///
    ///| **Property** | **Description**                     | **Examples**                     |**V1 Property**|   
    ///|--------------|-------------------------------------|---------------------------------|---------------|
    ///|**name**      | A name for the user or bot          | Joe Smith | name |
    ///|**id**        | A global id which represents a bot or user | 1jsk1jkdidr4F| id |
    ///| n/a  | The channel that the address is for | email, slack, groupme, sms, etc.| ChannelId |
    ///|  n/a  | The address on the channel          | joe @hotmail.com, +14258828080, etc.| Address |
    ///
    ///
    /// Each user has 1..N ChannelAccounts which represent their identities on each channel.
    ///
    /// Each bot has 1..N ChannelAccounts which represent their identities on each channel.
    ///
    /// The Bot Framework connector service is primarily a switch which routes messages between bots and users represented by ChannelAccount records.
    ///
    ///\subsubsection channelaccounts Activity object properties that use ChannelAccounts
    ///
    ///| **Property** | **Description**                     | **V1 Property** |                    
    ///|--------------|-------------------------------------|------------------|
    ///|**Activity.From**       | The address for the sender | Message.From |   
    ///|**Activity.Recipient**         | The address for the recipient | Message.To |
    ///|**Activity.Entities**   | A mixed collection of entities including mentions | Message.Mentions |
    ///|**Activity.UsersAdded** | A List<> of users added to the group conversation | n/a |
    ///|**Activity.UsersRemoved** | A List<> of users removed from the group conversation | n/a |
    ///| n/a | ChannelAccounts of known participants in the conversation (see Conversations.GetActivityMembers()) | Message.Participants |     
    ///
    ///When you receive a Activity from a user it will have the From field set to the
    ///user who created the message and the Recipient field will always be your bot's identity
    ///in that conversation.
    ///
    ///>It is important to note that a bot doesn't always know
    ///it's identity before hand because some channels assign out new identities for
    ///a bot when a bot is added to a conversation. (For example groupme and slack do this.)
    ///
    ///As a result, it is important when you are replying to a conversation to create a new
    ///message which appropriately sets the From and Recipient fields(see[Replying to an Activity](/en-us/connector/replying/) for more details). 
    ///
    ///The difference between a new conversation, and a reply to an existing conversation is the value of the Conversation property.Conversation is a required property on an Activity.
    ///
    ///* If the Conversation property is set using the value from an existing conversation, it will continue that conversation.
    ///* A new Conversation can be created with the CreateConversation or CreateDirectConversation methods.
    ///
    ///\subsubsection Mentions
    ///Many communication clients have mechanisms to "mention" someone.Knowing that someone is 
    ///mentioned can be an important piece of information for a bot that the channel knows and needs to be able to pass to you.
    ///
    ///Frequently a bot needs to know that **they** were mentioned, but with some channels
    ///they don't always know what their name is on that channel. (again see Slack and Group me where names
    ///are assigned per conversation)
    ///
    ///To accomodate these needs the Activity.Entities List includes Mention objects, accessible through the GetMentions() method.A Mention object is made up of
    ///*** Mentioned** - ChannelAccount of the person or user who was mentiond
    ///*** Text** - the text in the Activity.Text property which represents the mention. (this can be empty or null)
    ///
    ///Example:
    ///The user on slack says:
    ///
    ///> @ColorBot pick me a new color
    ///
    ///~~~
    /// {   
    ///     ...
    ///    "mentions": [{ "Mentioned": { "Id": "UV341235", "Name":"Color Bot" },"Text": "@ColorBot" }]
    ///     ...
    /// }
    ///~~~
    ///
    ///This allows the bot to know that they were mentioned and to ignore the @ColorBot part of the input when
    ///trying to determine the user intent.
    ///
    ///> NOTE: Mentions go both ways.  A bot may want to mention a user in a reply to a conversation.If they fill out the Mentions object
    /// with the mention information then it allows the Channel to map it to the mentioning semantics of the channel.
    ///
    ///\section Messages
    ///\subsection composingactivity Composing with Activity
    ///An Activity is the object that is used to communicate between a user and a bot.When you send an Activity
    ///there are a number of properties that you can use to control your message and how it is presented to the
    ///user when they receive it.
    ///
    ///\subsubsection textlanguages Text and Language 
    ///Most of the time Text is the only property you need to worry about.  A person sent you some text, or your bot is sending some text back.
    ///
    ///| Property    | Description                               | Example
    ///| ------------|-------- ----------------------------------| ----------
    ///| **Text**  | A text payload in markdown syntax which will be rendered as appropriate on each channel| Hello, how are you?
    ///
    ///If all you do is give simple one-line text responses, you don't have to read any further.
    ///
    ///\subsubsection textproperty The Text property is **Markdown**
    ///For many (but not all) channels, the text property can be expressed in markdown.This allows each channel to render the markdown as appropriate.
    ///
    ///The markdown that is supported:
    ///
    ///|Style          | Markdown                                                               |Description                              | Example                                                             
    ///|---------------| -----------------------------------------------------------------------|-----------------------------------------| ------ -------------------------------------------------------------
    ///|**Bold**           | \*\* text\*\*                                                           | make the text bold                      | **text**                                                            
    ///|**Italic**         | \* text\*                                                               | make the text italic                    | * text*
    ///|**Header1-5**      | # H1                                                                   | Mark a line as a header                 | # H1                                                       
    ///|**Strikethrough**  | ~~text~~                                                           | make the text strikethrough                 | ~~text~~
    ///|**Hr**             | \-\-\-                                                                 | insert a horizontal rule                |                                                                    |   
    ///|**Unordered list** | \*                                                                     | Make an unordered list item             | * text
    ///|**Ordered list**   | 1.                                                                     | Make an ordered list item starting at 1 | 1. text                                                          
    ///|**Pre**            | \`text\`                                                               | Preformatted text(can be inline)                 | `text`                                                              
    ///|**Block quote**    | \> text                                                                | quote a section of text                 | > text                                                              
    ///|**link**           | \[bing](http://bing.com)                                               | create a hyperlink with title           | [bing](http://bing.com)                                             
    ///|**image link**     | \![duck]\(http://aka.ms/Fo983c) | link to an image                     | ![duck](http://aka.ms/Fo983c)
    ///
    ///\subsubsection mdparagraphs Markdown Paragraphs
    ///As with most markdown systems, to represent a paragraph break you need to have a blank line.
    ///
    ///Markdown like this:
    ///
    ///    This is
    ///    paragraph one
    ///
    ///    This is 
    ///    paragraph two
    ///
    ///
    ///Will be rendered as
    ///
    ///    This is paragraph one
    ///    This is paragraph two
    ///
    ///\subsubsection mdfallback Markdown Fallback
    ///
    ///Not all channels can represent all markdown fields.  As appropriate channels will fallback to a reasonable approximation, for 
    ///example, bold will be represented in text messaging as \*bold\* 
    ///
    ///> Tables: If you are communicating with a channel which supports fixed width fonts or html you can use standard table
    ///> markdown, but because many channels (such as SMS) do not have a known display width and/or have variable width fonts it 
    ///> is not possible to render a table properly on all channels.      
    ///
    ///\subsubsection Attachments
    ///The Attachments property is an array of Attachment objects which allow you to send and receive images and other content.
    ///The primary fields for an Attachment object are:
    ///
    ///| Name        | Description                               | Example   
    ///| ------------|-------- ----------------------------------| ----------
    ///|**ContentType** | The contentType of the ContentUrl property| image/png
    ///|**ContentUrl**  | A link to content of type ContentType     | http://somedomain.com/cat.jpg 
    ///|**Content**     | An embedded object of type contentType    | If contentType = Location then this could be an object that represents the location
    ///
    ///> When images are sent by a user to the bot they will come in as attachments with a ContentType and ContentUrl pointing to the image.  
    ///
    ///Some channels allow you to represent a card responses made up of a title, link, description and images. There are multiple card formats, including HeroCard,
    ///ThumbnailCard, Receipt Card and Sign in.  Additionally your card can optionally be displayed as a list or a carousel using the **AttachmentLayout**
    /// property of the Acivity.See[Attachments](/en-us/connector/message-actions) for more info about Attachments.
    ///
    ///\subsubsection channeldataproperty ChannelData Property
    ///As you can see above the default message gives you a pretty rich pallete to describe your response in way that allows your message to "just work" across
    ///a variety of channels.Most of the heavy lifting is done by the channel adapter, adapating your message to the way it is expressed on that channel.
    ///
    ///
    ///If you want to be able to take advantage of special features or concepts for a channel we provide a way for you to send native
    ///metadata to that channel giving you much deeper control over how your bot interacts on a channel.The way you do this is to pass
    ///extra properties via the *ChannelData* property. 
    ///
    ///
    ///Go to [ChannelData in Messages](/en-us/connector/custom-channeldata) for more detailed description of what each channel enables via the CustomData field.
    ///
    ///
    ///\subsection attachmentscardsactions Attachments, Cards and Actions
    ///Many messaging channels provide the ability to attach richer objects.In the Bot Connector we map
    ///our attachment data structure to media attachments and rich cards on each channel.
    ///
    ///\subsubsection imagefileattachments Image and File Attachments
    ///To pass a simple attachment to a piece of content you add a simple attachment data structure with a link to the
    ///content.
    /// 
    ///| Property | Description |
    ///|-----|------|
    ///| ContentType | mimetype/contenttype of the url |
    ///| ContentUrl  | a link to the actual file |
    ///
    ///
    ///If the content type is a image or media content type then it will be passed to the channel in a way that
    ///allows the image to be displayed.If it is a file then it will simply come through as a link.
    ///
    ///~~~
    ///
    ///replyMessage.Attachments.Add(new Attachment()
    ///{
    ///    ContentUrl = "https://upload.wikimedia.org/wikipedia/en/a/a6/Bender_Rodriguez.png",
    ///    ContentType = "image/png"
    ///});
    ///~~~
    ///
    ///~~~{.json}
    ///
    ///{
    ///    "attachments": [
    ///        {
    ///            "contentType": "image/png",
    ///            "contentUrl": "https://upload.wikimedia.org/wikipedia/en/a/a6/Bender_Rodriguez.png"
    ///        }
    ///    ]
    ///}
    ///
    ///~~~
    ///
    ///\subsubsection richcards Rich card attachments
    ///We also have the ability to render rich cards as attachments.There are several types of cards supported:
    ///
    ///| Card Type | Description | Supported Modes |
    ///|-----------|-------------|-----------------|
    ///| Hero Card | A card with one big image | Single or Carousel |
    ///| Thumbnail Card | A card with a single small image | Single or Carousel |
    ///| Receipt Card | A card that lets the user deliver an invoice or receipt | Single |
    ///| Sign-In Card | A card that lets the bot initiatea sign-in procedure | Single |
    ///
    ///\subsubsection herocard Hero Card
    ///The Hero card is a multipurpose card; it primarily hosts a single large image, a button, and a "tap action", along with text content to display on the card.
    ///
    ///| Property | Description |
    ///|-----|------|
    ///| Title | Title of card|
    ///| Subtitle | Link for the title |
    ///| Text | Text of the card |
    ///| Images[] | For a hero card, a single image is supported |
    ///| Buttons[] | Hero cards support one or more buttons |
    ///| Tap | An action to take when tapping on the card |
    ///
    ///Sample using the C# SDK:
    ///
    ///~~~{.cs}
    ///
    ///Activity replyToConversation = message.CreateReply("Should go to conversation, with a hero card");
    ///replyToConversation.Recipient = message.From;
    ///replyToConversation.Type = "message";
    ///replyToConversation.Attachments = new List<Attachment>();
    ///
    ///List<CardImage> cardImages = new List<CardImage>();
    ///cardImages.Add(new CardImage(url: "https://3.bp.blogspot.com/-7zDiZVD5kAk/T47LSvDM_jI/AAAAAAAAByM/AUhkdynaJ1Y/s200/i-speak-pig-latin.png"));
    ///cardImages.Add(new CardImage(url: "https://2.bp.blogspot.com/-Ab3oCVhOBjI/Ti23EzV3WCI/AAAAAAAAB1o/tiTeBslO6iU/s1600/bacon.jpg"));
    ///
    ///List<CardAction> cardButtons = new List<CardAction>();
    ///
    ///CardAction plButton = new CardAction()
    ///{
    ///Value = "https://en.wikipedia.org/wiki/Pig_Latin",
    ///Type = "openUrl",
    ///Title = "WikiPedia Page"
    ///};
    ///cardButtons.Add(plButton);
    ///
    ///HeroCard plCard = new HeroCard()
    ///{
    ///    Title = "I'm a hero card",
    ///    Subtitle = "Pig Latin Wikipedia Page",
    ///    Images = cardImages,
    ///    Buttons = cardButtons
    ///};
    ///
    ///Attachment plAttachment = plCard.ToAttachment();
    ///replyToConversation.Attachments.Add(plAttachment);
    ///
    ///var reply = await connector.Conversations.SendToConversationAsync(replyToConversation);
    ///
    ///~~~
    ///
    ///~~~{.json}
    ///
    ///	{
    ///	  "type": "message",
    ///	  "timestamp": "2016-06-20T14:42:59.3314511Z",
    ///	  "from": {
    ///	    "id": "1a392f33",
    ///	    "name": "piglatinbotv3d"
    ///	  },
    ///	  "conversation": {
    ///	    "id": "8a684db8",
    ///	    "name": "Conv1"
    ///	  },
    ///	  "recipient": {
    ///	    "id": "2c1c7fa3",
    ///	    "name": "User1"
    ///	  },
    ///	  "text": "Should go to conversation, with a hero card",
    ///	  "attachments": [
    ///	    {
    ///	      "contentType": "application/vnd.microsoft.card.hero",
    ///	      "content": {
    ///	        "title": "I'm a hero card",
    ///	        "subtitle": "Pig Latin Wikipedia Page",
    ///	        "images": [
    ///	          {
    ///	            "url": "https://3.bp.blogspot.com/-7zDiZVD5kAk/T47LSvDM_jI/AAAAAAAAByM/AUhkdynaJ1Y/s200/i-speak-pig-latin.png"
    ///	          },
    ///	          {
    ///	            "url": "https://2.bp.blogspot.com/-Ab3oCVhOBjI/Ti23EzV3WCI/AAAAAAAAB1o/tiTeBslO6iU/s1600/bacon.jpg"
    ///	          }
    ///	        ],
    ///	        "buttons": [
    ///	          {
    ///	            "type": "openUrl",
    ///	            "title": "WikiPedia Page",
    ///	            "value": "https://en.wikipedia.org/wiki/Pig_Latin"
    ///	          }
    ///	        ]
    ///	      }
    ///	    }
    ///	  ],
    ///	  "replyToId": "bb6316f968184744bf080531dfe10e11"
    ///}
    ///
    ///~~~
    ///
    ///\subsubsection thumbnailcard Thumbnail Card
    ///The Thumbnail card is a multipurpose card; it primarily hosts a single small image, a button, and a "tap action", along with text content to display on the card.
    ///
    ///| Property | Description |
    ///|-----|------|
    ///| Title | Title of card|
    ///| Subtitle | Link for the title |
    ///| Text | Text of the card |
    ///| Images[] | For a hero card, a single image is supported |
    ///| Buttons[] | Hero cards support one or more buttons |
    ///| Tap | An action to take when tapping on the card |
    ///
    ///Sample using the C# SDK:
    ///
    ///~~~{.cs}
    /// 
    ///Activity replyToConversation = message.CreateReply("Should go to conversation, with a thumbnail card");
    ///replyToConversation.Recipient = message.From;
    ///replyToConversation.Type = "message";
    ///replyToConversation.Attachments = new List<Attachment>();
    ///
    ///List<CardImage> cardImages = new List<CardImage>();
    ///cardImages.Add(new CardImage(url: "https://3.bp.blogspot.com/-7zDiZVD5kAk/T47LSvDM_jI/AAAAAAAAByM/AUhkdynaJ1Y/s200/i-speak-pig-latin.png"));
    ///
    ///List<CardAction> cardButtons = new List<CardAction>();
    ///
    ///CardAction plButton = new CardAction()
    ///{
    ///Value = "https://en.wikipedia.org/wiki/Pig_Latin",
    ///Type = "openUrl",
    ///Title = "WikiPedia Page"
    ///};
    ///cardButtons.Add(plButton);
    ///
    ///ThumbnailCard plCard = new ThumbnailCard()
    ///{
    ///    Title = "I'm a thumbnail card",
    ///    Subtitle = "Pig Latin Wikipedia Page",
    ///    Images = cardImages,
    ///    Buttons = cardButtons
    ///};
    ///
    ///Attachment plAttachment = plCard.ToAttachment();
    ///replyToConversation.Attachments.Add(plAttachment);
    ///
    ///var reply = await connector.Conversations.SendToConversationAsync(replyToConversation);
    ///
    ///~~~
    ///
    ///~~~{.json}
    ///
    ///	{
    ///	  "type": "message",
    ///	  "timestamp": "2016-06-20T14:43:00.3167215Z",
    ///	  "from": {
    ///	    "id": "1a392f33",
    ///	    "name": "piglatinbotv3d"
    ///	  },
    ///	  "conversation": {
    ///	    "id": "8a684db8",
    ///	    "name": "Conv1"
    ///	  },
    ///	  "recipient": {
    ///	    "id": "2c1c7fa3",
    ///	    "name": "User1"
    ///	  },
    ///	  "text": "Should go to conversation, with a thumbnail card",
    ///	  "attachments": [
    ///	    {
    ///	      "contentType": "application/vnd.microsoft.card.thumbnail",
    ///	      "content": {
    ///	        "title": "I'm a thumbnail card",
    ///	        "subtitle": "Pig Latin Wikipedia Page",
    ///	        "images": [
    ///	          {
    ///	            "url": "https://3.bp.blogspot.com/-7zDiZVD5kAk/T47LSvDM_jI/AAAAAAAAByM/AUhkdynaJ1Y/s200/i-speak-pig-latin.png"
    ///	          }
    ///	        ],
    ///	        "buttons": [
    ///	          {
    ///	            "type": "openUrl",
    ///	            "title": "WikiPedia Page",
    ///	            "value": "https://en.wikipedia.org/wiki/Pig_Latin"
    ///	          }
    ///	        ]
    ///	      }
    ///	    }
    ///	  ],
    ///	  "replyToId": "bb6316f968184744bf080531dfe10e11"
    ///}
    ///
    ///~~~
    ///
    ///\subsubsection receiptcard Receipt Card
    ///The receipt card allows the Bot to present a receipt to the user.
    ///
    ///| Property | Description |
    ///|-----|------|
    ///| Title | Title of card |
    ///| Facts[] | Key Value pair list of information to display on the receipt |
    ///| Items[] | The list of ReceiptItem objects on this receipt |
    ///| Tap | An action to take when tapping on the card |
    ///| Tax | Tax on this receipt |
    ///| VAT | Any additional VAT on this receipt |
    ///| Total | The Sum Total of the Receipt |
    ///| Buttons[] | Hero cards support one or more buttons |
    ///
    ///
    ///Sample using the C# SDK:
    ///
    ///~~~{.cs}
    ///
    ///Activity replyToConversation = message.CreateReply("Receipt card");
    ///replyToConversation.Recipient = message.From;
    ///replyToConversation.Type = "message";
    ///replyToConversation.Attachments = new List<Attachment>();
    ///
    ///List<CardImage> cardImages = new List<CardImage>();
    ///cardImages.Add(new CardImage(url: "https://3.bp.blogspot.com/-7zDiZVD5kAk/T47LSvDM_jI/AAAAAAAAByM/AUhkdynaJ1Y/s200/i-speak-pig-latin.png"));
    ///
    ///List<CardAction> cardButtons = new List<CardAction>();
    ///
    ///CardAction plButton = new CardAction()
    ///{
    ///Value = "https://en.wikipedia.org/wiki/Pig_Latin",
    ///Type = "openUrl",
    ///Title = "WikiPedia Page"
    ///};
    ///cardButtons.Add(plButton);
    ///
    ///ReceiptItem lineItem1 = new ReceiptItem()
    ///{
    ///    Title = "Pork Shoulder",
    ///    Subtitle = "8 lbs",
    ///    Text = null,
    ///    Image = new CardImage(url: "https://3.bp.blogspot.com/-_sl51G9E5io/TeFkYbJ2lDI/AAAAAAAAAL8/Ug_naHX6pAk/s400/porkshoulder.jpg"),
    ///    Price = "16.25",
    ///    Quantity = "1",
    ///    Tap = null
    ///};
    ///
    ///ReceiptItem lineItem2 = new ReceiptItem()
    ///{
    ///Title = "Bacon",
    ///Subtitle = "5 lbs",
    ///Text = null,
    ///Image = new CardImage(url: "https://2.bp.blogspot.com/-Ab3oCVhOBjI/Ti23EzV3WCI/AAAAAAAAB1o/tiTeBslO6iU/s1600/bacon.jpg"),
    ///Price = "34.50",
    ///Quantity = "2",
    ///Tap = null
    ///};
    ///
    ///List<ReceiptItem> receiptList = new List<ReceiptItem>();
    ///receiptList.Add(lineItem1);
    ///receiptList.Add(lineItem2);
    ///
    ///ReceiptCard plCard = new ReceiptCard()
    ///{
    ///    Title = "I'm a receipt card, isn't this bacon expensive?",
    ///    Buttons = cardButtons,
    ///    Items = receiptList,
    ///    Total = "275.25",
    ///    Tax = "27.52"
    ///};
    ///
    ///Attachment plAttachment = plCard.ToAttachment();
    ///replyToConversation.Attachments.Add(plAttachment);
    ///
    ///var reply = await connector.Conversations.SendToConversationAsync(replyToConversation);
    ///
    ///~~~
    ///
    ///~~~{.json}
    ///
    ///	{
    ///	  "type": "message",
    ///	  "timestamp": "2016-06-20T14:43:00.4573774Z",
    ///	  "from": {
    ///	    "id": "1a392f33",
    ///	    "name": "piglatinbotv3d"
    ///	  },
    ///	  "conversation": {
    ///	    "id": "8a684db8",
    ///	    "name": "Conv1"
    ///	  },
    ///	  "recipient": {
    ///	    "id": "2c1c7fa3",
    ///	    "name": "User1"
    ///	  },
    ///	  "text": "Receipt card",
    ///	  "attachments": [
    ///	    {
    ///	      "contentType": "application/vnd.microsoft.card.receipt",
    ///	      "content": {
    ///	        "title": "I'm a receipt card, isn't this bacon expensive?",
    ///	        "items": [
    ///	          {
    ///	            "title": "Pork Shoulder",
    ///	            "subtitle": "8 lbs",
    ///	            "image": {
    ///	              "url": "https://3.bp.blogspot.com/-_sl51G9E5io/TeFkYbJ2lDI/AAAAAAAAAL8/Ug_naHX6pAk/s400/porkshoulder.jpg"
    ///	            },
    ///	            "price": "16.25",
    ///	            "quantity": "1"
    ///	          },
    ///	          {
    ///	            "title": "Bacon",
    ///	            "subtitle": "5 lbs",
    ///	            "image": {
    ///	              "url": "https://2.bp.blogspot.com/-Ab3oCVhOBjI/Ti23EzV3WCI/AAAAAAAAB1o/tiTeBslO6iU/s1600/bacon.jpg"
    ///	            },
    ///	            "price": "34.50",
    ///	            "quantity": "2"
    ///	          }
    ///	        ],
    ///	        "total": "275.25",
    ///	        "tax": "27.52",
    ///	        "buttons": [
    ///	          {
    ///	            "type": "openUrl",
    ///	            "title": "WikiPedia Page",
    ///	            "value": "https://en.wikipedia.org/wiki/Pig_Latin"
    ///	          }
    ///	        ]
    ///	      }
    ///	    }
    ///	  ],
    ///	  "replyToId": "bb6316f968184744bf080531dfe10e11"
    ///}
    ///
    ///~~~
    ///
    ///\subsubsection signincard Sign-In Card
    ///
    ///The Thumbnail card is a multipurpose card; it primarily hosts a single small image, a button, and a "tap action", along with text content to display on the card.
    ///
    ///| Property | Description |
    ///|-----|------|
    ///| Title | Title of card|
    ///| Text | Text of the card |
    ///| Buttons[] | Hero cards support one or more buttons |
    ///| Tap | An action to take when tapping on the card |
    ///
    ///~~~{.json}
    ///
    ///Activity replyToConversation = message.CreateReply(translateToPigLatin("Should go to conversation, sign-in card"));
    ///replyToConversation.Recipient = message.From;
    ///replyToConversation.Type = "message";
    ///replyToConversation.Attachments = new List<Attachment>();
    ///
    ///List<CardAction> cardButtons = new List<CardAction>();
    ///
    ///CardAction plButton = new CardAction()
    ///{
    ///Value = "https://oauthbot.azurewebsites.net/?LinkId=RegistrationLink_1HEuPKSba6cwj9Sn6j6U8MOpPuAKgNZSt4mJ28f3bXJDN%2B4fWTV8SmC6jFsJcq0yYHLa5QYeuKKvMKQNJ8UO%2Fz4jo4LajhN%2Bjc3W%2FpBP%2BiTGfziBEa%2B03TPS4YSLRrursLWSEdnKYk4AJY4EE6UGYiMNAlIb0HbvnICLuFHDwnI%3D",
    ///Type = "signin",
    ///Title = "Connect"
    ///};
    ///cardButtons.Add(plButton);
    ///
    ///SigninCard plCard = new SigninCard(title: "You need to authorize me", button: plButton);
    ///
    ///Attachment plAttachment = plCard.ToAttachment();
    ///replyToConversation.Attachments.Add(plAttachment);
    ///
    ///var reply = await connector.Conversations.SendToConversationAsync(replyToConversation);
    ///
    ///~~~
    ///
    ///
    ///Generates JSON
    ///
    ///~~~{.json}
    ///
    ///	{
    ///	    "type": "message/card.signin",
    ///	    "attachments": [
    ///	    {
    ///	        "contentType": "application/vnd.microsoft.card.signin",
    ///	        "content":
    ///	        {
    ///	            "text": "You need to authorize me",
    ///	            "buttons": [
    ///	            {
    ///	                "type": "signin",
    ///	                "title": "Connect",
    ///	                "value": "https://oauthbot.azurewebsites.net/?LinkId=RegistrationLink_1HEuPKSba6cwj9Sn6j6U8MOpPuAKgNZSt4mJ28f3bXJDN%2B4fWTV8SmC6jFsJcq0yYHLa5QYeuKKvMKQNJ8UO%2Fz4jo4LajhN%2Bjc3W%2FpBP%2BiTGfziBEa%2B03TPS4YSLRrursLWSEdnKYk4AJY4EE6UGYiMNAlIb0HbvnICLuFHDwnI%3D"
    ///	            }
    ///	            ]
    ///	        }
    ///	    }
    ///	    ]
    ///}
    ///
    ///~~~
    ///
    ///
    ///\subsubsection carousel Carousel of Cards
    ///You can send multiple rich card attachments in a single message.On most channels they will be sent
    ///as multiple rich cards, but some channels(like Skype and Facebook) can render them as a carousel of rich cards.
    ///
    ///As the developer you have the abiltity to control whether the list is rendered as a carousel or a vertical list using the **AttachmentLayout** property.
    ///
    ///~~~{.cs}
    ///
    ///Activity replyToConversation = message.CreateReply("Should go to conversation, with a carousel");
    ///replyToConversation.Recipient = message.From;
    ///replyToConversation.Type = "message";
    ///replyToConversation.Attachments = new List<Attachment>();
    ///
    ///Dictionary<string, string> cardContentList = new Dictionary<string, string>();
    ///cardContentList.Add("PigLatin", "https://3.bp.blogspot.com/-7zDiZVD5kAk/T47LSvDM_jI/AAAAAAAAByM/AUhkdynaJ1Y/s200/i-speak-pig-latin.png");
    ///cardContentList.Add("Pork Shoulder", "https://3.bp.blogspot.com/-_sl51G9E5io/TeFkYbJ2lDI/AAAAAAAAAL8/Ug_naHX6pAk/s400/porkshoulder.jpg");
    ///cardContentList.Add("Bacon", "http://www.drinkamara.com/wp-content/uploads/2015/03/bacon_blog_post.jpg");
    ///
    ///foreach(KeyValuePair<string, string> cardContent in cardContentList)
    ///{
    ///    List<CardImage> cardImages = new List<CardImage>();
    ///cardImages.Add(new CardImage(url:cardContent.Value ));
    ///
    ///    List<CardAction> cardButtons = new List<CardAction>();
    ///
    ///CardAction plButton = new CardAction()
    ///{
    ///Value = $"https://en.wikipedia.org/wiki/{cardContent.Key}",
    ///Type = "openUrl",
    ///Title = "WikiPedia Page"
    ///};
    ///cardButtons.Add(plButton);
    ///
    ///    HeroCard plCard = new HeroCard()
    ///    {
    ///        Title = $"I'm a hero card about {cardContent.Key}",
    ///        Subtitle = $"{cardContent.Key} Wikipedia Page",
    ///        Images = cardImages,
    ///        Buttons = cardButtons
    ///    };
    ///
    ///Attachment plAttachment = plCard.ToAttachment();
    ///replyToConversation.Attachments.Add(plAttachment);
    ///}
    ///
    ///replyToConversation.AttachmentLayout = AttachmentLayoutTypes.Carousel;
    ///
    ///var reply = await connector.Conversations.SendToConversationAsync(replyToConversation);
    ///
    ///~~~
    ///
    ///~~~{.json}
    ///
    ///{
    ///"type": "message",
    ///"timestamp": "2016-06-20T16:04:22.8213061Z",
    ///"from": {
    ///    "id": "1a392f33",
    ///    "name": "piglatinbotv3d"
    ///},
    ///"conversation": {
    ///    "id": "8a684db8",
    ///    "name": "Conv1"
    ///},
    ///"recipient": {
    ///    "id": "2c1c7fa3",
    ///    "name": "User1"
    ///},
    ///"attachmentLayout": "carousel",
    ///"text": "Should go to conversation, with a carousel",
    ///"attachments": [
    ///    {
    ///    "contentType": "application/vnd.microsoft.card.hero",
    ///    "content": {
    ///        "title": "I'm a hero card about Pig Latin",
    ///        "subtitle": "PigLatin Wikipedia Page",
    ///        "images": [
    ///        {
    ///            "url": "https://3.bp.blogspot.com/-7zDiZVD5kAk/T47LSvDM_jI/AAAAAAAAByM/AUhkdynaJ1Y/s200/i-speak-pig-latin.png"
    ///        }
    ///        ],
    ///        "buttons": [
    ///        {
    ///            "type": "openUrl",
    ///            "title": "WikiPedia Page",
    ///            "value": "https://en.wikipedia.org/wiki/{cardContent.Key}"
    ///        }
    ///        ]
    ///    }
    ///    },
    ///    {
    ///    "contentType": "application/vnd.microsoft.card.hero",
    ///    "content": {
    ///        "title": "I'm a hero card about pork shoulder",
    ///        "subtitle": "Pork Shoulder Wikipedia Page",
    ///        "images": [
    ///        {
    ///            "url": "https://3.bp.blogspot.com/-_sl51G9E5io/TeFkYbJ2lDI/AAAAAAAAAL8/Ug_naHX6pAk/s400/porkshoulder.jpg"
    ///        }
    ///        ],
    ///        "buttons": [
    ///        {
    ///            "type": "openUrl",
    ///            "title": "WikiPedia Page",
    ///            "value": "https://en.wikipedia.org/wiki/{cardContent.Key}"
    ///        }
    ///        ]
    ///    }
    ///    },
    ///    {
    ///    "contentType": "application/vnd.microsoft.card.hero",
    ///    "content": {
    ///        "title": "I'm a hero card about Bacon",
    ///        "subtitle": "Bacon Wikipedia Page",
    ///        "images": [
    ///        {
    ///            "url": "http://www.drinkamara.com/wp-content/uploads/2015/03/bacon_blog_post.jpg"
    ///        }
    ///        ],
    ///        "buttons": [
    ///        {
    ///            "type": "openUrl",
    ///            "title": "WikiPedia Page",
    ///            "value": "https://en.wikipedia.org/wiki/{cardContent.Key}"
    ///        }
    ///        ]
    ///    }
    ///    }
    ///],
    ///"replyToId": "80514c1703e047a3b42aa8eff22367c6"
    ///}   
    ///
    ///~~~
    ///
    ///
    ///\subsection customcapabilities Custom Channel Capabilities
    ///
    ///\subsubsection channeldataproperty Activity.ChannelData Property
    ///If you want to be able to take advantage of special features or concepts for a channel we provide a way for you to send native
    ///metadata to that channel giving you much deeper control over how your bot interacts on a channel.The way you do this is to pass
    ///extra properties via the *ChannelData* property.
    ///
    ///>NOTE: You do not need to use this feature unless you feel the need to access functionality not provided by the normal Activity.
    ///
    ///\subsubsection customemailmessages Custom Email Messages
    ///The Email channel optionally supports the ability to send custom properties to take advantage of what you can do with email.
    ///
    ///
    ///When you receive a message via email the channelData will have metadata from the source message.
    ///
    ///
    ///When you reply to a message via the email channel you can specify the following properties:
    ///
    ///|**Property** |**Description**
    ///|---------|  -----
    ///|*HtmlBody*   | The HTML to use for the body of the message
    ///|*Subject*    | The subject to use for the message
    ///|*Importance* | The importance flag to use for the message (Low/Normal/High)
    ///
    ///
    ///Example Message:
    ///
    ///~~~{.json}
    ///
    ///    {
    ///        "type": "message",
    ///        "locale": "en-Us",
    ///        "channelID":"email",
    ///        "from": { "id":"mybot@gmail.com", "name":"My bot"},
    ///        "recipient": { "id":"joe@gmail.com", "name":"Joe Doe"},
    ///        "conversation": { "id":"123123123123", "topic":"awesome chat" },
    ///        "channelData":
    ///        {
    ///            "htmlBody" : "<html><body style = \"font-family: Calibri; font-size: 11pt;\" >This is more than awesome</body></html>",
    ///            "subject":"Super awesome mesage subject",
    ///            "importance":"high"
    ///        }
    ///    }
    ///
    ///~~~
    ///
    ///
    ///\subsubsection customslackmessages Custom Slack Messages
    ///    Slack supports the ability to create full fidelity slack cards using their message attachments property.The slack
    ///channel gives access to this via the channelData field.
    ///
    ///> See[Slack Message Attachments](https://api.slack.com/docs/attachments) for a description of all of the properties
    ///that go into the attachments property
    ///
    ///|**Property** | **Description**
    ///|---------|  -----
    ///|*attachments*  | An array of attachments *See[Slack Message Attachments](https://api.slack.com/docs/attachments)*
    ///|*unfurl_links*  | true or false *See[Slack unfurling](https://api.slack.com/docs/unfurling)*
    ///|*unfurl_media*  | true or false *See[Slack unfurling](https://api.slack.com/docs/unfurling)*
    ///
    ///When slack processes a bot connector message it will use the normal message properties to create a slack message, and
    ///then it will merge in the values from the *channelData* property if they are provided by the sender.
    ///
    ///Example Message:
    ///
    ///~~~{.json}
    ///    {
    ///        "type": "message",
    ///        "locale": "en-Us",
    ///        "channelID":"slack",
    ///        "text": "This is a test",
    ///        "conversation": { "id":"123123123123", "topic":"awesome chat" },
    ///        "from": { "id":"12345", "name":"My Bot"},
    ///        "recipient": { "id":"67890", "name":"Joe Doe"},
    ///        "channelData":
    ///        {
    ///            "attachments": [
    ///                {
    ///                    "fallback": "Required plain-text summary of the attachment.",
    ///
    ///                    "color": "#36a64f",
    ///
    ///                    "pretext": "Optional text that appears above the attachment block",
    ///
    ///                    "author_name": "Bobby Tables",
    ///                    "author_link": "http://flickr.com/bobby/",
    ///                    "author_icon": "http://flickr.com/icons/bobby.jpg",
    ///
    ///                    "title": "Slack API Documentation",
    ///                    "title_link": "https://api.slack.com/",
    ///
    ///                    "text": "Optional text that appears within the attachment",
    ///
    ///                    "fields": [
    ///                        {
    ///                            "title": "Priority",
    ///                            "value": "High",
    ///                            "short": false
    ///                        }
    ///                    ],
    ///
    ///                    "image_url": "http://my-website.com/path/to/image.jpg",
    ///                    "thumb_url": "http://example.com/path/to/thumb.png"
    ///                }
    ///            ],
    ///            "unfurl_links":false,
    ///            "unfurl_media":false,
    ///        },
    ///        ...
    ///    }
    ///~~~
    ///
    ///
    ///\subsubsection customfacebookmessages Custom Facebook Messages
    ///The Facebook adapter supports sending full attachments via the channelData field.This allows you to do anything
    ///natively that Facebook supports via the attachment schema, such as reciept.
    ///
    ///|**Property** | **Description**
    ///|---------|  -----
    ///|* notification_type*  | Push notification type: REGULAR, SILENT_PUSH, NO_PUSH
    ///|* attachment*  | A Facebook formatted attachment * See[Facebook Send API Reference](https://developers.facebook.com/docs/messenger-platform/send-api-reference#guidelines)*
    ///
    ///Example Message:
    ///
    ///~~~{.json}
    ///
    ///    {
    ///        "type": "message",
    ///        "locale": "en-Us",
    ///        "text": "This is a test",
    ///        "channelID":"facebook", 
    ///        "conversation": { "id":"123123123123", "topic":"awesome chat" },
    ///        "from": { "id":"12345", "name":"My Bot"},
    ///        "recipient": {  "id":"67890", "name":"Joe Doe"},
    ///        "channelData":
    ///        {
    ///            "notification_type" : "NO_PUSH",
    ///            "attachment":
    ///            {
    ///                "type":"template",
    ///                "payload":
    ///                {
    ///                    "template_type":"receipt",
    ///                    "recipient_name":"Stephane Crozatier",
    ///                    "order_number":"12345678902",
    ///                    "currency":"USD",
    ///                    "payment_method":"Visa 2345",
    ///                    "order_url":"http://petersapparel.parseapp.com/order?order_id=123456",
    ///                    "timestamp":"1428444852",
    ///                    "elements":
    ///                    [
    ///                        {
    ///                            "title":"Classic White T-Shirt",
    ///                            "subtitle":"100% Soft and Luxurious Cotton",
    ///                            "quantity":2,
    ///                            "price":50,
    ///                            "currency":"USD",
    ///                            "image_url":"http://petersapparel.parseapp.com/img/whiteshirt.png"
    ///                        },
    ///                        {
    ///                            "title":"Classic Gray T-Shirt",
    ///                            "subtitle":"100% Soft and Luxurious Cotton",
    ///                            "quantity":1,
    ///                            "price":25,
    ///                            "currency":"USD",
    ///                            "image_url":"http://petersapparel.parseapp.com/img/grayshirt.png"
    ///                        }
    ///                    ],
    ///                    "address":
    ///                    {
    ///                        "street_1":"1 Hacker Way",
    ///                        "street_2":"",
    ///                        "city":"Menlo Park",
    ///                        "postal_code":"94025",
    ///                        "state":"CA",
    ///                        "country":"US"
    ///                    },
    ///                    "summary":
    ///                    {
    ///                        "subtotal":75.00,
    ///                        "shipping_cost":4.95,
    ///                        "total_tax":6.19,
    ///                        "total_cost":56.14
    ///                    },
    ///                    "adjustments":
    ///                    [
    ///                        {
    ///                            "name":"New Customer Discount",
    ///                            "amount":20
    ///                        },
    ///                        {
    ///                            "name":"$10 Off Coupon",
    ///                            "amount":10
    ///                        }
    ///                    ]
    ///                }
    ///            }
    ///        }
    ///    }
    ///~~~
    ///
    ///\subsubsection customtelegrammessages Custom Telegram Messages
    ///
    ///The Telegram channel supports calling Telegram Bot API methods via the channelData field.This allows your bot to perform Telegram-specific actions, such as sharing a voice memo, or a sticker.
    ///
    ///|**Property** | **Description**
    ///|---------|  -----
    ///|*method* | The Telegram Bot API method to call.See below for supported methods.
    ///|*parameters* | Associative array containing method parameters.Parameters are method-specific.
    ///
    ///>See the [Telegram Bot API Documentation](https://core.telegram.org/bots/api) for a description of all available methods, parameters, and types.
    ///
    ///
    ///Special Notes:
    ///
    ///1. The `chat_id` parameter is common to all Telegram methods.If not provided, the framework will fill in this value for you.
    ///2. The Telegram channel expresses Telegram's `InputFile` type differently than the way it appears in the [Telegram Bot API](https://core.telegram.org/bots/api#inputfile). Rather than sending the file contents, your bot should pass the file's `url` and `mediaType`. This is shown in the example message below.
    ///3. When your bot receives a Connector message from the Telegram channel, the original Telegram message will be present in the channelData field.
    ///
    ///Example Message:
    ///
    ///~~~{.json}
    ///{
    ///    "type": "message",
    ///    "locale": "en-Us",
    ///    "channelID":"telegram", 
    ///    "from": { "id":"12345", "name":"My Bot"},
    ///    "recipient": { "id":"67890"}, "name":"Joe Doe"},
    ///    "conversation": { "id":"123123123123", "topic":"awesome chat" },
    ///    "channelData":
    ///    {
    ///        "method": "sendSticker",
    ///        "parameters":
    ///        {
    ///            "sticker":
    ///            {
    ///                "url": "https://upload.wikimedia.org/wikipedia/commons/3/33/LittleCarron.gif",
    ///                "mediaType": "image/gif"
    ///            }
    ///        }
    ///    }
    ///}
    ///~~~
    ///
    ///You may pass multiple Telegram methods as an array:
    ///
    ///~~~{.json}
    ///{
    ///    "type": "message",
    ///    "locale": "en-Us",
    ///    "channelID":"telegram", 
    ///    "from": { "id":"12345", "name":"My Bot"},
    ///    "recipient": { "id":"67890"}, "name":"Joe Doe"},
    ///    "conversation": { "id":"123123123123", "topic":"awesome chat" },
    ///    "channelData":
    ///    [
    ///        {
    ///            "method": "sendSticker",
    ///            "parameters":
    ///            {
    ///                "sticker":
    ///                {
    ///                    "url": "http://www.gstatic.com/webp/gallery/1.webp",
    ///                    "mediaType": "image/webp"
    ///                }
    ///            }
    ///        },
    ///        {
    ///            "method": "sendMessage",
    ///            "parameters":
    ///            {
    ///                "text": "<b>This message is HTML-formatted.</b>",
    ///                "parse_mode": "HTML"
    ///            }
    ///        }
    ///    ]
    ///}
    ///~~~
    ///
    ///
    ///Supported Methods:
    ///
    ///
    ///|---------|---------|---------
    ///| [sendMessage](https://core.telegram.org/bots/api#sendmessage) | [forwardMessage](https://core.telegram.org/bots/api#forwardmessage) | [sendPhoto](https://core.telegram.org/bots/api#sendphoto)
    ///| [sendAudio](https://core.telegram.org/bots/api#sendaudio) | [sendDocument](https://core.telegram.org/bots/api#senddocument) | [sendSticker](https://core.telegram.org/bots/api#sendsticker)
    ///| [sendVideo](https://core.telegram.org/bots/api#sendvideo) | [sendVoice](https://core.telegram.org/bots/api#sendvoice) | [sendLocation](https://core.telegram.org/bots/api#sendlocation)
    ///| [sendVenue](https://core.telegram.org/bots/api#sendvenue) | [sendContact](https://core.telegram.org/bots/api#sendcontact) | [sendChatAction](https://core.telegram.org/bots/api#sendchataction)
    ///| [kickChatMember](https://core.telegram.org/bots/api#kickchatmember) | [unbanChatMember](https://core.telegram.org/bots/api#unbanchatmember) | [answerInlineQuery](https://core.telegram.org/bots/api#answerinlinequery)
    ///| [editMessageText](https://core.telegram.org/bots/api#editmessagetext) | [editMessageCaption](https://core.telegram.org/bots/api#editmessagecaption) | [editMessageReplyMarkup](https://core.telegram.org/bots/api#editmessagereplymarkup)
    ///
    ///\subsubsection customkikmessages Custom Kik Messages
    ///
    ///The Kik adapter supports sending native Kik messages via the channelData field.This allows you to do anything
    ///natively that Kik supports.
    ///
    ///|**Property** | **Description**
    ///|---------|  -----
    ///|*messages*  | An array of messages. *See[Kik Messages](https://dev.kik.com/#/docs/messaging#message-formats)*
    ///
    ///Example Message:
    ///
    ///~~~{.json} 
    ///{
    ///    "type": "message",
    ///    "locale": "en-Us",
    ///    "channelID":"kik", 
    ///    "from": { "id":"12345", "name":"My Bot"},
    ///    "recipient": { "id":"67890"}, "name":"Joe Doe"},
    ///    "conversation": { "id":"123123123123", "topic":"awesome chat" },
    ///	"channelData": {
    ///		"messages": [
    ///		{
    ///			"chatId": "c6dd81652051b8f02796e152422cce678a40d0fb6ad83acd8f91cae71d12f1e0",
    ///			"type": "link",
    ///			"to": "kikhandle",
    ///			"title": "My Webpage",
    ///			"text": "Some text to display",
    ///			"url": "http://botframework.com",
    ///			"picUrl": "http://lorempixel.com/400/200/",
    ///			"attribution": {
    ///				"name": "My App",
    ///				"iconUrl": "http://lorempixel.com/50/50/"
    ///			 },
    ///			"noForward": true,
    ///			"kikJsData": {
    ///			  "key": "value"
    ///			}
    ///		}
    ///		]
    ///	}
    ///}
    ///~~~
    ///
    ///\subsection trackingstate Tracking Bot State
    ///
    ///
    ///If a bot is implemented in a stateless way then it is very easy to scale your bot to handle load. 
    ///
    ///Unfortunately a bot is all about conversations and as soon as you introduce conversation into a bot then
    ///your bot needs to track state in order to remember things like "what was the last question I asked them?". 
    ///
    ///We make it easy for the bot developer to track this information because we provide contextual information that
    ///they can use to store data in their own store or database.
    ///
    ///In addition, we provide a simple cookie like system for tracking state that makes it super easy for most bots to not have 
    ///to worry about having their own store.
    ///
    ///\subsubsection contextualproperties Contextual properties
    ///Every Activity has several properties which are useful for tracking state.
    ///
    ///|**Property**                    | **Description**                                        | **Use cases**                                                
    ///|----------------------------|----------------------------------------------------|----------------------------------------------------------
    ///|**From.Id**                     | An ID for the user across all channels and conversations| Remembering context for a user
    ///|**ChannelId + From.Id** | A Users's address on a channel (ex: email address) | Remembering context for a user on a channel                 
    ///|**Conversation**              | A unique id for a conversation                     | Remembering context all users in a conversation    
    ///|**From.Id + Conversation**    | A user in a conversation                           | Remembering context for a user in a conversation   
    ///
    ///You can use these keys to store information in your own database as appropriate to your needs.
    ///
    ///\subsubsection botstateapi Bot State API
    ///After writing a bunch of bots we came to the realization that many bots have pretty simple needs for tracking state. 
    ///To support this case we have state objects exopsed by the Bot State API which can be used by the developer for simple user & conversation keyed storage.
    ///
    ///Here are the Bot State Methods 
    ///
    ///|**Method**                            | **Description**                                                | **Use cases**                                                
    ///|------------------------------------|------------------------------------------------------------|----------------------------------------------------------
    ///|**botState.GetUserData**                 | an object saved based on the channel and from.Id                       | Remembering context object with a user
    ///|**botState.GetConversationData**         | an object saved based on the channel and conversationId                | Remembering context object with a conversation
    ///|**botState.GetPrivateConversationData**| an object saved based on the channel, from.Id & conversationId      | Remembering context object with a person in a conversation
    ///|**botState.SetUserData**                 | an object saved based on the channel and from.Id                      | Remembering context object with a user
    ///|**botState.SetConversationData**         | an object saved based on the channel and conversationId                 | Remembering context object with a conversation
    ///|**botState.SetPrivateConversationData**| an object saved based on the channel, from.Id & conversationId      | Remembering context object with a person in a conversation
    ///|**botState.DeleteStateForUser**         | deletes all user data based on the from.Id  | When the user requests data be deleted or removes the bot contact
    ///
    ///When your bot sends a reply you  simply set your object in one of the BotData records properties and it will be persisted and
    ///played back to you on future messages when the context is the same. 
    ///
    ///Example of setting the data for the sender of an incoming message:
    ///
    ///~~~{.cs}
    ///
    ///    StateClient sc = new StateClient(new Microsoft.Bot.Connector.MicrosoftAppCredentials());
    ///    BotState botState = new BotState(sc);
    ///    botData = new BotData(eTag: "*");
    ///    botData.SetProperty("UserData", myUserData);
    ///    var response = await botState.SetUserDataAsync(incomingMessage.ChannelId, incomingMessage.From.Id, botData);
    ///    
    ///~~~
    ///
    ///When a new message comes in, you can retrieve data from GetPrivateConversationData() which will have your conversation state for the user.
    ///
    ///~~~{.cs}
    ///
    ///    pigLatinBotUserData addedUserData = new pigLatinBotUserData();
    ///    BotData botData = new BotData();
    ///    try
    ///    {
    ///        botData = (BotData)await botState.GetUserDataAsync(message.ChannelId, message.From.Id);
    ///    }
    ///    catch (Exception e)
    ///    {
    ///        if (e.Message == "Resource not found")
    ///        { // No data was stored for that user }
    ///        else
    ///            throw e;
    ///    }
    ///    myUserData = botData.GetProperty<myUserData>("UserData") ?? new myUserData();
    ///
    ///~~~
    ///
    ///\subsubsection concurrency Concurrency
    ///When these botData objects are being setthey are not able to be stored
    ///in a way which guarentees you won't overwrite data from another overlapping storage operation from your bot
    ///
    ///For many bots which have low load or simple sequential conversations with non-overlapping messages
    /// the convenience of just storing your state inline is worth the possibility of stomping on a
    ///previous message.   
    ///
    ///Other bots can are sensitive to data getting stomped and desire a more reliable storage system.  the ETag can be used to help your bot manage concurrency.
    ///
    ///Or you can simply use the userId and conversationId to store you own data in your own database.
    ///
    ///
    ///Example of using the REST API client library:
    ///~~~{.cs}
    ///    var client = new ConnectorClient();
    ///    try
    ///    {
    ///        // get the user data object
    ///        var userData = await botState.GetUserDataAsync(botId: message.Recipient.Id, userId: message.From.Id);
    ///  
    ///        // modify it...
    ///        userData.Data = ...modify...;
    ///    
    ///        // save it
    ///        await botState.SetUserDataAsync(botId: message.Recipient.Id, userId: message.From.Id, userData);
    ///    }
    ///    catch(HttpOperationException err)
    ///    {
    ///        // handle precondition failed error if someone else has modified your object
    ///    }
    ///~~~
    ///
    ///\section Configure
    ///\subsection activitytypes Activity Types
    ///
    ///Your bot's endpoint will recieve Activity objects that are communications to the bot.
    ///There more than one type of Activity which are used to convey system operations or channel system operations
    ///to the bot.  They exist to give the bot information about the state of the channel and the opportunity to respond
    ///to them.
    ///
    ///This table gives you basic overview of the Activity types:
    ///
    ///| **ActivityType**           | **Description**                                                        | **V1 Message Type** 
    ///| --- ----------------------|-------------------------------------------------------------------------|--------------|
    ///| **message**                   | a simple communication between a user <-> bot                                 | message      
    ///| **deleteUserData**            | A compliance request from the the user to delete any profile / user data      | deleteUserData
    ///| **conversationUpdate**        | your bot was added to a conversation or other conversation metadata changed   | Bot/UserAddedTo/RemovedFromConversation                
    ///| **contactRelationUpdate**     | The bot was added to or removed from a user's contact list                    | n/a         
    ///| **typing**                    | The user or bot on the other end of the conversation is typing                | n/a
    ///
    ///\subsubsection message 
    ///> a simple communication between a user <-> bot
    ///
    ///Each Activity being routed through the connector has a Type field. Primarily, these will be of type message unless they are system notifications for the Bot.
    ///
    ///\subsubsection deleteUserData
    ///>A compliance request from the the user to delete any profile / user data 
    ///
    ///Bots have access to users conversation data.  Many countries have legal requirements that a user
    ///has the ability to request their data to be dropped.  If you receive a message of this type
    ///you should remove any personally identifyable information (PII) for the user.  
    ///
    ///\subsubsection conversationUpdate
    ///> the membership or metadata of a conversation involving the bot changed
    ///
    ///Your bot often needs to know when the state of the conversation it's in has changed.  This may represent the bot being added to the conversation, or a person added or remove from the chat.  When these changes happen, your bot will receive a conversationUpdate Activity.
    ///
    ///In this event, the membersAdded and membersRemoved lists will contain the changes to the conversation since the last event. One of the members may be the Bot; which can be tested for by comparing the membersAdded\[n].id field with the recipient.id field. 
    ///
    ///The message From field will have an Address of **$service$**
    ///
    ///conversationUpdate is a great opportunity for the Bot to send welcome messages to users.
    ///
    ///\subsubsection contactRelationUpdate
    ///> The bot was added to or removed from a user's contact list
    ///
    ///For some channels your bot can be a member of the user's contact list on that chat service (Skype for example). In the event the channel supports this action, it can notify the Bot that this has occurred. When this event is delivered, the **Action** property will indicate whether the operation was an **add** or a **remove**.
    ///
    ///\subsubsection typing
    ///> A message that indicates that the user or Bot is typing
    ///
    ///Typing is an indicator of activity on the other side of the conversation.  Generally it's used by Bots to cover "dead air" while the bot is fulfilling a request of some sort.  The Bot may also receive Typing messages from the user, for whatever purposes it might find useful.
    ///
    ///
    ///\subsection botoptions Bot Options
    ///
    /// 
    ///When you configure your bot there are several optional features you can select which are described in more depth here.
    ///
    ///\subsubsection listeningspeaking Listening and speaking modes
    ///
    ///### Listen to all messages
    ///* **Option is off** *(default)*-  when this is option is off, bots are in **Group conversation mode**. 
    ///* **Option is on**-  the bot will receive ALL messages in the conversation.  It is up to the bot
    /// to make sure that it's interaction is appropriate for the conversation.
    ///
    ///### Group conversation mode 
    ///1. if bot is in a conversation which is only the user and the bot, all messages will be sent to the bot regardless of mentions.
    ///2. if in group conversation
    ///    * if a user mentions the bot then the message will be sent to the bot and the user and bot will be in an *Active Conversation*
    ///    * While in *Active Conversation* all future messages from that user will be sent to the bot regardless of mentions until
    ///        * the user says a goodbye statement (like 'see you later', or 'goodbye', etc.) 
    ///        * 5 minutes of inactivity pass
    ///
    ///\subsubsection publishdirectory Publish in Bot directory
    ///* **Off** *(default)*- Your bot will only be visible to you or to someone you give the link to your contact card to. 
    ///* **On**- Your bot will show up on the [Bot Gallery](https://bots.botframework.com)
    ///
    ///\subsection configurationconventions Configuration conventions
    ///\subsubsection serialization Serialization
    ///All of the objects described use lower-camel casing on the wire.  The C# nuget library uses
    ///strongly typed names that are pascal cased. Our documentation sometimes will use one or the
    ///other but they are interchangable.
    ///
    ///| **C# property** | wire serialization | javascript name |
    ///| ----| ---- | ---- |
    ///| Conversation | conversation | conversation|
    ///
    ///
    ///\subsubsection exampleserialization Example serialization
    ///~~~{.json}
    ///
    ///{
    ///  "type": "Message",
    ///  "conversation": {
    ///    "Id": "GZxAXM39a6jdG0n2HQF5TEYL1vGgTG853w2259xn5VhGfs"
    ///  },
    ///  "timestamp": "2016-03-22T04:19:11.2100568Z",
    ///  "channelid": "skype",
    ///  "text": "You said:test",
    ///  "attachments": [],
    ///  "from": {
    ///    "name": "Test Bot",
    ///    "id": "MyTestBot",
    ///  },
    ///  "recipient": {
    ///    "name": "tom",
    ///    "id": "1hi3dbQ94Kddb",
    ///  },
    ///  "locale": "en-Us",
    ///  "replyToId": "7TvTPn87HlZ",
    ///  "entities": [],
    ///}
    ///
    ///~~~
    ///
    ///\subsection securing Securing your bot
    ///
    ///Developers should ensure that their bot's endpoint can only be called by the Bot Connector.
    ///
    ///To do this you should
    ///
    ///* Configure your endpoint to only use HTTPS
    ///* Use the Bot Framework SDK's authentication: MicrosoftAppId Password: MicrosoftAppPassword 
    ///
    ///\subsubsection botauthattributes BotAuthentication Attribute
    ///To make it easy for our C# developers we have created an attribute which does this for your method or controller.
    ///
    ///To use with the AppId and AppSecret coming from the web.config
    ///
    ///~~~{.cs}
    ///    [BotAuthentication()]
    ///    public class MessagesController : ApiController
    ///    {
    ///    }
    ///~~~
    ///
    ///Or you can pass in the appId appSecret to the attribute directly:
    ///
    ///~~~{.cs}
    ///    [BotAuthentication("..MicrosoftappId...","...MicrosoftappSecret...")]
    ///    public class MessagesController : ApiController
    ///    {
    ///    }
    ///~~~
    ///
    ///\subsubsection implementingvalidation Implementing your own caller validation
    ///[[ Content coming soon ]]
    ///
}
