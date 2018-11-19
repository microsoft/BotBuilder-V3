// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Connector;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public class GetTokenRefreshTests
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
        public async Task TokenTests_GetCredentialsWorks()
        {
            MicrosoftAppCredentials credentials = new MicrosoftAppCredentials("a40e1db0-b7a2-4e6e-af0e-b4987f73228f", "sbF0902^}tyvpvEDXTMX9^|");
            var result = await credentials.GetTokenAsync();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task TokenTests_GetTokenTwice()
        {
            MicrosoftAppCredentials credentials = new MicrosoftAppCredentials("a40e1db0-b7a2-4e6e-af0e-b4987f73228f", "sbF0902^}tyvpvEDXTMX9^|");
            var result = await credentials.GetTokenAsync();
            Assert.IsNotNull(result);
            var result2 = await credentials.GetTokenAsync();
            Assert.AreEqual(result2, result2);
        }

        [TestMethod]
        public async Task TokenTests_GetTokenWaitAndRefresh()
        {
            MicrosoftAppCredentials credentials = new MicrosoftAppCredentials("a40e1db0-b7a2-4e6e-af0e-b4987f73228f", "sbF0902^}tyvpvEDXTMX9^|");
            var result = await credentials.GetTokenAsync();
            Assert.IsNotNull(result);
            var result2 = await credentials.GetTokenAsync(true);
            Assert.AreNotEqual(result, result2);

             result = await credentials.GetTokenAsync();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task TokenTests_RefreshTestLoad()
        {
            MicrosoftAppCredentials credentials = new MicrosoftAppCredentials("a40e1db0-b7a2-4e6e-af0e-b4987f73228f", "sbF0902^}tyvpvEDXTMX9^|");

            List<Task<string>> tasks = new List<Task<string>>();
            for (int i = 0; i < 1000; i++)
            {
                tasks.Add(credentials.GetTokenAsync());
            }

            foreach (var item in tasks)
            {
                Assert.IsFalse(item.IsFaulted);
                Assert.IsFalse(item.IsCanceled);
                string result = await item;
            }

            tasks.Clear();
            bool forceRefresh = false;

            for (int i = 0; i < 1000; i++)
            {
                forceRefresh = i % 100 == 50;
                tasks.Add(credentials.GetTokenAsync(forceRefresh));
            }

            HashSet<AuthenticationResult> results = new HashSet<AuthenticationResult>(new AuthenticationResultEqualityComparer());
            for (int i = 0; i < 1000; i++)
            {
                Assert.IsFalse(tasks[i].IsFaulted);
                Assert.IsFalse(tasks[i].IsCanceled);
                string result = await tasks[i];
            }
        }
    }

    class AuthenticationResultEqualityComparer : IEqualityComparer<AuthenticationResult>
    {
        public bool Equals(AuthenticationResult x, AuthenticationResult y)
        {
            return x.AccessToken == y.AccessToken;
        }

        public int GetHashCode(AuthenticationResult obj)
        {
            return obj.AccessToken.GetHashCode();
        }
    }
}