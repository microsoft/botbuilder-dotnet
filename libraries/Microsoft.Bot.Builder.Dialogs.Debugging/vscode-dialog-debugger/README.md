# Microsoft Bot Framework Debugger

Bots are stateless web services made up of a graph of objects which are executed against.
This architecture provides a highly scalable execution environment but makes it hard to debug because the bot's
conversation is stepping through the instance object graph of dialogs as opposed to raw code.  Declarative
.dialog files compound this, as the definition of your code moves to a declarative .json file

The Bot Framework Debugger works by allowing you to set breakpoints on instances of objects (either in source
or .dialog files) and inspect the execution of your logic as it flows through dialogs which make up your application.

## Setting up Visual Studio Code to use the debugger

To configure Visual Studio Code you need to add a target in your launch.settings file.

You can do that by adding a debug configuration.  There should be 2 configuration templates available:

* **Bot: Launch .NET Core Configuration** - Configuration for building and launching your but via dotnet and connecting to it
* **Bot: Attach Configuration** - Configuration for attaching to the debug port of an already running bot.

## Using

* Open any source file (*.cs or *.dialog) and set breakpoints.
* Hit F5 to start debugger.

As you interact with the bot your breakpoint should hit and you should be able to inspect memory, call context, etc.
