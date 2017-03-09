using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.AlarmBot.Models;

namespace Microsoft.Bot.Sample.AlarmBot.Dialogs
{
    public sealed class MyLuisModelAttribute : LuisModelAttribute
    {
        public MyLuisModelAttribute()
            : base(modelID: "unitTestMockReturnedFromMakeService", subscriptionKey: "unitTestMockReturnedFromMakeService")
        {
        }
    }

    /// <summary>
    /// The top-level natural language dialog for the alarm sample.
    /// </summary>
    [Serializable]
    [MyLuisModel]
    public sealed class AlarmDispatchDialog : DispatchDialog
    {
        private readonly IAlarmService service;
        private readonly IEntityToType entityToType;
        private readonly ILuisService luis;
        private readonly IClock clock;
        public AlarmDispatchDialog(IAlarmService service, IEntityToType entityToType, ILuisService luis, IClock clock)
        {
            SetField.NotNull(out this.service, nameof(service), service);
            SetField.NotNull(out this.entityToType, nameof(entityToType), entityToType);
            SetField.NotNull(out this.luis, nameof(luis), luis);
            SetField.NotNull(out this.clock, nameof(clock), clock);
        }

        protected override ILuisService MakeService(ILuisModel model)
        {
            return this.luis;
        }

        [LuisIntent("builtin.intent.none")]
        // ScorableOrder allows the user to override the scoring process to create
        // ordered scorable groups, where the scores from the first scorable group
        // are compared first, and if there is no scorable that wishes to participate
        // from the first scorable group, then the second scorable group is considered, and so forth.
        // You might use this to ensure that regular expression scorables are considered
        // before LUIS intent scorables.
        [ScorableGroup(1)]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry I did not understand: " + string.Join(", ", result.Intents.Select(i => i.Intent));
            await context.PostAsync(message);
            context.Wait(ActivityReceivedAsync);
        }

        public bool TryFindTitle(LuisResult result, out string title)
        {
            EntityRecommendation entity;
            if (result.TryFindEntity(BuiltIn.Alarm.Title, out entity))
            {
                title = entity.Entity;
                return true;
            }

            title = null;
            return false;
        }

        [LuisIntent("builtin.intent.alarm.delete_alarm")]
        [ScorableGroup(1)]
        public async Task DeleteAlarm(IDialogContext context, LuisResult result)
        {
            string title;
            TryFindTitle(result, out title);
            try
            {
                await this.service.DeleteAsync(title);
            }
            catch (AlarmNotFoundException)
            {
                await context.PostAsync("did not find alarm");
            }

            context.Wait(ActivityReceivedAsync);
        }

        [LuisIntent("builtin.intent.alarm.find_alarm")]
        [ScorableGroup(1)]
        public async Task FindAlarm(IDialogContext context, LuisResult result)
        {
            string title;
            TryFindTitle(result, out title);
            try
            {
                await this.service.UpsertAsync(title, null, null);
            }
            catch (AlarmNotFoundException)
            {
                await context.PostAsync("did not find alarm");
            }

            context.Wait(ActivityReceivedAsync);
        }

        [LuisIntent("builtin.intent.alarm.set_alarm")]
        [ScorableGroup(1)]
        public async Task SetAlarm(IDialogContext context, IMessageActivity activity, LuisResult result)
        {
            string title;
            bool? state = null;
            DateTime? when = null;

            TryFindTitle(result, out title);

            EntityRecommendation entity;
            if (result.TryFindEntity(BuiltIn.Alarm.Alarm_State, out entity))
            {
                state = entity.Entity.Equals("on", StringComparison.InvariantCultureIgnoreCase);
            }

            var now = this.clock.Now;

            IEnumerable<Range<DateTime>> ranges;
            if (entityToType.TryMapToDateRanges(now, result.Entities, out ranges))
            {
                using (var enumerator = ranges.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        when = enumerator.Current.Start;
                    }
                }
            }

            await this.service.UpsertAsync(title, when, state);

            context.Wait(ActivityReceivedAsync);
        }

        [LuisIntent("builtin.intent.alarm.snooze")]
        [ScorableGroup(1)]
        public async Task AlarmSnooze(IDialogContext context, LuisResult result)
        {
            string title;
            TryFindTitle(result, out title);
            try
            {
                await this.service.SnoozeAsync(title);
            }
            catch (AlarmNotFoundException)
            {
                await context.PostAsync("did not find alarm");
            }

            context.Wait(ActivityReceivedAsync);
        }

        [LuisIntent("builtin.intent.alarm.turn_off_alarm")]
        [ScorableGroup(1)]
        public async Task TurnOffAlarm(IDialogContext context, LuisResult result)
        {
            string title;
            TryFindTitle(result, out title);
            try
            {
                await this.service.UpsertAsync(title, null, state: false);
            }
            catch (AlarmNotFoundException)
            {
                await context.PostAsync("did not find alarm");
            }

            context.Wait(ActivityReceivedAsync);
        }

        [LuisIntent("builtin.intent.alarm.time_remaining")]
        [LuisIntent("builtin.intent.alarm.alarm_other")]
        [ScorableGroup(1)]
        public async Task AlarmOther(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Sorry, I don't know how to handle that.");
            context.Wait(ActivityReceivedAsync);
        }
    }
}