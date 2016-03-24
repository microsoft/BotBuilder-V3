using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Microsoft.Bot.Builder;

namespace Microsoft.Bot.Sample.SimpleAlarmBot
{
    [LuisModel("https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=fe054e042fd14754a83f0a205f6552a5&q=")]
    [Serializable]
    public class SimpleAlarmBot : LuisDialog
    {
        public SimpleAlarmBot()
        {
        }

        protected SimpleAlarmBot(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// default intent handler because it is marked by [LuisIntent("")]
        /// </summary>
        /// <param name="session"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry. I didn't understand you.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("builtin.intent.alarm.alarm_other")]
        [LuisIntent("builtin.intent.alarm.delete_alarm")]
        [LuisIntent("builtin.intent.alarm.find_alarm")]
        [LuisIntent("builtin.intent.alarm.set_alarm")]
        [LuisIntent("builtin.intent.alarm.snooze")]
        [LuisIntent("builtin.intent.alarm.time_remaining")]
        [LuisIntent("builtin.intent.alarm.turn_off_alarm")]
        public async Task AlarmOperation(IDialogContext context, LuisResult result)
        {
            var entities = string.Join(",", result.Entities.Select( s => s.Entity).ToArray());
            bool sendEntities = true;

            string reply;

            switch (result.Intents.FirstOrDefault()?.Intent)
            {
                case "builtin.intent.alarm.alarm_other":
                    reply = "alarm changed!";
                    break;
                case "builtin.intent.alarm.delete_alarm":
                    reply = "alarm deleted!";
                    break;
                case "builtin.intent.alarm.set_alarm":
                    reply = "alarm created!";
                    break;
                case "builtin.intent.alarm.find_alarm":
                    reply = "alarm found!";
                    break;
                case "builtin.intent.alarm.snooze":
                    reply = "alarm snoozed! ";
                    break;
                case "builtin.intent.alarm.time_remaining":
                    reply = "There are 5 minutes remaining.";
                    break;
                case "builtin.intent.alarm.turn_off_alarm":
                    Prompts.Confirm(context, AlarmDialogResumeHandler, "Are you sure?");
                    reply = null;
                    sendEntities = false; 
                    break;
                default:
                    reply = "Couldn't understand the command!";
                    break;
            }

            if (reply != null)
            {
                if (sendEntities)
                {
                    reply += $"\nentities: {entities}";
                }

                await context.PostAsync(reply);
                context.Wait(MessageReceived);
            }
        }

        public async Task AlarmDialogResumeHandler(IDialogContext context, IAwaitable<bool> confirmation)
        {
            if (await confirmation)
            {
                await context.PostAsync("Ok, alarm disabled.");
            }
            else
            {
                await context.PostAsync("Ok! We haven't modified your alarms!");
            }

            context.Wait(MessageReceived);
        }
    }
}