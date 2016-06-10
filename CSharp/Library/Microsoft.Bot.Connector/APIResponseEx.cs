using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector
{
    public partial class APIResponseEx
    {
        public partial class Entity
        {
            [JsonExtensionData(ReadData = true, WriteData = true)]
            public JObject Properties { get; set; }
        }
    }
}
