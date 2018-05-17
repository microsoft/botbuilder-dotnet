Quite often conversations require the bot to capture missing pieces of information from the user to help complete a task or answer user's query. 

This sample shows how to use a text prompt to ask for information from the user. 

## To try this sample
- Clone the repository
```bash
git clone https://github.com/Microsoft/botbuilder-dotnet.git
```

### Visual studio
- Navigate to the samples folder and open AspNetCore-SinglePrompt-Bot.csproj in Visual studio 
- Hit F5

### Visual studio code
- open samples\4.AspNetCore-SinglePrompt-Bot folder
- Bring up a terminal, navigate to samples\4.AspNetCore-SinglePrompt-Bot folder
- type 'dotnet run'

# Prompting for information
Often times bots gather their information through questions posed to the user. You can simply send the user a standard message by using send activity to ask for a string input, however the Bot Builder SDK provides a prompts library that you can use to ask for different types for information. This topic details how to use prompts library to ask user for input.

# Further reading

- [Azure Bot Service Introduction](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Bot State](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-storage-concept?view=azure-bot-service-4.0)
- [Prompting for information](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-prompts?view=azure-bot-service-4.0&tabs=csharptab)
