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
using System.Text;
using Microsoft.Azure.Search.Models;

namespace Microsoft.Bot.Sample.SearchDialogs
{
    [Serializable]
    public class SearchQueryBuilder
    {
        private const int DefaultHitPerPage = 5;

        public SearchQueryBuilder()
        {
            this.Refinements = new Dictionary<string, IEnumerable<string>>();
        }

        public string SearchText { get; set; }

        public int PageNumber { get; set; }

        public int HitsPerPage { get; set; } = DefaultHitPerPage;

        public Dictionary<string, IEnumerable<string>> Refinements { get; private set; }

        public virtual SearchParameters BuildParameters()
        {
            SearchParameters parameters = new SearchParameters
            {
                Top = this.HitsPerPage,
                Skip = this.PageNumber * this.HitsPerPage,
                SearchMode = SearchMode.All
            };

            if (this.Refinements.Count > 0)
            {
                StringBuilder filter = new StringBuilder();
                string separator = string.Empty;

                foreach (var entry in this.Refinements)
                {
                    foreach (string value in entry.Value)
                    {
                        filter.Append(separator);
                        filter.Append($"{entry.Key} eq '{EscapeFilterString(value)}'");
                        separator = " and ";
                    }
                }

                parameters.Filter = filter.ToString();
            }

            return parameters;
        }

        public virtual void Reset()
        {
            this.SearchText = null;
            this.PageNumber = 0;
            this.Refinements.Clear();
        }

        private static string EscapeFilterString(string s)
        {
            return s.Replace("'", "''");
        }
    }
}
