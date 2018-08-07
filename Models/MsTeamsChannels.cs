using System.Collections.Generic;

namespace QuickTeams.Models
{
    public class MsTeamsChannels
    {
        public class Channels
        {
            public List<ChannelObject> value { get; set; }
            // public string __invalid_name__@odata.context { get; set; }        
        }

        public class ChannelObject
        {
            public string id { get; set; }
            public string displayName { get; set; }
            public object description { get; set; } = "";
        }
    }
}