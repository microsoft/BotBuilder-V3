using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.SearchDialogs;

namespace Microsoft.Bot.Sample.RealEstateBot.Dialogs
{
    [Serializable]
    public class IntroDialog : IDialog<IMessageActivity>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(StartSearchDialog);
            return Task.CompletedTask;
        }

        public Task StartSearchDialog(IDialogContext context, IAwaitable<IMessageActivity> input)
        {
            context.Call(new RealEstateSearchDialog(), Done);
            return Task.CompletedTask;
        }

        public async Task Done(IDialogContext context, IAwaitable<IList<SearchHit>> input)
        {
            var selection = await input;
            string list = string.Join(", ", selection.Select(s => s.Key));
            await context.PostAsync("Done! For future reference, you selected these properties: " + list);
            context.Done<IMessageActivity>(null);
        }
    }
}
