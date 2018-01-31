namespace Microsoft.Bot.Connector
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;
    using Newtonsoft.Json.Linq;
    /// <summary>
    /// </summary>
    public partial class Mention : Entity
    {
        partial void CustomInit()
        {
            this.Type = "mention";
        }
    }
}
