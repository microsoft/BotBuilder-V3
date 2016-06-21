using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    public sealed class DeleteProfileScorable : IScorable<double>
    {
        private readonly IDialogStack stack;
        private readonly Regex regex = new Regex("^(\\s)*/deleteprofile", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        private readonly Func<IDialog<object>> makeroot;
        private readonly IStateClient stateClient; 


        public DeleteProfileScorable(IDialogStack stack, IStateClient stateClient, Func<IDialog<object>> makeroot)
        {
            SetField.NotNull(out this.stack, nameof(stack), stack);
            SetField.NotNull(out this.stateClient, nameof(stateClient), stateClient);
            SetField.NotNull(out this.makeroot, nameof(makeroot), makeroot);
        }

        async Task<object> IScorable<double>.PrepareAsync<Item>(Item item, Delegate method, CancellationToken token)
        {
            var message = item as IMessageActivity;
            if (message != null && message.Text != null)
            {
                var text = message.Text;
                var match = regex.Match(text);
                if (match.Success)
                {
                    return match.Groups[0].Value;
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
            var dialog = makeroot();

            var message = (IMessageActivity)(object)item;
            await stateClient.BotState.DeleteStateForUserAsync(message.ChannelId, message.From.Id, token);
            
            await this.stack.Forward(dialog.Void<object, IMessageActivity>(), null, item, token);
            await this.stack.PollAsync(token);
        }
    }


}
