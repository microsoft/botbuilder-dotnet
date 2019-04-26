# Bot Framework SDK Hack

Welcome and thank you for taking part at today Conversational-AI hack.

Today's hack includes the following topics:
- Virtual Assistant and Skills
- Adaptive dialogs 
- DirectLine Speech
- Conversation Designer 

## Prerequisites:
For today's hack you will need 
-	Visual Studio or VS Code 
-	[Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator/releases). Get the latest [here](https://github.com/Microsoft/BotFramework-Emulator/releases)   

> Please note, that some topics reuqires additional prerequisistes. Read each topic instruction carefuly.


## Virtual Assistent and SKills
For BUild, we are releasing a new version of Virtual Assistant, which includes the ability to use Skills. A Skill is a V4 bot with a manifest file. 

To get start - http://aka.ms/VABugBash

File Bugs: https://aka.ms/vaskillsnewbug 

## Adaptive dialogs
Adaptive dialog is a new dialog type to model conversations. It improves the current waterfall dialogs and simplifies sophisticated conversation modelling primitives such as dialog dispatcher and handle interruptions elegantly. An Adaptive dialog is a derevite of a Dialog and interacts with the rest of the SDK dialog system.

Get staretd with [initial set of docs](https://github.com/Microsoft/BotBuilder-Samples/tree/master/experimental/adaptive-dialog#Why-Adaptive-Dialog)

Adapdtive dialog samples are [here](https://github.com/Microsoft/BotBuilder-Samples/blob/master/experimental/adaptive-dialog/csharp_dotnetcore)

File bugs:
- [BotBuilder C# repo](https://github.com/microsoft/botbuilder-dotnet/issues), use [hackathon] as a prefix
- [BotBuilder JS repo](https://github.com/microsoft/botbuilder-js/issues), use [hackathon] as a prefix

A good exercise will be to build a To Do bot or a Reminder bot (without actually reminding anything) 

### Language Generation
LG is a new package in the SDK that help developer generate sophisticated responses. At the core of language generation lies template expansion and entity substitution. You can provide one-of variation for expansion as well as conditionally expand a template. The output from language generation can be a simple text string or multi-line response or a complex object payload that a layer above language generation will use to construct a full blown activity.

- Get started with LG - [here](https://github.com/Microsoft/BotBuilder-Samples/tree/master/experimental/language-generation)
- LG [Docs](https://github.com/Microsoft/BotBuilder-Samples/tree/master/experimental/language-generation/docs)
- LG [Samples](https://github.com/Microsoft/BotBuilder-Samples/tree/master/experimental/language-generation/csharp_dotnetcore)

### Common Expression Langague
The Common Expression Langauge is a new library in the SDK to support the ability to evalute logical expression and condition. This is reuqried to support conditional evaluation for LG as well as Declerative dialogs. 

To learn more about the [Common Expression Langauge](https://github.com/Microsoft/BotBuilder-Samples/tree/master/experimental/common-expression-language)


## DirectLine speech
[Getting started](https://cognitionwiki.com/pages/viewpage.action?pageId=61671091)

## Conversation Designer 
The Bot Framework Conversation Designer (code name Composer) provides a tool for 1st and 3rd party professional conversation creators with an extensible framework to build compelling Conversational AI solutions for Microsoft customers. The Composer conversatio modeling is based on the Bot Framework SDK Declarative dialogs. See [supported types Cheat sheet](../doc/AdaptiveDialog/cheatSheet.md). While it is important to understand the capabilaties of the SDK, the Composer role is to abstract the SDK complexity and the need to directly manipulate JSON objects. 

- To get started, clone the [BotFramework-Composer repo](https://github.com/Microsoft/BotFramework-Composer/)
- Follow the [installation instructions](https://github.com/Microsoft/BotFramework-Composer/tree/master/Composer#instructions)
- file bugs [here](https://github.com/Microsoft/BotFramework-Composer/issues) - use [hackathon] prefix

For the purpose of today's hack, you will be able to edit existing bots (you can't create new one yet) from the Composer *samples* directory.  Just click the Open button at the top of the Composer window, it should point to the Sample Bots folder. In each sample folder, you will find a bot.botproj file, which is the one you want to select. 
