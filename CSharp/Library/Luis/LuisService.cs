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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis.Models;

namespace Microsoft.Bot.Builder.Luis
{
    /// <summary>
    /// A mockable interface for the LUIS service.
    /// </summary>
    public interface ILuisService
    {
        /// <summary>
        /// Build the query uri for the query text.
        /// </summary>
        /// <param name="text">The query text.</param>
        /// <returns>The query uri.</returns>
        Uri BuildUri(string text);

        /// <summary>
        /// Query the LUIS service using this uri.
        /// </summary>
        /// <param name="uri">The query uri.</param>
        /// <returns>The LUIS result.</returns>
        Task<LuisResult> QueryAsync(Uri uri);
    }

    /// <summary>
    /// Standard implementation of ILuisService against actual LUIS service.
    /// </summary>
    [Serializable]
    public sealed class LuisService : ILuisService
    {
        private readonly LuisModelAttribute model;

        /// <summary>
        /// Construct the LUIS service using the model information.
        /// </summary>
        /// <param name="model">The LUIS model information.</param>
        public LuisService(LuisModelAttribute model)
        {
            SetField.NotNull(out this.model, nameof(model), model);
        }

        /// <summary>
        /// The base URi for accessing LUIS.
        /// </summary>
        public static readonly Uri UriBase = new Uri("https://api.projectoxford.ai/luis/v1/application");

        Uri ILuisService.BuildUri(string text)
        {
            var id = HttpUtility.UrlEncode(this.model.ModelID);
            var sk = HttpUtility.UrlEncode(this.model.SubscriptionKey);
            var q = HttpUtility.UrlEncode(text);

            var builder = new UriBuilder(UriBase);
            builder.Query = $"id={id}&subscription-key={sk}&q={q}";
            return builder.Uri;
        }

        async Task<LuisResult> ILuisService.QueryAsync(Uri uri)
        {
            string json;
            using (HttpClient client = new HttpClient())
            {
                json = await client.GetStringAsync(uri);
            }

            try
            {
                var result = JsonConvert.DeserializeObject<LuisResult>(json);
                return result;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Unable to deserialize the LUIS response.", ex);
            }
        }
    }

    /// <summary>
    /// LUIS extension methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Query the LUIS service using this text.
        /// </summary>
        /// <param name="service">LUIS service.</param>
        /// <param name="text">The query text.</param>
        /// <returns>The LUIS result.</returns>
        public static async Task<LuisResult> QueryAsync(this ILuisService service, string text)
        {
            var uri = service.BuildUri(text);
            return await service.QueryAsync(uri);
        }
    }
}

