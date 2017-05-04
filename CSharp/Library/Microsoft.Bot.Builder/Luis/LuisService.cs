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
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Rest;
using Newtonsoft.Json;

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
        public string Query;

        /// <summary>
        /// Indicates if logging of queries to LUIS is allowed.
        /// </summary>
        public bool? Log;

        /// <summary>
        /// Turn on spell checking.
        /// </summary>
        public bool? SpellCheck;

        /// <summary>
        /// Use the staging endpoint.
        /// </summary>
        public bool? Staging;

        /// <summary>
        /// The time zone offset.
        /// </summary>
        public double? TimezoneOffset;

        /// <summary>
        /// The verbose flag.
        /// </summary>
        public bool? Verbose;

        /// <summary>
        /// Any extra query parameters for the URL.
        /// </summary>
        public string ExtraParameters;

        /// <summary>
        /// The context id.
        /// </summary>
        [Obsolete("Action binding in LUIS should be replaced with code.")]
        public string ContextId;

        /// <summary>
        /// Force setting the parameter when using action binding.
        /// </summary>
        [Obsolete("Action binding in LUIS should be replaced with code.")]
        public string ForceSet;

        /// <summary>
        /// Constructs an instance of the LuisReqeuest.
        /// </summary>
        /// <param name="query"> The text query.</param>
        /// <param name="log"> Allow queries to be logged by LUIS.</param>
        /// <param name="spellCheck">Turn on spell checking.</param>
        /// <param name="staging">Whether or not to use staging.</param>
        /// <param name="timezoneOffset"> The time zone offset used for resolving time expressions.</param>
        /// <param name="verbose"> Indicates if the <see cref="LuisResult"/> should be verbose.</param>
        /// <param name="contextId"> The context id for Luis dialog.</param>
        /// <param name="forceSet"> Force setting the parameter when using action binding.</param>
        /// <param name="extraParameters">Extra URL query parameters.</param>
        public LuisRequest(string query,
            bool? log = default(bool?), bool? spellCheck = default(bool?), bool? staging = default(bool?), double? timezoneOffset = default(double?), bool? verbose = default(bool?),
            string contextId = default(string), string forceSet = default(string), string extraParameters = null
            )
        {
            this.Query = query;
            this.Log = log;
            this.SpellCheck = spellCheck;
            this.Staging = staging;
            this.Verbose = verbose;
            this.TimezoneOffset = timezoneOffset;
#pragma warning disable CS0618
            this.ContextId = contextId;
            this.ForceSet = forceSet;
#pragma warning restore CS0618
            this.ExtraParameters = extraParameters;
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
#pragma warning disable CS0612
                case LuisApiVersion.V1:
                    builder = new UriBuilder(model.UriBase);
                    queryParameters.Add($"id={id}");
                    break;
#pragma warning restore CS0612
                case LuisApiVersion.V2:
                    //v2.0 have the model as path parameter
                    builder = new UriBuilder(new Uri(model.UriBase, id));
                    break;
                default:
                    throw new ArgumentException($"{model.ApiVersion} is not a valid Luis api version.");
            }

            if (Log != null)
            {
                queryParameters.Add($"log={Uri.EscapeDataString(Convert.ToString(Log))}");
            }
            if (SpellCheck != null)
            {
                queryParameters.Add($"spellCheck={Uri.EscapeDataString(Convert.ToString(SpellCheck))}");
            }
            if (Staging != null)
            {
                queryParameters.Add($"staging={Uri.EscapeDataString(Convert.ToString(Staging))}");
            }
            if (TimezoneOffset != null)
            {
                queryParameters.Add($"timezoneOffset={Uri.EscapeDataString(Convert.ToString(TimezoneOffset))}");
            }
            if (Verbose != null)
            {
                queryParameters.Add($"verbose={Uri.EscapeDataString(Convert.ToString(Verbose))}");
            }
#pragma warning disable CS0618
            if (ContextId != null)
            {
                queryParameters.Add($"contextId={Uri.EscapeDataString(ContextId)}");
            }
            if (ForceSet != null)
            {
                queryParameters.Add($"forceSet={Uri.EscapeDataString(ForceSet)}");
            }
#pragma warning restore CS0618
            if (ExtraParameters != null)
            {
                queryParameters.Add(ExtraParameters);
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
        /// Modify the incoming LUIS request.
        /// </summary>
        /// <param name="request">Request so far.</param>
        /// <returns>Modified request.</returns>
        LuisRequest ModifyRequest(LuisRequest request);

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

        public LuisRequest ModifyRequest(LuisRequest request)
        {
            return model.ModifyRequest(request);
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
            return await service.QueryAsync(new LuisRequest(query: text), token);
        }

        /// <summary>
        /// Query the LUIS service using this request.
        /// </summary>
        /// <param name="service">LUIS service.</param>
        /// <param name="request">Query request.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>LUIS result.</returns>
        public static async Task<LuisResult> QueryAsync(this ILuisService service, LuisRequest request, CancellationToken token)
        {
            var uri = service.BuildUri(request);
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
            return service.BuildUri(service.ModifyRequest(new LuisRequest(query: text)));
        }
    }
}

