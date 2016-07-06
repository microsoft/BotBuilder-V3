namespace Microsoft.Bot.Builder.Calling
{
    /// \page calling %Calling 
    ///\section tutorialivr Tutorial: Interactive Voice Response (IVR) Bot
    ///
    ///Let's say you want to build an [IVR](https://en.wikipedia.org/wiki/Interactive_voice_response) bot to automate common tasks for incoming customer calls.
    ///
    ///\subsection ivrrequirements Requirements
    ///
    ///If you don't already have them, download:
    ///
    ///* Visual Studio 2013 or later
    ///    Make sure you have [.Net Framework 4.5.2](https://www.microsoft.com/en-us/download/details.aspx?id=42637) or later and [Azure SDK for .NET](https://azure.microsoft.com/en-gb/documentation/articles/dotnet-sdk/).
    ///* A free Azure account or [an active Visual Studio subscription](https://azure.microsoft.com/pricing/member-offers/msdn-benefits-details/?WT.mc_id=A261C142F)
    ///    You get \$200 in free credits with your account. If you've used up your credits, you can still use the free services and features (such as the Web Apps in the Azure App Service).
    ///
    /// <div class="docs-text-note"><strong>Note:</strong> If you're using a different version of Visual Studio, your screens will look a little different.</div>
    ///
    ///\subsection ivrquickstarts Quick Starts
    ///
    ///If you don't feel like working through the tutorial, you can use the completed quick start solution:
    ///
    ///1.  Download the [completed quick start](https://microsoft.sharepoint.com/teams/SkypeSMS/Shared%20Documents/Agents/CallingQuickStartWebApp.zip) solution and unzip it.
    ///2.  Navigate to the IvrSample folder. While running VisualStudio from an Administrator account, open the .sln file.
    ///3.  Edit the application settings in appSettings section of Web.config file. Use the settings registered on the portal.  
    ///
    ///    You'll need to populate these settings:
    ///
    ///    -   ​Skype.Bots.Calling.CallbackUrl – Defines the URL that should be used for
    ///        callbacks from Calling Platform.
    ///
    ///        Example: if your route is set to v1/callback and your service is deployed to ivrtest.azurewebsites.net, then the URL configured on the portal will be *https://ivrtest.azurewebsites.net/v1/callback*
    ///
    ///4.  Modify the Routes for both methods in Controllers\CallingController.cs
    ///
    ///    -   **ProcessIncomingCall** - Route depends on the calling URL that was used during registration. For example if [https://ivrtest.azurewebsites.net/v1/calling/](https://ivrtest.cloudapp.net/v1/calling/) was configured the route should be **"v1/calling"**
    ///
    ///    -   **ProcessCallingEvent** - Route needs to match the URL specified in Service configuration. It will be **"v1/callback"** in our example
    ///
    ///You should have a working bot now.
    ///
    ///
    ///\subsection ivrstepbystep Step-by-step tutorial
    ///
    ///Let's build a Skype Bot. 
    ///
    ///Create an ASP.NET Web Application.   
    ///Don't know how?  Follow the steps in [Creating ASP.NET Web Projects in Visual Studio](http://www.asp.net/visual-studio/overview/2013/creating-web-projects-in-visual-studio). In "Select a template" window choose the "Empty" option. In the same window please choose "Web API" option from "Add folders and core references" menu.
    ///
    /// <div class="docs-text-note"><strong>Note:</strong> The Library Package Manager has been renamed. As of Visual Studio 2015 (and updated versions of 2013) it's called the NuGet Package Manager</div>
    ///
    ///
    ///\subsection ivrbotsettings Bot Settings
    ///
    ///It's time to configure your bot.  The settings are in Web.config file in *appSettings* section. See Configuration Options (below) for descriptions of what each setting does.
    ///
    ///~~~
    ///<configuration>
    ///  <appSettings>
    ///    <add key="Skype.Bots.Calling.CallbackUrl" value="https://put your service URL here/v1/callback" />
    ///  </appSettings>
    ///~~~
    ///
    ///
    ///\subsection ivraddskypebotsreference Add the Microsoft.Skype.Bots reference
    ///
    ///The C\# Botkit is provided as a Nuget package. To get started follow the next steps:
    ///
    ///1.  Download the [C\# botkit Nuget](https://microsoft.sharepoint.com/teams/SkypeSMS/Shared%20Documents/Agents/Microsoft.Skype.Bots.1.0.39.113.nupkg) package
    ///
    ///2.  Create a local [NuGet feed](https://docs.nuget.org/create/hosting-your-own-nuget-feeds) on your system and drop the package there.
    ///
    ///3.  Right click the your web project.
    ///
    ///4.  Select Manage NuGet Packages...
    ///
    ///5.  Select the local repository you created earlier.
    ///
    ///6.  Enter Microsoft.Skype.Bots
    ///
    ///7.  Click Install.
    ///
    /// ![](/en-us/images/ivr/image1.png)
    ///
    ///
    ///\subsection ivraddunityioccontainerreference Add reference to the Unity IoC container
    ///
    ///The Unity Container Library is a dependency injection container which interoperates with WebAPI. It resolves and injects reference for the core %Bot service instance.
    ///
    ///Let's add a reference:
    ///
    ///1.  Right click on your web project.
    ///
    ///2.  Select Manage NuGet Packages...
    ///
    ///3.  Enter Unity.WebAPI
    ///
    ///4.  Select the latest version.
    ///
    ///5.  Click Install.
    ///
    ///After the Unity container reference is added, ignore the instructions in the package's readme.txt file. We will setup the container differently. If it's automatically added, completely remove UnityConfig.cs class. If not, add reference to the .NET framework System.Runtime package.
    ///
    ///\subsection ivr implementation Implementation
    ///
    ///\subsubsection ivrimplementationcreatecontrollerclass Create Controller class
    ///
    ///The controller needs to inherit from BotController class. It needs to take *ICallingBotService* as contructor parameter.
    ///
    ///Let's assume that during registration the bot's URL for calling was set to [https://ivrtest.azurewebsites.net/v1/call](https://ivrtest.cloudapp.net/v1/call) and
    ///**Skype.Bots.Calling.CallbackUrl**  option was set to [https://ivrtest.azurewebsites.net/v1/callback](https://ivrtest.cloudapp.net/v1/callback).  
    ///
    ///Route attributes for methods:
    ///
    ///-   **ProcessIncomingCall** - Route depends on the calling URL that was used during registration, it's *"v1/call"* in our example
    ///
    ///-   **ProcessCallingEvent** - Route needs to match the URL specified in Service configuration. It needs to be **"v1/callback"** in our example
    ///
    ///
    ///It is strongly recommended to validate the incoming request – verify that it originates from %Bot Platform. To use this feature, you need to configure your Azure service to require client certificate. Follow "Configure Web App for Client Certificate Authentication" paragraph at
    ///[https://azure.microsoft.com/en-gb/documentation/articles/app-service-web-configure-tls-mutual-auth/](https://azure.microsoft.com/en-gb/documentation/articles/app-service-web-configure-tls-mutual-auth/) to turn on this feature. The feature requires at least B1 Basic pricing tier for
    ///Web App.
    ///
    ///
    ///The code of controller should look like:
    ///
    ///~~~{.cs}
    ///using System.Net;
    ///using System.Net.Http;
    ///using System.Threading.Tasks;
    ///using System.Web;
    ///using System.Web.Http;
    ///using Microsoft.Skype.Bots;
    ///using Microsoft.Skype.Bots.Filters;
    ///using Microsoft.Skype.Bots.Interfaces;
    ///
    ///namespace IvrSample.Controllers
    ///{
    ///    public class CallingController : BotController
    ///    {
    ///        private static readonly MtlsAuthenticationFromHeader MtlsAuth = new MtlsAuthenticationFromHeader();
    ///        public CallingController(ICallingBotService callingBotService) : base(callingBotService, "CallingQuickStart")
    ///        {
    ///        }
    ///
    ///        [Route("v1/callback")]
    ///        public override Task<HttpResponseMessage> ProcessCallingEventAsync()
    ///        {
    ///            if (!MtlsAuth.ValidateClientCertificate(Request.RequestUri.ToString(), Request.Headers, GetClientIp()))
    ///            {
    ///                return Task.FromResult(Request.CreateResponse(HttpStatusCode.Forbidden));
    ///            }
    ///
    ///            return base.ProcessCallingEventAsync();
    ///        }
    ///
    ///        [Route("v1/call")]
    ///        public override Task<HttpResponseMessage> ProcessIncomingCallAsync()
    ///        {
    ///            if (!MtlsAuth.ValidateClientCertificate(Request.RequestUri.ToString(), Request.Headers, GetClientIp()))
    ///            {
    ///                return Task.FromResult(Request.CreateResponse(HttpStatusCode.Forbidden));
    ///            }
    ///
    ///            return base.ProcessIncomingCallAsync();
    ///        }
    ///
    ///        private string GetClientIp()
    ///        {
    ///            const string MsHttpContextName = "MS_HttpContext";
    ///            if (Request.Properties.ContainsKey(MsHttpContextName))
    ///            {
    ///                return ((HttpContextWrapper)Request.Properties[MsHttpContextName]).Request.UserHostAddress;
    ///            }
    ///
    ///            return null;
    ///        }
    ///    }
    ///}
    ///~~~
    ///
    ///\subsubsection ivrimplementationdefinetextmessages Define text messages for menus
    ///
    ///We'll start with defining text messages that will be read and used for the menus.
    ///
    ///~~~{.cs}
    ///public static class IvrOptions
    ///{
    ///    internal const string WelcomeMessage = "Hello, you have successfully contacted XY internet service provider.";
    ///    internal const string MainMenuPrompt = "If you are a new client press 1, for technical support press 2, if you need information about payments press 3, to hear more about the company press 4. To repeat the options press 5.";
    ///    internal const string NewClientPrompt = "To check our latest offer press 1, to order a new service press 2. Press the hash key to return to the main menu";
    ///    internal const string SupportPrompt = "To check our current outages press 1, to contact the technical support consultant press 2. Press the hash key to return to the main menu";
    ///    internal const string PaymentPrompt = "To get the payment details press 1, press 2 if your payment is not visible in the system. Press the hash key to return to the main menu";
    ///    internal const string MoreInfoPrompt = "XY is the leading Internet Service Provider in Prague. Our company was established in 1995 and currently has 2000 employees.";
    ///    internal const string NoConsultants = "Unfortunately there are no consultants available at this moment. Please leave your name, and a brief message after the signal. You can press the hash key when finished. We will call you as soon as possible.";
    ///    internal const string Ending = "Thank you for leaving the message, goodbye";
    ///    internal const string Offer = "You can sign up for 100 megabit connection just for 10 euros per month till the end of month";
    ///    internal const string CurrentOutages = "There is currently 1 outage in Prague 5, we are working on fixing the issue";
    ///    internal const string PaymentDetailsMessage = "You should do the wire transfer till the 5th day of month to account number 3983815";
    ///} 
    ///~~~
    ///
    ///
    ///\subsubsection ivrimplementationmainbotclass Implementation of main Bot class
    ///
    ///The main %Bot class needs to accept IBotService as one of the arguments of its constructor. In our example we also wire the events there.
    ///
    ///~~~{.cs}
    ///private readonly ICallingBotService _callingBotService;
    ///
    ///public IvrBot(ICallingBotService callingBotService)
    ///{
    ///    if (callingBotService == null)
    ///        throw new ArgumentNullException(nameof(callingBotService));
    ///
    ///    _callingBotService = callingBotService;
    ///    _callingBotService.OnIncomingCallReceived += OnIncomingCallReceived;
    ///    _callingBotService.OnPlayPromptCompleted += OnPlayPromptCompleted;
    ///    _callingBotService.OnRecordCompleted += OnRecordCompleted;
    ///    _callingBotService.OnRecognizeCompleted += OnRecognizeCompleted;
    ///    _callingBotService.OnHangupCompleted += OnHangupCompleted;
    ///}
    ///~~~
    ///
    ///Before going further let's define the constants that we'll use for mapping of
    ///user's DTMF choices to our actions. The values of constants define the choices
    ///user needs to make to reach the particular option. First four items define the
    ///choices for the main menu. The following options define the choices for the
    ///second level menus.
    ///
    ///~~~{.cs}
    ///private const string NewClient = "1";
    ///private const string Support = "2";
    ///private const string Payments = "3";
    ///private const string MoreInfo = "4";
    ///private const string NewClientOffer = "1";
    ///private const string NewClientOrder = "2";
    ///private const string SupportOutages = "1";
    ///private const string SupportConsultant = "2";
    ///private const string PaymentDetails = "1";
    ///private const string PaymentNotVisible = "2";
    ///~~~
    ///
    ///
    ///During the call our bot needs to remember the choices that the user has made. In the presented scenario the important choice is the item chosen from the main menu. For example, if the user chooses the Payments Support menu (he presses the '3' button) and then presses key '1' we know that he wants to reach
    ///*PaymentDetails* section. We'll use a helper class to keep the state:
    ///
    ///~~~{.cs}
    ///private class CallState
    ///{
    ///    public string InitiallyChosenMenuOption { get; set; }
    ///}
    ///~~~
    ///
    ///We'll use the dictionary to keep the state information. We will use the Call Id as the key.
    ///
    ///~~~{.cs}
    ///private readonly Dictionary<string, CallState> _callStateMap = new Dictionary<string, CallState>();
    ///~~~
    ///
    ///
    ///\subsubsection implementationhelpermethods Let's also define the helper methods that we're going to use in the code.
    ///
    ///The first method creates a simple Action that reads the provided text.
    ///
    ///~~~{.cs}
    ///private static PlayPrompt GetPromptForText(string text)
    ///{
    ///    var prompt = new Prompt { Value = text, Voice = VoiceGender.Male };
    ///
    ///    return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
    ///}
    ///~~~
    ///
    ///Next method automates the creation of Recognize action. When this action is sent to Calling Platform the *textToBeRead* is read and the user can use the numpad to make a choice. The available options are defined by *numberOfOptions* parameter.
    ///
    ///For example *CreateIvrOptions("test", 3, true)* will allow the user to choose one from *{'1', '2', '3', '\#'}* options*.*
    ///
    ///~~~{.cs}
    ///private static Recognize CreateIvrOptions(string textToBeRead, int numberOfOptions, bool includeBack)
    ///{
    ///    if (numberOfOptions > 9)
    ///        throw new Exception("too many options specified");
    ///
    ///    var id = Guid.NewGuid().ToString();
    ///    var choices = new List<RecognitionOption>();
    ///
    ///    for (int i = 1; i <= numberOfOptions; i++)
    ///    {
    ///        choices.Add(new RecognitionOption { Name = Convert.ToString(i), DtmfVariation = (char)('0' + i) });
    ///    }
    ///
    ///    if (includeBack)
    ///        choices.Add(new RecognitionOption { Name = "#", DtmfVariation = '#' });
    ///
    ///    var recognize = new Recognize
    ///        {
    ///            OperationId = id,
    ///            PlayPrompt = GetPromptForText(textToBeRead),
    ///            BargeInAllowed = true,
    ///            Choices = choices
    ///        };
    ///
    ///    return recognize;
    ///}
    ///~~~
    ///
    ///Below method sets up the recording for the user.
    ///
    ///~~~{.cs}
    ///private static void SetupRecording(Workflow workflow)
    ///{
    ///    var id = Guid.NewGuid().ToString();
    ///    var prompt = GetPromptForText(IvrOptions.NoConsultants);
    ///
    ///    var record = new Record
    ///        {
    ///            OperationId = id,
    ///            PlayPrompt = prompt,
    ///            MaxDurationInSeconds = 10,
    ///            InitialSilenceTimeoutInSeconds = 5,
    ///            MaxSilenceTimeoutInSeconds = 2,
    ///            PlayBeep = true,
    ///            StopTones = new List<char> { '#' }
    ///        };
    ///
    ///    workflow.Actions = new List<ActionBase> { record };
    ///}
    ///~~~
    ///
    ///
    ///Next methods are responsible for creation of menus for different states.
    ///
    ///~~~{.cs}
    ///private void SetupInitialMenu(Workflow workflow)
    ///{
    ///    workflow.Actions = new List<ActionBase> { CreateIvrOptions(IvrOptions.MainMenuPrompt, 5, false) };
    ///}
    ///
    ///private Recognize CreateNewClientMenu()
    ///{
    ///    return CreateIvrOptions(IvrOptions.NewClientPrompt, 2, true);
    ///}
    ///
    ///private Recognize CreateSupportMenu()
    ///{
    ///    return CreateIvrOptions(IvrOptions.SupportPrompt, 2, true);
    ///}
    ///
    ///private Recognize CreatePaymentsMenu()
    ///{
    ///    return CreateIvrOptions(IvrOptions.PaymentPrompt, 2, true);
    ///}
    ///~~~
    ///
    ///\subsubsection ivrimplementationeventhandling Event handling
    ///
    ///When the incoming call is received it is Answered and the user is presented with
    ///welcome message. New state entry is created for him.
    ///
    ///~~~{.cs}
    ///private Task OnIncomingCallReceived(IncomingCallEvent incomingCallEvent)
    ///{
    ///    var id = Guid.NewGuid().ToString();
    ///    _callStateMap[incomingCallEvent.IncomingCall.Id] = new CallState();
    ///
    ///    incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase>
    ///        {
    ///            new Answer { OperationId = id },
    ///            GetPromptForText(IvrOptions.WelcomeMessage)
    ///        };
    ///
    ///    return Task.FromResult(true);
    ///}
    ///~~~
    ///
    ///
    ///Handler for the result of PlayPrompt action. The initial menu is presented to
    ///the user.
    ///
    ///~~~{.cs}
    ///private Task OnPlayPromptCompleted(PlayPromptOutcomeEvent playPromptOutcomeEvent)
    ///{
    ///    var callStateForClient = _callStateMap[playPromptOutcomeEvent.ConversationResult.Id];
    ///
    ///    callStateForClient.InitiallyChosenMenuOption = null;
    ///    SetupInitialMenu(playPromptOutcomeEvent.ResultingWorkflow);
    ///
    ///    return Task.FromResult(true);
    ///}
    ///~~~
    ///
    ///
    ///Handler for the result of Recognize option. It handles the value that user specified based on his initial choice (in case of the beginning of call the choice is *null*).
    ///
    ///
    ///~~~{.cs}
    ///private Task OnRecognizeCompleted(RecognizeOutcomeEvent recognizeOutcomeEvent)
    ///{
    ///    var callStateForClient = _callStateMap[recognizeOutcomeEvent.ConversationResult.Id];
    ///
    ///    switch (callStateForClient.InitiallyChosenMenuOption)
    ///    {
    ///        case null:
    ///            ProcessMainMenuSelection(recognizeOutcomeEvent, callStateForClient);
    ///            break;
    ///        case NewClient:
    ///            ProcessNewClientSelection(recognizeOutcomeEvent, callStateForClient);
    ///            break;
    ///        case Support:
    ///            ProcessSupportSelection(recognizeOutcomeEvent, callStateForClient);
    ///            break;
    ///        case Payments:
    ///            ProcessPaymentsSelection(recognizeOutcomeEvent, callStateForClient);
    ///            break;
    ///        default:
    ///            SetupInitialMenu(recognizeOutcomeEvent.ResultingWorkflow);
    ///            break;
    ///    }
    ///
    ///    return Task.FromResult(true);
    ///}
    ///~~~
    ///
    ///
    ///Below are the methods for analyzing the choice the user has made. After choosing the option from initial menu the choice is saved in the state object. If the user chooses to contact the consultant we set up the recording of message for him.
    ///
    ///
    ///~~~{.cs}
    ///private void ProcessMainMenuSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
    ///{
    ///    if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
    ///    {
    ///        SetupInitialMenu(outcome.ResultingWorkflow);
    ///        return;
    ///    }
    ///
    ///    switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
    ///    {
    ///        case NewClient:
    ///            callStateForClient.InitiallyChosenMenuOption = NewClient;
    ///            outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreateNewClientMenu() };
    ///            break;
    ///        case Support:
    ///            callStateForClient.InitiallyChosenMenuOption = Support;
    ///            outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreateSupportMenu() };
    ///            break;
    ///        case Payments:
    ///            callStateForClient.InitiallyChosenMenuOption = Payments;
    ///            outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreatePaymentsMenu() };
    ///            break;
    ///        case MoreInfo:
    ///            callStateForClient.InitiallyChosenMenuOption = MoreInfo;
    ///            outcome.ResultingWorkflow.Actions = new List<ActionBase> { GetPromptForText(IvrOptions.MoreInfoPrompt) };
    ///            break;
    ///        default:
    ///            SetupInitialMenu(outcome.ResultingWorkflow);
    ///            break;
    ///    }
    ///}
    ///
    ///private void ProcessNewClientSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
    ///{
    ///    if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
    ///    {
    ///        outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreateNewClientMenu() };
    ///        return;
    ///    }
    ///
    ///    switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
    ///    {
    ///        case NewClientOffer:
    ///            outcome.ResultingWorkflow.Actions = new List<ActionBase>
    ///                {
    ///                    GetPromptForText(IvrOptions.Offer),
    ///                    CreateNewClientMenu()
    ///                };
    ///            break;
    ///        case NewClientOrder:
    ///            SetupRecording(outcome.ResultingWorkflow);
    ///            break;
    ///        default:
    ///            callStateForClient.InitiallyChosenMenuOption = null;
    ///            SetupInitialMenu(outcome.ResultingWorkflow);
    ///            break;
    ///    }
    ///}
    ///
    ///private void ProcessSupportSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
    ///{
    ///    if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
    ///    {
    ///        outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreateSupportMenu() };
    ///        return;
    ///    }
    ///
    ///    switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
    ///    {
    ///        case SupportOutages:
    ///            outcome.ResultingWorkflow.Actions = new List<ActionBase>
    ///                {
    ///                    GetPromptForText(IvrOptions.CurrentOutages),
    ///                    CreateSupportMenu()
    ///                };
    ///            break;
    ///        case SupportConsultant:
    ///            SetupRecording(outcome.ResultingWorkflow);
    ///            break;
    ///        default:
    ///            callStateForClient.InitiallyChosenMenuOption = null;
    ///            SetupInitialMenu(outcome.ResultingWorkflow);
    ///            break;
    ///    }
    ///}
    ///
    ///private void ProcessPaymentsSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
    ///{
    ///    if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
    ///    {
    ///        outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreatePaymentsMenu() };
    ///        return;
    ///    }
    ///
    ///    switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
    ///    {
    ///        case PaymentDetails:
    ///            outcome.ResultingWorkflow.Actions = new List<ActionBase>
    ///                {
    ///                    GetPromptForText(IvrOptions.PaymentDetailsMessage),
    ///                    CreatePaymentsMenu()
    ///                };
    ///            break;
    ///        case PaymentNotVisible:
    ///            SetupRecording(outcome.ResultingWorkflow);
    ///            break;
    ///        default:
    ///            callStateForClient.InitiallyChosenMenuOption = null;
    ///            SetupInitialMenu(outcome.ResultingWorkflow);
    ///            break;
    ///    }
    ///}
    ///~~~
    ///
    ///Once the recording is finished the call is hang up after playing simple thank you message. In the presented sample the actual recording is not used but this part can be easily changed.  
    ///
    ///The stream containing the recording is available as *recordOutcomeEvent.RecordedContent* if the recording was successful (check the *recordOutcomeEvent.RecordOutcome.Outcome* flag).
    ///
    ///~~~{.cs}
    ///private Task OnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
    ///{
    ///    var id = Guid.NewGuid().ToString();
    ///
    ///    recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
    ///        {
    ///            GetPromptForText(IvrOptions.Ending),
    ///            new Hangup { OperationId = id }
    ///        };
    ///    recordOutcomeEvent.ResultingWorkflow.Links = null;
    ///    _callStateMap.Remove(recordOutcomeEvent.ConversationResult.Id);
    ///
    ///    return Task.FromResult(true);
    ///}
    ///~~~
    ///
    ///
    ///\subsection ivrsetuowebapi Setup the Web API configuration
    ///
    ///The next step is to configure the Asp.Net WebApi service to use the Route, Controller and IvrBot classes that we created so far.
    ///
    ///We're going to edit the App\_Start\WebApiConfig.cs file. You'll need to:
    ///
    ///-   Create a Unity Container.
    ///-   Register the needed types/instances.
    ///-   Set it as the DependencyResolver for the Web API.
    ///
    ///Your code should look like:
    ///
    ///~~~{.cs}
    ///using System;
    ///using System.Collections.Generic;
    ///using System.Linq;
    ///using System.Web.Http;
    ///using Microsoft.Practices.Unity;
    ///using Microsoft.Skype.Bots;
    ///using Microsoft.Skype.Bots.Interfaces;
    ///using Unity.WebApi;
    ///
    ///namespace CallingQuickStart
    ///{
    ///    public static class WebApiConfig
    ///    {
    ///        public static void Register(HttpConfiguration config)
    ///        {
    ///            // Web API routes
    ///            config.MapHttpAttributeRoutes();
    ///
    ///            var container = new UnityContainer();
    ///            RegisterTypes(container);
    ///            config.DependencyResolver = new UnityDependencyResolver(container);
    ///        }
    ///
    ///        private static void RegisterTypes(UnityContainer container)
    ///        {
    ///            CallingBotServiceSettings callingSettings = CallingBotServiceSettings.LoadFromCloudConfiguration();
    ///
    ///            var callingService = new CallingBotService(callingSettings);
    ///            var ivrBot = new IvrBot(callingService);
    ///
    ///            container.RegisterType<ICallingBotService, CallingBotService>();
    ///            container.RegisterInstance(callingService, new ContainerControlledLifetimeManager());
    ///            container.RegisterInstance(ivrBot, new ContainerControlledLifetimeManager());
    ///        }
    ///    }
    ///}
    ///~~~
    ///
    ///\subsection ivrfinishingup Finishing Up
    ///
    ///Compile the code, run it locally, and confirm no exceptions are being thrown.
    ///
    ///Once you're sure the bot works locally, enable SSL by doing the next steps: 
    ///
    ///-   Select the IvrSampleQuickStart project in Solution Explorer 
    ///
    ///-   Click on Properties tab 
    ///
    ///-   Change the SSL Enabled setting to True
    ///
    ///Now you are ready to deploy to Azure using the same steps as in the tutorial [Publish to an Azure Web App using Visual Studio](http://docs.asp.net/en/latest/tutorials/publish-to-azure-webapp-using-vs.html). If you encounter the issues on service startup, setting the "Remove additional files at destination" option in publishing settings may help.
    ///
    ///Add the bot to your contact list, and you're finished.
    ///
    ///\subsection ivrtestingngrok Testing with ngrok
    ///
    ///There are tools that can create a public url to your local webserver on your machine, e.g. [ngrok](https://ngrok.com/).  
    ///
    ///We'll show how you can test your bot running locally over skype.
    ///
    ///Visual Studio needs to be run as administrator. The client certificate validation that was placed in CallingController class needs to be temporarily disabled.
    ///
    ///You'll need to download ngrok and modify your bot's registration.  
    ///First step is to start ngrok on your machine and map it to a local port (in our
    ///example we'll use port 12345):
    ///
    ///~~~
    ///\> ngrok http 12345
    ///~~~
    ///
    ///This will create a new tunnel from a public url to localhost:12345 on your machine. After you start the command, you can see the status of the tunnel:
    ///
    ///~~~
    ///ngrok by \@inconshreveable (Ctrl+C to quit)
    ///
    ///Tunnel Status online  
    ///Update update available (version 2.0.24, Ctrl-U to update)  
    ///Version 2.0.19/2.0.25  
    ///Web Interface <http://127.0.0.1:4040>  
    ///Forwarding http://78191649.ngrok.io -\> localhost:12345  
    ///Forwarding https://78191649.ngrok.io -\> localhost:12345  
    ///
    ///Connections ttl opn rt1 rt5 p50 p90  
    ///            0 0 0.00 0.00 0.00 0.00
    ///~~~
    ///
    ///Notice the "Forwarding" lines, in this case you can see that ngrok created two endpoints for us <http://78191649.ngrok.io> and <https://78191649.ngrok.io> for http and https traffic.
    ///
    ///The next step is to configure IIS Express to run our service on the port we specified (12345).
    ///
    ///In Visual Studio please right click on the IvrSample project and choose Properties.
    ///
    ///Set the port that will be used by IIS Express for local runs as shown below (creation of virtual directory may be required) and save the changes.  
    ///
    /// ![](/en-us/images/ivr/image2.png)
    ///
    ///
    ///Now we will need to configure IIS Express to serve requests coming from outside network. Please navigate to file IvrSample\.vs\config\applicationhost.config inside the project. Locate the Ivr website inside \<sites\> section in the configuration file. It should contain lines:
    ///
    ///~~~  
    ///<bindings>
    ///    <binding protocol="http" bindingInformation="*:12345:localhost" />
    ///</bindings>
    ///~~~
    ///
    ///Please add second binding entry so the section looks like:  
    ///
    ///~~~
    ///<bindings>
    ///    <binding protocol="http" bindingInformation="*:12345:localhost" />
    ///    <binding protocol="http" bindingInformation="*:12345:*" />
    ///</bindings>
    ///~~~
    ///
    ///Check if the IIS Express is running (if there is IIS Express icon in system tray). If yes, right click on the icon and choose Exit.
    ///
    ///The next step is to configure your %Bot in the portal to use ngrok endpoints.
    ///
    ///Don't forget to append your route when updating the messaging url, the new url should look like this: <https://78191649.ngrok.io/v1/call>.  
    ///
    ///Please also update the CallbackUrl setting in Web.config file (it should be <https://78191649.ngrok.io/v1/callback> in presented sample).
    ///
    ///Now you can start your server locally and send messages to your bot over skype, they will be sent by %Bot Platform to <https://78191649.ngrok.io/v1/>call and ngrok will forward them to your machine. You just need to keep ngrok running.
    ///
    ///You will see each request logged in the ngrok's tunnel status table:
    ///
    ///~~~
    ///HTTP Requests
    ///-------------
    ///POST /v1/call                  200 OK
    ///~~~
    ///
    ///
    ///If you are done with testing, you can stop ngrok (Ctrl+C), your agent will stop working as there is nothing to forward the requests to your local server.
    ///
    ///Note: Free version of ngrok will create a new unique url for you everytime you start it. That means you always need to go back and update the messaging url for your bot.
}