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
    public class ThrottleException : Exception
    {
        public RetryParams RetryParams { get; set; }
    }

    public class AdalAuthenticator 
    {
        private static readonly TimeSpan SemaphoreTimeout = TimeSpan.FromSeconds(2);

        // ADAL recommends having a single authentication context and reuse across requests.
        // An authentication context also has an internal token cache which we can reuse.
        private static AuthenticationContext authContext = new AuthenticationContext(JwtConfig.ToChannelFromBotLoginUrl) { ExtendedLifeTimeEnabled = true };

        // We limit concurrency when acquiring tokens. ADAL requires us to limit concurrency. 
        // In the (currently under preview) new version of ADAL called MSAL, the concurrency will be managed by MSAL. 
        // We'll use the semaphore to make sure only one request sends a token request to the server at a given time,
        // but the rest of the time requests will hit the local memory cache.
        private static SemaphoreSlim authContextSemaphore = new SemaphoreSlim(1, 1);

        // Depending on the responses we get from the service, we update a shared retry policy with the RetryAfter header
        // from the HTTP 429 we receive.
        // When everything seems to be OK, this retry policy will be empty.
        // The reason for this is that if a request gets throttled, even if we wait to retry that, another thread will try again right away.
        // With the shared retry policy, if a request gets throttled, we know that other threads have to wait as well.
        // This variable is guarded by the authContextSemaphore semphore. Don't modify it outside of the semaphore scope.
        private static volatile RetryParams currentRetryPolicy;

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

                    // This means we acquired a valid token successfully. We can make our retry policy null.
                    // Note that the retry policy is set under the semaphore so no additional synchronization is needed.
                    if (currentRetryPolicy != null)
                    {
                        currentRetryPolicy = null;
                    }

                    return res;
                }
                else
                {
                    // If the token is taken, it means that one thread is trying to acquire a token from the server.
                    // If we already received information about how much to throttle, it will be in the currentRetryPolicy.
                    // Use that to inform our next delay before trying.
                    throw new ThrottleException() { RetryParams = currentRetryPolicy };
                }
            }
            catch (Exception ex)
            {
                // If we are getting throttled, we set the retry policy according to the RetryAfter headers
                // that we receive from the auth server.
                // Note that the retry policy is set under the semaphore so no additional synchronization is needed.
                if (IsAdalServiceUnavailable(ex))
                {
                    currentRetryPolicy = ComputeAdalRetry(ex);
                }
                throw ex;
            }
            finally
            {
                // Always release the semaphore if we acquired it.
                if (acquired)
                {
                    authContextSemaphore.Release();
                }

            }
        }

        private RetryParams HandleAdalException(Exception ex, int currentRetryCount)
        {
            if (IsAdalServiceUnavailable(ex))
            {
                return ComputeAdalRetry(ex);
            }
            else if (ex is ThrottleException)
            {
                // This is an exception that we threw, with knowledge that 
                // one of our threads is trying to acquire a token from the server
                // Use the retry parameters recommended in the exception
                ThrottleException throttlException = (ThrottleException)ex;
                return throttlException.RetryParams ?? RetryParams.DefaultBackOff(currentRetryCount);
            }
            else
            {
                // We end up here is the exception is not an ADAL exception. An example, is under high traffic
                // where we could have a timeout waiting to acquire a token, waiting on the semaphore.
                // If we hit a timeout, we want to retry a reasonable number of times.
                return RetryParams.DefaultBackOff(currentRetryCount);
            }
        }

        private bool IsAdalServiceUnavailable(Exception ex)
        {
            AdalServiceException adalServiceException = ex as AdalServiceException;
            if (adalServiceException == null)
            {
                return false;
            }

            // When the Service Token Server (STS) is too busy because of “too many requests”, 
            // it returns an HTTP error 429
            return adalServiceException.ErrorCode == AdalError.ServiceUnavailable || adalServiceException.StatusCode == 429;
        }

        private RetryParams ComputeAdalRetry(Exception ex)
        {
            if (ex is AdalServiceException)
            {
                AdalServiceException adalServiceException = (AdalServiceException)ex;

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
                    return RetryParams.DefaultBackOff(0);
                }
            }
            return RetryParams.DefaultBackOff(0);
        }
    }
}
