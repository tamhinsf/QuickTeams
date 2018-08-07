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
    public class Channels
    {

        public static string SelectChannel(string teamId, string aadAccessToken)
        {
            var msTeamsChannels = GetChannels(teamId, aadAccessToken);
            Console.WriteLine("Here are the channels for this Team");
            Console.WriteLine("WARNING: If you don't have permission to create new channels for a given Team, your attempt to create a channel will fail");
            for (int i = 0; i < msTeamsChannels.value.Count; i++)
            {
                Console.WriteLine("[" + i + "]" + " " + msTeamsChannels.value[i].displayName + " " + msTeamsChannels.value[i].description);
            }

            Console.Write("Enter the channel number you want to work with or type \"new\" to create a new channel: ");
            var selectedChannelIndex = Console.ReadLine();
            if (selectedChannelIndex.StartsWith("n", StringComparison.CurrentCultureIgnoreCase))
            {
                dynamic newChannelObject = new JObject();
                Console.Write("Please enter the new channel name: ");
                newChannelObject.displayName = Console.ReadLine();
                Console.Write("Please enter the new channel description: ");
                newChannelObject.description = Console.ReadLine();
                return CreateChannel(newChannelObject, teamId, aadAccessToken);
            }
            var selectedChannel = msTeamsChannels.value[Convert.ToInt16(selectedChannelIndex)];

            Console.Write("Download all messages for this channel? ");
            if (Console.ReadLine().StartsWith("y", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.Write("Please enter the path to the folder: ");
                var channelDownloadFolder = Console.ReadLine();
                DownloadChannelMessagesAndReplies("",channelDownloadFolder, selectedChannel, teamId, aadAccessToken);
            }

            return "";

        }

        public static string CreateChannel(dynamic channelObject, string teamId, string aadAccessToken)
        {
            //  this might break on some platforms
            var createTeamsChannelPostData = JsonConvert.SerializeObject(channelObject);
            var httpResponseMessage =
                Helpers.httpClient.PostAsync(O365.MsGraphBetaEndpoint + "teams/" + teamId + "/channels",
                    new StringContent(createTeamsChannelPostData, Encoding.UTF8, "application/json")).Result;

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("ERROR: Teams Channel could not be created " + channelObject.channelName);
                Console.WriteLine("REASON: " + httpResponseMessage.Content.ReadAsStringAsync().Result);
                return null;
            }
            else
            {
                var createdMsTeamsChannel = JsonConvert.DeserializeObject<MsTeamsChannels.ChannelObject>(httpResponseMessage.Content.ReadAsStringAsync().Result);
                return createdMsTeamsChannel.id;
            }

        }

        // recursion is your friend

        public static void DownloadChannelMessagesAndReplies(string parentMessageId, string channelDownloadFolder, MsTeamsChannels.ChannelObject selectedChannel, string teamId, string aadAccessToken)
        {
            var archivePath = Path.Combine(channelDownloadFolder, "quickteams", selectedChannel.id);
            
            if(parentMessageId == "")
            {
                Files.CreateArchivePath(archivePath);
                Files.CreateArchiveFile(JsonConvert.SerializeObject(selectedChannel), "channelSettings", archivePath);
            }

            // be greedy - try to grab 100 messages at a time
            // the odata:next link will re-use this if there are more than 100 messages

            string nextUrl = O365.MsGraphBetaEndpoint + "teams/" + teamId + "/channels/" + selectedChannel.id + "/messages"; 
            if(parentMessageId != "")
            {
                nextUrl += "/" + parentMessageId + "/replies";
            }
            nextUrl += "?$top=100";

            while (nextUrl != "")
            {
                var httpResponseMessage = Helpers.httpClient.GetAsync(nextUrl).Result;
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    return;
                }
                var httpResultString = httpResponseMessage.Content.ReadAsStringAsync().Result;
                var channelArchiveJson = JsonConvert.DeserializeObject<MsTeamsChannelArchive.ChannelArchive>(httpResultString);
                for(int i=0;i < channelArchiveJson.value.Count; i++)
                {
                    dynamic messageString = JObject.Parse(channelArchiveJson.value[i].ToString());
                    string messageId = messageString.id;
                    if(parentMessageId == "")
                    {
                        Files.CreateArchiveFile(channelArchiveJson.value[i].ToString(), messageId, archivePath);
                        DownloadChannelMessagesAndReplies(messageId,channelDownloadFolder,selectedChannel, teamId, aadAccessToken);
                    }
                    else
                    {
                        Files.CreateArchiveFile(channelArchiveJson.value[i].ToString(), parentMessageId + "." + messageId, archivePath);
                    }
                }
                if (String.IsNullOrEmpty(channelArchiveJson.odataNextLink))
                {
                    return;
                }
                else
                {
                    nextUrl = channelArchiveJson.odataNextLink;
                }
            }
            return;
        }

        public static MsTeamsChannels.Channels GetChannels(string teamId, string aadAccessToken)
        {
            MsTeamsChannels.Channels msTeamsChannels = new MsTeamsChannels.Channels();

            Helpers.httpClient.DefaultRequestHeaders.Clear();
            Helpers.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", aadAccessToken);
            Helpers.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var httpResponseMessage =
                    Helpers.httpClient.GetAsync(O365.MsGraphBetaEndpoint + "groups/" + teamId + "/channels").Result;
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var httpResultString = httpResponseMessage.Content.ReadAsStringAsync().Result;
                msTeamsChannels = JsonConvert.DeserializeObject<MsTeamsChannels.Channels>(httpResultString);
            }
            else
            {
                return null;
            }

            return msTeamsChannels;
        }
    }
}