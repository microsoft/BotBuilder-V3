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
            if (typeof(ObjectT).IsArray)
            {
                IList list = (IList)result.Body;
                if(list == null)
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