using System;
using System.Net;
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
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                var token = await credentials.GetTokenAsync(true).ConfigureAwait(false);
                await credentials.ProcessHttpRequestAsync(request, cancellationToken).ConfigureAwait(false);
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            if (this.Unauthorized(response.StatusCode))
            {
                var statusCode = response.StatusCode;
                response.Dispose();
                throw new UnauthorizedAccessException($"Authorization for Microsoft App ID {credentials.MicrosoftAppId} failed with status code {statusCode}");
            }

            return response;
        }

        private bool Unauthorized(System.Net.HttpStatusCode code)
        {
            return code == HttpStatusCode.Forbidden || code == HttpStatusCode.Unauthorized;
        }
    }
}
