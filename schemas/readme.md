# Component meta-schema for Bot Framework Declarative Files
This directory contains two externally visible schema files:
* __component.schema__: The meta-schema for describing the data required by a new component for .dialog files.  
* __sdk.schema__: The .dialog file used for validating all standard SDK declarative components.

In order to work with .schema files you should ensure install the latest version of the [dialogSchema](https://github.com/microsoft/botbuilder-tools/tree/V.Future/packages/DialogSchema) tool.

There are two tools here:
* `update [branch]` is used to update .schema files when developing.  It will udpate .schema files so that when the current branch is merged into __branch__ they will point to __branch__.  By default it points to 4.Future.
* `updateBranch branch` is used to update and push .schema files directly into __branch__.  This is typically done when releasing a new Bot Builder SDK version.

This will update all of the .schema files and make them accessible through urls like:
 * __component.schema__: `https://raw.githubusercontent.com/Microsoft/botbuilder-dotnet/{branch}/schemas/component.schema` 
 * __sdk.shema__: `https://raw.githubusercontent.com/Microsoft/botbuilder-dotnet/{branch}/schemas/sdk.schema`

  