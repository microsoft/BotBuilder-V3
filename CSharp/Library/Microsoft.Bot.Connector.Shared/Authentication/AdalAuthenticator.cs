// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Shared.Authentication
{
    public class AdalAuthenticator 
    {
        private static readonly TimeSpan SemaphoreTimeout = TimeSpan.FromSeconds(1);
        private const int MaxRetries = 5;

        // ADAL recommends having a single authentication context and reuse across requests.
        // An authentication context also has an internal token cache which we can reuse.
        private static AuthenticationContext authContext = new AuthenticationContext(JwtConfig.ToChannelFromBotLoginUrl) { ExtendedLifeTimeEnabled = true };

        // We limit concurrency when acquiring tokens. Experiments show that if we don't limit concurrency,
        // when a token is refreshed we get throttled 20x more and also response times are 4x slower under load tests.
        private static SemaphoreSlim authContextSemaphore = new SemaphoreSlim(50, 50);

        private readonly ClientCredential clientCredential;

        public AdalAuthenticator(ClientCredential clientCredential)
        {
            this.clientCredential = clientCredential ?? throw new ArgumentNullException(nameof(clientCredential));
        }

        public async Task<AuthenticationResult> GetTokenAsync()
        {
            return await Retry.Run(
                task: () => AcquireTokenAsync(),
                retryExceptionHandler: (ex, ct) => HandleAdalException(ex, ct)).ConfigureAwait(false);
        }

        public void ClearTokenCache()
        {
            // To force a refresh, just clear the AuthenticationContext's internal token cache.
            authContext.TokenCache.Clear();
        }

        private async Task<AuthenticationResult> AcquireTokenAsync()
        {
            bool acquired = false;

            try
            {
                // The ADAL client team recommends limiting concurrency of calls. When the Token is in cache there is never 
                // contention on this semaphore, but when tokens expire there is some. However, after measuring performance
                // with and without the semaphore (and different configs for the semaphore), not limiting concurrency actually
                // results in higher response times overall. Without the use of this semaphore calls to AcquireTokenAsync can take up
                // to 5 seconds under high concurrency scenarios.
                acquired = await authContextSemaphore.WaitAsync(SemaphoreTimeout).ConfigureAwait(false);

                // If we are allowed to enter the semaphore, acquire the token.
                if (acquired)
                {
                    // Acquire token async using ADAL.NET
                    // https://github.com/AzureAD/azure-activedirectory-library-for-dotnet
                    // Given that this is a ClientCredential scenario, it will use the cache without the 
                    // need to call AcquireTokenSilentAsync (which is only for user credentials).
                    var res = await authContext.AcquireTokenAsync(JwtConfig.OAuthResourceUri, clientCredential).ConfigureAwait(false);
                    return res;
                }

                throw new TimeoutException("Timeout waiting for token acquisition.");
            }
            finally
            {
                // Always release the semaphore.
                if (acquired)
                {
                    authContextSemaphore.Release();
                }

            }
        }

        private RetryParams HandleAdalException(Exception ex, int currentRetryCount)
        {
            if (ex is AdalServiceException)
            {
                AdalServiceException adalServiceException = (AdalServiceException) ex;

                // When the Service Token Server (STS) is too busy because of “too many requests”, 
                // it returns an HTTP error 429 with a hint about when you can try again (Retry-After response field) as a delay in seconds
                if (adalServiceException.ErrorCode == AdalError.ServiceUnavailable || adalServiceException.StatusCode == 429)
                {
                    RetryConditionHeaderValue retryAfter = adalServiceException.Headers.RetryAfter;

                    // Depending on the service, the recommended retry time may be in retryAfter.Delta or retryAfter.Date. Check both.
                    if (retryAfter != null && retryAfter.Delta.HasValue)
                    {
                        return new RetryParams(retryAfter.Delta.Value);
                    }
                    else if (retryAfter != null && retryAfter.Date.HasValue)
                    {
                        return new RetryParams(retryAfter.Date.Value.Offset);
                    }
                    // We got a 429 but didn't get a specific back-off time. Use the default
                    return RetryParams.DefaultBackOff(currentRetryCount);
                }
                else
                {
                    return RetryParams.DefaultBackOff(currentRetryCount);
                }
            }
            else
            {
                // We end up here is the exception is not an ADAL exception. An example, is under high traffic
                // where we could have a timeout waiting to acquire a token, waiting on the semaphore.
                // If we hit a timeout, we want to retry a reasonable number of times.
                return RetryParams.DefaultBackOff(currentRetryCount);
            }
        }
    }
}
