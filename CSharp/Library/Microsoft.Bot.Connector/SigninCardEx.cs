using System.Text.RegularExpressions;

namespace Microsoft.Bot.Connector
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;
    using Newtonsoft.Json.Linq;
    using System.Net.Http;
    using System.Configuration;
    using System.Text;

    public partial class SigninCard 
    {
        /// <summary>
        /// Creates a <see cref="SigninCard"/>
        /// </summary>
        /// <param name="text"> The <see cref="Text"/></param>
        /// <param name="buttonLabel"> The signin button label.</param>
        /// <param name="url"> The sigin url.</param>
        /// <returns> The created sigin card.</returns>
        public static SigninCard Create(string text, string buttonLabel, string url)
        {
            return new SigninCard
            {
                Text = text,
                Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                       Title =  buttonLabel,
                       Type =  ActionTypes.Signin,
                       Value =  url
                    }
                }
            };
        }
    }
}
