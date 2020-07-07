
#  How to update the schema file
Once the bot has been setup with Composer and we wish to make changes to the schema, the first step in this process is to eject the runtime through the `Runtime Config` in Composer. The ejected runtime folder will broadly have the following structure

```
bot
  /bot.dialog
  /language-generation
  /language-understanding
  /dialogs
     /customized-dialogs
  /runtime
     /azurewebapp
     /azurefunctions
  /schemas
     sdk.schema
```

### Setup bfdialog tool (Prerequisite)
The bfdialog is part of our suite of botframework tools and helps merge partial schemas into a single consolidated schema

#####  To point npm to nightly builds
```
npm config set registry https://botbuilder.myget.org/F/botframework-cli/npm/
```
#####  To install BF tool:
```
npm i -g @microsoft/botframework-cli
```

#####  To install bf dialog plugin
```
bf plugins:install @microsoft/bf-dialog
```

##  Adding Custom Actions to your Composer bot
**NOTE: These steps assume you are using azurewebapp as your deployment solution. Replicating it on azurefunctions would be similar
**
- In this tutorial, we will be going over the steps to include a custom action `MultiplyDialog` that multiplies two numbers passed as inputs. Note that the ejected runtime should contain a `customaction` folder that has this sample.

- Navigate to the csproj file inside the `runtime` folder (bot/runtime/azurewebapp/Microsoft.BotFramework.Composer.WebApp.csproj) and include a project reference to the customaction project like `<ProjectReference Include="..\customaction\Microsoft.BotFramework.Composer.CustomAction.csproj" />`.

- Then Uncomment line 28 and 139 in azurewebapp/Startup.cs file so as to register this action.
```
using Microsoft.BotFramework.Composer.CustomAction;
// This is for custom action component registration.
ComponentRegistration.Add(new CustomActionComponentRegistration());
```

- Run the command `dotnet build` on the azurewebapp project to verify if it passes build after adding custom actions to it.

- Navigate to to the `Schemas (bot/runtime/azurewebapp/Schemas)` folder and then run the command `sh update.sh`.

- Validate that the partial schema (MultiplyDialog.schema inside customaction/Schema) has been appended to the default sdk.schema file to generate one single consolidated sdk.schema file.

- Copy the newly generated sdk.schema into the `schemas (bot/schemas)` folder at the root of the ejected runtime.

The above steps should have generated a new sdk.schema file inside `schemas` folder for Composer to use. Reload the bot and you should be able to include your new custom action!
