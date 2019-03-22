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

## Hot to use AdaptiveDialog in Json

- Enlist in botbuilder-dotnet
- Checkout the ComposableDialog branch
- Open solution in Visual Studio
- There is a test bot ready to adapt to your needs: Microsoft.Bot.Builder.TestBot.Json
- Select a sample in the samples folder that best matches what you want to achieve
- Open TestBot.cs and replace the line below with a pointer to the root dialog you want for your bot:

```csharp
rootDialog = DeclarativeTypeLoader.Load<IDialog>(File.ReadAllText(@"Samples\Planning 11 - HttpRequest\HttpRequest.main.dialog"), resourceProvider);
```
- Run the project and open in the emulator!
- Editor hint: Visual Studio does not support our json schemas. Use Visual Studio Code to edit json files!
- Troubleshooting: If your bot does not respond, double check that the port in which your bot is running matches the bot file in the project.

## How to use AdaptiveDialog in code

- Enlist in botbuilder-dotnet
- Checkout the ComposableDialog branch
- Open solution in Visual Studio
- There is a test bot ready to adapt to your needs: Microsoft.Bot.Builder.TestBot
- Create a new AdaptiveDialog and start hacking! Example:

### Simple example: Text Prompt

```csharp
        {
            var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.AddRule(
                new DefaultRule(
                    new List<IDialog>()
                    {
                        new IfProperty()
                        {
                            Expression = new CommonExpression("user.name == null"),
                            IfTrue = new List<IDialog>()
                            {
                                new TextPrompt()
                                {
                                    InitialPrompt = new ActivityTemplate("Hello, what is your name?"),
                                    Property = "user.name"
                                }
                            }
                        },
                        new SendActivity("Hello {user.name}, nice to meet you!")
                    }));
```

### CallDialog example

```csharp
var convoState = new ConversationState(new MemoryStorage());
            var userState = new UserState(new MemoryStorage());

            var ruleDialog = new AdaptiveDialog("planningTest");

            ruleDialog.Recognizer = new RegexRecognizer() { Intents = new Dictionary<string, string>() { { "JokeIntent", "joke" } } };

            ruleDialog.AddRules(new List<IRule>()
            {
                new ReplacePlanRule("JokeIntent",
                    steps: new List<IDialog>()
                    {
                        new CallDialog("TellJokeDialog")
                    }),
                new WelcomeRule(
                    steps: new List<IDialog>()
                    {
                        new SendActivity("I'm a joke bot. To get started say 'tell me a joke'")
                    }),
                new DefaultRule(
                    new List<IDialog>()
                    {
                        new CallDialog("AskNameDialog")
                    })});

            ruleDialog.AddDialog(new[] {
                new AdaptiveDialog("AskNameDialog")
                {
                    Rules = new List<IRule>()
                    {
                        new DefaultRule(new List<IDialog>()
                        {
                            new IfProperty()
                            {
                                Expression = new CommonExpression("user.name == null"),
                                IfTrue = new List<IDialog>()
                                {
                                    new TextPrompt()
                                    {
                                        InitialPrompt = new ActivityTemplate("Hello, what is your name?"),
                                        OutputBinding = "user.name"
                                    }
                                }
                            },
                            new SendActivity("Hello {user.name}, nice to meet you!")
                        })
                    }
                }

                });

            ruleDialog.AddDialog(new[] {
                new AdaptiveDialog("TellJokeDialog")
                    {
                        Rules = new List<IRule>() {
                            new DefaultRule(new List<IDialog>()
                            {
                                new SendActivity("Why did the chicken cross the road?"),
                                new WaitForInput(),
                                new SendActivity("To get to the other side")
                            })
                        }
                    }
                });
```
