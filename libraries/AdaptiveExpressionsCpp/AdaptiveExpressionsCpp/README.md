# AdaptiveExpressions Library for Cpp

# Getting started

## Generating the antlr Lexer and Parser

### Pre-requisites

- [Java]
- [Antlr Java binaries](https://www.antlr.org/download.html)

Antlr is composed of two parts, the tool to generate the lexer and parser and the runtime.

1. Install the tool you can follow the instructions from this [tutorial](https://tomassetti.me/antlr-mega-tutorial/#setup-antlr)
2. Copy the grammar and lex files from `\libraries\AdaptiveExpressions\parser` to `\libraries\AdaptiveExpressionsCpp\AdaptiveExpressionsCpp\Parser`
3. Execute the following commands
```
antlr4 .\ExpressionAntlrLexer.g4 -Dlanguage=Cpp -no-listener -visitor
antlr4 .\ExpressionAntlrParser.g4 -Dlanguage=Cpp -no-listener -visitor
```
> Note: antlr4 is an alias for antlr.jar

We are generating the parser and lexer to have a visitor pattern to allow values to be returned. It's also important to note that the lexer has to be generated before the parser. The results should contain the following files:
* ExpressionAntlrLexer.h
* ExpressionAntlrLexer.cpp
* ExpressionAntlrParser.h
* ExpressionAntlrParser.cpp

## Downloading the Runtime

The runtime can be downloaded from the antlr [download page](https://www.antlr.org/download.html) under the C++ Target section.

### Building the runtime

If the binaries from the website don't work you can download the antlr source code from the download page, you can also download the source code from the same section or from their [GitHub repository](https://github.com/antlr/antlr4).

To build the runtime from the source code:
> Pre-requisite: Install 7zip and add the install directory to your PATH system variable 
1. Open a Visual Studio Command Prompt
2. Change directory to `CodePath/antlr4/runtime/Cpp/`
3. Execute `deploy-windows.cmd Professional`

> If you need to generate Debug DLLs you need to modify the script file changing the build configuration from "Release DLL" to "Debug DLL"

A zip file named "antlr4-cpp-runtime-vsxxxx.zip" should be generated, that zip file includes:
* antlr4-runtime.dll
* antlr4-runtime.lib
* `antlr4-runtime` folder containing all necesary header files 

Finally, move the `antlr4-runtime` and `lib` folders to `libraries\AdaptiveExpressionsCpp\AdaptiveExpressionsCpp`