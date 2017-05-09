using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.SimpleDispatchDialog
{
    [LuisModel("4311ccf1-5ed1-44fe-9f10-a6adbad05c14", "6d0966209c6e4f6b835ce34492f3e6d9")]
    [Serializable]
    public class SimpleDispatchDialog : DispatchDialog
    {
        private int count = 1;
        
        // generic activity handler.
        [MethodBind]
        [ScorableGroup(0)]
        public async Task ActivityHandler(IDialogContext context, IActivity activity)
        {
            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    //sending typing and submit for next group of scorables. 
                    var reply = context.MakeMessage();
                    reply.Type = ActivityTypes.Typing;
                    await context.PostAsync(reply);
                    // continue to next scorable group
                    ContinueWithNextGroup();
                    break;

                case ActivityTypes.Ping:
                    Trace.TraceInformation("Ping received");
                    break;
                case ActivityTypes.ContactRelationUpdate:
                case ActivityTypes.Typing:
                case ActivityTypes.DeleteUserData:                
                default:
                    await context.PostAsync($"Unknown activity type ignored: {activity.Type}");
                    break;
            }
        }

        // conversation update handler.
        [MethodBind]
        [ScorableGroup(0)]
        public async Task ConversationUpdateHandler(IDialogContext context, IConversationUpdateActivity update)
        {   
            var reply = context.MakeMessage();
            if (update.MembersAdded.Any())
            {
                var newMembers = update.MembersAdded?.Where(t => t.Id != update.Recipient.Id);
                foreach (var newMember in newMembers)
                {
                    reply.Text = "Welcome";
                    if (!string.IsNullOrEmpty(newMember.Name))
                    {
                        reply.Text += $" {newMember.Name}";
                    }
                    reply.Text += "!";
                    await context.PostAsync(reply);
                }
            }
        }

        // Luis model didn't recognize the query and "None" intent was winner.
        [LuisIntent("None")]
        [ScorableGroup(1)]
        public async Task None(IDialogContext context, LuisResult result)
        {
            // Luis returned with None as the winning intent
            ContinueWithNextGroup();
        }

        // When you say "Store hour on <day of the week>".
        [LuisIntent("StoreHours")]
        [ScorableGroup(1)]
        public async Task ProcessStoreHours(IDialogContext context, LuisResult result, [Entity("Day")] string day)
        {
            await context.PostAsync($"On {day}, we are open from 11am to 11pm!");
        }

        // When you say reset it will trigger the prompt dialog to confirm reset of counter.
        [RegexPattern("^reset")]
        [ScorableGroup(2)]
        public async Task Reset(IDialogContext context, IActivity activity)
        {
            PromptDialog.Confirm(context, AfterResetAsync,
            "Are you sure you want to reset the count?",
            "Didn't get that!");
        }
        
        // When you say "echo: <text to echo>", it will increment the counter and echo the <text to echo>.
        [RegexPattern("^echo: (?<echo>.*)")]
        [ScorableGroup(2)]
        public async Task Echo(IDialogContext context, IActivity activity, [Entity("echo")] string echoString)
        {
            await context.PostAsync($"{this.count++}: You said {echoString}");
        }

        // Since none of the scorables in previous group won, the dialog send help message.
        [MethodBind]
        [ScorableGroup(3)]
        public async Task Default(IDialogContext context, IActivity activity)
        {
            await context.PostAsync("I'm sorry. I didn't understand you.");
            await context.PostAsync("You can tell me: \"echo: <some text>\"");
            await context.PostAsync("Or ask me about the store hours, for example: \"what are the store hours on Friday?\"");
        }
        
        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                context.UserData.SetValue("count", 1);
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
        }
    }
}