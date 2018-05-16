This sample shows how to handle diferent activity types.

Every single incoming message to your bot is an activity and there are different [activity types](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-activities-entities?view=azure-bot-service-4.0&tabs=cs#activity-types). Most commonly used are [conversationUpdate](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-activities-entities?view=azure-bot-service-4.0&tabs=cs#conversationupdate) activity and the [message](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-activities-entities?view=azure-bot-service-4.0&tabs=cs#message) activity.

# To try this sample
- Clone the repository
```bash
git clone https://github.com/Microsoft/botbuilder-dotnet.git
```

## Visual studio
- Navigate to the samples folder and open AspNetCore-ConversationUpdate-Bot.csproj in Visual studio 
- Hit F5

## Visual studio code
- open samples\1.Console-EchoBot-With-State folder
- Bring up a terminal, navigate to samples\2.AspNetCore-ConversationUpdate-Bot folder
- type 'dotnet run'


# Further reading

- [Azure Bot Service Introduction](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Bot basics](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Channels and Bot Connector service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Activity types](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-activities-entities?view=azure-bot-service-4.0&tabs=cs#activity-types)