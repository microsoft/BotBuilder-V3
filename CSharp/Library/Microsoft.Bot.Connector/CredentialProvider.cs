using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public interface ICredentialProvider
    {
        /// <summary>
        /// Validate AppId
        /// </summary>
        /// <param name="appId"></param>
        /// <returns>true if it is a valid AppId for the controller</returns>
        Task<bool> IsValidAppIdAsync(string appId);
        
        /// <summary>
        /// Get the app password for a given bot appId, if it is not a valid appId, return Null
        /// </summary>
        /// <param name="appId">bot appid</param>
        /// <returns>password or null for invalid appid</returns>
        Task<string> GetAppPasswordAsync(string appId);

        /// <summary>
        /// Checks if bot authentication is disabled.
        /// </summary>
        /// <returns>true if bot authentication is disabled.</returns>
        Task<bool> IsAuthenticationDisabledAsync();
    }

    public class SimpleCredentialProvider : ICredentialProvider
    {
        public string AppId { get; set; }

        public string Password { get; set; }

        public Task<bool> IsValidAppIdAsync(string appId)
        {
            return Task.FromResult(appId == AppId);
        }

        public Task<string> GetAppPasswordAsync(string appId)
        {
            return Task.FromResult((appId == this.AppId) ? this.Password : null);
        }

        public Task<bool> IsAuthenticationDisabledAsync()
        {
            return Task.FromResult(string.IsNullOrEmpty(AppId));
        }
    }

    /// <summary>
    /// Static credential provider which has the appid and password static
    /// </summary>
    public sealed class StaticCredentialProvider : SimpleCredentialProvider
    {
        public StaticCredentialProvider(string appId, string password)
        {
            this.AppId = appId;
            this.Password = password;
        }
    }

    /// <summary>
    /// Credential provider which uses config settings to lookup appId and password
    /// </summary>
    public sealed class SettingsCredentialProvider : SimpleCredentialProvider
    {
        public SettingsCredentialProvider(string appIdSettingName=null, string appPasswordSettingName=null)
        {
            var appIdKey = appIdSettingName ?? MicrosoftAppCredentials.MicrosoftAppIdKey;
            var passwordKey = appPasswordSettingName ?? MicrosoftAppCredentials.MicrosoftAppPasswordKey;
            this.AppId = ConfigurationManager.AppSettings[appIdKey] ?? Environment.GetEnvironmentVariable(appIdKey, EnvironmentVariableTarget.Process);
            this.Password = ConfigurationManager.AppSettings[passwordKey] ?? Environment.GetEnvironmentVariable(passwordKey, EnvironmentVariableTarget.Process);
        }
    }

}
