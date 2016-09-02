using System;
using Microsoft.Azure.Search.Models;
using Microsoft.Bot.Sample.SearchDialogs;

namespace Microsoft.Bot.Sample.RealEstateBot.Dialogs
{
    [Serializable]
    public class RealEstateSearchDialog : SearchDialog
    {
        private static readonly string[] TopRefiners = { "region", "city", "type" };

        public RealEstateSearchDialog() : base(multipleSelection: true)
        {
        }

        protected override string[] GetTopRefiners()
        {
            return TopRefiners;
        }

        protected override SearchHit ToSearchHit(SearchResult hit)
        {
            return new SearchHit
            {
                Key = (string)hit.Document["listingId"],
                Title = GetTitleForItem(hit),
                PictureUrl = (string)hit.Document["thumbnail"],
                Description = (string)hit.Document["description"]
            };
        }

        private static string GetTitleForItem(SearchResult result)
        {
            return string.Format("{0} bedroom, {1} bath in {2}, ${3:#,0}",
                                 result.Document["beds"],
                                 result.Document["baths"],
                                 result.Document["city"],
                                 result.Document["price"]);
        }
    }
}
