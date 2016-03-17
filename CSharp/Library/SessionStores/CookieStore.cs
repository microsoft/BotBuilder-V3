using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class CookieStore : ISessionStore
    {
        private HttpRequestMessage request;
        private TimeSpan maxAge;
        private Dictionary<string, IDictionary<string,object>> savedState;

        public CookieStore(HttpRequestMessage request, TimeSpan? maxAge = null)
        {
            this.request = request;
            this.maxAge = maxAge ?? TimeSpan.FromHours(3);
            this.savedState = new Dictionary<string, IDictionary<string,object>>(); 
        }

        private String GetCookieName(string ID)
        {
            return $"session{ID ?? String.Empty}";
        }

        public void Load(string sessionID, ISessionData sessionData)
        {
            var userData = CookieHelper.GetSessionCookie<IDictionary<string, object>>(this.request, this.GetCookieName(string.Format("{0}UserData", sessionID)));
            var conversationData = CookieHelper.GetSessionCookie<IDictionary<string, object>>(this.request, this.GetCookieName(string.Format("{0}ConversationData", sessionID)));
            var perUserInConversationData = CookieHelper.GetSessionCookie<IDictionary<string, object>>(this.request, this.GetCookieName(string.Format("{0}PerUserInConversationData", sessionID)));

            if (userData != null)
            {
                foreach (var data in userData)
                {
                    sessionData.SetUserData(data.Key, data.Value);
                }
            }

            if (conversationData != null)
            {
                foreach (var data in conversationData)
                {
                    sessionData.SetConversationData(data.Key, data.Value);
                }
            }

            if (perUserInConversationData != null)
            {
                foreach (var data in perUserInConversationData)
                {
                    sessionData.SetPerUserInConversationData(data.Key, data.Value);
                }
            }
        }

        private void SaveDictionary(string dictionaryKey, IReadOnlyDictionary<string,object> state)
        {
            if (state == null && savedState.ContainsKey(dictionaryKey))
            {
                savedState.Remove(dictionaryKey);
            }
            else
            {
                savedState[dictionaryKey] = state.ToDictionary(kvp=> kvp.Key, kvp => kvp.Value);
            }
        }

        public void Save(string sessionID, ISessionData sessionData)
        {
            SaveDictionary(string.Format("{0}UserData", sessionID), sessionData.UserData);
            SaveDictionary(string.Format("{0}PerUserInConversationData", sessionID), sessionData.PerUserInConversationData);
            SaveDictionary(string.Format("{0}ConversationData", sessionID), sessionData.ConversationData);
        }

        public void AddSavedStateToCookies(ref HttpResponseMessage response)
        {
            List<CookieHeaderValue> cookies = new List<CookieHeaderValue>(); 
            foreach(var kv in savedState)
            {
                cookies.Add(CookieHelper.CreateSessionCookie(response,
                    HttpContext.Current != null ? HttpContext.Current.Request.Url.Host : null, 
                    this.GetCookieName(kv.Key), kv.Value.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), this.maxAge));
            }

            response.Headers.AddCookies(cookies);
        }

        async Task ISessionStore.LoadAsync(string sessionID, ISessionData sessionData)
        {
            this.Load(sessionID, sessionData);
        }

        async Task ISessionStore.SaveAsync(string sessionID, ISessionData sessionData)
        {
            this.Save(sessionID, sessionData);
        }
    }
}