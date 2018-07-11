This sample shows how to integrate LUIS to a bot with ASP.Net Core 2. 

## To try this sample
- Clone the repository
```bash
git clone https://github.com/Microsoft/botbuilder-dotnet.git
```
### [Required] Getting set up with LUIS.ai model
- Navigate to [LUIS](http://luis.ai)
- Sign in
- Click on My apps
- "Import new App"
- Choose file -> select [LUIS-Reminders.json](LUIS-Reminders.json)
- Update [appsettings.json](appsettings.json) with your Luis-ModelId, Luis-SubscriptionId and Luis-Url. You can find this information under "Publish" tab for your LUIS application at [luis.ai](https://luis.ai). E.g. For https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/XXXXXXXXXXXXX?subscription-key=YYYYYYYYYYYY&verbose=true&timezoneOffset=0&q= 
    - Luis-ModelId = XXXXXXXXXXXXX
    - Luis-SubscriptionId = YYYYYYYYYYYY
    - Luis-Url = https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/
    
NOTE: Once you publish your app on LUIS portal for the first time, it takes some time for the endpoint to become available, about 5 minutes of wait should be sufficient.

### Visual studio
- Navigate to the samples folder and open AspNetCore-LUIS-Bot.csproj in Visual studio 
- Hit F5

### Visual studio code
- open samples\8.AspNetCore-LUIS-Bot folder
- Bring up a terminal, navigate to samples\8.AspNetCore-LUIS-Bot folder
- type 'dotnet run'

## Testing the bot using Bot Framework Emulator
[Microsoft Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework emulator from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to bot using Bot Framework Emulator **V4**
- Launch Bot Framework Emulator
- File -> Open bot and navigate to samples\8.AspNetCore-LUIS-Bot folder
- Select AspNetCore-LUIS-Bot.bot file

### Connect to bot using Bot Framework Emulator **V3**
- Launch Bot Framework Emulator
- Paste this URL in the emulator window - http://localhost:5000/api/messages

# LUIS
Language Understanding service (LUIS) allows your application to understand what a person wants in their own words. LUIS uses machine learning to allow developers to build applications that can receive user input in natural language and extract meaning from it.

# Further reading

- [Azure Bot Service Introduction](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Bot State](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-storage-concept?view=azure-bot-service-4.0)
- [LUIS documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/LUIS/)

