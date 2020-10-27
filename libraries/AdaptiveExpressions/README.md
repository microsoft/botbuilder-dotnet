### Introduction
Bots, like any other application, require use of expressions to evaluate outcome of a condition based on runtime information available in memory or to the dialog or the language generation system.

Common expression language was put together to address this core need as well as to rationalize and snap to a common expression language that will be used across Bot Framework SDK and other conversational AI components that need an expression language.

[More info](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-adaptive-expressions?view=azure-bot-service-4.0)

See [API reference for Expression](https://docs.microsoft.com/en-us/azure/bot-service/adaptive-expressions/adaptive-expressions-api-reference?view=azure-bot-service-4.0) for API reference.

See [Here](https://docs.microsoft.com/en-us/azure/bot-service/adaptive-expressions/adaptive-expressions-prebuilt-functions?view=azure-bot-service-4.0) for a complete list of prebuilt functions supported by the common expression language library.

### Generated folder

If you changed the g4 file, please follow the instruction [here](https://github.com/antlr/antlr4/tree/master/runtime/CSharp#step-4-generate-the-c-code) to generate the new Lexer and Parser file.
The specific command is: 
```
 java -jar antlr4-4.8.jar -Dlanguage=CSharp CommonmRegex.g4
 java -jar antlr4-4.8.jar -Dlanguage=CSharp ExpressionAntlrLexer.g4
 java -jar antlr4-4.8.jar -Dlanguage=CSharp ExpressionAntlrParser.g4 -visitor
```
`antlr4-4.8.jar` presents the path of antlr jar.
`xx.g4` presents the path of corresponding g4 file.

By the way, You will need to have a modern version of Java (>= JRE 1.6) to use it.
