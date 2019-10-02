namespace Microsoft.Bot.Connector
{
    using System;

    
    public static class OAuthClientExtensions
    {
		public static IOAuthApiEx GetOAuthApiEx(this IOAuthClient client)
        {
            var oauthClient = client as OAuthClient;

			if (oauthClient == null)
            {
                throw new InvalidOperationException("Unable to get OAuthApiEx instance.");
            }

            return oauthClient.OAuthApiEx;
        }
    }
}
