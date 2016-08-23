using Microsoft.Bot.Builder.Dialogs.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Internals.Fibers;

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
    public sealed class AlarmScorable : IAlarmActions, IScorable<double>
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

        async Task<object> IScorable<double>.PrepareAsync<Item>(Item item, Delegate method, CancellationToken token)
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

        bool IScorable<double>.TryScore(object state, out double score)
        {
            bool matched = state != null;
            score = matched ? 1.0 : double.NaN;
            return matched;
        }

        async Task IScorable<double>.PostAsync<Item>(IPostToBot inner, Item item, object state, CancellationToken token)
        {
            var verbTitle = (Tuple<string, string>)state;
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
    }
}