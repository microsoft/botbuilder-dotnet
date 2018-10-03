This sample shows how to create a bot that relies on language generation sdk/library as a response resolution engine.

# To try this sample
- Clone the repository
    ```bash
    git https://github.com/Microsoft/botbuilder-dotnet.git
    ```
- In a terminal, switch to MSLG branch, 
    ```bash
    git checkout MSLG
    cd samples/Cafe-Bot
- Using any editor, open Keys.cs and add your subscription key
- open TemplateResponses.cs to inspect what templates are referenced in the bot and change them if you need
- Language generation models used for this sample could be found under samples/Cafe-Bot/models

## Visual Studio
- Navigate to  `Cafe-bot` sample folder and open Cafe-Bot.csproj in Visual Studio 
- Hit F5

## Testing the bot using Bot Framework Emulator
[Microsoft Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator from [here](https://aka.ms/botframeworkemulator).

## Connect to bot using Bot Framework Emulator **V4**
- Launch Bot Framework Emulator
- File -> Open bot and navigate to `Cafe-bot` sample folder
- Select BotConfiguration.bot file

## Further reading
- To learn how to write .lg models and deploy them using MSLG authoring tool please check [Authoring tool]( https://github.com/Microsoft/botbuilder-tools/tree/mslg/packages/MSLG)