using System.Collections.Generic;

namespace QuickTeams.Models
{
    public class MsTeamsApps
    {
        public class App
        {
            public List<AppObject> value { get; set; }
        }

        public class AppObject
        {
            public string id { get; set; }
            public string name { get; set; }
            public string version { get; set; }
            public bool isBlocked { get; set; }
            public string installedState { get; set; }
            public string context { get; set; }
        }
    }
}
