using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.GraphBot.Models
{
    public static class Keys
    {
        public static class Bot
        {
            public const string ID = "YourAppId";
            public const string Secret = "YourAppSecret";
        }

        public static readonly string TenantID = "http://schemas.microsoft.com/identity/claims/tenantid";
        public static readonly string ObjectID = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public static readonly string TokenCache = typeof(TokenCache).Name;
        public static readonly string Resource = "https://graph.microsoft.com/";
    }

    public interface IClientKeys
    {
        string ClientID { get; }
        string ClientSecret { get; }
    }

    public sealed class ClientKeys : IClientKeys
    {
        private readonly IConfiguration configuration;
        public ClientKeys(IConfiguration configuration)
        {
            SetField.NotNull(out this.configuration, nameof(configuration), configuration);
        }

        string IClientKeys.ClientID
        {
            get
            {
                return this.configuration["ClientID"];
            }
        }

        string IClientKeys.ClientSecret
        {
            get
            {
                return this.configuration["ClientSecret"];
            }
        }
    }
}
