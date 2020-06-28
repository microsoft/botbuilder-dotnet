# ![Bot Framework for dotnet](./doc/media/BotFrameworkDotnet_header.png)

### [What's new with Bot Framework](https://docs.microsoft.com/en-us/azure/bot-service/what-is-new?view=azure-bot-service-4.0)

This repository contains code for the .NET version of the [Microsoft Bot Framework SDK](https://github.com/Microsoft/botframework-sdk), which is part of the Microsoft Bot Framework - a comprehensive framework for building enterprise-grade conversational AI experiences. 

This SDK enables developers to model conversation and build sophisticated bot applications using .NET. SDKs for [JavaScript](https://github.com/Microsoft/botbuilder-js), [Python](https://github.com/Microsoft/botbuilder-python) and [Java (preview)](https://github.com/Microsoft/botbuilder-java) are also available.

To get started building bots using the SDK, see the [Azure Bot Service Documentation](https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0).

For more information jump to a section below.

* [Build status](#build-status)
* [Packages](#packages)
* [Getting started](#getting-started)
* [Getting support and providing feedback](#getting-support-and-providing-feedback)
* [Contributing and our code of conduct](contributing-and-our-code-of-conduct)
* [Reporting security sssues](#reporting-security-issues)

## Build Status

 | Branch | Description        | Build Status | Coverage Status | Windows Bot Test Status | Linux Bot Test Status |
 |----|---------------|--------------|-----------------|--|--|
 |Master | 4.10.* Preview Builds |[![Build Status](https://fuselabs.visualstudio.com/SDK_v4/_apis/build/status/DotNet/BotBuilder-DotNet-Signed-daily?branchName=master)](https://fuselabs.visualstudio.com/SDK_v4/_build/latest?definitionId=277&branchName=master) |[![Coverage Status](https://coveralls.io/repos/github/Microsoft/botbuilder-dotnet/badge.svg?branch=master&service=github)](https://coveralls.io/github/Microsoft/botbuilder-dotnet?branch=master) | [![Tests Status](https://fuselabs.visualstudio.com/SDK_v4/_apis/build/status/DotNet/FunctionalTests/BotBuilder-DotNet-Functional-Tests-Windows-yaml?branchName=master)](https://fuselabs.visualstudio.com/SDK_v4/_build/latest?definitionId=834&branchName=master) |  [![Tests Status](https://fuselabs.visualstudio.com/SDK_v4/_apis/build/status/DotNet/FunctionalTests/BotBuilder-DotNet-Functional-Tests-Linux-yaml?branchName=master)](https://fuselabs.visualstudio.com/SDK_v4/_build/latest?definitionId=779&branchName=master)

## Packages

| Name                                  | Released Package | Daily Build                                                                                                                                                                  |
|---------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|--|
| Microsoft.Bot.Builder                 | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Builder?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Builder/)                                 | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder) |
| Microsoft.Bot.Builder.AI.LUIS         | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Builder.AI.LUIS?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Builder.AI.LUIS/)                 | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder.AI.LUIS?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.AI.LUIS) |
| Microsoft.Bot.Builder.AI.QnA          | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Builder.AI.QnA?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Builder.AI.Qna/)                   | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder.AI.QnA?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.AI.QnA) |
| Microsoft.Bot.Builder.ApplicationInsights         | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Builder.ApplicationInsights?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Builder.ApplicationInsights/)                 | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder.ApplicationInsights?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.ApplicationInsights) |
| Microsoft.Bot.Builder.Azure           | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Builder.Azure?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/)                     | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder.Azure?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.Azure) |
| Microsoft.Bot.Builder.Dialogs         | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Builder.Dialogs?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Builder.Dialogs/)                 | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder.Dialogs?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.Dialogs) |
| Microsoft.Bot.Builder.Dialogs.Adaptive         | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Builder.Dialogs.Adaptive?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Builder.Dialogs.Adaptive/)                 | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder.Dialogs.Adaptive?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.Dialogs.Adaptive) |
| Microsoft.Bot.Builder.Dialogs.Declarative         | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Builder.Dialogs.Declarative?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Builder.Dialogs.Declarative/)                 | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder.Dialogs.Declarative?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.Dialogs.Declarative) |
| Microsoft.Bot.Builder.TemplateManager | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Builder.TemplateManager?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Builder.TemplateManager/) | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder.TemplateManager?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.TemplateManager) |
| Microsoft.Bot.Builder.Integration.ApplicationInsights.Core | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Builder.Integration.ApplicationInsights.Core?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Builder.Integration.ApplicationInsights.Core/) | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder.Integration.ApplicationInsights.Core?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.Integration.ApplicationInsights.Core) |
| Microsoft.Bot.Builder.Integration.ApplicationInsights.WebApi | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Builder.Integration.ApplicationInsights.WebApi?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Builder.Integration.ApplicationInsights.WebApi/) | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder.Integration.ApplicationInsights.WebApi?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.Integration.ApplicationInsights.WebApi) |
| Microsoft.Bot.Builder.LanguageGeneration | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Builder.LanguageGeneration?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Builder.LanguageGeneration/) | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder.LanguageGeneration?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.LanguageGeneration) |
| Microsoft.Bot.Builder.Testing           | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Builder.Testing?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Builder.Testing/)                     | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Builder.Testing?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Builder.Testing) |
| Microsoft.Bot.Configuration           | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Configuration?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Configuration/)                     | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Configuration?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Configuration) |
| Microsoft.Bot.Connector               | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Connector?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Connector/)                             | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Connector?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Connector) |
| Microsoft.Bot.Schema                  | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Schema?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Schema/)                                   | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Schema?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Schema) |
| Microsoft.Bot.Streaming                  | [![BotBuilder Badge](https://buildstats.info/nuget/Microsoft.Bot.Streaming?dWidth=70)](https://www.nuget.org/packages/Microsoft.Bot.Streaming/)                                   | [![BotBuilder Badge](https://buildstats.info/myget/botbuilder/botbuilder-v4-dotnet-daily/Microsoft.Bot.Streaming?includePreReleases=true&dWidth=50)](https://botbuilder.myget.org/feed/botbuilder-v4-dotnet-daily/package/nuget/Microsoft.Bot.Streaming) |

You can refer to the [dependency graph](https://botbuildersdkblobstorage.blob.core.windows.net/sdk-dotnet-dependency-reports/4.9.2/InterdependencyGraph.html) for our libraries.

To use the daily builds, which are published to MyGet, please follow the instructions [here](UsingMyGet.md).

## Getting Started
To get started building bots using the SDK, see the [Azure Bot Service Documentation](https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0).

The [Bot Framework Samples](https://github.com/microsoft/botbuilder-samples) includes a rich set of samples repository.

If you want to debug an issue, would like to [contribute](#contributing), or understand how the Bot Builder SDK works, instructions for building and testing the SDK are below.

### Prerequisites
- [Git](https://git-scm.com/downloads) 
- [Visual Studio](https://www.visualstudio.com/)

### Clone
Clone a copy of the repo:

```bash
git clone https://github.com/Microsoft/botbuilder-dotnet.git
```

Change to the SDK's directory:

```bash
cd botbuilder-dotnet
```

### Build and test locally
Open `Microsoft.Bot.Builder.sln` in Visual Studio. 
On the menu bar, choose **Build** > **Build Solution**.

When the solution is built, local NuGet package files (.nupkg) are generated for each project and are available under the `outputPackages` directory.  You can add this folder to your NuGet Package Manager source list in Visual Studio (choose **Tools > NuGet Package Manager > Package Manager Settings** from the Visual Studio menu and add an additional **Package Sources** from there), allowing you to consume these in your local projects.

## Getting support and providing feedback
Below are the various channels that are available to you for obtaining support and providing feedback. Please pay carful attention to which channel should be used for which type of content. e.g. general "how do I..." questions should be asked on Stack Overflow, Twitter or Gitter, with GitHub issues being for feature requests and bug reports.

### Github issues
[Github issues](https://github.com/Microsoft/botbuilder-dotnet/issues) should be used for bugs and feature requests. 

### Stack overflow
[Stack Overflow](https://stackoverflow.com/questions/tagged/botframework) is a great place for getting high-quality answers. Our support team, as well as many of our community members are already on Stack Overflow providing answers to 'how-to' questions.

### Azure Support 
If you issues relates to [Azure Bot Service](https://azure.microsoft.com/en-gb/services/bot-service/), you can take advantage of the available [Azure support options](https://azure.microsoft.com/en-us/support/options/).

### Twitter
We use the [@botframework](https://twitter.com/botframework) account on twitter for announcements and members from the development team watch for tweets for @botframework.

### Gitter Chat Room
The [Gitter Channel](https://gitter.im/Microsoft/BotBuilder) provides a place where the Community can get together and collaborate.

## Contributing and our code of conduct
We welcome contributions and suggestions. Please see our [contributing guidelines](./contributing.md) for more information.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). 
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact
 [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Reporting Security Issues
Security issues and bugs should be reported privately, via email, to the Microsoft Security Response Center (MSRC) 
at [secure@microsoft.com](mailto:secure@microsoft.com).  You should receive a response within 24 hours.  If for some
 reason you do not, please follow up via email to ensure we received your original message. Further information, 
 including the [MSRC PGP](https://technet.microsoft.com/en-us/security/dn606155) key, can be found in the 
[Security TechCenter](https://technet.microsoft.com/en-us/security/default).

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](./LICENSE.md) License.

