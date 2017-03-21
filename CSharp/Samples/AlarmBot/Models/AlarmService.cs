using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.AlarmBot.Models
{
    /// <summary>
    /// This service represents actions that can taken against alarms.
    /// </summary>
    public interface IAlarmService
    {
        Task UpsertAsync(string title, DateTime? when, bool? state);
        Task DeleteAsync(string title);
        Task SnoozeAsync(string title);
    }
    public sealed class AlarmNotFoundException : Exception
    {
    }

    [Serializable]
    public sealed class AlarmService : IAlarmService
    {
        private readonly IAlarmScheduler scheduler;
        private readonly ConversationReference cookie;
        public AlarmService(IAlarmScheduler scheduler, ConversationReference cookie)
        {
            SetField.NotNull(out this.scheduler, nameof(scheduler), scheduler);
            SetField.NotNull(out this.cookie, nameof(cookie), cookie);
        }
        async Task IAlarmService.DeleteAsync(string title)
        {
            Alarm alarm;
            if (this.scheduler.TryFindAlarm(title, out alarm))
            {
                this.scheduler.Alarms.Remove(alarm);
            }
            else
            {
                throw new AlarmNotFoundException();
            }
        }
        async Task IAlarmService.SnoozeAsync(string title)
        {
            Alarm alarm;
            if (this.scheduler.TryFindAlarm(title, out alarm))
            {
                alarm.When = alarm.When + TimeSpan.FromMinutes(1);
            }
            else
            {
                throw new AlarmNotFoundException();
            }
        }
        async Task IAlarmService.UpsertAsync(string title, DateTime? when, bool? state)
        {
            Alarm alarm;
            if (!this.scheduler.TryFindAlarm(title, out alarm))
            {
                alarm = new Alarm() { Title = title, State = true, Next = ExternalEvent.HandleAlarm, Cookie = this.cookie };

                this.scheduler.Alarms.Add(alarm);
            }

            if (state.HasValue)
            {
                alarm.State = state.Value;
            }

            if (when.HasValue)
            {
                alarm.When = when.Value;
            }
        }
    }

    public sealed class RenderingAlarmService : IAlarmService
    {
        private readonly IAlarmService inner;
        private readonly Func<IAlarmRenderer> renderer;
        private readonly IBotToUser botToUser;
        private readonly IClock clock;
        public RenderingAlarmService(IAlarmService inner, Func<IAlarmRenderer> renderer, IBotToUser botToUser, IClock clock)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.renderer, nameof(renderer), renderer);
            SetField.NotNull(out this.botToUser, nameof(botToUser), botToUser);
            SetField.NotNull(out this.clock, nameof(clock), clock);
        }
        async Task IAlarmService.DeleteAsync(string title)
        {
            await this.inner.DeleteAsync(title);
        }

        async Task IAlarmService.SnoozeAsync(string title)
        {
            await this.inner.SnoozeAsync(title);
            await this.renderer().RenderAsync(this.botToUser, title, this.clock.Now);
        }

        async Task IAlarmService.UpsertAsync(string title, DateTime? when, bool? state)
        {
            await this.inner.UpsertAsync(title, when, state);
            await this.renderer().RenderAsync(this.botToUser, title, this.clock.Now);
        }
    }
}