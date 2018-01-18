using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.Shared.Utils
{
    public class SoftCache<T>
    {
        private T _value;
        private DateTime _updated = DateTime.MinValue;
        private readonly TimeSpan _softExpiry;
        private readonly TimeSpan _hardExpiry;
        private readonly Func<Task<T>> _refresh;
        private readonly object _sync = new object();

        /// <summary>
        /// Create a new instance of the SoftCache class
        /// </summary>
        /// <param name="softExpiry">Soft expiration, after which the refresh function is called in the background</param>
        /// <param name="hardExpiry">Hard expiration, after which the refresh function is called synchronously</param>
        /// <param name="refresh">Refresh function</param>
        /// <param name="refreshOnStart">True to force refresh on class creation</param>
        public SoftCache(TimeSpan softExpiry, TimeSpan hardExpiry, Func<Task<T>> refresh, bool refreshOnStart = false)
        {
            if (softExpiry >= hardExpiry)
                throw new ArgumentException("softExpiry must be shorter than hardExpiry");

            _softExpiry = softExpiry;
            _hardExpiry = hardExpiry;
            _refresh = refresh;

            // Kick off of main thread in case there's a non-dropcontext await
            if (refreshOnStart)
                Task.Run(() => RefreshAsync()).Wait();
        }

        public T Value
        {
            get
            {
                bool doSynchronousRefresh = false;

                lock (_sync)
                {
                    // Our data is very old. Do a synchronous refresh.
                    if (DateTime.UtcNow > _updated + _hardExpiry)
                    {
                        // Clear the lock(_sync) before we actually start the refresh so we don't deadlock
                        // We risk running the request more than once but it's no worse than without this cache
                        doSynchronousRefresh = true;
                    }
                    // Our data is only a little old. Queue a background refresh but return immediately.
                    else if (DateTime.UtcNow > _updated + _softExpiry)
                    {
                        // Even though we haven't updated, we need to keep other callers from queueing
                        // refreshes which this runs. If it fails, it'll set _updated to the beginning of time.
                        _updated = DateTime.UtcNow;
                        BackgroundQueue.QueueTask(() => RefreshAsync());
                    }
                }

                if (doSynchronousRefresh)
                    Task.Run(() => RefreshAsync()).Wait();

                return _value;
            }
        }

        private async Task RefreshAsync(bool isRetry = false)
        {
            try
            {
                _value = await _refresh().ConfigureAwait(false);
                _updated = DateTime.UtcNow;
            }
            catch (Exception e)
            {
                // Reset update time so next call does a pull-through
                _updated = DateTime.MinValue;

                //AppInsights.TrackException(e);

                if (isRetry)
                    throw;
                else
                    await RefreshAsync(true).ConfigureAwait(false);
            }
        }
    }

    public static class BackgroundQueue
    {
        private static Task previousTask = Task.FromResult(true);
        private static object key = new object();
        
        public static Task QueueTask(Action action)
        {
            lock (key)
            {
                previousTask = previousTask.ContinueWith(t => action()
                    , CancellationToken.None
                    , TaskContinuationOptions.None
                    , TaskScheduler.Default);
                return previousTask;
            }
        }

        public static Task<T> QueueTask<T>(Func<T> work)
        {
            lock (key)
            {
                var task = previousTask.ContinueWith(t => work()
                    , CancellationToken.None
                    , TaskContinuationOptions.None
                    , TaskScheduler.Default);
                previousTask = task;
                return task;
            }
        }
    }
}
