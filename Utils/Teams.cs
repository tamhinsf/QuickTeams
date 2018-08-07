using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Graph;
using QuickTeams.Models;

namespace QuickTeams.Utils
{
    public class Teams
    {
        public static string SelectJoinedTeam(string aadAccessToken)
        {
            MsTeams.Team msTeam = new MsTeams.Team();

            Helpers.httpClient.DefaultRequestHeaders.Clear();
            Helpers.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", aadAccessToken);
            Helpers.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpResponseMessage =
                    Helpers.httpClient.GetAsync(O365.MsGraphBetaEndpoint + "me/joinedTeams").Result;
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var httpResultString = httpResponseMessage.Content.ReadAsStringAsync().Result;
                msTeam = JsonConvert.DeserializeObject<MsTeams.Team>(httpResultString);
            }
            else
            {
                return null;
            }

            if (msTeam.value.Count == 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Whoops!");
                Console.WriteLine("You're not a member of any existing Microsoft Teams");
                Console.WriteLine("You must be a member of an existing Team before you can do something with it!");
                Console.WriteLine("");
                Console.WriteLine("You can create a new Team right now!");
                return CreateNewTeam(aadAccessToken);
            }

            Console.WriteLine("You're currently a member of these Teams");
            Console.WriteLine("WARNING: If you don't have permission to create new channels for a given Team, your attempt to create or migrate channels will fail");
            for (int i = 0; i < msTeam.value.Count; i++)
            {
                Console.WriteLine("[" + i + "]" + " " + msTeam.value[i].displayName + " " + msTeam.value[i].description + " is archived? " + 
                    msTeam.value[i].isArchived);
            }

            Console.Write("Enter the Team number you want to work with or type \"new\" to create a new Team: ");
            var selectedTeamIndex = Console.ReadLine();
            if (selectedTeamIndex.StartsWith("n", StringComparison.CurrentCultureIgnoreCase))
            {
                return CreateNewTeam(aadAccessToken);
            }
            var selectedTeamId = msTeam.value[Convert.ToInt16(selectedTeamIndex)].id;
            return selectedTeamId;
        }

        public static string CreateNewTeam(string aadAccessToken)
        {
            Console.Write("Enter your new Team name: ");
            var newGroupAndTeamName = Console.ReadLine();
            var newTeamId = Groups.CreateGroupAndTeam(aadAccessToken, newGroupAndTeamName.Trim());
            return newTeamId;
        }

        public static string GetTeamDetails(string sourceTeamId, string aadAccessToken)
        {
            Helpers.httpClient.DefaultRequestHeaders.Clear();
            Helpers.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", aadAccessToken);
            Helpers.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpResponseMessage =
                    Helpers.httpClient.GetAsync(O365.MsGraphBetaEndpoint + "teams/" + sourceTeamId).Result;
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var httpResultString = httpResponseMessage.Content.ReadAsStringAsync().Result;
                return httpResultString;
            }
            else
            {
                return null;
            }   
        }

        public static bool ArchiveTeam(string sourceTeamId, string aadAccessToken)
        {
            var httpResponseMessage =
            Helpers.httpClient.PostAsync(O365.MsGraphBetaEndpoint + "teams/" + sourceTeamId + "/archive",
                new StringContent("", Encoding.UTF8, "application/json")).Result;

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("ERROR: Operation failed.");
                Console.WriteLine("REASON: " + httpResponseMessage.Content.ReadAsStringAsync().Result);
                return false;
            }
            else
            {
                Console.WriteLine("Operation started.  It may take some time for the operation to complete. ");
            }

            return true;

        }

        public static bool UnArchiveTeam(string sourceTeamId, string aadAccessToken)
        {
            var httpResponseMessage =
            Helpers.httpClient.PostAsync(O365.MsGraphBetaEndpoint + "teams/" + sourceTeamId + "/unarchive",
                new StringContent("", Encoding.UTF8, "application/json")).Result;

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("ERROR: Operation failed.");
                Console.WriteLine("REASON: " + httpResponseMessage.Content.ReadAsStringAsync().Result);
                return false;
            }
            else
            {
                Console.WriteLine("Operation started.  It may take some time for the operation to complete. ");
            }

            return true;

        }

        public static bool CloneTeam(string sourceTeamId, string aadAccessToken)
        {
            Console.Write("Enter your new Team name: ");
            var newGroupAndTeamName = Console.ReadLine();
            Console.Write("Enter your new Team description (optional): ");
            var newGroupAndTeamDescription = Console.ReadLine();
            Console.WriteLine("Which parts of the source team do you want to clone? ");
            var newGroupAndTeamPartsToClone = "";
            Console.Write("Tabs? This will also clone any installed apps (y/n) ");
            if (Console.ReadLine().StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
            {
                newGroupAndTeamPartsToClone += "tabs ";
                newGroupAndTeamPartsToClone += "apps ";
            }
            else
            {
                Console.Write("Apps (only)? Tabs that use these apps will not be cloned (y/n) ");
                if (Console.ReadLine().StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
                {
                    newGroupAndTeamPartsToClone += "apps ";
                }
            }
            Console.Write("Settings? (y/n) ");
            if (Console.ReadLine().StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
            {
                newGroupAndTeamPartsToClone += "settings ";
            }
            Console.Write("Channel structure? (y/n) ");
            if (Console.ReadLine().StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
            {
                newGroupAndTeamPartsToClone += "channels ";
            }
            Console.Write("Members? (y/n) ");
            if (Console.ReadLine().StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
            {
                newGroupAndTeamPartsToClone += "members ";
            }

            // this might break on some platforms
            dynamic clonedMsTeam = new JObject();
            clonedMsTeam.displayName = newGroupAndTeamName;
            clonedMsTeam.description = newGroupAndTeamDescription;
            clonedMsTeam.mailNickname = newGroupAndTeamName.Replace(" ", "");
            clonedMsTeam.partsToClone = newGroupAndTeamPartsToClone.Trim().Replace(" ", ",");

            var clonedTeamPostData = JsonConvert.SerializeObject(clonedMsTeam);
            var httpResponseMessage =
                Helpers.httpClient.PostAsync(O365.MsGraphBetaEndpoint + "teams/" + sourceTeamId + "/clone",
                    new StringContent(clonedTeamPostData, Encoding.UTF8, "application/json")).Result;

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("ERROR: Operation failed.");
                Console.WriteLine("REASON: " + httpResponseMessage.Content.ReadAsStringAsync().Result);
                return false;
            }
            else
            {
                Console.WriteLine("Operation started.  It may take some time for the operation to complete. ");
            }

            return true;
        }

        public static void DownloadTeam(string sourceTeamId,string aadAccessToken)
        {
            Console.Write("Please enter the path to the folder: ");
            var channelDownloadFolder = Console.ReadLine();
            Files.CreateArchivePath(Path.Combine(channelDownloadFolder,"quickteams"));

            var teamDetails = Teams.GetTeamDetails(sourceTeamId, aadAccessToken);
            Files.CreateArchiveFile(teamDetails, "teamSettings", Path.Combine(channelDownloadFolder,"quickteams"));

            var groupSettingDetails = Groups.GetGroupDetails("",sourceTeamId, aadAccessToken);
            Files.CreateArchiveFile(groupSettingDetails, "groupSettings", Path.Combine(channelDownloadFolder,"quickteams"));

            var groupMemberDetails = Groups.GetGroupDetails("/members",sourceTeamId, aadAccessToken);
            Files.CreateArchiveFile(groupMemberDetails, "groupMembers", Path.Combine(channelDownloadFolder,"quickteams"));

            var groupOwnerDetails = Groups.GetGroupDetails("/owners",sourceTeamId, aadAccessToken);
            Files.CreateArchiveFile(groupOwnerDetails, "groupOwners", Path.Combine(channelDownloadFolder,"quickteams"));

            var msTeamsChannels = Channels.GetChannels(sourceTeamId, aadAccessToken);
            for (int i = 0; i < msTeamsChannels.value.Count; i++)
            {
                Console.WriteLine("Downloading " + msTeamsChannels.value[i].displayName);
                Channels.DownloadChannelMessagesAndReplies("",channelDownloadFolder, msTeamsChannels.value[i], sourceTeamId, aadAccessToken);
            }
            
            return;
        }
    }
}