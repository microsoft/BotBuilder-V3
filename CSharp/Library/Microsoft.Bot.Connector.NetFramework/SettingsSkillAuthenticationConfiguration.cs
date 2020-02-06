// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Configuration;
using System.Linq;
using System.Security.Authentication;

namespace Microsoft.Bot.Connector.SkillAuthentication
{
    /// <summary>
    /// AllowedCallers is a comma delimited setting in web.config file that consists of the parent bot ids 
    /// which are allowed to access the skill.
    /// To add a new parent bot simply go to the AllowedCallers and add the parent bot's MicrosoftAppId
    /// to the string.
    /// To allow all bots to call the skill, place only an '*' in the AllowedCallers.
    /// </summary>
    public class SettingsSkillAuthenticationConfiguration : AuthenticationConfiguration
    {
        private const string AllowedCallersConfigKey = "AllowedCallers";

        public SettingsSkillAuthenticationConfiguration()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[AllowedCallersConfigKey]))
            {
                throw new AuthenticationException($"{AllowedCallersConfigKey} is required in AppSettings.");
            }

            var allowedCallersValue = SettingsUtils.GetAppSettings(AllowedCallersConfigKey);
            var allowedCallers = allowedCallersValue.Split(',').Select(s => s.Trim()).ToList();

            ClaimsValidator = new DefaultAllowedCallersClaimsValidator(allowedCallers);
        }

        public override ClaimsValidator ClaimsValidator { get => base.ClaimsValidator; set => base.ClaimsValidator = value; }
    }
}
