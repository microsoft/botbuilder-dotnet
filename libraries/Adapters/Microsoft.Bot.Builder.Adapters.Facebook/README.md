> ## Important Notice!
>
> The Bot Framework Custom Adapters are being moved to the [BotBuilder Community](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet) repository.
> We recommend moving bot dependencies and submit new feature requests and contributions to the new location.
>

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

This project is no longer accepting community contributions. Please refer to the project's [new location](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet) for future development.

## License

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](https://github.com/Microsoft/vscode/blob/master/LICENSE.txt) License.

