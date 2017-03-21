using System;
using System.Configuration;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Credential provider which uses config settings to lookup appId and password
    /// </summary>
    public sealed class SettingsCredentialProvider : SimpleCredentialProvider
    {
        public SettingsCredentialProvider(string appIdSettingName = null, string appPasswordSettingName = null)
        {
            this.AppId = BotServiceProvider.Instance.ConfigurationRoot.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value;
            this.Password = BotServiceProvider.Instance.ConfigurationRoot.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value;
        }
    }
}
