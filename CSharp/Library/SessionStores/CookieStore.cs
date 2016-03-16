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

        private String GetCookieName(string Id)
        {
            return $"session{Id ?? String.Empty}";
        }

        public void Load(string Id, ISessionData sessionData)
        {
            var userData = CookieHelper.GetSessionCookie<IDictionary<string, object>>(this.request, this.GetCookieName(string.Format("{0}UserData", Id)));
            var conversationData = CookieHelper.GetSessionCookie<IDictionary<string, object>>(this.request, this.GetCookieName(string.Format("{0}ConversationData", Id)));
            var perUserInConversationData = CookieHelper.GetSessionCookie<IDictionary<string, object>>(this.request, this.GetCookieName(string.Format("{0}PerUserInConversationData", Id)));

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

        private void SaveDictionary(string Id, IReadOnlyDictionary<string,object> state)
        {
            if (state == null && savedState.ContainsKey(Id))
            {
                savedState.Remove(Id);
            }
            else
            {
                savedState[Id] = state.ToDictionary(kvp=> kvp.Key, kvp => kvp.Value);
            }
        }

        public void Save(string Id, ISessionData sessionData)
        {
            SaveDictionary(string.Format("{0}UserData", Id), sessionData.UserData);
            SaveDictionary(string.Format("{0}PerUserInConversationData", Id), sessionData.PerUserInConversationData);
            SaveDictionary(string.Format("{0}ConversationData", Id), sessionData.ConversationData);
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

        async Task ISessionStore.LoadAsync(string Id, ISessionData sessionData)
        {
            this.Load(Id, sessionData);
        }

        async Task ISessionStore.SaveAsync(string Id, ISessionData sessionData)
        {
            this.Save(Id, sessionData);
        }
    }
}