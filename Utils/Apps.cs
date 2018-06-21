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
    public class Apps
    {
        public static void ListApps(string teamId, string aadAccessToken)
        {
            var appsList = new MsTeamsApps.App();
            Helpers.httpClient.DefaultRequestHeaders.Clear();
            Helpers.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", aadAccessToken);
            Helpers.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpResponseMessage =
                    Helpers.httpClient.GetAsync(O365.MsGraphBetaEndpoint + "teams/" + teamId + "/apps").Result;
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var httpResultString = httpResponseMessage.Content.ReadAsStringAsync().Result;
                appsList = JsonConvert.DeserializeObject<MsTeamsApps.App>(httpResultString);
            }

            Console.WriteLine("Here's what's installed for this Team");
            for (int i = 0; i < appsList.value.Count; i++)
            {
                Console.WriteLine("[" + i + "]" + " " + appsList.value[i].name + " " + appsList.value[i].id + " " + appsList.value[i].context + 
                    " " + appsList.value[i].installedState);
            }

            return;
        }

        public static void AddApp(string teamId, string appId, string aadAccessToken)
        {
            Helpers.httpClient.DefaultRequestHeaders.Clear();
            Helpers.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", aadAccessToken);
            Helpers.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // this might break on some platforms
            dynamic newAppObject = new JObject();
            newAppObject.id = appId;

            var createMsGroupPostData = JsonConvert.SerializeObject(newAppObject);
            var httpResponseMessage =
                Helpers.httpClient.PostAsync(O365.MsGraphBetaEndpoint + "teams/" + teamId + "/apps",
                    new StringContent(createMsGroupPostData, Encoding.UTF8, "application/json")).Result;
        }

        public static void DeleteApp(string teamId, string appId, string aadAccessToken)
        {
            Helpers.httpClient.DefaultRequestHeaders.Clear();
            Helpers.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", aadAccessToken);
            Helpers.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var httpResponseMessage =
                Helpers.httpClient.DeleteAsync(O365.MsGraphBetaEndpoint + "teams/" + teamId + "/apps/" + appId).Result;

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("ERROR: Operation failed.");
                Console.WriteLine("REASON: " + httpResponseMessage.Content.ReadAsStringAsync().Result);
                return;
            }
            else
            {
                Console.WriteLine("Operation started.  It may take some time for the operation to complete. ");
            }
        }
    }
}