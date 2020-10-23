# Language Generation ***_[PREVIEW]_***

Learning from our customers experiences and bringing together capabilities first implemented by Cortana and Cognition teams, we are introducing Language Generation; which allows the developer to extract the embedded strings from their code and resource files and manage them through a Language Generation runtime and file format.  Language Generation enable customers to define multiple variations on a phrase, execute simple expressions based on context, refer to conversational memory, and over time will enable us to bring additional capabilities all leading to a more natural conversational experience.

At the core of language generation lies template expansion and entity substitution. You can provide one-of variation for expansion as well as conditionally expand a template. The output from language generation can be a simple text string or multi-line response or a complex object payload that a layer above language generation will use to construct a full blown [activity][1].

Language generation is achieved through:

- markdown based .lg file that describes the templates and their composition. See [here][3] for the .lg file format.
- full access to current bots memory so you can data bind language to the state of memory.
- parser and runtime libraries that help achieve runtime resolution. See [here][2] for API-reference.

```markdown
# greetingTemplate
- Hello ${user.name}, how are you?
- Good morning ${user.name}. It's nice to see you again.
- Good day ${user.name}. What can I do for you today?
```

You can use language generation to:

- achieve a coherent personality, tone of voice for your bot
- separate business logic from presentation
- include variations and sophisticated composition based resolution for any of your bot's replies
- structured LG


## structured LG
The type of LG output could be string or object, string is by default. But LG could generate a json object by Structured LG feature.

Example here:

```markdown
    # HeroCardTemplate(buttonsCollection)
    [Herocard
        title=${TitleText())}
        subtitle=${SubText())}
        text=${DescriptionText())}
        images=${CardImages())}
        buttons=${buttonsCollection}
    ]

    # TitleText
    - Here are some ${TitleSuffix()}

    # TitleSuffix
    - cool photos
    - pictures
    - nice snaps

    # SubText
    - What is your favorite?
    - Don't they all look great?
    - sorry, some of them are repeats

    # DescriptionText
    - This is description for the hero card

    # CardImages
    - https://picsum.photos/200/200?image=100
    - https://picsum.photos/300/200?image=200
    - https://picsum.photos/200/200?image=400
```

the result could be:
```json
{
  "lgType": "Herocard",
  "title": "Here are some pictures",
  "text": "This is description for the hero card",
  "images": "https://picsum.photos/200/200?image=100",
  "buttons": [
    "click to see",
    "back"
  ]
}
```
the structured name would be placed into property 'lgType'.
See more tests here : [structured LG test][4]

By this, You can use the `ActivityFactory.fromObject(lgResult)` method to transform the lg output into a Bot Framework activity to post back to the user. 

see more samples here: [Structured LG to Activity][5]

## Language Generation in action

When building a bot, you can use language generation in several different ways. To start with, examine your current bot's code (or the new bot you plan to write) and create [.lg file][3] to cover all possible scenarios where you would like to use the language generation sub-system with your bot's replies to user.

Then make sure you include the platform specific language generation library.

For C#, add Microsoft.Bot.Builder.LanguageGeneration.
For NodeJS, add botbuilder-lg

Load the template manager with your .lg file

```typescript
    let templates = new Templates.parseFile(filePath, importResolver?, expressionParser?);
```

When you need template expansion, call the templates and pass in the relevant template name

```typescript
    await turnContext.sendActivity(templates.evaluate("<TemplateName>", entitiesCollection));
```

If your template needs specific entity values to be passed for resolution/ expansion, you can pass them in on the call to `evaluateTemplate`

```typescript
    await turnContext.sendActivity(templates.evaluate("WordGameReply", { GameName = "MarcoPolo" } ));
```

## Generated folder

If you changed the g4 file, please follow the instruction [here](https://github.com/antlr/antlr4/tree/master/runtime/CSharp#step-4-generate-the-c-code) to generate the new Lexer and Parser file.
The specific command is: 
```
 java -jar antlr4-4.8.jar -Dlanguage=CSharp LGFileLexer.g4
 java -jar antlr4-4.8.jar -Dlanguage=CSharp LGFileParser.g4 -visitor
 java -jar antlr4-4.8.jar -Dlanguage=CSharp LGTemplateLexer.g4
 java -jar antlr4-4.8.jar -Dlanguage=CSharp LGTemplateParser.g4 -visitor
```
`antlr4-4.8.jar` presents the path of antlr jar.
`xx.g4` presents the path of corresponding g4 file.

By the way, You will need to have a modern version of Java (>= JRE 1.6) to use it.

[1]:https://github.com/Microsoft/BotBuilder/blob/main/specs/botframework-activity/botframework-activity.md
[2]:https://docs.microsoft.com/en-us/azure/bot-service/language-generation/language-generation-api-reference?view=azure-bot-service-4.0
[3]:https://docs.microsoft.com/en-us/azure/bot-service/file-format/bot-builder-lg-file-format?view=azure-bot-service-4.0
[4]:https://github.com/microsoft/botbuilder-js/blob/main/libraries/botbuilder-lg/tests/testData/examples/StructuredTemplate.lg
[5]:https://github.com/microsoft/botbuilder-js/blob/main/libraries/botbuilder-lg/tests/testData/examples/NormalStructuredLG.lg
