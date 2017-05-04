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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Calling
{
    /// <summary>
    /// The parsed calling request
    /// </summary>
    public class ParsedCallingRequest
    {
        /// <summary>
        /// The status code indicating if the request parser was successful or not.
        /// </summary>
        /// <remarks>
        /// <see cref="HttpStatusCode.OK"/> if successful.
        /// </remarks>
        public HttpStatusCode ParseStatusCode { set; get; }

        /// <summary>
        /// The calling request content.
        /// </summary>
        public string Content { set; get; }

        /// <summary>
        /// The additional data when calling request has multipart content. 
        /// </summary>
        public Task<Stream> AdditionalData { set; get; }

        /// <summary>
        /// The Skype chain Id, look at the documentation X-Microsoft-Skype-Chain-ID header value.
        /// </summary>
        public string SkypeChainId { set; get; }

        /// <summary>
        /// Check if the parser is faulted on parsing incoming request.
        /// </summary>
        /// <returns> True if <see cref="CallingContext"/> is successful in processing the request; False otherwise.</returns>
        public bool Faulted()
        {
            return ParseStatusCode != HttpStatusCode.OK;
        }
    }

    /// <summary>
    /// The type of calling request
    /// </summary>
    public enum CallRequestType
    {
        IncomingCall,
        CallingEvent
    }

    /// <summary>
    /// The context for this request. It is reponsible in parsing the calling request before calling into <see cref="ICallingBotService"/>.
    /// </summary>
    public class CallingContext
    {
        /// <summary>
        /// The calling request.
        /// </summary>
        public readonly HttpRequestMessage Request;

        /// <summary>
        /// Creates a new instance of calling context. 
        /// </summary>
        /// <param name="request"> The calling request.</param>
        public CallingContext(HttpRequestMessage request)
        {
            SetField.NotNull<HttpRequestMessage>(out this.Request, nameof(request), request);
        }

        /// <summary>
        /// Process the calling request and returns <see cref="ParsedCallingRequest"/>.
        /// </summary>
        /// <param name="callType"> The type of calling request.</param>
        /// <returns> the parsed calling request.</returns>
        public virtual async Task<ParsedCallingRequest> ProcessRequest(CallRequestType callType)
        {
            ParsedCallingRequest parsedRequest;
            switch (callType)
            {
                case CallRequestType.IncomingCall:
                    parsedRequest = await ProcessIncomingCallAsync();
                    break;
                case CallRequestType.CallingEvent:
                    parsedRequest = await ProcessCallingEventAsync();
                    break;              
                default:
                    parsedRequest = GenerateParsedResults(HttpStatusCode.BadRequest, $"{callType} not accepted");
                    break;
            }
            parsedRequest.SkypeChainId = ExtractSkypeChainId(this.Request);
            return parsedRequest;
        }
       
        protected virtual async Task<ParsedCallingRequest> ProcessIncomingCallAsync()
        {
            try
            {
                if (Request.Content == null)
                {
                    Trace.TraceError("No content in the request");
                    return GenerateParsedResults(HttpStatusCode.BadRequest);
                }

                var content = await Request.Content.ReadAsStringAsync().ConfigureAwait(false);
                return GenerateParsedResults(HttpStatusCode.OK, content);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Failed to process the incoming call, exception: {e}");
                return GenerateParsedResults(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        protected virtual async Task<ParsedCallingRequest> ProcessCallingEventAsync()
        {
            try
            {
                if (Request.Content == null)
                {
                    Trace.TraceError("No content in the request");
                    return GenerateParsedResults(HttpStatusCode.BadRequest);
                }

                if (Request.Content.IsMimeMultipartContent())
                {
                    return await HandleMultipartRequest(Request).ConfigureAwait(false);
                }

                var content = await Request.Content.ReadAsStringAsync().ConfigureAwait(false);
                return GenerateParsedResults(HttpStatusCode.OK, content);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Failed to process the callback request, exception: {e}");
                return GenerateParsedResults(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        private async Task<ParsedCallingRequest> HandleMultipartRequest(HttpRequestMessage request)
        {
            var streamProvider = await request.Content.ReadAsMultipartAsync().ConfigureAwait(false);
            var jsonContent =
                streamProvider.Contents.FirstOrDefault(content => content.Headers.ContentType?.MediaType == "application/json");
            if (jsonContent == null)
            {
                Trace.TraceError("No json content in MultiPart content");
                return GenerateParsedResults(HttpStatusCode.BadRequest);
            }
            var json = await jsonContent.ReadAsStringAsync().ConfigureAwait(false);

            var otherContent =
                streamProvider.Contents.FirstOrDefault(content => content.Headers.ContentType?.MediaType != "application/json");
            if (otherContent == null)
            {
                Trace.TraceError("MultiPart content does not contain non json content");
                return GenerateParsedResults(HttpStatusCode.BadRequest);
            }

            return GenerateParsedResults(HttpStatusCode.OK, json, otherContent.ReadAsStreamAsync());
        }

        /// <summary>
        /// Generate the <see cref="ParsedCallingRequest"/> from the arguments
        /// </summary>
        /// <param name="statusCode">Status code indicating if the parsing was successful or not</param>
        /// <param name="content">Content from the request. If the request had multipart content, the first part should be json and this will contain the first json content</param>
        /// <param name="additionalData">If the request had multipart content, this will contain the additional data present after the first json content</param>
        /// <returns></returns>
        public static ParsedCallingRequest GenerateParsedResults(HttpStatusCode statusCode, string content = null, Task<Stream> additionalData = null)
        {
            return new ParsedCallingRequest
            {
                Content = content,
                ParseStatusCode = statusCode,
                AdditionalData = additionalData
            };
        }

        /// <summary>
        /// Extracts the X-Microsoft-Skype-Chain-Id header from the request that is used for correlating logs across different services for a call
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string ExtractSkypeChainId(HttpRequestMessage request)
        {
            string chainId = null;
            IEnumerable<string> headerValues;
            if (request.Headers.TryGetValues("X-Microsoft-Skype-Chain-ID", out headerValues))
            {
                chainId = headerValues.FirstOrDefault();
            }
            return chainId;
        }
    }
}
