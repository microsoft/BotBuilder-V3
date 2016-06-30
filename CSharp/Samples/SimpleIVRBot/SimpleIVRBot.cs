using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Builder.Calling.Events;
using Microsoft.Bot.Builder.Calling.ObjectModel.Contracts;
using Microsoft.Bot.Builder.Calling.ObjectModel.Misc;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.SimpleIVRBot
{
    public class SimpleIVRBot : IDisposable, ICallingBot
    {
        // below are the dtmf keys required for each of option, will be used for parsing results of recognize
        private const string NewClient = "1";
        private const string Support = "2";
        private const string Payments = "3";
        private const string MoreInfo = "4";
        private const string NewClientOffer = "1";
        private const string NewClientOrder = "2";
        private const string SupportOutages = "1";
        private const string SupportConsultant = "2";
        private const string PaymentDetails = "1";
        private const string PaymentNotVisible = "2";

        private readonly Dictionary<string, CallState> _callStateMap = new Dictionary<string, CallState>();

        public ICallingBotService CallingBotService { get; private set; }

        public SimpleIVRBot(ICallingBotService callingBotService)
        {
            if (callingBotService == null)
                throw new ArgumentNullException(nameof(callingBotService));

            CallingBotService = callingBotService;

            CallingBotService.OnIncomingCallReceived += OnIncomingCallReceived;
            CallingBotService.OnPlayPromptCompleted += OnPlayPromptCompleted;
            CallingBotService.OnRecordCompleted += OnRecordCompleted;
            CallingBotService.OnRecognizeCompleted += OnRecognizeCompleted;
            CallingBotService.OnHangupCompleted += OnHangupCompleted;
        }

        private Task OnIncomingCallReceived(IncomingCallEvent incomingCallEvent)
        {
            var id = Guid.NewGuid().ToString();
            _callStateMap[incomingCallEvent.IncomingCall.Id] = new CallState();
            incomingCallEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    new Answer { OperationId = id },
                    GetPromptForText(IvrOptions.WelcomeMessage)
                };

            return Task.FromResult(true);
        }

        private Task OnHangupCompleted(HangupOutcomeEvent hangupOutcomeEvent)
        {
            hangupOutcomeEvent.ResultingWorkflow = null;
            return Task.FromResult(true);
        }

        private Task OnPlayPromptCompleted(PlayPromptOutcomeEvent playPromptOutcomeEvent)
        {
            var callStateForClient = _callStateMap[playPromptOutcomeEvent.ConversationResult.Id];
            callStateForClient.InitiallyChosenMenuOption = null;
            SetupInitialMenu(playPromptOutcomeEvent.ResultingWorkflow);

            return Task.FromResult(true);
        }

        private Task OnRecordCompleted(RecordOutcomeEvent recordOutcomeEvent)
        {
            var id = Guid.NewGuid().ToString();
            recordOutcomeEvent.ResultingWorkflow.Actions = new List<ActionBase>
                {
                    GetPromptForText(IvrOptions.Ending),
                    new Hangup { OperationId = id }
                };
            recordOutcomeEvent.ResultingWorkflow.Links = null;
            _callStateMap.Remove(recordOutcomeEvent.ConversationResult.Id);
            return Task.FromResult(true);
        }

        private Task OnRecognizeCompleted(RecognizeOutcomeEvent recognizeOutcomeEvent)
        {
            var callStateForClient = _callStateMap[recognizeOutcomeEvent.ConversationResult.Id];

            switch (callStateForClient.InitiallyChosenMenuOption)
            {
                case null:
                    ProcessMainMenuSelection(recognizeOutcomeEvent, callStateForClient);
                    break;
                case NewClient:
                    ProcessNewClientSelection(recognizeOutcomeEvent, callStateForClient);
                    break;
                case Support:
                    ProcessSupportSelection(recognizeOutcomeEvent, callStateForClient);
                    break;
                case Payments:
                    ProcessPaymentsSelection(recognizeOutcomeEvent, callStateForClient);
                    break;
                default:
                    SetupInitialMenu(recognizeOutcomeEvent.ResultingWorkflow);
                    break;
            }
            return Task.FromResult(true);
        }

        private void SetupInitialMenu(Workflow workflow)
        {
            workflow.Actions = new List<ActionBase> { CreateIvrOptions(IvrOptions.MainMenuPrompt, 5, false) };
        }

        private Recognize CreateNewClientMenu()
        {
            return CreateIvrOptions(IvrOptions.NewClientPrompt, 2, true);
        }

        private Recognize CreateSupportMenu()
        {
            return CreateIvrOptions(IvrOptions.SupportPrompt, 2, true);
        }

        private Recognize CreatePaymentsMenu()
        {
            return CreateIvrOptions(IvrOptions.PaymentPrompt, 2, true);
        }

        private void ProcessMainMenuSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
        {
            if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
            {
                SetupInitialMenu(outcome.ResultingWorkflow);
                return;
            }

            switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
            {
                case NewClient:
                    callStateForClient.InitiallyChosenMenuOption = NewClient;
                    outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreateNewClientMenu() };
                    break;
                case Support:
                    callStateForClient.InitiallyChosenMenuOption = Support;
                    outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreateSupportMenu() };
                    break;
                case Payments:
                    callStateForClient.InitiallyChosenMenuOption = Payments;
                    outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreatePaymentsMenu() };
                    break;
                case MoreInfo:
                    callStateForClient.InitiallyChosenMenuOption = MoreInfo;
                    outcome.ResultingWorkflow.Actions = new List<ActionBase> { GetPromptForText(IvrOptions.MoreInfoPrompt) };
                    break;
                default:
                    SetupInitialMenu(outcome.ResultingWorkflow);
                    break;
            }
        }

        private void ProcessNewClientSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
        {
            if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
            {
                outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreateNewClientMenu() };
                return;
            }
            switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
            {
                case NewClientOffer:
                    outcome.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            GetPromptForText(IvrOptions.Offer),
                            CreateNewClientMenu()
                        };
                    break;
                case NewClientOrder:
                    SetupRecording(outcome.ResultingWorkflow);
                    break;
                default:
                    callStateForClient.InitiallyChosenMenuOption = null;
                    SetupInitialMenu(outcome.ResultingWorkflow);
                    break;
            }
        }

        private void ProcessSupportSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
        {
            if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
            {
                outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreateSupportMenu() };
                return;
            }
            switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
            {
                case SupportOutages:
                    outcome.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            GetPromptForText(IvrOptions.CurrentOutages),
                            CreateSupportMenu()
                        };
                    break;
                case SupportConsultant:
                    SetupRecording(outcome.ResultingWorkflow);
                    break;
                default:
                    callStateForClient.InitiallyChosenMenuOption = null;
                    SetupInitialMenu(outcome.ResultingWorkflow);
                    break;
            }
        }

        private void ProcessPaymentsSelection(RecognizeOutcomeEvent outcome, CallState callStateForClient)
        {
            if (outcome.RecognizeOutcome.Outcome != Outcome.Success)
            {
                outcome.ResultingWorkflow.Actions = new List<ActionBase> { CreatePaymentsMenu() };
                return;
            }
            switch (outcome.RecognizeOutcome.ChoiceOutcome.ChoiceName)
            {
                case PaymentDetails:
                    outcome.ResultingWorkflow.Actions = new List<ActionBase>
                        {
                            GetPromptForText(IvrOptions.PaymentDetailsMessage),
                            CreatePaymentsMenu()
                        };
                    break;
                case PaymentNotVisible:
                    SetupRecording(outcome.ResultingWorkflow);
                    break;
                default:
                    callStateForClient.InitiallyChosenMenuOption = null;
                    SetupInitialMenu(outcome.ResultingWorkflow);
                    break;
            }
        }

        private static Recognize CreateIvrOptions(string textToBeRead, int numberOfOptions, bool includeBack)
        {
            if (numberOfOptions > 9)
                throw new Exception("too many options specified");

            var id = Guid.NewGuid().ToString();
            var choices = new List<RecognitionOption>();
            for (int i = 1; i <= numberOfOptions; i++)
            {
                choices.Add(new RecognitionOption { Name = Convert.ToString(i), DtmfVariation = (char)('0' + i) });
            }
            if (includeBack)
                choices.Add(new RecognitionOption { Name = "#", DtmfVariation = '#' });
            var recognize = new Recognize
            {
                OperationId = id,
                PlayPrompt = GetPromptForText(textToBeRead),
                BargeInAllowed = true,
                Choices = choices
            };

            return recognize;
        }

        private static void SetupRecording(Workflow workflow)
        {
            var id = Guid.NewGuid().ToString();

            var prompt = GetPromptForText(IvrOptions.NoConsultants);
            var record = new Record
            {
                OperationId = id,
                PlayPrompt = prompt,
                MaxDurationInSeconds = 10,
                InitialSilenceTimeoutInSeconds = 5,
                MaxSilenceTimeoutInSeconds = 2,
                PlayBeep = true,
                StopTones = new List<char> { '#' }
            };
            workflow.Actions = new List<ActionBase> { record };
        }

        private static PlayPrompt GetPromptForText(string text)
        {
            var prompt = new Prompt { Value = text, Voice = VoiceGender.Male };
            return new PlayPrompt { OperationId = Guid.NewGuid().ToString(), Prompts = new List<Prompt> { prompt } };
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (CallingBotService != null)
            {
                CallingBotService.OnIncomingCallReceived -= OnIncomingCallReceived;
                CallingBotService.OnPlayPromptCompleted -= OnPlayPromptCompleted;
                CallingBotService.OnRecordCompleted -= OnRecordCompleted;
                CallingBotService.OnRecognizeCompleted -= OnRecognizeCompleted;
                CallingBotService.OnHangupCompleted -= OnHangupCompleted;
            }
        }

        #endregion

        private class CallState
        {
            public string InitiallyChosenMenuOption { get; set; }
        }
    }

}