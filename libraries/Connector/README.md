# Azure Bot Framework Connector for .NET

### Getting Started

Bot Framework Connector allows you to build applications that connects with Bot Framework Connector service.

### Target Frameworks:

* .NET Framework 4.5.2
* Netstandard 1.4, based on the NetCore framework

### Installation

Install Bot Framework Connector by launching the command:
    
    nuget install Microsoft.Azure.BotFramework.Connector

### API Documentation

For API Documentation, please see our [API reference](https://docs.microsoft.com/en-us/Bot-Framework/rest-api/bot-framework-rest-connector-api-reference).

### Samples

Authentication, client creation and conversation initialization as an example:

````C#
var credentials = new MicrosoftAppCredentials("<your-app-id>", "<your-app-password>");
var serviceUri = new Uri("https://slack.botframework.com", UriKind.Absolute);
var bot = new ChannelAccount() { Id = "<bot-id>" };
var user = new ChannelAccount() { Id = "<user-id>" };

var activity = new Activity()
{
    Type = ActivityType.Message,
    Recipient = user,
    FromProperty = bot,
    Text = "this is a message from Bot Connector SDK"
};

var param = new ConversationParameters()
{
    Members = new ChannelAccount[] { user },
    Bot = bot
};

using (var client = new BotConnector(serviceUri, credentials))
{
    var conversation = await client.Conversations.CreateConversationAsync(param);
    var response = await client.Conversations.SendToConversationAsync(conversation.Id, activity);
}
````

## Build Instructions:

To build this project, please follow the instructions [here](https://github.com/Azure/azure-sdk-for-net/blob/psSdkJson6/README.md).
