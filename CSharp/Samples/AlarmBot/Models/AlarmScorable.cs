using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;

namespace Microsoft.Bot.Sample.AlarmBot.Models
{
    /// <summary>
    /// This service provides a list of actions that can be taken for an alarm.
    /// These actions are globally available based on the <see cref="AlarmScorable"/> implementation
    /// that inspects every incoming message.
    /// </summary>
    public interface IAlarmActions
    {
        IEnumerable<CardAction> ActionsFor(Alarm alarm);
    }

    [Serializable]
    public sealed class AlarmScorable : ScorableBase<IActivity, Tuple<string, string>, double>, IAlarmActions
    {
        private readonly IAlarmService service;
        public AlarmScorable(IAlarmService service)
        {
            SetField.NotNull(out this.service, nameof(service), service);
        }

        public static class Verbs
        {
            public const string Snooze = "snooze";
            public const string Delete = "delete";
            public const string Enable = "enable";
            public const string Disable = "disable";
        }

        public static readonly IReadOnlyList<string> AllowedVerbs = typeof(Verbs).GetFields().Select(p => (string)p.GetValue(null)).ToArray();

        public const string Prefix = "button";
        public static string FormatValue(string verb, Alarm alarm)
        {
            return $"{Prefix}-{verb}-{alarm.Title}";
        }

        public bool TryParseValue(string value, out string verb, out string title)
        {
            if (value != null)
            {
                var parts = value.Split('-');
                if (parts.Length == 3)
                {
                    if (parts[0] == Prefix)
                    {
                        verb = parts[1];
                        title = parts[2];

                        if (AllowedVerbs.Contains(verb))
                        {
                            return true;
                        }
                    }
                }
            }

            verb = null;
            title = null;
            return false;
        }

        IEnumerable<CardAction> IAlarmActions.ActionsFor(Alarm alarm)
        {
            Func<string, CardAction> ActionFor = verb =>
                new CardAction()
                {
                    Type = ActionTypes.ImBack,
                    Title = verb,
                    Value = FormatValue(verb, alarm)
                };

            yield return ActionFor(Verbs.Snooze);
            yield return ActionFor(Verbs.Delete);

            if (alarm.State)
            {
                yield return ActionFor(Verbs.Disable);
            }
            else
            {
                yield return ActionFor(Verbs.Enable);
            }
        }

        protected override async Task<Tuple<string, string>> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item as IMessageActivity;
            if (message != null && message.Text != null)
            {
                var text = message.Text;
                string verb;
                string title;
                if (TryParseValue(text, out verb, out title))
                {
                    return Tuple.Create(verb, title);
                }
            }

            return null;
        }
        protected override bool HasScore(IActivity item, Tuple<string, string> verbTitle)
        {
            return verbTitle != null;
        }
        protected override double GetScore(IActivity item, Tuple<string, string> verbTitle)
        {
            return 1.0;
        }
        protected override async Task PostAsync(IActivity item, Tuple<string, string> verbTitle, CancellationToken token)
        {
            var verb = verbTitle.Item1;
            var title = verbTitle.Item2;
            switch (verb)
            {
                case Verbs.Snooze:
                    await this.service.SnoozeAsync(title);
                    break;
                case Verbs.Delete:
                    await this.service.DeleteAsync(title);
                    break;
                case Verbs.Disable:
                    await this.service.UpsertAsync(title, when: null, state: false);
                    break;
                case Verbs.Enable:
                    await this.service.UpsertAsync(title, when: null, state: true);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        protected override Task DoneAsync(IActivity item, Tuple<string, string> verbTitle, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}