# Microsoft Bot Framework Facebook Adapter for .NET

This package contains an adapter that communicates directly with the Facebook API, and translates messages to and from a standard format used by your bot. 
<br>Includes support for Facebook Handover Protocol.

## How to Install

````
PM> Install-Package Microsoft.Bot.Builder.Adapters.Facebook
````
## How to Use

### Set the Facebook Credentials

To authenticate the requests, you'll need to configure the Adapter with the Verify Token, the App Secret and a Access Token.

You could create in the project an `appsettings.json` file to set the Facebook credentials as follows:

```json
{
  "FacebookVerifyToken": "",
  "FacebookAppSecret": "",
  "FacebookAccessToken":  ""
}
```

### Use FacebookAdapter in your App

FacebookAdapter provides a translation layer for BotBuilder so that bot developers can connect to Facebook and have access to the Facebook API.

To add the Facebook Adapter to a bot, for example, an `EchoBot`, in the `Startup` class you should add:

```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    // Create the Bot Framework Facebook Adapter.
    services.AddSingleton<IBotFrameworkHttpAdapter, FacebookAdapter>();

    // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
    services.AddTransient<IBot, EchoBot>();
}
```

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## License

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](https://github.com/Microsoft/vscode/blob/master/LICENSE.txt) License.

