using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters;
using System.Web;

namespace Microsoft.Bot.Builder
{
    public static class CookieHelper
    {
        /// <summary>
        /// Example retrieving a session cookie and pulling a value from it
        /// </summary>
        /// <returns></returns>
        public static TypeT GetSessionCookie<TypeT>(HttpRequestMessage request, string name)
            where TypeT : class
        {
            TypeT value = default(TypeT);
            CookieHeaderValue cookie = request.Headers.GetCookies(name).FirstOrDefault();
            if (cookie != null)
            {
                value = Serializers.CookieSerializer.Deserialize<TypeT>(cookie[name].Value); 
            }
            return value;
        }

        /// <summary>
        /// Example of tracking session state by serializing into a session cookie
        /// </summary>
        /// <param name="value"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static CookieHeaderValue CreateSessionCookie<TypeT>(HttpResponseMessage response, string domain, string name, TypeT value, TimeSpan expiry)
        {
            return CreateCookieHeaderValue<TypeT>(domain, name, value, expiry);
        }

        /// <summary>
        /// Example of routine to create a cookieHeaderValue used for setting cookie values as part of async callbacks
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static CookieHeaderValue CreateCookieHeaderValue<TypeT>(string domain, string name, TypeT value, TimeSpan expiry)
        {
            var cookie = new CookieHeaderValue(name, Serializers.CookieSerializer.Serialize(value));
            cookie.Expires = DateTimeOffset.Now + expiry;
            cookie.Domain = domain;
            cookie.Path = "/";
            return cookie;
        }
    }
}