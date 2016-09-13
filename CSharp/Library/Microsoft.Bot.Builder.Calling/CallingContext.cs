using Microsoft.Bot.Builder.Internals.Fibers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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
        public string SkypeChaindId { set; get; }

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
                    parsedRequest = GenerateParsedResults(HttpStatusCode.InternalServerError);
                    break;
            }
            parsedRequest.SkypeChaindId = ExtractSkypeChainId();
            return parsedRequest;
        }

        protected virtual string ExtractSkypeChainId()
        {
            string chaindId = null;
            IEnumerable<string> headerValues;
            if (Request.Headers.TryGetValues("X-Microsoft-Skype-Chain-ID", out headerValues))
            {
                chaindId = headerValues.FirstOrDefault();
            }
            return chaindId;
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
                return GenerateParsedResults(HttpStatusCode.InternalServerError);
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
                return GenerateParsedResults(HttpStatusCode.InternalServerError);
            }
        }

        private ParsedCallingRequest GenerateParsedResults(HttpStatusCode statusCode, string content = null, Task<Stream> additionalData = null)
        {
            return new ParsedCallingRequest
            {
                Content = content,
                ParseStatusCode = statusCode,
                AdditionalData = additionalData
            };
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
    }
}
