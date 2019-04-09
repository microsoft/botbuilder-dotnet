# Debugger for Microsoft Bot Framework .dialog files

Moving to a declarative model with .declarative files is great until you need to debug the model you have created.
This extension is the registration information for you to be able to attach to a process and set breakpoints in your .dialog files directly


## Setting up Visual Studio Code to use the debugger

To configure Visual Studio Code you need to add a target in your launch.settings file.

```json
{
    "type": "dialog",
    "request": "attach",
    "trace": true,
    "name": "Attach to bot",
    "debugServer": 4712
}
```

When your bot is running with the Debug Adapter active it has an open port 4712.  When you attach to the bot you are
attaching to this port. 

Simply open any .dialog files and set breakpoints.  As you interact with the bot your breakpoint should hit and you should be able to
inspect memory, call context, etc.

