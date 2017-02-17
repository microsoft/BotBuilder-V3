// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Rest;

namespace Microsoft.Bot.Builder.Luis
{
    /// <summary>
    /// Object that contains all the possible parameters to build Luis request.
    /// </summary>
    public sealed class LuisRequest
    {
        /// <summary>
        /// The text query.
        /// </summary>
        public readonly string Query;

        /// <summary>
        /// The time zone offset.
        /// </summary>
        public readonly double? TimezoneOffset;

        /// <summary>
        /// The context id.
        /// </summary>
        public readonly string ContextId;

        /// <summary>
        /// The verbose flag.
        /// </summary>
        public readonly bool? Verbose;

        /// <summary>
        /// Force setting the parameter.
        /// </summary>
        public readonly string ForceSet;

        /// <summary>
        /// Indicates if sampling is allowed.
        /// </summary>
        public readonly string AllowSampling;

        /// <summary>
        /// Constructs an instance of the LuisReqeuest.
        /// </summary>
        /// <param name="query"> The text query.</param>
        /// <param name="timezoneOffset"> The time zone offset.</param>
        /// <param name="contextId"> The context id for Luis dialog.</param>
        /// <param name="verbose"> Indicates if the <see cref="LuisResult"/> should be verbose.</param>
        /// <param name="forceSet"> Force setting the parameter.</param>
        /// <param name="allowSampling"> Allow sampling.</param>
        public LuisRequest(string query, double? timezoneOffset = default(double?),
            string contextId = default(string), bool? verbose = default(bool?), string forceSet = default(string),
            string allowSampling = default(string))
        {
            this.Query = query;
            this.TimezoneOffset = timezoneOffset;
            this.ContextId = contextId;
            this.Verbose = verbose;
            this.ForceSet = forceSet;
            this.AllowSampling = allowSampling;
        }

        /// <summary>
        /// Build the Uri for issuing the request for the specified Luis model.
        /// </summary>
        /// <param name="model"> The Luis model.</param>
        /// <returns> The request Uri.</returns>
        public Uri BuildUri(ILuisModel model)
        {
            if (model.ModelID == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "id");
            }
            if (model.SubscriptionKey == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "subscriptionKey");
            }

            var queryParameters = new List<string>();
            queryParameters.Add($"subscription-key={Uri.EscapeDataString(model.SubscriptionKey)}");
            queryParameters.Add($"q={Uri.EscapeDataString(Query)}");
            UriBuilder builder;

            var id = Uri.EscapeDataString(model.ModelID);
            switch (model.ApiVersion)
            {
                case LuisApiVersion.V1:
                    builder = new UriBuilder(model.UriBase);
                    queryParameters.Add($"id={id}");
                    break;
                case LuisApiVersion.V2:
                    //v2.0 have the model as path parameter
                    builder = new UriBuilder(new Uri(model.UriBase, id));
                    break;
                default:
                    throw new ArgumentException($"{model.ApiVersion} is not a valid Luis api version.");
            }

            if (TimezoneOffset != null)
            {
                queryParameters.Add($"timezoneOffset={Uri.EscapeDataString(Convert.ToString(TimezoneOffset))}");
            }
            if (ContextId != null)
            {
                queryParameters.Add($"contextId={Uri.EscapeDataString(ContextId)}");
            }
            if (Verbose != null)
            {
                queryParameters.Add($"verbose={Uri.EscapeDataString(Convert.ToString(Verbose))}");
            }
            if (ForceSet != null)
            {
                queryParameters.Add($"forceSet={Uri.EscapeDataString(ForceSet)}");
            }
            if (AllowSampling != null)
            {
                queryParameters.Add($"allowSampling={Uri.EscapeDataString(AllowSampling)}");
            }

            builder.Query = string.Join("&", queryParameters);
            return builder.Uri;
        }
    }

    /// <summary>
    /// A mockable interface for the LUIS service.
    /// </summary>
    public interface ILuisService
    {
        /// <summary>
        /// Build the query uri for the <see cref="LuisRequest"/>.
        /// </summary>
        /// <param name="luisRequest">The luis request text.</param>
        /// <returns>The query uri.</returns>
        Uri BuildUri(LuisRequest luisRequest);

        /// <summary>
        /// Query the LUIS service using this uri.
        /// </summary>
        /// <param name="uri">The query uri.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The LUIS result.</returns>
        Task<LuisResult> QueryAsync(Uri uri, CancellationToken token);
    }

    /// <summary>
    /// Standard implementation of ILuisService against actual LUIS service.
    /// </summary>
    [Serializable]
    public sealed class LuisService : ILuisService
    {
        private readonly ILuisModel model;

        /// <summary>
        /// Construct the LUIS service using the model information.
        /// </summary>
        /// <param name="model">The LUIS model information.</param>
        public LuisService(ILuisModel model)
        {
            SetField.NotNull(out this.model, nameof(model), model);
        }

        Uri ILuisService.BuildUri(LuisRequest luisRequest)
        {
            return luisRequest.BuildUri(this.model);
        }

        async Task<LuisResult> ILuisService.QueryAsync(Uri uri, CancellationToken token)
        {
            string json;
            using (var client = new HttpClient())
            using (var response = await client.GetAsync(uri, HttpCompletionOption.ResponseContentRead, token))
            {
                response.EnsureSuccessStatusCode();
                json = await response.Content.ReadAsStringAsync();
            }

            try
            {
                var result = JsonConvert.DeserializeObject<LuisResult>(json);
                // fix up Luis result for backward compatibility
                // v2 api is not returning list of intents if verbose query parameter 
                // is not set. This will move IntentRecommendation in TopScoringIntent
                // to list of Intents.
                if (result.TopScoringIntent != null && result.Intents == null)
                {
                    result.Intents = new List<IntentRecommendation> { result.TopScoringIntent };
                }
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
        /// <param name="token">The cancellation token.</param>
        /// <returns>The LUIS result.</returns>
        public static async Task<LuisResult> QueryAsync(this ILuisService service, string text, CancellationToken token)
        {
            var uri = service.BuildUri(new LuisRequest(query: text));
            return await service.QueryAsync(uri, token);
        }
        
        /// <summary>
        /// Builds luis uri with text query.
        /// </summary>
        /// <param name="service">LUIS service.</param>
        /// <param name="text">The query text.</param>
        /// <returns>The LUIS request Uri.</returns>
        public static Uri BuildUri(this ILuisService service, string text)
        {
            return service.BuildUri(new LuisRequest(query: text));
        }
    }
}

