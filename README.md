# Quick Teams for Microsoft Teams 

Download, clone, archive, un-archive your Microsoft Teams and manage the apps you've installed to them.  Here's what Quick Teams can do for you:

* Download all messages on a per channel or Team-wide basis
   * We'll create a folder called quickteams, which contains subfolders that correspond to the channel(s) downloaded.
      * Each message will be contained in an individual JSON file, whose name corresponds to the internally generated message ID
      * Each reply will also be contained in an individual JSON file, whose name corresponds to the ID of the parent message and the ID of the reply
      * A file called channelSettings will contain metadata about the channel
   * Team-wide downloads will include metadata about Team and underlying O365 group in the quickteams folder
      * teamSettings - metadata about the team
      * groupSettings - metadata about the underlying O365 group
      * groupMembers - group member list
      * groupOwners - group owner list
* Clone an existing Team into a new one - in whole or in part!  It's up to you to select which elements you want:
   * Channel structure
   * Tabs (structure)
   * Installed applications
   * Settings
   * Members
* Archive and un-archive a Team
* Manage the apps you've installed to a Team

Looking to create your own archival or compliance tool?  Use our project as your starter kit!  

We'll add features as more Microsoft Teams APIs become available and as time permits.  For now, let's get started!  


## Setup a development environment 

* Clone this GitHub repository.
* Install Visual Studio 2017.  Don't have it?  Download the free [Visual Studio Community Edition](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx)
* Don't want to use Visual Studio?  Quick Teams was written using .NET Core 2.1 and runs on Windows, macOS, and Linux.  Instead of using Visual Studio, you can simply download the SDK necessary to build and run this application.
  * https://www.microsoft.com/net/download/core

## Identify a test user account

* Sign in to your Office 365 environment as an administrator at [https://portal.office.com/admin/default.aspx](https://portal.office.com/admin/default.aspx)
* Ensure you have enabled Microsoft Teams for your organization [https://portal.office.com/adminportal/home#/Settings/ServicesAndAddIns](https://portal.office.com/adminportal/home#/Settings/ServicesAndAddIns)  
* Identify a user whose account you'd like to use 
  * Alternatively, you can choose to use your Office 365 administrator account 

## Create the Quick Teams Application in Azure Active Directory

You must register this application in the Azure Active Directory tenant associated with your Office 365 organization.  

* Sign in to your Azure Management Portal at https://portal.azure.com
    * Or, from the Office 365 Admin center select "Azure AD"
* Within the Azure Portal, select Azure Active Directory -> App registrations -> New application registration  
    * Name: QuickTeams (anything will work - we suggest you keep this value)
    * Application type: Native
    * Redirect URI: https://quickteams (anything else will work if you want to change it)
    * Click Create
* Once Azure has created your app, copy your Application Id and give your application access to the required Microsoft Graph API permissions.  
   * Click your app's name (i.e. QuickTeams) from the list of applications
   * Copy the Application Id
   * All settings -> Required permissions
     * Click Add
     * Select an API -> Microsoft Graph -> Select (button)
     * Select permissions 
	   * Read all users' full profiles
	   * Read and write all groups
     * Click Select
     * Click Done
	
  * If you plan to run Quick Teams as a non-administrator: applications built using the Graph API permissions above require administrative consent before non-administrative users can sign in - which fortunately, you'll only need to do once.  
    * You can immediately provide consent to all users in your organization using the Azure Portal. Click the "Grant permissions" button, which you can reach via your app's "Required permissions" link.
      * Here's the full path to "Grant permissions": Azure Active Directory -> App registrations -> Your app (i.e. QuickTeams) -> All settings ->  Required permissions -> Grant permissions
    * Or, whenever you successfully launch Quick Teams, we'll show you a URL that an administrative user can visit to provide consent.
      * Note: if you've configured the re-direct URL to be the same value as we've shown you on this page (i.e. https://quickteams, you'll be sent to an invalid page after successfully signing in.  Don't worry!
* Take note of your tenant name, which is typically in the form of your-domain.onmicrosoft.com.  You'll need to supply this when building or running Quick Teams.
 
  
## Build Quick Teams

* Open the cloned code from this repository in Visual Studio, Visual Studio Code, or your favorite editor
 * Update appsettings.json in the QuickTeams folder.  Within the section called "AzureAd", update the fields with your tenant name (TenantId), Application ID (ClientId), and Redirect URI (AadRedirectUri).  You can leave the value for AadInstance as-is.
 * Or, you can leave these values empty and provide them whenever you run the application.
* Build QuickTeams
  * In Visual Studio select QuickTeams from the Solution Explorer, then from the top menu pick Build -> QuickTeams
  * Or, using the .NET Core SDK, you can perform these steps from the command line
    * Open a command prompt, and navigate to the QuickTeams folder 
      * dotnet clean
      * dotnet restore
      * dotnet build

## Using Quick Teams
 
* Launch the Microsoft Teams app and make sure you are a member of an existing Team.  If not - create a Team!
* Open a command prompt, and navigate to the QuickTeams folder
* Run one the following command
   * dotnet run
* If prompted, provide your Active Directory Tenant Name and Application Id
* Follow the instructions provided to sign in:
   * Start a web browser and go to https://aka.ms/devicelogin - we strongly suggest you use your web browser's "private mode".
   * Enter the security code provided in the command prompt
   * Consent to using Quick Teams
   * Enter your O365 credentials.  
   * Return to your command prompt 
* Select the target Microsoft Team you want to manage.  You can also choose to create a new Microsoft Team.
   * We'll show you a list of commands you can perform against that team.
     * apps - manage the apps associated with this team though a dedicated sub-menu
         * list - list the apps installed to the current Team
         * add - add an app to the current Team by specifying its ID
         * delete - "delete" an app within certain limitations.
           * installedAndPermanent apps cannot be deleted
           * teamsOwned apps upon "delete" will be hidden but not deleted
         * back - go back to the top level menu
     * clone - clone this team into a new one.  We'll ask you for the name of the new team and the parts you want to clone.
     * archive - archive this team
     * unarchive - unarchive this team
     * download - download an entire Team's messages, metadata about both the Team and its channels, and membership information
     * delete - delete this team
     * switch - change to another team
     * channel - create a new channel or download an individual channel's messages and metadata
     * exit - leave the application
* Go back to the Microsoft Team app and explore the results of the actions you started from Quick Teams!

## Questions and comments

We'd love to get your feedback about this sample. You can send your questions and suggestions to us in the Issues section of this repository.

Questions about Microsoft Graph development in general should be posted to [Stack Overflow](http://stackoverflow.com/questions/tagged/microsoftgraph). Make sure that your questions or comments are tagged with [microsoftgraph].

## Additional resources

* [Use the Microsoft Graph API to work with Microsoft Teams](https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/resources/teams_api_overview)
* [Microsoft Graph Beta Endpoint Reference](https://developer.microsoft.com/en-us/graph/docs/api-reference/beta/beta-overview)
* [Microsoft Graph API Explorer](https://developer.microsoft.com/en-us/graph/graph-explorer)
* [Overview - Microsoft Graph](https://developer.microsoft.com/en-us/graph/docs)
* [Microsoft Teams - Dev Center](https://dev.office.com/microsoft-teams)

## Copyright

Copyright (c) 2018 Tam Huynh. All rights reserved. 


### Disclaimer ###
**THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.**
