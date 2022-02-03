> ## Important Notice!
>
> The Bot Framework Custom Adapters are being moved to the [BotBuilder Community](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet) repository.
> We recommend moving bot dependencies and submit new feature requests and contributions to the new location.
>
# Microsoft Bot Framework SlackAdapter for .NET

This package contains an adapter that communicates directly with the Slack API, and translates messages to and from a standard format used by your bot.

## How to Install

````
PM> Install-Package Microsoft.Bot.Builder.Adapters.Slack
````
## How to Use

### Set the Slack Credentials

To authenticate the requests, you'll need to configure the Adapter with the Verification Token, the Bot Token and a Client Signing Secret.

You could create in the project an `appsettings.json` file to set the Slack credentials as follows:

```json
{
  "SlackVerificationToken": "",
  "SlackBotToken": "",
  "SlackClientSigningSecret": ""
}
```

### Use SlackAdapter in your App

SlackAdapter provides a translation layer for BotBuilder so that bot developers can connect to Slack and have access to the Slack API.

To add the Slack Adapter to a bot, for example, an `EchoBot`, in the `Startup` class you should add:

```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    // Create the Bot Framework Slack Adapter.
    services.AddSingleton<IBotFrameworkHttpAdapter, SlackAdapter>();

    // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
    services.AddTransient<IBot, EchoBot>();
}
```

## Contributing

This project is no longer accepting community contributions. Please refer to the project's [new location](https://github.com/BotBuilderCommunity/botbuilder-community-dotnet) for future development.

## License

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](https://github.com/Microsoft/vscode/blob/master/LICENSE.txt) License.

