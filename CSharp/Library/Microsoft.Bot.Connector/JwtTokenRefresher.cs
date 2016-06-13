using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public class JwtTokenRefresher : DelegatingHandler
    {
        private readonly MicrosoftAppCredentials credentials;

        public JwtTokenRefresher(MicrosoftAppCredentials credentials)
            : base()
        {
            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }
            this.credentials = credentials;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if (this.Unathorized(response.StatusCode))
            {
                var token = await credentials.GetTokenAsync(true);
                await credentials.ProcessHttpRequestAsync(request, cancellationToken);
                response = await base.SendAsync(request, cancellationToken);
            }

            if (this.Unathorized(response.StatusCode))
            {
                throw new UnauthorizedAccessException($"Security token for MicrosoftAppId: {credentials.MicrosoftAppId} is unauthorized to post to connector!");
            }

            return response;
        }

        private bool Unathorized(System.Net.HttpStatusCode code)
        {
            return code == System.Net.HttpStatusCode.Forbidden || code == System.Net.HttpStatusCode.Unauthorized;
        }
    }
}
