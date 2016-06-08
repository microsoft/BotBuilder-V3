using System.Security.Claims;

namespace Microsoft.Bot.Connector
{
    public class BasicAuthIdentity : ClaimsIdentity
    {
        public BasicAuthIdentity(string id, string password) : base("Basic")
        {
            this.Id = id;
            this.Password = password;
        }

        public string Id { get; set; }

        public string Password { get; set; }
    }
}