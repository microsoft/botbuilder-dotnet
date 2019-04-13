# Bot Framework SDK Hack

Welcome and thank you for taking part at a Bot Framework hack.

We are looking for feedback on several topics we plan to release as preview at Build – May 2019. Your help is highly appreciated.  We are looking for feedback on few topics: 1) Language Generation; 2) Memory and Expressions; 3) a set of new dialogs; 4) declarative format for writing dialogs.

## Prerequisites:
-	Visual Studio or VS Code 
-	[Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator/releases). Get the latest [here](https://github.com/Microsoft/BotFramework-Emulator/releases)   

For the purpose of this hack you will use the C# version of the SDK. To participate in this hack, you will need: 
- [Composable Dialog](https://github.com/Microsoft/botbuilder-dotnet/tree/ComposableDialog) branch from BotBuilder-dotnet. The Composable Dialog branch has the latest bits for the SDK including LG, Memory, expression and decelerative.
-  [SchemaGen](https://github.com/Microsoft/botbuilder-tools/tree/SchemaGen) branch from BotFramework-tools for the set of tools needed for working with the JSON declarative dialogs. 

> Note: We recommend you fork the BotBuilder-dotnet to work without worrying about any potential code changes. 

## Samples 
Currently, we are light on documentation. However, there are few samples to help you bootstrap and get started using the new dialogs, LG, memory, and decelrative. 
-	The csharp [samples](https://github.com/Microsoft/botbuilder-dotnet/tree/ComposableDialog/samples) folder include basic documentation for Memory, Input prompts and dialogs, and Rule base dialog system. 
- We also created a version of the samples that uses packaged nuget pckages. [TestBot.Json as sample based on nuget packages called 60-AdaptiveBot](https://github.com/Microsoft/BotBuilder-Samples/blob/4.next/samples/csharp_dotnetcore/60.AdaptiveBot/README.md).  [Here](https://botbuilder.myget.org/F/botbuilder-declarative/api/v3/index.json )  the nuget feed for C# packages. 
-	The [Microsoft.Bot.Bbuilder.Testbot.Json](https://github.com/Microsoft/botbuilder-dotnet/tree/ComposableDialog/samples/Microsoft.Bot.Builder.TestBot.Json) folder include samples for LG and declarative dialogs.
-	In Microsoft.Bot.Bbuilder.Testbot.Json , the [samples](https://github.com/Microsoft/botbuilder-dotnet/tree/ComposableDialog/samples/Microsoft.Bot.Builder.TestBot.Json/Samples) folder includes a series of bots defined declaratively, showing the different dialog/ prompts and available steps. 
-	The [LG](https://github.com/Microsoft/botbuilder-dotnet/tree/ComposableDialog/samples/Microsoft.Bot.Builder.TestBot.Json/LG) folder includes list of .lg files explaining various features and functionality  of the new local LG library


## Docs
-   See [here](../doc/LanguageGeneration/readme.md) for Language Generation documentation. 
-   See [here](../doc/CommonExpressionLanguage/redme.md) for common expression language.
-   See [here](../doc/AdaptiveDialog/readme.md) for overview of Adaptive dialogs.
-	Checkout the [Supported types Cheat sheet](../doc/AdaptiveDialog/cheatSheet.md)

## Reporting Bugs 
-	Simply create a new issue on the botbuilder-dotnet repo. Use this [link](https://github.com/Microsoft/botbuilder-dotnet/issues/new?template=-net-sdk-bug.md) 
-	Make sure you add **[declarative-hack]** a prefix to both the issue title and main content 


## What bot should I build?
We would like you to try either using the Adaptive Dialog using code or using JSON. While using these dialogs, feel free to explore and follow any scenario you want. If you are looking for inspiration, we would like you to pick one of the skills in the [Virtual Assistent solution](https://github.com/Microsoft/AI/tree/master/solutions/Virtual-Assistant/src), identify one or two dialogs, and try to convert these dialogs to the new Adaptive Dialog. All VA skills includes full LUIS models that you can use and code (using Waterfall) 


## How to use AdaptiveDialog in NodeJs

- Enlist in botbuilder-js
- Checkout [4.next](https://github.com/Microsoft/botbuilder-js/tree/4.next) branch
- AdaptiveDialog samples can be found [here](https://github.com/Microsoft/botbuilder-js/tree/4.next/samples)
- From the root of the entire repo, make sure lerna is installed globally by using ```npm install -g lerna```
- From the root of the entire repo, run ```lerna bootstrap --hoist``` to setup dependencies
- From the root of the entire repo, run ```npm run build``` to setup dependencies
- Choose your favorite sample under the samples directory
- Navigate to the selected sample in a command line
- On the sample directory ```npm run build```
- On the sample directory ```npm run start``` to start the bot

## How to use AdaptiveDialog in Json

- Enlist in botbuilder-dotnet
- Checkout the [ComposableDialog](https://github.com/Microsoft/botbuilder-dotnet/tree/ComposableDialog) branch
- Open solution in Visual Studio
- We also created a version of the samples that uses packaged nuget pckages. [TestBot.Json as sample based on nuget packages called 60-AdaptiveBot](https://github.com/Microsoft/BotBuilder-Samples/blob/4.next/samples/csharp_dotnetcore/60.AdaptiveBot/README.md).  [Here](https://botbuilder.myget.org/F/botbuilder-declarative/api/v3/index.json )  the nuget feed for C# packages. 
- There is a test bot ready to adapt to your needs: Microsoft.Bot.Builder.TestBot.Json
- Select a sample in the samples folder that best matches what you want to achieve
- Open TestBot.cs and replace the line below with a pointer to the root dialog you want for your bot:

```csharp
 var rootFile = resourceExplorer.GetResource(@"ToDoBot.main.dialog");
```
- Run the project and open in the emulator!
- *Editor hint:* Visual Studio does not support our json schemas. Use Visual Studio Code to edit json files!
- *Troubleshooting:* If your bot does not respond, double check that the port in which your bot is running matches the bot file in the project.

## How to use AdaptiveDialog in C#

- Enlist in botbuilder-dotnet
- Checkout the ComposableDialog branch
- Open solution in Visual Studio
- There is a test bot ready to adapt to your needs: Microsoft.Bot.Builder.TestBot
- Create a new AdaptiveDialog and start hacking! Example:


# Trying the LG sample

The [Microsoft.Bot.Bbuilder.Testbot.Json](https://github.com/Microsoft/botbuilder-dotnet/tree/ComposableDialog/samples/Microsoft.Bot.Builder.TestBot.Json) folder include samples for LG, see [here](./LG-file-format.md) for an overivew of the LG file format. 

The code include a TestBotLG.cs that you can use to run and experiment with LG.  As this test botproject  is also used for running declarative dialogs, you will need to change Startup.cs and switch between which bot is used.  To do so, comment the line that return a new TestBot, and uncomment the line that returns TestBotLG. 

```
            services.AddBot<IBot>(
                (IServiceProvider sp) =>
                {
                    // declarative Adaptive dialogs bot sample
                    return new TestBot(accessors, botResourceManager);

                    // LG bot sample
                    // return new TestBotLG(accessors);
                },

```


## Building the SDK tools locally
To use DialogLint, DialogSchema, and DialogTracker tools, you will need to build the Botbuilder-tools SDK locally. 

- Enlist in [botbuilder-tools](https://github.com/Microsoft/botbuilder-tools/tree/SchemaGen)
- Checkout the SchemaGen branch
- from root, run *npm run build*
- from the tool folder, install the tool. For example: *npm i -g .* 

```
cd botbuilder-tools

npm run build 

cd .\packages\DialogLint\

npm i -g .
```
