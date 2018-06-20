using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using QuickTeams.Models;
using Microsoft.Graph;

namespace QuickTeams.Utils
{
    public class Channels
    {
        public static string SelectJoinedTeam(string aadAccessToken)
        {
            MsTeams.Team msTeam = new MsTeams.Team();

            Helpers.httpClient.DefaultRequestHeaders.Clear();
            Helpers.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", aadAccessToken);
            Helpers.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpResponseMessage =
                    Helpers.httpClient.GetAsync(O365.MsGraphBetaEndpoint + "me/joinedTeams").Result;
            Console.WriteLine("httpResponseMessage is  " + httpResponseMessage.Content.ReadAsStringAsync().Result);
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var httpResultString = httpResponseMessage.Content.ReadAsStringAsync().Result;
                msTeam = JsonConvert.DeserializeObject<MsTeams.Team>(httpResultString);
                Console.WriteLine("Groups " + httpResultString);
            }
            else
            {
                return "";
            }

            if (msTeam.value.Count == 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Whoops!");
                Console.WriteLine("You're not a member of any existing Microsoft Teams");
                Console.WriteLine("You must be a member of an existing Team before you can import channels.");
                Console.WriteLine("");
                Console.WriteLine("You can create a new Team right now!");
                return CreateNewTeam(aadAccessToken);
            }

            Console.WriteLine("You're currently a member of these Teams");
            Console.WriteLine("WARNING: If you don't have permission to create new channels for a given Team, your attempt to create or migrate channels will fail");
            for (int i = 0; i < msTeam.value.Count; i++)
            {
                Console.WriteLine("[" + i + "]" + " " + msTeam.value[i].displayName + " " + msTeam.value[i].description);
            }

            Console.Write("Enter the destination Team number or type \"new\" to create a new Team: ");
            var selectedTeamIndex = Console.ReadLine();
            if (selectedTeamIndex.StartsWith("n", StringComparison.CurrentCultureIgnoreCase))
            {
                return CreateNewTeam(aadAccessToken);
            }
            var selectedTeamId = msTeam.value[Convert.ToInt16(selectedTeamIndex)].id;
            Console.WriteLine("Team ID is " + selectedTeamId);
            return selectedTeamId;
        }

        public static string CreateNewTeam(string aadAccessToken)
        {
            Console.Write("Enter your new Team name: ");
            var newGroupAndTeamName = Console.ReadLine();
            var newTeamId = Groups.CreateGroupAndTeam(aadAccessToken, newGroupAndTeamName.Trim());
            return newTeamId;
        }

        public static string CloneTeam(string sourceTeamId, string aadAccessToken)
        {
            Console.Write("Enter your new Team name: ");
            var newGroupAndTeamName = Console.ReadLine();
            Console.Write("Enter your new Team description (optional): ");
            var newGroupAndTeamDescription = Console.ReadLine();
            Console.WriteLine("Which parts of the source team do you want to clone? ");
            var newGroupAndTeamPartsToClone = "";
            Console.Write("Tabs? This will also clone any installed apps (y/n) ");
            if(Console.ReadLine().StartsWith("y",StringComparison.CurrentCultureIgnoreCase))
            {   
                newGroupAndTeamPartsToClone += "tabs ";
                newGroupAndTeamPartsToClone += "apps ";
            }
            else
            {
                Console.Write("Apps (only)? Tabs that use these apps will not be cloned (y/n) ");
                if(Console.ReadLine().StartsWith("y",StringComparison.CurrentCultureIgnoreCase))
                {   
                    newGroupAndTeamPartsToClone += "apps ";
                }
            }
            Console.Write("Settings? (y/n) ");
            if(Console.ReadLine().StartsWith("y",StringComparison.CurrentCultureIgnoreCase))
            {   
                newGroupAndTeamPartsToClone += "settings ";
            }            
            Console.Write("Channel structure? (y/n) ");
            if(Console.ReadLine().StartsWith("y",StringComparison.CurrentCultureIgnoreCase))
            {   
                newGroupAndTeamPartsToClone += "channels ";
            }       
            Console.Write("Members? (y/n) ");
            if(Console.ReadLine().StartsWith("y",StringComparison.CurrentCultureIgnoreCase))
            {   
                newGroupAndTeamPartsToClone += "members ";
            }  

            Console.WriteLine("parts to clone are " + newGroupAndTeamPartsToClone.Trim().Replace(" ",","));
            
            // this might break on some platforms
            dynamic clonedMsTeam = new JObject();
            clonedMsTeam.displayName = newGroupAndTeamName;
            clonedMsTeam.description = newGroupAndTeamDescription;
            clonedMsTeam.mailNickname = newGroupAndTeamName.Replace(" ","");
            clonedMsTeam.partsToClone = newGroupAndTeamPartsToClone.Trim().Replace(" ",",");

            var clonedTeamPostData = JsonConvert.SerializeObject(clonedMsTeam);
            var httpResponseMessage =
                Helpers.httpClient.PostAsync(O365.MsGraphBetaEndpoint + "teams/" + sourceTeamId + "/clone",
                    new StringContent(clonedTeamPostData, Encoding.UTF8, "application/json")).Result;

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("ERROR: Team could not be cloned " + newGroupAndTeamName + " with this Description " + newGroupAndTeamDescription);
                Console.WriteLine("REASON: " + httpResponseMessage.Content.ReadAsStringAsync().Result);
            }
            else
            {
                Console.WriteLine("Clone operation started ");
            }

            return "uno";//newTeamId;
        }

    }
}