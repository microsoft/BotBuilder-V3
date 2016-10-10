using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Search.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.SearchDialogs;

namespace Microsoft.Bot.Sample.JobListingBot.Dialogs
{
    [Serializable]
    public class JobsDialog : SearchDialog
    {
        private static readonly string[] TopRefiners = { "business_title", "agency", "work_location", "tags" };

        public JobsDialog(SearchQueryBuilder queryBuilder) : base(queryBuilder, new JobStyler(), multipleSelection: true)
        {
        }

        protected override string[] GetTopRefiners()
        {
            return TopRefiners;
        }

        protected override SearchHit ToSearchHit(SearchResult hit)
        {
            string description = (string)hit.Document["job_description"];

            return new SearchHit
            {
                Key = (string)hit.Document["id"],
                Title = GetTitleForItem(hit),
                PictureUrl = null,
                Description = description.Length > 512 ? description.Substring(0, 512) + "..." : description
            };
        }

        private static string GetTitleForItem(SearchResult result)
        {
            return string.Format("{0} at {1}, {2:C0} to {3:C0}",
                                 result.Document["business_title"],
                                 result.Document["agency"],
                                 result.Document["salary_range_from"],
                                 result.Document["salary_range_to"]);
        }

        [Serializable]
        class JobStyler : PromptStyler
        {
            public override void Apply<T>(ref IMessageActivity message, string prompt, IList<T> options)
            {
                var hits = (IList<SearchHit>)options;
                var actions = hits.Select(h => new CardAction(ActionTypes.ImBack, h.Title, null, h.Key)).ToList();
                var attachments = new List<Attachment>
                {
                    new HeroCard(text: prompt, buttons: actions).ToAttachment()
                };

                message.AttachmentLayout = AttachmentLayoutTypes.List;
                message.Attachments = attachments;
            }
        }
    }
}
