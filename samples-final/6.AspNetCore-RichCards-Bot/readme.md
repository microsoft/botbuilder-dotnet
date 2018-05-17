Bots support multi-modal conversations so you can add media to messages sent by the bot to the user. 

This sample shows how to use different types of rich cards. This sample uses [prompts](../4.AspNetCore-SinglePrompt-Bot) and [waterfall dialogs](../5.AspNetCore-MultiplePrompts-Bot) to demonstrate different rich card types.

## To try this sample
- Clone the repository
```bash
git clone https://github.com/Microsoft/botbuilder-dotnet.git
```

### Visual studio
- Navigate to the samples folder and open AspNetCore-RichCards-Bot.csproj in Visual studio 
- Hit F5

### Visual studio code
- open samples\6.AspNetCore-RichCards-Bot folder
- Bring up a terminal, navigate to samples\6.AspNetCore-RichCards-Bot folder
- type 'dotnet run'

## Testing the bot using Bot Framework Emulator
[Microsoft Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework emulator from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to bot using Bot Framework Emulator **V4**
- Launch Bot Framework Emulator
- File -> Open bot and navigate to samples\6.AspNetCore-RichCards-Bot folder
- Select AspNetCore-RichCards-Bot.bot file

### Connect to bot using Bot Framework Emulator **V3**
- Launch Bot Framework Emulator
- Paste this URL in the emulator window - http://localhost:5000/api/messages

# Adding media to messages
A message exchange between user and bot can contain media attachments, such as cards, images, video, audio, and files.

There are several different card types supported by Bot Framework including:
- [Adaptive card](http://adaptivecards.io)
- [Hero card](https://docs.microsoft.com/en-us/azure/bot-service/rest-api/bot-framework-rest-connector-api-reference?view=azure-bot-service-4.0#herocard-object)
- [Thumbnail card](https://docs.microsoft.com/en-us/azure/bot-service/rest-api/bot-framework-rest-connector-api-reference?view=azure-bot-service-4.0#thumbnailcard-object)
- [More...](https://docs.microsoft.com/en-us/azure/bot-service/rest-api/bot-framework-rest-connector-add-rich-cards?view=azure-bot-service-4.0)

# Further reading

- [Azure Bot Service Introduction](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Bot State](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-storage-concept?view=azure-bot-service-4.0)
- [Add media to messages](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-add-media-attachments?view=azure-bot-service-4.0&tabs=csharp)
- [Rich card types](https://docs.microsoft.com/en-us/azure/bot-service/rest-api/bot-framework-rest-connector-add-rich-cards?view=azure-bot-service-4.0)
