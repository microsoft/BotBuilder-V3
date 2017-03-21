using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.AlarmBot.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Sample.AlarmBot.Models
{
    /// <summary>
    /// This method represents the logic necessary to respond to an external event.
    /// </summary>
    public static class ExternalEvent
    {
        public static async Task HandleAlarm(Alarm alarm, DateTime now, CancellationToken token)
        {
            // since this is an externally-triggered event, this is the composition root
            // find the dependency injection container
            var container = Global.FindContainer();

            await HandleAlarm(container, alarm, now, token);
        }

        public static async Task HandleAlarm(ILifetimeScope container, Alarm alarm, DateTime now, CancellationToken token)
        {
            // the ConversationReference has the "key" necessary to resume the conversation
            var message = alarm.Cookie.GetPostToBotMessage();
            // we instantiate our dependencies based on an IMessageActivity implementation
            using (var scope = DialogModule.BeginLifetimeScope(container, message))
            {
                // find the bot data interface and load up the conversation dialog state
                var botData = scope.Resolve<IBotData>();
                await botData.LoadAsync(token);

                // resolve the dialog stack
                var task = scope.Resolve<IDialogTask>();
                // make a dialog to push on the top of the stack
                var child = scope.Resolve<AlarmRingDialog>(TypedParameter.From(alarm.Title));
                // wrap it with an additional dialog that will restart the wait for
                // messages from the user once the child dialog has finished
                var interruption = child.Void(task);

                try
                {
                    // put the interrupting dialog on the stack
                    task.Call(interruption, null);
                    // start running the interrupting dialog
                    await task.PollAsync(token);
                }
                finally
                {
                    // save out the conversation dialog state
                    await botData.FlushAsync(token);
                }
            }
        }
    }
}