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
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.SearchDialogs
{
    public delegate string CanonicalizerDelegate(string propertyName);

    [Serializable]
    public abstract class SearchDialog : IDialog<IList<SearchHit>>
    {
        protected readonly SearchQueryBuilder queryBuilder;
        protected readonly PromptStyler hitStyler;
        protected readonly bool multipleSelection;
        protected readonly CanonicalizerDelegate canonicalizer;
        private readonly List<SearchHit> selected = new List<SearchHit>();

        protected bool firstPrompt = true;
        private List<SearchHit> found;

        public SearchDialog(SearchQueryBuilder queryBuilder = null, PromptStyler searchHitStyler = null, bool multipleSelection = false, CanonicalizerDelegate canonicalizer = null)
        {
            this.queryBuilder = queryBuilder ?? new SearchQueryBuilder();
            this.hitStyler = searchHitStyler ?? new SearchHitStyler();
            this.multipleSelection = multipleSelection;
            this.canonicalizer = canonicalizer;
        }

        public Task StartAsync(IDialogContext context)
        {
            return InitialPrompt(context);
        }

        protected virtual Task InitialPrompt(IDialogContext context)
        {
            string prompt = "What would you like to search for?";

            if (!this.firstPrompt)
            {
                prompt = "What else would you like to search for?";
                if (this.multipleSelection)
                {
                    prompt += " You can also *list* all items you've added so far.";
                }
            }
            this.firstPrompt = false;

            PromptDialog.Text(context, Search, prompt);
            return Task.CompletedTask;
        }

        public async Task Search(IDialogContext context, IAwaitable<string> input)
        {
            string text = input != null ? await input : null;
            if (this.multipleSelection && text != null && text.ToLowerInvariant() == "list")
            {
                await ListAddedSoFar(context);
                await InitialPrompt(context);
            }
            else
            {
                if (text != null)
                {
                    this.queryBuilder.SearchText = text;
                }

                var response = await ExecuteSearch();

                if (response.Results.Count == 0)
                {
                    await NoResultsConfirmRetry(context);
                }
                else
                {
                    var message = context.MakeMessage();
                    this.found = response.Results.Select(r => ToSearchHit(r)).ToList();
                    this.hitStyler.Apply(ref message,
                                         "Here are a few good options I found:",
                                         this.found);
                    await context.PostAsync(message);
                    await context.PostAsync(
                        this.multipleSelection ?
                        "You can select one or more to add to your list, *list* what you've selected so far, *refine* these results, see *more* or search *again*." :
                        "You can select one, *refine* these results, see *more* or search *again*.");
                    context.Wait(ActOnSearchResults);
                }
            }
        }

        protected virtual Task NoResultsConfirmRetry(IDialogContext context)
        {
            PromptDialog.Confirm(context, ShouldRetry, "Sorry, I didn't find any matches. Do you want to retry your search?");
            return Task.CompletedTask;
        }

        private async Task ShouldRetry(IDialogContext context, IAwaitable<bool> input)
        {
            bool retry = await input;
            if (retry)
            {
                await InitialPrompt(context);
            }
            else
            {
                context.Done<IList<SearchHit>>(null);
            }
        }

        private async Task ActOnSearchResults(IDialogContext context, IAwaitable<IMessageActivity> input)
        {
            var activity = await input;
            var choice = activity.Text;

            switch (choice.ToLowerInvariant())
            {
                case "again":
                case "reset":
                    this.queryBuilder.Reset();
                    await InitialPrompt(context);
                    break;

                case "more":
                    this.queryBuilder.PageNumber++;
                    await Search(context, null);
                    break;

                case "refine":
                    SelectRefiner(context);
                    break;

                case "list":
                    await ListAddedSoFar(context);
                    context.Wait(ActOnSearchResults);
                    break;

                case "done":
                    context.Done<IList<SearchHit>>(this.selected);
                    break;

                default:
                    await AddSelectedItem(context, choice);
                    break;
            }
        }

        protected virtual async Task ListAddedSoFar(IDialogContext context)
        {
            var message = context.MakeMessage();
            if (this.selected.Count == 0)
            {
                await context.PostAsync("You have not added anything yet.");
            }
            else
            {
                hitStyler.Apply(ref message, "Here's what you've added to your list so far.", this.selected);
                await context.PostAsync(message);
            }
        }

        protected virtual async Task AddSelectedItem(IDialogContext context, string selection)
        {
            SearchHit hit = found.Find(h => h.Key == selection);
            if (hit == null)
            {
                await UnknownActionOnResults(context, selection);
            }
            else
            {
                if (!this.selected.Exists(h => h.Key == hit.Key))
                {
                    this.selected.Add(hit);
                }

                if (this.multipleSelection)
                {
                    await context.PostAsync("Added to your list!");
                    PromptDialog.Confirm(context, ShouldContinueSearching, "Do you want to continue searching and adding more items?");
                }
                else
                {
                    context.Done(this.selected);
                }
            }
        }

        protected virtual async Task UnknownActionOnResults(IDialogContext context, string action)
        {
            await context.PostAsync("Not sure what you mean. You can search *again*, *refine*, *list* or select one of the items above. Or are you *done*?");
            context.Wait(ActOnSearchResults);
        }

        protected virtual async Task ShouldContinueSearching(IDialogContext context, IAwaitable<bool> input)
        {
            bool shouldContinue = await input;
            if (shouldContinue)
            {
                await InitialPrompt(context);
            }
            else
            {
                context.Done(this.selected);
            }
        }

        protected void SelectRefiner(IDialogContext context)
        {
            // var dialog = new SearchLanguageDialog(canonicalizer);
            var dialog = new SearchSelectRefinerDialog(GetTopRefiners().Select(r => SearchDialogIndexClient.Schema.Fields[r]), this.queryBuilder);
            context.Call(dialog, Refine);
        }

        protected async Task Refine(IDialogContext context, IAwaitable<SearchField> input)
        {
            SearchField refiner = await input;
            var dialog = new SearchRefineDialog(refiner, this.queryBuilder);
            context.Call(dialog, ResumeFromRefine);
        }

        protected async Task ResumeFromRefine(IDialogContext context, IAwaitable<FilterExpression> input)
        {
            await input; // refiner filter is already applied to the SearchQueryBuilder instance we passed in
            await Search(context, null);
        }

        protected Task<DocumentSearchResult> ExecuteSearch()
        {
            return SearchDialogIndexClient.Client.Documents.SearchAsync(queryBuilder.SearchText, queryBuilder.BuildParameters());
        }

        protected abstract string[] GetTopRefiners();

        protected abstract SearchHit ToSearchHit(SearchResult hit);
    }
}
