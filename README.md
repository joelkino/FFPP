# This is Proof of Concept (PoC) code not for production use!

## Overview

This is a C# ASP.NET Core web API minimal port of the awesome [CIPP-API](https://github.com/KelvinTegelaar/CIPP-API) project which is currently developed in PowerShell, and deployed as an Azure Function app.

The main goal of this project is to speed up CIPP by utilising a web API version of CIPP-API instead of Azure Functions (no more error 500), with a secondary goal of decoupling CIPP-API from the requirement to use Azure Functions/Azure Storage etc. Initially we will retain the use of Azure Key Vault for our secrets in production, however in the future I expect to provide some alternative options as well, such as using a local TPM/fTPM, device keychain, or other mechanisms to keep secrets on-prem.

This port is designed/targeted for small Linux devices/servers, however being dotnet core you should be able to run it anywhere (in theory at least).

## Licensing

Licensed under the [MIT License](https://choosealicense.com/licenses/mit/) so do whatever you want with it, for free, but you get no warranty and it's a case of use at your own risk. The license and contained copyright notice would need to be included with any copies of this repository/software as per MIT licensing requirements.

## Prerequisites for development environment (devenv)

- You have an [SSH key configured in your account on GitHub](https://docs.github.com/en/authentication/connecting-to-github-with-ssh/adding-a-new-ssh-key-to-your-github-account) and you can clone repositories using SSH.

- You have a SAM app (Azure AD Enterprise Application) setup for CIPP ([see here](https://cipp.app/docs/user/gettingstarted/permissions/)).

- You have all the required tokens and SAM app info just as if you were creating a CIPP instance eg: TenantId, ApplicationId, ApplicationSecret, RefreshToken and ExchangeRefreshToken.

- You have the [.NET 6 SDK installed](https://dotnet.microsoft.com/en-us/download/dotnet/6.0), and have the dotnet binary defined in your PATH environment variable so that in **cmd/terminal** you can type `dotnet --version` and it reports a version which is 6.0.302 or greater.

- You have an IDE for dotnet core development such as [Visual Studio](https://visualstudio.microsoft.com/) or [Visual Studio Code (Free)](https://visualstudio.microsoft.com/) + [C# Extension](https://code.visualstudio.com/docs/languages/dotnet), unless you are hardcore and enjoy coding from the command line (I have known people who do this for stuff that isn't Python 😂).

- You know how to run a .NET 6 project in "Debug" mode in your IDE (this will allow you to make use of the Swagger UI).

- You have a development environment setup to run the [CIPP](https://github.com/KelvinTegelaar/CIPP) react/swa front end. You can find the instructions to setup a devenv [here](https://cipp.app/docs/dev/settingup/).

- **[Optional]** If you wish to contribute code, it is a good idea to run a devenv of the official [CIPP-API](https://github.com/KelvinTegelaar/CIPP-API) and compare output between APIs to ensure both are matching. If you follow the complete instructions [here](https://cipp.app/docs/dev/settingup/) you will end up with a devenv for both CIPP and CIPP-API which is ideal. Also, it's a nice thing to do to port new functionality back to original CIPP-API if we can do it, to try and maintain feature parity across repositories.

## Setting up devenv

### Clone the repository

If you are on Windows make sure you have [git installed](https://git-scm.com/downloads) first.

On any platform, open **cmd/terminal** and navigate (`cd`) to where you want to download this project.

Run the command `git clone git@github.com:White-Knight-IT/CIPP-ALT-API.git` and it will download this project into a folder named CIPP-API-ALT.

### Secrets

This project utilises the official dotnet core method of creating secrets for our local devenv, that is using the [user-secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=linux) tool. This puts our secrets in a location outside of the repository, so it is impossible for us to accidentally commit local secrets into the project.

Open **cmd/terminal** and navigate (`cd`) to the folder that contains the project file (CIPP-API-ALT.csproj). If you cloned the repository to your home directory this will be `cd ~/CIPP-API-ALT/CIPP-API-ALT` .

Run the command `dotnet user-secrets init` and this will create a project specific secrets container identified by a GUID for us at the following locations:

- Windows: `%APPDATA%\Microsoft\UserSecrets\`

- macOS/Linux: `~/.microsoft/usersecrets/`

Now that our project secrets container has been made, it is time for us to populate it with secrets. To stash secrets in the project secret container, when we are in the project folder with **cmd/terminal**, we run `dotnet user-secrets set "[secret_name]" "[value]"`. An example of saving the TenantId secret would be `dotnet user-secrets set "TenantId" "goatfloater.onmicrosoft.com"`. We can repeat this step for all the secrets listed below.

We must be sure to stash the following secrets in our secrets cache (**CASE SENSITIVE**):

- TenantId

- ApplicationId

- ApplicationSecret

- RefreshToken

- ExchangeRefreshToken

This has created a file named `secrets.json` in the project secrets container. Feel free to modify the JSON in the file manually if you wish should you need to update tokens, it's not encrypted or anything. Using the user-secrets tool just means you get perfect JSON and not something that might break your build.

### Setting up Entity Framework & Databases

This project is using [Microsoft's Entity Framework Core platform](https://docs.microsoft.com/en-au/ef/core/cli/dotnet#update-the-tools) to consume local SQLite databases for lightweight/portable data stores.

We must [install the dotnet ef tools](https://docs.microsoft.com/en-au/ef/core/cli/dotnet#install-the-tools) to manage the databases. Using **cmd/terminal** in the project folder, run the command `dotnet tool install --global dotnet-ef` and this will eventually tell us that we have sucessfully installed the tools.

Now that the tools are installed, we need to instruct ef to create our databases from the project migrations. Using **cmd/terminal** in the project folder, run the command `dotnet ef database update --context CippLogs` and it will build the project and eventually you should see an output similar to:
```
Build started...
Build succeeded.
Applying migration '20220710174729_InitialCreate_CippLogs'.
Done.
```

Now we repeat this procedure for the remaining databases such as `dotnet ef database update --context ExcludedTenants`. There is a file in the project folder of this repository named `update_databases.txt`, this will contain all of these commands so you can create all databases as necessary by using all the commands in this file.

**This process will be automated in production**.

### Running the project

Now that we have stashed our secrets and created our databases, we are free to run the project in our IDE. `Open CIPP-API-ALT.sln` in Visual Studio for example, ensure `Debug` is selected top left and **NOT** `Release` (to get the Swagger UI you must run in DEBUG mode), then hit the play `▶️` button.

Given that this is a web API, it is not expected to have a user interface when it runs in production. For development however, we are utilising the tool [Swagger](https://swagger.io/) which provides both automated documentaion of our API, and a user interface that lets us perform the RESTful API HTTP methods against our API routes from the browser.

### Swagger UI screenshot example

![Swagger UI Screenshot 1](/README-IMAGES/Swagger-UI-1.png)
