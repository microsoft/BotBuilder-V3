// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Connector;
using System.Threading.Tasks;

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

        public class TestMicrosoftAppCredentials : MicrosoftAppCredentials
        {
            public TestMicrosoftAppCredentials(string appId, string appPassword)
                : base(appId, appPassword)
            {
            }

            public KeyValuePair<string, ICollection<string>> GetServiceUrlsAndScopes(string host)
            {
                return new KeyValuePair<string, ICollection<string>>(host, TrustedHostNames[host].OAuthScopes.Keys);
            }
        }

        [TestMethod]
        public async Task TokenTests_OAuthScopeWorks()
        {
            string host = "testurl.com";
            string baseUrl = $"http://{host}/";
            string scope1 = "testscope1";
            string scope2 = "testscope2";

            // Add the first path twice (to ensure duplicates are not added)
            MicrosoftAppCredentials.TrustServiceUrl($"{baseUrl}{scope2}", DateTime.Now.AddDays(1), scope2);
            MicrosoftAppCredentials.TrustServiceUrl($"{baseUrl}{scope2}", DateTime.Now.AddDays(1), scope2);
            MicrosoftAppCredentials.TrustServiceUrl($"{baseUrl}{scope1}", DateTime.Now.AddDays(1), scope1);
            MicrosoftAppCredentials.TrustServiceUrl($"SomeOtherBaseUrl/{scope1}", DateTime.Now.AddDays(1), scope1);

            var scopes = new TestMicrosoftAppCredentials(string.Empty, string.Empty).GetServiceUrlsAndScopes(host);

            Assert.AreEqual(2, scopes.Value.Count);
        }
        
        [TestMethod]
        public async Task TokenTests_OAuthScopeNoDateWorks()
        {
            string host = "testurl.com";
            string baseUrl = $"http://{host}/";
            string scope1 = "testscope1";
            string scope2 = "testscope2";
            // Add the first path twice (to ensure duplicates are not added)
            MicrosoftAppCredentials.TrustServiceUrl($"{baseUrl}{scope2}", oauthScope: scope2);
            MicrosoftAppCredentials.TrustServiceUrl($"{baseUrl}{scope2}", oauthScope: scope2);
            MicrosoftAppCredentials.TrustServiceUrl($"{baseUrl}{scope1}", oauthScope: scope1);
            MicrosoftAppCredentials.TrustServiceUrl($"SomeOtherBaseUrl/{scope1}", oauthScope: scope1);

            var scopes = new TestMicrosoftAppCredentials(string.Empty, string.Empty).GetServiceUrlsAndScopes(host);

            Assert.AreEqual(2, scopes.Value.Count);
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

            for (int i = 0; i < 1000; i++)
            {
                Assert.IsFalse(tasks[i].IsFaulted);
                Assert.IsFalse(tasks[i].IsCanceled);
                string result = await tasks[i];
            }
        }
    }
}