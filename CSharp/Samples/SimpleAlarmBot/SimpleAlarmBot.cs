using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Sample.SimpleAlarmBot
{
    [LuisModel("https://api.projectoxford.ai/luis/v1/application?id=c413b2ef-382c-45bd-8ff0-f76d60e2a821&subscription-key=fe054e042fd14754a83f0a205f6552a5&q=")]
    public class SimpleAlarmBot : LuisDialog<string, DialogResult>
    {
        public static readonly SimpleAlarmBot Instance = new SimpleAlarmBot();

        private SimpleAlarmBot()
        {
        }

        /// <summary>
        /// default intent handler because it is marked by [LuisIntent("")]
        /// </summary>
        /// <param name="session"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        [LuisIntent("")]
        public Task<DialogResponse> None(ISession session, LuisResult result)
        {
            return session.CreateDialogResponse("I'm sorry. I didn't understand you.");
        }

        [LuisIntent("builtin.intent.alarm.alarm_other")]
        [LuisIntent("builtin.intent.alarm.delete_alarm")]
        [LuisIntent("builtin.intent.alarm.find_alarm")]
        [LuisIntent("builtin.intent.alarm.set_alarm")]
        [LuisIntent("builtin.intent.alarm.snooze")]
        [LuisIntent("builtin.intent.alarm.time_remaining")]
        [LuisIntent("builtin.intent.alarm.turn_off_alarm")]
        public async Task<DialogResponse> AlarmOperation(ISession session, LuisResult result)
        {
            DialogResponse reply = null;
            var entities = string.Join(",", result.Entities.Select( s => s.Entity).ToArray());
            bool sendEntities = true; 

            switch(result.Intents.FirstOrDefault()?.Intent)
            {
                case "builtin.intent.alarm.alarm_other":
                    reply = await session.CreateDialogResponse("alarm changed!");
                    break;
                case "builtin.intent.alarm.delete_alarm":
                    reply = await session.CreateDialogResponse("alarm deleted!");
                    break;
                case "builtin.intent.alarm.set_alarm":
                    reply = await session.CreateDialogResponse("alarm created!");
                    break;
                case "builtin.intent.alarm.find_alarm":
                    reply = await session.CreateDialogResponse("alarm found!");
                    break;
                case "builtin.intent.alarm.snooze":
                    reply = await session.CreateDialogResponse("alarm snoozed! ");
                    break;
                case "builtin.intent.alarm.time_remaining":
                    reply = await session.CreateDialogResponse("There are 5 minutes remaining.");
                    break;
                case "builtin.intent.alarm.turn_off_alarm":
                    reply = await Prompts.Confirm(session, "Are you sure?");
                    sendEntities = false; 
                    break;
                default:
                    reply = await session.CreateDialogResponse("Couldn't understand the command!");
                    break;
            }

            if (sendEntities)
            {
                reply.Reply.Text += string.Format(" entities: {0}", entities);
            }

            return reply;
        }

        [LuisIntent("builtin.intent.alarm.turn_off_alarm", resumeHandler: true)]
        public async Task<DialogResponse> AlarmDialogResumeHandler(ISession session, DialogResult result)
        {
            DialogResponse reply = null;
            if(result is PromptDialogResult<bool>)
            {
                var promptRes = (PromptDialogResult<bool>)result;

                if (promptRes.Completed && promptRes.Response)
                {
                    reply = await session.CreateDialogResponse("Ok, alarm disabled.");
                }
                else
                {
                    reply = await session.CreateDialogResponse("Ok! We haven't modified your alarms!");
                }

            }
            return reply; 
        }

    }
}