using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Sample.GraphBot.Models
{
    public sealed class AuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Use the Active Directory Authentication Library to get and verify the token.
        /// </summary>
        public static async Task<AuthenticationResult> AcquireTokenSilentAsync(string clientID, string clientSecret, string objectIdentifier, string tenantID, TokenCache tokenCache)
        {
            var userId = new UserIdentifier(objectIdentifier, UserIdentifierType.UniqueId);
            var item = tokenCache.ReadItems().First(i => i.TenantId == tenantID);
            var context = new AuthenticationContext(item.Authority, tokenCache);

            var credential = new ClientCredential(clientID, clientSecret);
            AuthenticationResult result;

            try
            {
                result = await context.AcquireTokenSilentAsync(Keys.Resource, credential, userId);
            }
            catch (AdalSilentTokenAcquisitionException)
            {
                throw;
            }

            return result;
        }

        private readonly IClientKeys keys;
        private readonly IBotDataBag bag;
        public AuthenticationProvider(IClientKeys keys, IBotDataBag bag)
        {
            SetField.NotNull(out this.keys, nameof(keys), keys);
            SetField.NotNull(out this.bag, nameof(bag), bag);
        }

        async Task IAuthenticationProvider.AuthenticateRequestAsync(HttpRequestMessage request)
        {
            // look in the IBotDataBag for the token
            string objectIdentifier;
            string tenantID = null;
            byte[] tokenBlob = null;
            bool found
                = bag.TryGetValue(Keys.ObjectID, out objectIdentifier)
                && bag.TryGetValue(Keys.TenantID, out tenantID)
                && bag.TryGetValue(Keys.TokenCache, out tokenBlob);

            // if not found, then throw the exception that will restart the login flow
            if (! found)
            {
                throw new AdalSilentTokenAcquisitionException();
            }

            // deserialize the TokenCache and try to refresh the token silently
            var tokenCache = new TokenCache(tokenBlob);
            var token = await AcquireTokenSilentAsync(this.keys.ClientID, this.keys.ClientSecret, objectIdentifier, tenantID, tokenCache);

            // update the IBotDataBag with the new token if it's changed
            tokenBlob = tokenCache.Serialize();
            bag.SetValue(Keys.TokenCache, tokenBlob);

            // add the access token to the authentication header for the Microsoft Graph request
            var accessToken = token.AccessToken;
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
        }
    }
}
