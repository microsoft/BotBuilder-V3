using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{

    /// <summary>
    /// A custom redirect handler for <see cref="HttpStatusCode.RedirectKeepVerb"/>.
    /// </summary>
    /// <remarks>
    /// This makes sure that authorization headers stay intact between 307 redirects.
    /// </remarks>
    public sealed class CustomRedirectHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if(response.StatusCode == HttpStatusCode.RedirectKeepVerb && response.Headers.Contains("Location"))
            {
                request.RequestUri = new Uri(request.RequestUri, response.Headers.Location);
                response.Dispose();
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            return response;
        }
    }
}
