using System;
using Microsoft.Azure.Search.Models;
using Microsoft.Bot.Sample.SearchDialogs;
using System.Collections.Generic;

namespace Microsoft.Bot.Sample.RealEstateBot.Dialogs
{
    [Serializable]
    public class RealEstateSearchDialog : SearchDialog
    {
        private static readonly string[] TopRefiners = { "region", "city", "type", "beds", "baths", "price", "daysOnMarket", "sqft" };

        // TODO: This should ideally be canonicalized in LUIS, but barring that be data driven
        private static readonly Dictionary<string, string> map = new Dictionary<string, string> {
                { "bath", "baths"}, { "bathrooms", "baths"},
                {"bedroom", "beds" }, { "bedrooms", "beds" } };
        
        public RealEstateSearchDialog() : base(multipleSelection: true, canonicalizer: (prop) => map[prop])
        {
            // TODO: This should really be driven by analyzing the schema
            SearchDialogIndexClient.Schema.Fields["baths"].FilterPreference = PreferredFilter.MinValue;
            SearchDialogIndexClient.Schema.Fields["beds"].FilterPreference = PreferredFilter.MinValue;
            SearchDialogIndexClient.Schema.Fields["price"].FilterPreference = PreferredFilter.Range;
            SearchDialogIndexClient.Schema.Fields["daysOnMarket"].FilterPreference = PreferredFilter.RangeMax;
            SearchDialogIndexClient.Schema.Fields["sqft"].FilterPreference = PreferredFilter.RangeMax;
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
