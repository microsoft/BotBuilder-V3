using Microsoft.Bot.Builder.Calling.Exceptions;
using System;
using System.Configuration;

namespace Microsoft.Bot.Builder.Calling
{
    public class CallingBotServiceSettings
    {
        /// <summary>
        /// The url where the Callingcallbacks from Skype Bot platform will be sent. Needs to match the domain name of service and the route configured in BotController.
        /// For example "https://testservice.azurewebsites.net/api/calling/callback"   
        /// </summary>
        public string CallbackUrl { get; set; }

        /// <summary>
        /// Loads core bot library configuration from the cloud service configuration
        /// </summary>
        /// <returns>MessagingBotServiceSettings</returns>
        public static CallingBotServiceSettings LoadFromCloudConfiguration()
        {
            CallingBotServiceSettings settings;

            try
            {
                settings = new CallingBotServiceSettings
                {
                    CallbackUrl = ConfigurationManager.AppSettings.Get("Microsoft.Bot.Builder.Calling.CallbackUrl")
                };
            }
            catch (Exception e)
            {
                throw new BotConfigurationException(
                    "A mandatory configuration item is missing or invalid", e);
            }

            settings.Validate();
            return settings;
        }

        /// <summary>
        ///     Validates current bot configuration and throws BotConfigurationException if the configuration is invalid
        /// </summary>
        public void Validate()
        {
            Uri callBackUri;
            if (!Uri.TryCreate(this.CallbackUrl, UriKind.Absolute, out callBackUri))
            {
                throw new BotConfigurationException($"Bot calling configuration is invalid, callback url: {CallbackUrl} is not a valid url!");
            }
        }
    }
}
