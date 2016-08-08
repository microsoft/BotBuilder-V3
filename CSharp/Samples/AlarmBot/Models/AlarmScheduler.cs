using Microsoft.Bot.Builder.Internals.Fibers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Microsoft.Bot.Sample.AlarmBot.Models
{
    /// <summary>
    /// This service represents an alarm that will fire its next event at a specific time.
    /// </summary>
    public interface IAlarmable
    {
        bool TryFindNext(DateTime now, out DateTime next);
        Task NextAsync(DateTime now, CancellationToken token);
    }

    /// <summary>
    /// This service tracks the passage of time to fire <see cref="IAlarmable"/> instances when appropriate. 
    /// </summary>
    public interface IAlarmScheduler
    {
        ObservableCollection<IAlarmable> Alarms { get; }
    }

    public static partial class Extensions
    {
        public static bool TryFindAlarm(this IAlarmScheduler scheduler, string title, out Alarm alarm)
        {
            var alarms = scheduler.Alarms.Cast<Alarm>();
            alarm = alarms.SingleOrDefault(a => (a.Title ?? string.Empty) == (title ?? string.Empty));
            return alarm != null;
        }
    }

    public sealed class NaiveAlarmScheduler : IAlarmScheduler
    {
        private readonly IClock clock;
        private readonly ObservableCollection<IAlarmable> alarms = new ObservableCollection<IAlarmable>();
        private IAlarmable[] snapshot = Array.Empty<IAlarmable>();
        public NaiveAlarmScheduler(IClock clock)
        {
            SetField.NotNull(out this.clock, nameof(clock), clock);

            this.alarms.CollectionChanged += (sender, arguments) =>
            {
                Interlocked.Exchange(ref this.snapshot, this.alarms.ToArray());
            };

            HostingEnvironment.QueueBackgroundWorkItem(async token =>
            {
                var nowStart = DateTime.MinValue;
                var nowAfter = DateTime.MinValue;

                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    nowStart = nowAfter;
                    nowAfter = this.clock.Now;

                    var snapshot = Interlocked.CompareExchange(ref this.snapshot, null, null);

                    foreach (var alarm in snapshot)
                    {
                        DateTime next;
                        if (alarm.TryFindNext(nowStart, out next))
                        {
                            if (next < nowAfter)
                            {
                                await alarm.NextAsync(nowStart, token);
                            }
                        }
                    }

                    // polling is one of the naive aspects of this implementation
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });
        }

        ObservableCollection<IAlarmable> IAlarmScheduler.Alarms
        {
            get
            {
                return this.alarms;
            }
        }
    }
}