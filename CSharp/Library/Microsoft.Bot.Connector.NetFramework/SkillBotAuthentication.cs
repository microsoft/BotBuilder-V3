// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Microsoft.Bot.Connector.SkillAuthentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class SkillBotAuthentication : BotAuthentication
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
                    var authConfiguration = this.GetAuthenticationConfiguration();
                    var credentialProvider = this.GetCredentialProvider();
                    
                    try
                    {
                        foreach (var activity in activities)
                        {
                            var claimsIdentity = await JwtTokenValidation.AuthenticateRequest(activity, authorizationHeader.ToString(), credentialProvider, authConfiguration, _httpClient).ConfigureAwait(false);
                            // this is done in JwtTokenValidation.AuthenticateRequest, but the oauthScope is not set so we update it here
                            MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl, oauthScope: JwtTokenValidation.GetAppIdFromClaims(claimsIdentity.Claims), conversationId: activity.Conversation.Id);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        actionContext.Response = BotAuthenticator.GenerateUnauthorizedResponse(actionContext.Request, "BotAuthenticator failed to authenticate incoming request!");
                        return;
                    }

                    await base.ContinueOnActionExecutingAsync(actionContext, cancellationToken);
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

            return authenticationConfigurationProvider ?? new SkillAuthenticationConfiguration();
        }
    }
}
