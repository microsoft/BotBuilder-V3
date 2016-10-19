// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Bot.Builder.Dialogs;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Sample.SearchDialogs
{
    [Serializable]
    public class SearchRefineDialog : IDialog<FilterExpression>
    {
        protected readonly SearchField refiner;
        protected double rangeMin;
        protected readonly SearchQueryBuilder queryBuilder;
        protected readonly PromptStyler promptStyler;
        protected readonly string prompt;

        public SearchRefineDialog(SearchField refiner, SearchQueryBuilder queryBuilder = null, PromptStyler promptStyler = null, string prompt = null)
        {
            if (refiner == null)
            {
                throw new ArgumentNullException("refiner");
            }

            this.refiner = refiner;
            this.queryBuilder = queryBuilder ?? new SearchQueryBuilder();
            this.promptStyler = promptStyler;
            this.prompt = prompt ?? $"Here's what I found for {this.refiner.Name}:";
            this.queryBuilder.Refinements.Remove(refiner.Name);
        }

        public async Task StartAsync(IDialogContext context)
        {
            SearchParameters parameters = this.queryBuilder.BuildParameters();
            parameters.Facets = new List<string> { this.refiner.Name };
            DocumentSearchResult result = await SearchDialogIndexClient.Client.Documents.SearchAsync(this.queryBuilder.SearchText, parameters);

            List<string> options = new List<string>();
            var choices = (from facet in result.Facets[this.refiner.Name] orderby facet.Value ascending select facet);
            if (refiner.FilterPreference == PreferredFilter.None)
            {
                foreach (var choice in choices)
                {
                    options.Add($"{choice.Value} ({choice.Count})");
                }
            }
            else if (refiner.FilterPreference == PreferredFilter.MinValue)
            {
                var total = choices.Sum((choice) => choice.Count.Value);
                foreach (var choice in choices)
                {
                    options.Add($"{choice.Value}+ ({total})");
                    total -= choice.Count.Value;
                }
            }
            else if (refiner.FilterPreference == PreferredFilter.MaxValue)
            {
                long total = 0;
                foreach (var choice in choices)
                {
                    total += choice.Count.Value;
                    options.Add($"<= {choice.Value} ({total})");
                }
            }
            if (options.Any())
            {
                options.Add("Any");
                PromptOptions<string> promptOptions = new PromptOptions<string>(this.prompt, retry: "I did not understand, try one of these choices:", options: options.ToList(), promptStyler: this.promptStyler);
                PromptDialog.Choice(context, ApplyRefiner, promptOptions);
            }
            else if (refiner.FilterPreference == PreferredFilter.RangeMin)
            {
                PromptDialog.Number(context, MinRefiner, $"What is the minimum {this.refiner.Name}?");
            }
            else if (refiner.FilterPreference == PreferredFilter.RangeMax)
            {
                PromptDialog.Number(context, MinRefiner, $"What is the maximum {this.refiner.Name}?");
            }
            else if (refiner.FilterPreference == PreferredFilter.Range)
            {
                PromptDialog.Number(context, RangeMin, $"What is the minimum {this.refiner.Name}?");
            }
        }

        public async Task MinRefiner(IDialogContext context, IAwaitable<double> number)
        {
            var expression = new FilterExpression(Operator.GreaterThanOrEqual, await number);
            this.queryBuilder.Refinements.Add(this.refiner.Name, expression);
            context.Done<FilterExpression>(expression);
        }

        public async Task MaxRefiner(IDialogContext context, IAwaitable<double> number)
        {
            var expression = new FilterExpression(Operator.LessThanOrEqual, await number);
            this.queryBuilder.Refinements.Add(this.refiner.Name, expression);
            context.Done<FilterExpression>(expression);
        }

        public async Task RangeMin(IDialogContext context, IAwaitable<double> min)
        {
            rangeMin = await min;
            PromptDialog.Number(context, RangeMax, $"What is the maximum {this.refiner.Name}?");
        }

        public async Task RangeMax(IDialogContext context, IAwaitable<double> max)
        {
            var expression = new FilterExpression(Operator.And, new FilterExpression(Operator.GreaterThanOrEqual, rangeMin),
                new FilterExpression(Operator.LessThanOrEqual, await max));
            this.queryBuilder.Refinements.Add(this.refiner.Name, expression);
            context.Done(expression);
        }

        // Handles 3+, <=5 and 3-5.
        private static Regex extractValue = new Regex(@"(?<lt>\<\s*\=)?\s*(?<number1>[+-]?[0-9]+(.[0-9]+)?)\s*((?<gt>\+)|(-\s*(?<number2>[+-]?[0-9]+(.[0-9]+)?)))?", RegexOptions.Compiled);

        protected virtual FilterExpression ParseRefinerValue(SearchField field, string value)
        {
            var expression = new FilterExpression();

            if (field.Type == typeof(int) || field.Type == typeof(long) || field.Type == typeof(double))
            {
                var match = extractValue.Match(value);
                if (match.Success)
                {
                    var lt = match.Groups["lt"];
                    var gt = match.Groups["gt"];
                    var number1 = match.Groups["number1"];
                    var number2 = match.Groups["number2"];
                    if (number1.Success)
                    {
                        double num1;
                        if (double.TryParse(number1.Value, out num1))
                        {
                            if (lt.Success)
                            {
                                expression = new FilterExpression(Operator.LessThanOrEqual, num1);
                            }
                            else if (gt.Success)
                            {
                                expression = new FilterExpression(Operator.GreaterThanOrEqual, num1);
                            }
                            else if (number2.Success)
                            {
                                double num2;
                                if (double.TryParse(number2.Value, out num2) && num1 <= num2)
                                {
                                    expression = new FilterExpression(Operator.And,
                                            new FilterExpression(Operator.GreaterThanOrEqual, num1),
                                            new FilterExpression(Operator.LessThanOrEqual, num2));
                                }
                            }
                        }
                    }
                }
            }
            else if (field.Type == typeof(string) || field.Type == typeof(string[]))
            {
                // We append <space>(<count>) to the facet name, undo that here so filters work
                expression = new FilterExpression(Operator.Equal, value.Substring(0, value.LastIndexOf('(') - 1));
            }
            else
            {
                throw new NotSupportedException($"Unsupported field type: {field.Type.Name}");
            }

            return expression;
        }

        public async Task ApplyRefiner(IDialogContext context, IAwaitable<string> input)
        {
            string selection = await input;

            if (selection != null)
            {
                if (selection.Trim().ToLowerInvariant() == "any")
                {
                    this.queryBuilder.Refinements.Remove(this.refiner.Name);
                    context.Done<FilterExpression>(null);
                }
                else
                {
                    var expression = ParseRefinerValue(this.refiner, selection);
                    if (expression.Operator != Operator.None)
                    {
                        this.queryBuilder.Refinements.Add(this.refiner.Name, expression);
                        context.Done(expression);
                    }
                }
            }
        }
    }
}
