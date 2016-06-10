using System;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.Bot.Connector
{
    public class JwtIdentity : ClaimsIdentity
    {
        public const string JwtTokenAuthenticationType = "JwtToken";
        private readonly string _token;

        public JwtIdentity(string token, ClaimsIdentity identity)
            : base()
        {
            _token = token;

            foreach (Claim claim in identity.Claims)
                AddClaim(claim);
        }

        public override bool IsAuthenticated
        {
            // Identity is authenticated if we got a token
            get { return _token != null; }
        }
    }
}