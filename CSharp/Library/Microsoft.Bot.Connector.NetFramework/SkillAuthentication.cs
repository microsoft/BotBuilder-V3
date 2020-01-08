// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using Microsoft.Bot.Connector.Authentication;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Connector
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class SkillAuthenticationAttribute : BotAuthentication
    {
        /// <summary>
        /// Type which implements AuthenticationConfiguration to allow validation for Skills 
        /// </summary>
        public Type AuthenticationConfigurationProviderType { get; set; }

        private static HttpClient _httpClient = new HttpClient();

        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var authorizationHeader = actionContext.Request.Headers.Authorization;
            if (authorizationHeader != null && SkillValidation.IsSkillToken(authorizationHeader.ToString()))
            {
                var activities = base.GetActivities(actionContext);
                if (activities.Any())
                {
                    var authConfiguration = GetAuthenticationConfiguration();

                    var credentialProvider = this.GetCredentialProvider();

                    var identity = await JwtTokenValidation.AuthenticateRequest(activities[0], authorizationHeader.ToString(), credentialProvider, authConfiguration, _httpClient).ConfigureAwait(false);

                    MicrosoftAppCredentials.TrustServiceUrl(activities[0].ServiceUrl, oauthScope: JwtTokenValidation.GetAppIdFromClaims(identity.Claims));
                   
                    await base.BaseOnActionExecutingAsync(actionContext, cancellationToken);
                    return;
                }
            }

            await base.OnActionExecutingAsync(actionContext, cancellationToken);
        }

        protected AuthenticationConfiguration GetAuthenticationConfiguration()
        {
            AuthenticationConfiguration authenticationConfigurationProvider = null;
            if (AuthenticationConfigurationProviderType != null)
            {
                authenticationConfigurationProvider = Activator.CreateInstance(AuthenticationConfigurationProviderType) as AuthenticationConfiguration;
                if (authenticationConfigurationProvider == null)
                    throw new ArgumentNullException($"The AuthenticationConfigurationProviderType {AuthenticationConfigurationProviderType.Name} couldn't be instantiated with no params or doesn't implement AuthenticationConfiguration");
            }

            return authenticationConfigurationProvider ?? new AuthenticationConfiguration();
        }
    }
}
