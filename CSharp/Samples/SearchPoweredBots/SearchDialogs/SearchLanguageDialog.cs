using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using System.Diagnostics;
using Microsoft.Azure.Search.Models;

namespace Microsoft.Bot.Sample.SearchDialogs
{
    public class SearchSpec
    {
        public string Text;
        public SearchParameters Parameters;
    }

    public class Range
    {
        public string Property;
        public double Lower;
        public double Upper;
        public bool IncludeLower;
        public bool IncludeUpper;
    }

    class ComparisonEntity
    {
        public EntityRecommendation Entity;
        public EntityRecommendation Operator;
        public EntityRecommendation Lower;
        public EntityRecommendation Upper;
        public EntityRecommendation Property;

        public ComparisonEntity(EntityRecommendation comparison)
        {
            Entity = comparison;
        }

        public void AddEntity(EntityRecommendation entity)
        {
            if (entity.Type != "Comparison" && entity.StartIndex >= Entity.StartIndex && entity.EndIndex <= Entity.EndIndex)
            {
                switch (entity.Type)
                {
                    case "Currency": AddNumber(entity); break;
                    case "Number": AddNumber(entity); break;
                    case "Dimension": AddNumber(entity); break;
                    case "Operator": Operator = entity; break;
                    case "Property": Property = entity; break;
                }
            }
        }

        public Range Resolve(CanonicalizerDelegate canonicalizer)
        {
            var comparison = new Range { Property = canonicalizer(Property.Entity) };
            var lower = Lower == null ? double.NegativeInfinity : double.Parse(Lower.Entity);
            var upper = Upper == null ? double.PositiveInfinity : double.Parse(Upper.Entity);
            switch (Operator.Entity)
            {
                case ">=": break;
                case "+":
                case "greater than or equal":
                case "at least":
                    comparison.IncludeLower = true;
                    comparison.IncludeUpper = true;
                    upper = double.PositiveInfinity;
                    break;

                case ">": 
                case "greater than": 
                    comparison.IncludeLower = false;
                    comparison.IncludeUpper = true;
                    upper = double.PositiveInfinity;
                    break;

                case "-": 
                case "between":
                case "and":
                case "or":
                    comparison.IncludeLower = true;
                    comparison.IncludeUpper = true;
                    break;

                case "<=": 
                case "no more than":
                case "less than or equal":
                    comparison.IncludeLower = true;
                    comparison.IncludeUpper = true;
                    upper = lower;
                    lower = double.NegativeInfinity;
                    break;

                case "<": 
                case "less than":
                    comparison.IncludeLower = true;
                    comparison.IncludeUpper = false;
                    upper = lower;
                    lower = double.NegativeInfinity;
                    break;

                    // This is the case where we just have naked values
                case "":
                    comparison.IncludeLower = true;
                    comparison.IncludeUpper = true;
                    upper = lower;
                    break;

                default: throw new ArgumentException($"Unknown operator {Operator.Entity}");
            }
            comparison.Lower = lower;
            comparison.Upper = upper;
            return comparison;
        }

        private void AddNumber(EntityRecommendation entity)
        {
            if (Lower == null)
            {
                Lower = entity;
            }
            else if (entity.StartIndex < Lower.StartIndex)
            {
                Upper = Lower;
                Lower = entity;
            }
            else
            {
                Upper = entity;
            }
        }
    }

    [LuisModel("02acb226-e898-49b1-9c85-d1753ba1d102", "b89dbb1918be4673b68bb48ed0fef1b6")]
    [Serializable]
    class SearchLanguageDialog : LuisDialog<string>
    {
        private const int DefaultHitPerPage = 5;

        public string SearchText { get; set; }

        public int PageNumber { get; set; }

        public int HitsPerPage { get; set; } = DefaultHitPerPage;

        protected CanonicalizerDelegate _canonicalizer;
        protected string _defaultProperty;

        public SearchLanguageDialog(CanonicalizerDelegate canonicalizer, string defaultProperty = null)
        {
            _canonicalizer = canonicalizer;
            _defaultProperty = defaultProperty;
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry. I didn't understand you.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Comparison")]
        public Task ProcessComparison(IDialogContext context, LuisResult result)
        {
            var comparisons = (from entity in result.Entities where entity.Type == "Comparison" select new ComparisonEntity(entity)).ToList();
            foreach(var entity in result.Entities)
            {
                foreach(var comparison in comparisons)
                {
                    comparison.AddEntity(entity);
                }
            }
            var ranges = from comparison in comparisons select comparison.Resolve(_canonicalizer);
            var filter = ranges.GenerateFilter();
            context.Done<string>(filter);
            return Task.CompletedTask;
        }
    }

    public partial class Extensions
    {
        public static string GenerateFilter(this IEnumerable<Range> ranges)
        {
            var filter = new StringBuilder();
            var seperator = "";
            foreach (var range in ranges)
            {
                filter.Append($"{seperator}");
                var lowercmp = (range.IncludeLower ? "ge" : "gt");
                var uppercmp = (range.IncludeUpper ? "le" : "lt");
                if (double.IsNegativeInfinity(range.Lower))
                {
                    filter.Append($"{range.Property} {uppercmp} {range.Upper}");
                }
                else if (double.IsPositiveInfinity(range.Upper))
                {
                    filter.Append($"{range.Property} {lowercmp} {range.Lower}");
                }
                else if (range.Lower == range.Upper)
                {
                    filter.Append($"{range.Property} eq {range.Lower}");
                }
                else
                {
                    filter.Append($"({range.Property} {lowercmp} {range.Lower} and {range.Property} {uppercmp} {range.Upper})");
                }
                seperator = " and ";
            }
            return filter.ToString();
        }
    }
}
