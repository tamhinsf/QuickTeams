using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace QuickTeams.Models
{
    public class MsTeamsChannelArchive
    {
        public class ChannelArchive
        {
            [JsonProperty("@odata.content")]
            public string odataContent { get; set; }

            [JsonProperty("@odata.count")]
            public int odataCount { get; set; }

            [JsonProperty("@odata.nextLink")]
            public string odataNextLink { get; set; }
            public JArray value { get; set; }
        }
    }
}