using System;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// The keys for application settings.
    /// </summary>
    public class AppSettingKeys
    {
        /// <summary>
        /// The bot state endpoint key.
        /// </summary>
        public const string SateEndpoint = "BotStateEndpoint";

        /// <summary>
        /// The open id url key.
        /// </summary>
        public const string OpenIdMetadata = "BotOpenIdMetadata";

        /// <summary>
        /// The Microsoft app Id key.
        /// </summary>
        public const string AppId = "MicrosoftAppId";

        /// <summary>
        /// The Microsoft app password key.
        /// </summary>
        public const string Password = "MicrosoftAppPassword";

        /// <summary>
        /// The key for azure table storage connection string.
        /// </summary>
        public const string TableStorageConnectionString = "AzureWebJobsStorage";


        /// <summary>
        /// Key for the flag indicating if table storage should be used as bot state store.
        /// </summary>
        public const string UseTableStorageForConversationState = "UseTableStorageForConversationState";
    }


    /// <summary>
    /// A utility class for bots running on Azure.
    /// </summary>
    public sealed class Utils
    {
        /// <summary>
        /// Get value corresponding to the key from application settings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetAppSetting(string key)
        {
            return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }

        /// <summary>
        /// Get the open Id configuration url from application settings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The open id configuration.</returns>
        public static string GetOpenIdConfigurationUrl(string key = AppSettingKeys.SateEndpoint)
        {
            var result = Utils.GetAppSetting(key);
            if (String.IsNullOrEmpty(result))
            {
                result = JwtConfig.ToBotFromChannelOpenIdMetadataUrl;
            }
            return result;
        }

        /// <summary>
        /// Get the state api endpoint. 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The state api endpoint.</returns>
        public static string GetStateApiUrl(string key = AppSettingKeys.SateEndpoint)
        {
            var result = Utils.GetAppSetting(key);
            if (String.IsNullOrEmpty(result))
            {
                result = "https://state.botframework.com/";
            }
            return result;
        }

    }
}
