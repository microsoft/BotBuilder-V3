using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Bot.Sample.SimpleAlarmBot
{
    public class SimpleFluentAlarmBot
    {
        private static Func<ISession, LuisResult, Task<DialogResponse>> WrapHandler(string message)
        {
            return async (session, res) => await session.CreateDialogResponse(message);
        }

        public static readonly LuisDialog<string, PromptDialogResult<bool>> Instance = 
            new LuisDialog<string, PromptDialogResult<bool>>("SimpleAlarm", "fe054e042fd14754a83f0a205f6552a5", "c413b2ef-382c-45bd-8ff0-f76d60e2a821")
                    .OnDefault(WrapHandler("I'm sorry. I didn't understand you."))
                    .On("builtin.intent.alarm.alarm_other", WrapHandler("Alarm changed"))
                    .On("builtin.intent.alarm.delete_alarm", WrapHandler("Alarm deleted"))
                    .On("builtin.intent.alarm.find_alarm", WrapHandler("Alarm found"))
                    .On("builtin.intent.alarm.set_alarm", WrapHandler("Alarm created"))
                    .On("builtin.intent.alarm.snooze", WrapHandler("Alarm snoozed"))
                    .On("builtin.intent.alarm.time_remaining", WrapHandler("There are 5 minutes remaining."))
                    .On("builtin.intent.alarm.turn_off_alarm", async (session, res) =>
                    {
                        return await Prompts.Confirm(session, "Are you sure?");
                    }, async (session, result) =>
                    {
                        var res = result as PromptDialogResult<bool>;
                        if (res.Completed && res.Response)
                        {
                            return await session.CreateDialogResponse("Ok, alarm disabled.");
                        }
                        else
                        {
                            return await session.CreateDialogResponse("ok!");
                        }
                    });
    }
}