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

namespace Microsoft.Bot.Sample.SearchDialogs
{
    [Serializable]
    public class SearchRefineDialog : IDialog<string>
    {
        protected readonly string refiner;
        protected readonly SearchQueryBuilder queryBuilder;
        protected readonly PromptStyler promptStyler;
        protected readonly string prompt;

        public SearchRefineDialog(string refiner, SearchQueryBuilder queryBuilder = null, PromptStyler promptStyler = null, string prompt = null)
        {
            if (refiner == null)
            {
                throw new ArgumentNullException("refiner");
            }

            this.refiner = refiner;
            this.queryBuilder = queryBuilder ?? new SearchQueryBuilder();
            this.promptStyler = promptStyler;
            this.prompt = prompt ?? $"Here's what I found for {this.refiner} (select 'cancel' if you don't want to select any of these):";
        }

        public async Task StartAsync(IDialogContext context)
        {
            SearchParameters parameters = this.queryBuilder.BuildParameters();
            parameters.Facets = new List<string> { this.refiner };
            DocumentSearchResult result = await SearchDialogIndexClient.Client.Documents.SearchAsync(this.queryBuilder.SearchText, parameters);

            List<string> options = new List<string>() { "cancel" };
            options.AddRange(result.Facets[this.refiner].Select(f => FormatRefinerOption((string)f.Value, f.Count.Value)));

            PromptOptions<string> promptOptions = new PromptOptions<string>(this.prompt, options: options.ToList(), promptStyler: this.promptStyler);
            PromptDialog.Choice(context, ApplyRefiner, promptOptions);
        }

        protected virtual string FormatRefinerOption(string value, long count)
        {
            return $"{value} ({count})";
        }

        protected virtual string ParseRefinerValue(string value)
        {
            return value.Substring(0, value.LastIndexOf('(') - 1);
        }

        public async Task ApplyRefiner(IDialogContext context, IAwaitable<string> input)
        {
            string selection = await input;

            if (selection != null && selection.ToLowerInvariant() == "cancel")
            {
                context.Done<string>(null);
            }
            else
            {
                string value = ParseRefinerValue(selection);

                if (this.queryBuilder != null)
                {
                    this.queryBuilder.Refinements.Add(this.refiner, new string[] { value });
                }

                context.Done(value);
            }
        }
    }
}
