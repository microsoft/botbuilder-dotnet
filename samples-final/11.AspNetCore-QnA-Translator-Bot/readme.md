This sample shows how to integrate QnA to a bot with ASP.Net Core 2. 

## To try this sample
- Clone the repository
```bash
git clone https://github.com/Microsoft/botbuilder-dotnet.git
```
### [Required] Getting set up with QnA.ai knowledge base
- Navigate to [QnA](https://www.qnamaker.ai/)
- Sign in
- "Create or import an app"
- Get translator api key from here https://www.microsoft.com/en-us/translator/
- Update startup.cs file to include your
	- QnA-EndpointKey = XXXXXXXXXXXXX
    - QnA-KnowledgeBaseId = YYYYYYYYYYYY
	- Translator-Key = ZZZZZZZZZZZZZZ

### Visual studio
- Navigate to the samples folder and open 11-AspNetCore-QnA-Translator-Bot.csproj in Visual studio 
- Hit F5

## Testing the bot using Bot Framework Emulator
[Microsoft Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework emulator from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to bot using Bot Framework Emulator **V4**
- Launch Bot Framework Emulator
- Copy and paste the url that you launched your bot app to and concatenate to it /api/messages
	- The url should look like http://localhost/[PORT_NUMBER]/api/messages

# QnA maker
Language Understanding service allows your application to understand the question asked to the bot and provide the correct answer to that question.

#Bot Translator
Bot translator allows your application to support multiple languages without an explicit need to train different Language understanding models for each language.

# Further reading

- [Azure Bot Service Introduction](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Bot State](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-storage-concept?view=azure-bot-service-4.0)
- [QnA maker documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/qnamaker/overview/overview)
- [Bot Translator] (https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-translation?view=azure-bot-service-4.0&tabs=cs)