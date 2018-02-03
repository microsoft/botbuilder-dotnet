# Microsoft Bot Framework Connector for .NET

Within the Bot Framework, the Bot Connector service enables your bot to exchange messages with users on channels that are configured in the Bot Framework Portal.

## Target Frameworks:

* .NET Framework 4.5.2
* .NET Standard 2.0, based on the NetCore framework

## How to Install

````
PM> Install-Package Microsoft.Bot.Connector
````

## How to Use

### Authentication
Your bot communicates with the Bot Connector service using HTTP over a secured channel (SSL/TLS). When your bot sends a request to the Connector service, it must include information that the Connector service can use to verify its identity.

To authenticate the requests, you'll need configure the Connector with the App ID and password that you obtained for your bot during registration and the Connector will handle the rest.

More information: https://docs.microsoft.com/en-us/bot-framework/rest-api/bot-framework-rest-connector-authentication

### Example
Client creation (with authentication), conversation initialization and activity send to user.
````C#
var credentials = new MicrosoftAppCredentials("<your-app-id>", "<your-app-password>");
var serviceUri = new Uri("https://slack.botframework.com", UriKind.Absolute);
var bot = new ChannelAccount() { Id = "<bot-id>" };
var user = new ChannelAccount() { Id = "<user-id>" };

var activity = new Activity()
{
    Type = ActivityTypes.Message,
    Recipient = user,
    FromProperty = bot,
    Text = "This a message from Bot Connector Client (.Net)"
};

var param = new ConversationParameters()
{
    Members = new ChannelAccount[] { user },
    Bot = bot
};

using (var client = new ConnectorClient(serviceUri, credentials))
{
    var conversation = await client.Conversations.CreateConversationAsync(param);
    var response = await client.Conversations.SendToConversationAsync(conversation.Id, activity);
}
````

### Simple EchoBot Example ([source code](../../samples/Connector.EchoBot))
EchoBot is a minimal bot that recieves message activities and replies with the same content.
The sample shows how to use a WebAPI Controller for listening to activities and the ConnectorClient for sending activities.

## Rest API Documentation

For the Connector Service API Documentation, please see our [API reference](https://docs.microsoft.com/en-us/Bot-Framework/rest-api/bot-framework-rest-connector-api-reference).

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
