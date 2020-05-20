# Moving legacy ASP.NET apps with Windows authentication to Azure App Service 

## Introduction
The intent of this repo is to provide legacy ASP.NET application samples with the different technology stacks 
(ASMX, WCF and Web API) to showcase how they can be moved to Azure App Service leveraging App Service Authentication 
(Easy Auth) with limited code changes. View the [accompanying blog post](https://devblogs.microsoft.com/premier-developer/moving-legacy-asp-net-apps-with-windows-authentication-to-azure-app-service-part-2/) for more details.

*Note: most unnecessary things were intentionally removed from the sample applications to focus on the authentication.*

## Setup
### Prerequisites
- An Azure subscription to deploy the sample applications.<br>
  *Note: the sample applications are free to run since they are deployed 
  to a free App Service plan by default.*
- Either PowerShell or PowerShell Core to run the different scripts.
- The [latest Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) to setup your Cloud and 
  local environments.
- .NET Framework 4.7.2 to build the sample applications.<br>
  *Note: a lower .NET Framework version could be used, but I choosed to use .NET 4.7.2 to benefit from the 
  [SameSite Cookie patches](https://docs.microsoft.com/en-us/aspnet/samesite/system-web-samesite).*
- Visual Studio 2017 or 2019 to build and deploy the sample applications.<br><br>


### Instructions
- Start by running Setup.ps1 in the Scripts folder
- You will need to login to your Azure Account and provide your subscription and Azure AD tenant information
- You will then need to provide a name and location for your resource group and App Service plan
- The script will then accomplish the following:
  - A resource group and a free App Service plan will be created
  - One Web App and Azure AD app will be created for the frontend
    - The script will also take care of details like enabling Easy Auth, assigning a Managed Identity, setting a client
      secret, setting the reply URLs, requesting the proper OAuth permissions, etc.
  - Three Web Apps and Azure AD apps will be create for the backend services (ASMX, WCF and Web API)
    - The script will also take care of details like enabling Easy Auth, setting the correct audiences, preauthorizing 
      the frontend application, etc.
  - The application settings for the frontend will be updated
  - Configuration files will be generated to run the applications locally
  - Publish profiles will be generated to deploy the applications in App Service

## Running the application

### Running in App Service
- Start by running Deploy.ps1 in the Script folder
- The script will accomplish the following:
  - The latest version of NuGet will be downloaded and the nuget packages will be restored for
    the solution
  - The latest version of the [vswhere](https://github.com/microsoft/vswhere) will be downloaded
    and MSBuild will be located based on your Visual Studio install location
  - The four sample applications will be deployed to App Service using the publish profiles generated by the Setup script
- At the end, the script will output the URL of the frontend, just open a browser and navigate to there

*Note: if you run into any issues, try setting the NuGet or MSBuild path variables manually based
on your installation* 

### Navigating the application
The application is split into four parts (Home, ASMX, WCF and Web API). Every section displays the request headers and
the claims. Home is the frontend authentication section, which uses Open ID Connect. The first time you will connect to 
application, it will request the OAuth permissions to sign in using your Azure AD account. The three other sections (ASMX, 
WCF and Web API) are the backend sections for the corresponding technology stack and use Bearer authentication. Every backend 
section has four buttons:
1. **Call anonymously**: this call will always fail; the purpose is to show the authentication is working
2. **Call with managed identity**: this call will use the Managed Identity of the frontend App Service to call the backend
3. **Call with client secret**: this call will use the client ID and secret defined in the frontend App Service configuration
   to call the backend
4. **Call on-behalf-of (OBO)**: this call will acquire a token on behalf of the user connected to the frontend App Service to
   to call the backend

### Running locally
If you wish to run the sample applications locally, there's usually a few challenges since things like Managed Identity and
Easy Auth don't exist locally. Don't worry, I've got you covered! 

For the Managed Identity, fortunately the 
[Microsoft.Azure.Services.AppAuthentication](https://www.nuget.org/packages/Microsoft.Azure.Services.AppAuthentication) package 
has a few fallbacks, and one is the Azure CLI and the Setup script has already taken care of preauthorizing the Azure CLI 
application so it should work.

For Easy Auth, I've included a simulator that uses OWIN middlewares to introduce the proper authentication behavior, i.e. either
Open ID Connect for the frontend or Bearer for the backend. This is simply controlled by the owin:AppStartup application setting
in the different web.config files. *Note: this simulator is not used when you deploy to App Service.*

Also note the Setup script generates settings.user files to store your personal environment information. These files are excluded 
from source control, so you don't have to worry about checking in sensitive information if you fork this repo for example.

You can also choose to run only the frontend locally and run the backend services in App Service. To do so, you just have to change 
the different backend service URLs in the frontend application settings located in the web.config.

## Project structure

The project structure is pretty self-explanatory, but here's the details:
- **EasyAuthSimulator**: This is the Easy Auth simulator to run locally
- **LegacyAPIApp** : This is a backend service built with the Web API stack
- **LegacyASMXApp** : This is a backend service built with the Web Services (ASMX) stack
- **LegacyWCFApp** : This is a backend service built with the WCF stack
- **LegacyWebApp** : This is the frontend built with the Web Forms stack
- **Scripts** : This folder contains the different script to setup the Cloud and local environments
