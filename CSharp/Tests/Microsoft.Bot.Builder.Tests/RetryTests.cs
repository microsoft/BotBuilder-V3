using Microsoft.Bot.Connector.Shared.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class RetryTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public async Task Retry_NoRetryWhenTaskSucceeds()
        {
            FaultyClass faultyClass = new FaultyClass()
            {
                ExceptionToThrow = null
            };

            var result = await Retry.Run(
                task: () => faultyClass.FaultyTask(),
                retryExceptionHandler: (ex, ct) => faultyClass.ExceptionHandler(ex, ct));

            Assert.IsNull(faultyClass.ExceptionReceived);
            Assert.AreEqual(1, faultyClass.CallCount);
        }

        [TestMethod]
        public async Task Retry_RetryThenSucceed()
        {
            FaultyClass faultyClass = new FaultyClass()
            {
                ExceptionToThrow = new ArgumentNullException(),
                TriesUntilSuccess = 3
            };

            var result = await Retry.Run(
                task: () => faultyClass.FaultyTask(),
                retryExceptionHandler: (ex, ct) => faultyClass.ExceptionHandler(ex, ct));

            Assert.IsNotNull(faultyClass.ExceptionReceived);
            Assert.AreEqual(3, faultyClass.CallCount);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public async Task Retry_RetryUntilFailure()
        {
            FaultyClass faultyClass = new FaultyClass()
            {
                ExceptionToThrow = new ArgumentNullException(),
                TriesUntilSuccess = 8
            };

            var result = await Retry.Run(
                task: () => faultyClass.FaultyTask(),
                retryExceptionHandler: (ex, ct) => faultyClass.ExceptionHandler(ex, ct));
        }
    }

    public class FaultyClass
    {
        public Exception ExceptionToThrow { get; set; }
        public Exception ExceptionReceived { get; set; } = null;
        public int LatestRetryCount { get; set; }
        public int CallCount { get; set; } = 0;
        public int TriesUntilSuccess {get; set; } = 0;


        public async Task<string> FaultyTask()
        {
            CallCount++;

            if (CallCount < TriesUntilSuccess && ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }

            return string.Empty;
        }

        public RetryParams ExceptionHandler(Exception ex, int currentRetryCount)
        {
            ExceptionReceived = ex;
            LatestRetryCount = currentRetryCount;

            return RetryParams.DefaultBackOff(currentRetryCount);
        }
    }
}
