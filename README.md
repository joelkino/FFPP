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

- You have an IDE for dotnet core (currently .NET 6) development ([Visual Studio / Visual Studio Code (Free)](https://visualstudio.microsoft.com/)) unless you are hardcore and enjoy coding from the command line (I have seen people do this 😂).

- You have a development environment setup to run the [CIPP](https://github.com/KelvinTegelaar/CIPP) react/swa front end. You can find the instructions to setup a devenv [here](https://cipp.app/docs/dev/settingup/).

- **[Optional]** If you wish to contribute code, it is a good idea to run a devenv of the official [CIPP-API](https://github.com/KelvinTegelaar/CIPP-API) and compare output between APIs to ensure both are matching. If you follow the complete instructions [here](https://cipp.app/docs/dev/settingup/) you will end up with a devenv for both CIPP and CIPP-API which is ideal. Also, it's a nice thing to do to port new functionality back to original CIPP-API if we can do it, to try and maintain feature parity across repositories.

## Setting up development environment (devenv)

### Clone Repository

If you are on Windows make sure you have git installed first. On any platform, open cmd/terminal and navigate (`cd`) to where you want to download this project.

Run the command `git clone git@github.com:White-Knight-IT/CIPP-ALT-API.git`

### Secrets

We leverage the official dotnet core method of creating secrets for our local devenv, that is using the [user-secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=linux) tool.
