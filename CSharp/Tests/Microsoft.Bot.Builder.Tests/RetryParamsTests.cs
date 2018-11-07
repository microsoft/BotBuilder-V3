// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    public class RetryParamsTests
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
        public async Task RetryParams_StopRetryingValidation()
        {
            RetryParams retryParams = RetryParams.StopRetrying;
            Assert.IsFalse(retryParams.ShouldRetry);
        }

        [TestMethod]
        public async Task RetryParams_DefaultBackOffShouldRetryOnFirstRetry()
        {
            RetryParams retryParams = RetryParams.DefaultBackOff(0);

            // If this is the first time we retry, it should retry by default
            Assert.IsTrue(retryParams.ShouldRetry);
            Assert.AreEqual(TimeSpan.FromMilliseconds(50), retryParams.RetryAfter);
        }

        [TestMethod]
        public async Task RetryParams_DefaultBackOffShouldNotRetryAfter5Retries()
        {
            RetryParams retryParams = RetryParams.DefaultBackOff(10);
            Assert.IsFalse(retryParams.ShouldRetry);
        }

        [TestMethod]
        public async Task RetryParams_DelayOutOfBounds()
        {
            RetryParams retryParams = new RetryParams(TimeSpan.FromSeconds(11), true);

            // RetryParams should enforce the upper bound on delay time
            Assert.AreEqual(TimeSpan.FromSeconds(10), retryParams.RetryAfter);
        }
    }
}
