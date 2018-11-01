using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Connector.Shared.Authentication
{
    public class RetryParams
    {
        private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan DefaultBackOffTime = TimeSpan.FromMilliseconds(50);

        public static RetryParams StopRetrying
        {
            get
            {
                return new RetryParams() { ShouldRetry = false };
            }
        }

        public bool ShouldRetry { get; set; }
        public TimeSpan RetryAfter { get; set; }

        public RetryParams() { }

        public RetryParams(TimeSpan retryAfter, bool shouldRetry = true)
        {
            ShouldRetry = shouldRetry;
            RetryAfter = retryAfter;

            // We don't allow more than maxDelaySeconds seconds delay.
            if (RetryAfter > MaxDelay)
            {
                throw new ArgumentOutOfRangeException(nameof(retryAfter));
            }
        }

        public static RetryParams DefaultBackOff(int retryCount)
        {
            if (retryCount < 5)
            {
                return new RetryParams(DefaultBackOffTime);
            }
            else
            {
                return RetryParams.StopRetrying;
            }
        }
    }
}
