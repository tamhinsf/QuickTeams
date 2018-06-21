using System;
using System.IO;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.Reflection;

namespace QuickTeams
{

    class Program
    {
        // all of your per-tenant and per-environment settings are (now) in appsettings.json

        public static IConfigurationRoot Configuration { get; set; }

        // Don't change this constant
        // It is a constant that corresponds to fixed values in AAD that corresponds to Microsoft Graph

        // Required Permissions - Microsoft Graph -> API
        // Read all users' full profiles
        // Read and write all groups

        const string aadResourceAppId = "00000003-0000-0000-c000-000000000000";

        static AuthenticationContext authenticationContext = null;
        static AuthenticationResult authenticationResult = null;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // retreive settings from appsettings.json instead of hard coding them here

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            string commandString = string.Empty;

            Console.WriteLine("");
            Console.WriteLine("****************************************************************************************************");
            Console.WriteLine("Welcome to Quick Teams!");
            Console.WriteLine("****************************************************************************************************");
            Console.WriteLine("");

            while (Configuration["AzureAd:TenantId"] == "" || Configuration["AzureAd:ClientId"] == "")
            {
                if (args.Length == 2)
                {
                    Configuration["AzureAd:TenantId"] = args[0];
                    Configuration["AzureAd:ClientId"] = args[1];
                }
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("****************************************************************************************************");
                    Console.WriteLine("You need to provide your Azure Active Directory Tenant Name and the Application ID you created for");
                    Console.WriteLine("use with application to continue.  You can do this by altering Program.cs and re-compiling this app.");
                    Console.WriteLine("Or, you can provide it right now.");
                    Console.Write("Azure Active Directory Tenant Name (i.e your-domain.onmicrosoft.com): ");
                    Configuration["AzureAd:TenantId"] = Console.ReadLine();
                    Console.Write("Azure Active Directory Application ID: ");
                    Configuration["AzureAd:ClientId"] = Console.ReadLine();
                    Console.WriteLine("****************************************************************************************************");
                }
            }

            Console.WriteLine("**************************************************");
            Console.WriteLine("Tenant is " + (Configuration["AzureAd:TenantId"]));
            Console.WriteLine("Application ID is " + (Configuration["AzureAd:ClientId"]));
            Console.WriteLine("Redirect URI is " + (Configuration["AzureAd:AadRedirectUri"]));
            Console.WriteLine("**************************************************");

            Console.WriteLine("");
            Console.WriteLine("****************************************************************************************************");
            Console.WriteLine("Your tenant admin consent URL is https://login.microsoftonline.com/common/oauth2/authorize?response_type=id_token" +
                "&client_id=" + Configuration["AzureAd:ClientId"] + "&redirect_uri=" + Configuration["AzureAd:AadRedirectUri"] + "&prompt=admin_consent" + "&nonce=" + Guid.NewGuid().ToString());
            Console.WriteLine("****************************************************************************************************");
            Console.WriteLine("");


            Console.WriteLine("");
            Console.WriteLine("****************************************************************************************************");
            Console.WriteLine("Let's get started! Sign in to Microsoft with your Teams credentials:");

            authenticationResult = UserLogin();
            var aadAccessToken = authenticationResult.AccessToken;

            if (String.IsNullOrEmpty(authenticationResult.AccessToken))
            {
                Console.WriteLine("Something went wrong.  Please try again!");
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("You've successfully signed in.  Welcome " + authenticationResult.UserInfo.DisplayableId);
            }

            var sourceTeamId = Utils.Teams.SelectJoinedTeam(aadAccessToken);
            var sourceTeamName = Utils.Groups.GetGroupDetails(sourceTeamId, aadAccessToken);
            while (!commandString.Equals("Exit", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Selected Team: {0} ", sourceTeamName);
                Console.Write("Enter command ( apps | clone | archive | unarchive | delete | switch | exit ) > ");
                commandString = Console.ReadLine();
                switch (commandString.ToUpper())
                {
                    case "APPS":
                        AppsCommands(sourceTeamId, sourceTeamName, aadAccessToken);
                        break;
                    case "CLONE":
                        Utils.Teams.CloneTeam(sourceTeamId, aadAccessToken);
                        break;
                    case "ARCHIVE":
                        Utils.Teams.ArchiveTeam(sourceTeamId, aadAccessToken);
                        break;
                    case "UNARCHIVE":
                        Utils.Teams.UnArchiveTeam(sourceTeamId, aadAccessToken);
                        break;
                    case "DELETE":
                        Utils.Groups.DeleteGroup(sourceTeamId, aadAccessToken);
                        Console.WriteLine("Since you deleted the Team {0}, you need to select a new Team.", sourceTeamName);
                        sourceTeamId = Utils.Teams.SelectJoinedTeam(aadAccessToken);
                        sourceTeamName = Utils.Groups.GetGroupDetails(sourceTeamId, aadAccessToken);
                        break;
                    case "SWITCH":
                        sourceTeamId = Utils.Teams.SelectJoinedTeam(aadAccessToken);
                        sourceTeamName = Utils.Groups.GetGroupDetails(sourceTeamId, aadAccessToken);
                        break;
                    case "EXIT":
                        Console.WriteLine("Bye!");
                        break;
                    default:
                        Console.WriteLine("Invalid command.");
                        break;
                }
            }

        }

        static void AppsCommands(string sourceTeamId, string sourceTeamName, string aadAccessToken)
        {
            string appsCommandString = string.Empty;

            while (!appsCommandString.Equals("Back", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Selected Team: {0}", sourceTeamName);
                Console.Write("Enter apps command ( list | add | delete | back ) > ");
                appsCommandString = Console.ReadLine();
                switch (appsCommandString.ToUpper())
                {
                    case "LIST":
                        Utils.Apps.ListApps(sourceTeamId, aadAccessToken);
                        break;
                    case "ADD":
                        Console.Write("Enter the ID of the app you want to add: ");
                        var appIdToAdd = Console.ReadLine();
                        Utils.Apps.AddApp(sourceTeamId, appIdToAdd, aadAccessToken);
                        break;
                    case "DELETE":
                        Console.WriteLine("Delete has certain \"limitations\"");
                        Console.WriteLine("installedAndPermanent apps can't be deleted");
                        Console.WriteLine("installed apps that are teamsOwned will become installedAndHidden");
                        Console.Write("Enter the ID of the app you want to delete: ");
                        var appIdToDelete = Console.ReadLine();
                        Utils.Apps.DeleteApp(sourceTeamId, appIdToDelete, aadAccessToken);
                        break;
                    case "BACK":
                        Console.WriteLine("Going back!");
                        break;
                    default:
                        Console.WriteLine("Invalid command.");
                        break;
                }
            }
            return;
        }

        static AuthenticationResult UserLogin()
        {
            authenticationContext = new AuthenticationContext
                    (String.Format(CultureInfo.InvariantCulture, Configuration["AzureAd:AadInstance"], Configuration["AzureAd:TenantId"]));
            authenticationContext.TokenCache.Clear();
            DeviceCodeResult deviceCodeResult = authenticationContext.AcquireDeviceCodeAsync(aadResourceAppId, (Configuration["AzureAd:ClientId"])).Result;
            Console.WriteLine(deviceCodeResult.Message);
            return authenticationContext.AcquireTokenByDeviceCodeAsync(deviceCodeResult).Result;
        }
    }
}
