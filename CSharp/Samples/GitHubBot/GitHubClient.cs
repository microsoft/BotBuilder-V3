using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Sample.GitHubBot
{
    public class GitHubClient
    {
        private string _token;

        public GitHubClient(string token)
        {
            _token = token;
        }

        public async Task<IList<Repository>> GetRepositories()
        {
            using (HttpClient client = CreateClient(_token))
            {
                using (var response = await client.GetAsync("/user/repos"))
                {
                    var repos = await response.Content.ReadAsAsync<IList<Repository>>();
                    return repos;
                }
            }
        }

        public async Task<IList<Notification>> GetNotifications(bool all)
        {
            using (HttpClient client = CreateClient(_token))
            {
                using (var response = await client.GetAsync("/notifications" + "?all=" + all))
                {
                    var result = await response.Content.ReadAsAsync<IList<Notification>>();
                    return result;
                }
            }
        }

        public async Task<User> GetUser()
        {
            using (HttpClient client = CreateClient(_token))
            {
                using (var response = await client.GetAsync("/user"))
                {
                    var result = await response.Content.ReadAsAsync<User>();
                    return result;
                }
            }
        }

        public async Task<IList<UserEmail>> GetUserEmails()
        {
            using (HttpClient client = CreateClient(_token))
            {
                using (var response = await client.GetAsync("/user/emails"))
                {
                    var result = await response.Content.ReadAsAsync<IList<UserEmail>>();
                    return result;
                }
            }
        }

        private static HttpClient CreateClient(string token)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.github.com");
            client.DefaultRequestHeaders.Add("User-Agent", "GitHubBot v0.0.1");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
    
    public class Repository
    {
        public string name { get; set; }

        public string full_name { get; set; }

        public Owner owner { get; set; }
   
        public string url { get; set; }

        public string html_url { get; set; }

        [JsonProperty("private")]
        public bool isPrivate { get; set; }
    }

    public class Owner
    {
        public string login { get; set; }

        public string url { get; set; }
    }

    public class Notification
    {
        public string id { get; set; }

        public Repository repository { get; set; }

        public string reason { get; set; }

        public bool unread { get; set; }

        public DateTime updated_at { get; set; }

        public Subject subject { get; set; }
    }

    public class Subject
    {
        public string title { get; set; }

        public string url { get; set; }

        public string type { get; set; }

        public string latest_comment_url { get; set; }
    }

    public class User
    {
        public string login { get; set; }

        public int id { get; set; }

        public string avatar_url { get; set; }

        public string html_url { get; set; }

        public string company { get; set; }

        public string bio { get; set; }

        public string email { get; set; }
    }

    public class UserEmail
    {
        public string email { get; set; }
        public bool verified { get; set; }
        public bool primary { get; set; }
        public string visibility { get; set; }
    }
}