# Bot Framework SDK Rule and Declarative Hack

Welcome and thank you for taking part at a Bot Framework hack.

We are looking for feedback on several topics we plan to release as preview at Build – May 2019. Your help is highly appreciated.  We are looking for feedback on few topics: 1) Language Generation; 2) Memory and Expressions; 3) a set of new dialogs; 4) declarative format for writing dialogs.

## Prerequisites:
-	Visual Studio or VS Code 
-	[Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator/releases). Get the latest [here](https://github.com/Microsoft/BotFramework-Emulator/releases)   

For the purpose of this hack you will use the C# version of the SDK. To participate in this hack, you will need: 
- [Composable Dialog](https://github.com/Microsoft/botbuilder-dotnet/tree/ComposableDialog) branch from BotBuilder-dotnet. The Composable Dialog branch has the latest bits for the SDK including LG, Memory, expression and decelerative.
-  [SchemaGen](https://github.com/Microsoft/botbuilder-tools/tree/SchemaGen) branch from BotFramework-tools for the set of tools needed for working with the JSON declarative dialogs. 

> Note: We recommend you fork the BotBuilder-dotnet to work without worrying about any potential code changes. 

## Samples and Docs 
Currently, we are light on documentation. However, there are few samples to help you bootstrap and get started using the new dialogs, LG, memory, and decelrative. 
-	The main [samples](https://github.com/Microsoft/botbuilder-dotnet/tree/ComposableDialog/samples) folder include basic documentation for Memory, Input prompts and dialogs, and Rule base dialog system. 
-	The [Microsoft.Bot.Bbuilder.Testbot.Json](https://github.com/Microsoft/botbuilder-dotnet/tree/ComposableDialog/samples/Microsoft.Bot.Builder.TestBot.Json) folder include samples for LG and declarative dialogs.
-   See [here](./LG-file-format.md) for an overivew of the LG file format. 
-	In Microsoft.Bot.Bbuilder.Testbot.Json , the [samples](https://github.com/Microsoft/botbuilder-dotnet/tree/ComposableDialog/samples/Microsoft.Bot.Builder.TestBot.Json/Samples) folder includes a series of bots defined declaratively, showing the different dialog/ prompts and available steps. You can read more here.
-	The [LG](https://github.com/Microsoft/botbuilder-dotnet/tree/ComposableDialog/samples/Microsoft.Bot.Builder.TestBot.Json/LG) folder includes list of .lg files explaining various features and functionality  of the new local LG library

## Reporting Bugs 
-	Simply create a new issue on the botbuilder-dotnet repo. Use this [link](https://github.com/Microsoft/botbuilder-dotnet/issues/new?template=-net-sdk-bug.md) 
-	Make sure you add **[declarative-hack]** a prefix to both the issue title and main content 


## What bot should I build?
Feel free to explorer and follow any scenario you want. Form filling and task completion are the good candidate like Ice cream (or Pizza) ordering, managing lists of Todo, Alarm Bot; booking a table; etc.  In the process of creating your bot’s dialog, try to include new elements and combine different parts like LG, Memory and Decelerative. 
