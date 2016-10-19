
namespace Microsoft.Bot.Connector
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Rest;
    using System.Linq;

    public static class ClaimsIdentityEx
    {
        /// <summary>
        /// Get the AppId from the Claims Identity
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static string GetAppIdFromClaims(this ClaimsIdentity identity)
        {
            if (identity == null)
                return null;

            // emulator adds appid claim
            Claim botClaim = identity.Claims.FirstOrDefault(c => c.Type == "appid");
            if (botClaim != null)
                return botClaim.Value;

            // Fallback for BF-issued tokens
            botClaim = identity.Claims.FirstOrDefault(c => c.Issuer == "https://api.botframework.com" && c.Type == "aud");
            if (botClaim != null)
                return botClaim.Value;

            return null;
        }
    }
}
