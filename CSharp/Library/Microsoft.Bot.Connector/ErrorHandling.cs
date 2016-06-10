using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public static class ErrorHandling
    {
        public static void HandleError(this HttpOperationResponse<object> result)
        {
            if (!result.Response.IsSuccessStatusCode)
            {
                APIResponse errorMessage = result.Body as APIResponse;
                throw new HttpOperationException(String.IsNullOrEmpty(errorMessage?.Message) ? result.Response.ReasonPhrase : errorMessage.Message)
                {
                    Request = result.Request,
                    Response = result.Response,
                    Body = result.Body
                };
            }
        }

        public static ObjectT HandleError<ObjectT>(this HttpOperationResponse<object> result)
        {
            if (!result.Response.IsSuccessStatusCode)
            {
                APIResponse errorMessage = result.Body as APIResponse;
                throw new HttpOperationException(String.IsNullOrEmpty(errorMessage?.Message) ? result.Response.ReasonPhrase : errorMessage.Message)
                {
                    Request = result.Request,
                    Response = result.Response,
                    Body = result.Body
                };
            }
            return (ObjectT)result.Body;
        }
    }
}