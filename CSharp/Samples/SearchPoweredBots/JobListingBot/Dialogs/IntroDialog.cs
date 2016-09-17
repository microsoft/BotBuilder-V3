using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Search.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.SearchDialogs;

namespace Microsoft.Bot.Sample.JobListingBot.Dialogs
{
    [Serializable]
    public class IntroDialog : IDialog<IMessageActivity>
    {
        protected readonly SearchQueryBuilder queryBuilder = new SearchQueryBuilder();

        public Task StartAsync(IDialogContext context)
        {
            SearchDialogIndexClient.Schema = new SearchSchema().AddFields(
            new Field[] {
                new Field() { Name = "business_title", Type = DataType.String, IsFacetable = true, IsFilterable = true, IsKey = false, IsRetrievable = true, IsSearchable = true, IsSortable = true },
                new Field() { Name = "agency", Type = DataType.String, IsFacetable = true, IsFilterable = true, IsKey = false, IsRetrievable = true, IsSearchable = true, IsSortable = true },
                new Field { Name = "work_location", Type = DataType.String, IsFacetable = true, IsFilterable = true, IsKey = false, IsRetrievable = true, IsSearchable = true, IsSortable = true }
            });
            context.Wait(SelectTitle);
            return Task.CompletedTask;
        }

        public Task SelectTitle(IDialogContext context, IAwaitable<IMessageActivity> input)
        {
            context.Call(new SearchRefineDialog("business_title", queryBuilder, prompt: "Hi! To get started, what kind of position are you looking for?"),
                         StartSearchDialog);
            return Task.CompletedTask;
        }

        public async Task StartSearchDialog(IDialogContext context, IAwaitable<string> input)
        {
            string title = await input;

            if (string.IsNullOrEmpty(title))
            {
                context.Done<IMessageActivity>(null);
            }
            else
            {
                context.Call(new JobsDialog(this.queryBuilder), Done);
            }
        }

        public async Task Done(IDialogContext context, IAwaitable<IList<SearchHit>> input)
        {
            var selection = await input;
            string list = string.Join(", ", selection.Select(s => s.Key));
            await context.PostAsync("Done! For future reference, you selected these job listings: " + list);
            this.queryBuilder.Reset();
            context.Done<IMessageActivity>(null);
        }
    }
}
