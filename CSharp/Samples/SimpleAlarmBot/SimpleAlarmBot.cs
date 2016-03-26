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
        public async Task AlarmOther(IDialogContext context, LuisResult result)
        {
            string reply = "alarm changed!";
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("builtin.intent.alarm.delete_alarm")]
        public async Task DeleteAlarm(IDialogContext context, LuisResult result)
        {
            string reply = "alarm deleted!";
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("builtin.intent.alarm.find_alarm")]
        public async Task FindAlarm(IDialogContext context, LuisResult result)
        {
            string reply = "alarm found!";
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("builtin.intent.alarm.set_alarm")]
        public async Task SetAlarm(IDialogContext context, LuisResult result)
        {
            string reply = "alarm created!";
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("builtin.intent.alarm.snooze")]
        public async Task AlarmSnooze(IDialogContext context, LuisResult result)
        {
            string reply = "alarm snoozed!";
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("builtin.intent.alarm.time_remaining")]
        public async Task TimeRemaining(IDialogContext context, LuisResult result)
        {
            string reply = "There are 5 minutes remaining.";
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("builtin.intent.alarm.turn_off_alarm")]
        public async Task TurnOffAlarm(IDialogContext context, LuisResult result)
        {
            Prompts.Confirm(context, AfterConfirming_TurnOffAlarm, "Are you sure?");
        }

        public async Task AfterConfirming_TurnOffAlarm(IDialogContext context, IAwaitable<bool> confirmation)
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