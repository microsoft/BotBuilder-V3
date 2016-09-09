using Microsoft.Rest;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public static class ErrorHandling
    {
        public static async Task HandleErrorAsync(this HttpOperationResponse<object> result)
        {
            if (!result.Response.IsSuccessStatusCode)
            {
                APIResponse errorMessage = result.Body as APIResponse;

                string _requestContent = null;
                if (result.Request != null && result.Request.Content != null)
                {
                    try
                    {
                        _requestContent = await result.Request.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    catch (ObjectDisposedException)
                    {
                        //result.Request.Content is disposed. 
                        _requestContent = null;
                    }
                }

                string _responseContent = null;
                if (result.Response != null && result.Response.Content != null)
                {
                    try
                    {
                        _responseContent = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    catch (ObjectDisposedException)
                    {
                        _responseContent = null;
                    }
                }

                throw new HttpOperationException(String.IsNullOrEmpty(errorMessage?.Message) ? result.Response.ReasonPhrase : errorMessage.Message)
                {
                    Request = new HttpRequestMessageWrapper(result.Request, _requestContent),
                    Response = new HttpResponseMessageWrapper(result.Response, _responseContent),
                    Body = result.Body
                };
            }
        }

        public static async Task<ObjectT> HandleErrorAsync<ObjectT>(this HttpOperationResponse<object> result)
        {
            if (!result.Response.IsSuccessStatusCode)
            {
                APIResponse errorMessage = result.Body as APIResponse;

                string _requestContent = null;
                if (result.Request != null && result.Request.Content != null)
                {
                    try
                    {
                        _requestContent = await result.Request.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    catch (ObjectDisposedException)
                    {
                        //result.Request.Content is disposed. 
                        _requestContent = null;
                    }
                }

                string _responseContent = null;
                if (result.Response != null && result.Response.Content != null)
                {
                    try
                    {
                        _responseContent = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    catch (ObjectDisposedException)
                    {
                        _responseContent = null;
                    }
                }

                throw new HttpOperationException(String.IsNullOrEmpty(errorMessage?.Message) ? result.Response.ReasonPhrase : errorMessage.Message)
                {
                    Request = new HttpRequestMessageWrapper(result.Request, _requestContent),
                    Response = new HttpResponseMessageWrapper(result.Response, _responseContent),
                    Body = result.Body
                };
            }
            if (typeof(ObjectT).IsArray)
            {
                IList list = (IList)result.Body;
                if (list == null)
                {
                    return default(ObjectT);
                }
                IList array = (IList)Array.CreateInstance(typeof(ObjectT).GetElementType(), list.Count);
                int i = 0;
                foreach (var el in list)
                    array[i++] = el;
                return (ObjectT)array;
            }
            return (ObjectT)result.Body;
        }
    }
}