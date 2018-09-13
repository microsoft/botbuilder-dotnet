This sample demonstrates how to host Bot Builder v3 dialogs in a Bot Builder v4 bot.
# Concepts introduced in this sample
## What is a bot?
A bot is an app that users interact with in a conversational way using text, graphics (cards), or speech. It may be a simple question and answer dialog,
or a sophisticated bot that allows people to interact with services in an intelligent manner using pattern matching,
state tracking and artificial intelligence techniques well-integrated with existing business services.
## Dialogs Bridge
This simple example demonstrates how Bot Builder v3 dialogs can be hosted in a v4 bot by using the Microsoft.Bot.Builder.Dialogs.Bridge.BridgeDialog.  
## To try this sample
- Clone the repository.
```bash
git clone https://github.com/microsoft/botbuilder-samples.git
```
 ### Visual studio
- Navigate to the samples folder (`BotBuilder-Samples\csharp_dotnetcore\Multiple-Dialogs-Bridge\`) and open Multiple_Dialogs_Bridge.csproj in Visual Studio 
- Hit F5
 ### Visual studio code
- Open `BotBuilder-Samples\csharp_dotnetcore\Multiple-Dialogs-Bridge\` folder
- Bring up a terminal, navigate to BotBuilder-Samples\csharp_dotnetcore\Multiple-Dialogs-Bridge
- Type 'dotnet run'.
## Update packages
- In Visual Studio right click on the solution and select "Restore NuGet Packages".
  **Note:** this sample requires `Microsoft.Bot.Builder.Classic`, `Microsoft.Bot.Builder.Dialogs.Bridge`, and `Microsoft.Bot.Builder.Integration.AspNet.Core`.
# Further reading
- [Azure Bot Service Introduction](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Bot basics](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Channels and Bot Connector service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)