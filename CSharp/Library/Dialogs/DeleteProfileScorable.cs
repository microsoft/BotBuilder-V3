using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Resource;
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
        private readonly IBotData botData;
        private readonly IBotToUser botToUser;

        public DeleteProfileScorable(IDialogStack stack, IBotData botData, IBotToUser botToUser, Func<IDialog<object>> makeroot)
        {
            SetField.NotNull(out this.stack, nameof(stack), stack);
            SetField.NotNull(out this.botData, nameof(botData), botData);
            SetField.NotNull(out this.botToUser, nameof(botToUser), botToUser);
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
            var message = (IMessageActivity)(object)item;
            this.stack.Reset();
            botData.UserData.Clear();
            botData.PrivateConversationData.Clear();
            await botData.FlushAsync(token);
            await botToUser.PostAsync(Resources.UserProfileDeleted);
        }
    }


}
