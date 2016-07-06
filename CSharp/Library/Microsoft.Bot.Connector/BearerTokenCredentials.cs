using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Use credentials from AppSetting "AppId" "AppSecret"
    /// </summary>
    public class BearerTokenCredentials : ServiceClientCredentials
    {
        public string Token { get; protected set; }

        /// <summary>
        /// Create a new instance of the BearerTokenCredentials class
        /// </summary>
        /// <param name="token">Bearer token</param>
        public BearerTokenCredentials(string token)
        {
            this.Token = token;
        }

        /// <summary>
        /// Apply the credentials to the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request.</param><param name="cancellationToken">Cancellation token.</param>
        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(this.Token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.Token);
            }
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}