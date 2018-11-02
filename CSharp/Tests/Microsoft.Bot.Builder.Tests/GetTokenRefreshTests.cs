// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Connector;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
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
            Assert.AreEqual(result2.AccessToken, result2.AccessToken);
            Assert.AreEqual(result2.ExpiresOn, result2.ExpiresOn);
        }

        [TestMethod]
        public async Task TokenTests_GetTokenWaitAndRefresh()
        {
            MicrosoftAppCredentials credentials = new MicrosoftAppCredentials("a40e1db0-b7a2-4e6e-af0e-b4987f73228f", "sbF0902^}tyvpvEDXTMX9^|");
            var result = await credentials.GetTokenAsync();
            Assert.IsNotNull(result);
            credentials.ClearTokenCache();
            var result2 = await credentials.GetTokenAsync();
            Assert.AreNotEqual(result.ExpiresOn, result2.ExpiresOn);
            Assert.AreNotEqual(result.AccessToken, result2.AccessToken);
        }

        [TestMethod]
        public async Task TokenTests_RefreshTestLoad()
        {
            MicrosoftAppCredentials credentials = new MicrosoftAppCredentials("a40e1db0-b7a2-4e6e-af0e-b4987f73228f", "sbF0902^}tyvpvEDXTMX9^|");

            List<Task<AuthenticationResult>> tasks = new List<Task<AuthenticationResult>>();
            for (int i = 0; i < 1000; i++)
            {
                tasks.Add(credentials.GetTokenAsync());
            }

            foreach (var item in tasks)
            {
                Assert.IsFalse(item.IsFaulted);
                Assert.IsFalse(item.IsCanceled);
                AuthenticationResult result = await item;

                Assert.IsTrue(result.ExpiresOn > DateTimeOffset.UtcNow);
            }

            tasks.Clear();
            for (int i = 0; i < 1000; i++)
            {
                if (i % 100 == 50)
                    credentials.ClearTokenCache();

                tasks.Add(credentials.GetTokenAsync());
            }

            HashSet<AuthenticationResult> results = new HashSet<AuthenticationResult>(new AuthenticationResultEqualityComparer());
            for(int i=0; i < 1000; i++)
            {
                Assert.IsFalse(tasks[i].IsFaulted);
                Assert.IsFalse(tasks[i].IsCanceled);
                AuthenticationResult result = await tasks[i];

                Assert.IsTrue(result.ExpiresOn > DateTimeOffset.UtcNow);
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