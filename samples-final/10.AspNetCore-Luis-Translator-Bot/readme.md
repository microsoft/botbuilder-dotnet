This sample shows how to integrate LUIS to a bot with ASP.Net Core 2. 

## To try this sample
- Clone the repository
```bash
git clone https://github.com/Microsoft/botbuilder-dotnet.git
```
### [Required] Getting set up with LUIS.ai model
- Navigate to [LUIS](http://luis.ai)
- Sign in
- "Create or import an app"
- Get translator api key from here https://www.microsoft.com/en-us/translator/
- Update startup.cs file to include your
	- Luis-ModelId = XXXXXXXXXXXXX
    - Luis-SubscriptionId = YYYYYYYYYYYY
	- Translator-Key = ZZZZZZZZZZZZZZ

### Visual studio
- Navigate to the samples folder and open 10-AspNetCore-Luis-Translator-Bot.csproj in Visual studio 
- Hit F5

## Testing the bot using Bot Framework Emulator
[Microsoft Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework emulator from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to bot using Bot Framework Emulator **V4**
- Launch Bot Framework Emulator
- Copy and paste the url that you launched your bot app to and concatenate to it /api/messages
	- The url should look like http://localhost/[PORT_NUMBER]/api/messages
- You can trigger the translation by typing in the emulator "set my language to [LANGUAGE_ID]
- To configure which languages your application can support, edit the TranslatorLocaleHelper.cs class to add or remove more languages

# LUIS
Language Understanding service (LUIS) allows your application to understand what a person wants in their own words. LUIS uses machine learning to allow developers to build applications that can receive user input in natural language and extract meaning from it.

#Bot Translator
Bot translator allows your application to support multiple languages without an explicit need to train different Language understanding models for each language.

# Further reading

- [Azure Bot Service Introduction](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Bot State](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-storage-concept?view=azure-bot-service-4.0)
- [LUIS documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/LUIS/)
- [Bot Translator] (https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-translation?view=azure-bot-service-4.0&tabs=cs)