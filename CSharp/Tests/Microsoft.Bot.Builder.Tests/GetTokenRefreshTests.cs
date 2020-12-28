// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Connector;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using System.Linq;
using System.Runtime.Remoting.Messaging;

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
            public HttpRequestMessage RequestMessage { get; set; }

            public TestMicrosoftAppCredentials(string appId, string appPassword)
                : base(appId, appPassword)
            {
            }

            public Dictionary<string, string> GetTrustedUrls()
            {
                return TrustedHostNames.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.OAuthScope))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                await base.ProcessHttpRequestAsync(request, cancellationToken).ConfigureAwait(false);
                RequestMessage = request;

                // stop the flow (test looks for this exception)
                throw new Exception("ignore");
            }
        }

        [TestMethod]
        public async Task TokenTest_OAuthScope()
        {
            string host = "testurl.com";
            string baseUrl = $"http://{host}/morehere/extended/";
            
            var credentials = new TestMicrosoftAppCredentials("a40e1db0-b7a2-4e6e-af0e-b4987f73228f", "sbF0902^}tyvpvEDXTMX9^|");
            MicrosoftAppCredentials.TrustServiceUrl(baseUrl, oauthScope: "3851a47b-53ed-4d29-b878-6e941da61e98");

            var connectorClient = new ConnectorClient(new Uri(baseUrl), credentials, addJwtTokenRefresher: false);
            var activity = new Activity() { Type = ActivityTypes.Message, Text = "test" };

            try
            {
                var response = connectorClient.Conversations.SendToConversation("testConversationId", activity);
            } 
            catch (Exception ex)
            {
                if (!ex.Message.Equals("ignore", StringComparison.OrdinalIgnoreCase)) 
                {
                    Assert.Fail("Test did not throw 'ignore' exception as expected");
                }

                var oauthScope = "3851a47b-53ed-4d29-b878-6e941da61e98";
                var authResult = await credentials.GetTokenAsync(oauthScope: oauthScope).ConfigureAwait(false);
                var credentialsHeader = credentials.RequestMessage.Headers.Authorization.ToString();

                Assert.AreEqual("Bearer " + authResult, credentialsHeader);
            }
        }

        [TestMethod]
        public async Task TokenTests_OAuthScopeWorks()
        {
            string host = "testurl.com";
            string baseUrl = $"http://{host}/";
            string baseUrlExtended = "extended/url/";
            string baseUrlExtended2 = "extended2/url/";
            string scope1 = "testscope1";
            string scope2 = "testscope2";

            var url1 = $"{baseUrl}{baseUrlExtended}";
            var url2 = $"{baseUrl}{baseUrlExtended2}";
            var url3 = $"http://www.SomeOtherBaseUrl.com/";
            // Add the first path twice (to ensure duplicates are not added)
            MicrosoftAppCredentials.TrustServiceUrl(url1, DateTime.Now.AddDays(1), scope1);
            MicrosoftAppCredentials.TrustServiceUrl(url1, DateTime.Now.AddDays(1), scope1);
            MicrosoftAppCredentials.TrustServiceUrl(url2, DateTime.Now.AddDays(1), scope2);
            MicrosoftAppCredentials.TrustServiceUrl(url3, DateTime.Now.AddDays(1), scope2);

            var scopes = new TestMicrosoftAppCredentials(string.Empty, string.Empty).GetTrustedUrls();

            Assert.IsTrue(scopes[url1] == scope1, $"{url1} missing expected {scope1}");
            Assert.IsTrue(scopes[url2] == scope2, $"{url2} missing expected {scope2}");
            Assert.IsTrue(scopes[url3] == scope2, $"{url3} missing expected {scope2}");
        }
        
        [TestMethod]
        public async Task TokenTests_OAuthScopeNoDateWorks()
        {
            string host = "testurl.com";
            string baseUrl = $"http://{host}/";
            string baseUrlExtended = "extended/url/";
            string baseUrlExtended2 = "extended2/url/";
            string scope1 = "testscope1";
            string scope2 = "testscope2";

            var url1 = $"{baseUrl}{baseUrlExtended}";
            var url2 = $"{baseUrl}{baseUrlExtended2}";
            var url3 = $"http://www.SomeOtherBaseUrl.com/";
            // Add the first path twice (to ensure duplicates are not added)
            MicrosoftAppCredentials.TrustServiceUrl(url1, oauthScope: scope1);
            MicrosoftAppCredentials.TrustServiceUrl(url1, oauthScope: scope1);
            MicrosoftAppCredentials.TrustServiceUrl(url2, oauthScope: scope2);
            MicrosoftAppCredentials.TrustServiceUrl(url3, oauthScope: scope2);

            var scopes = new TestMicrosoftAppCredentials(string.Empty, string.Empty).GetTrustedUrls();

            Assert.IsTrue(scopes[url1] == scope1, $"{url1} missing expected {scope1}");
            Assert.IsTrue(scopes[url2] == scope2, $"{url2} missing expected {scope2}");
            Assert.IsTrue(scopes[url3] == scope2, $"{url3} missing expected {scope2}");
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