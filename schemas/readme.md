# Component meta-schema for Bot Framework Declarative Files
This directory contains two externally visible schema files:
* __component.schema__: The meta-schema for describing the data required by a new component for .dialog files.  
* __sdk.schema__: The .dialog file used for validating all standard SDK declartive components.

In order to work with .schema files you should ensure install the [dialogSchema](https://github.com/microsoft/botbuilder-tools/tree/V.Future/packages/DialogSchema) tool.

To update these files in preperation for a new SDK release in branch `branch` you should run `update <branch>`.  This will:
1. Switch to this directory.
1. Verify you want to do this.
1. `git checkout -b <branch>`
1. `git branch --set-upstream-to`
1. `dialogSchema ../libraries/**/*.schema -u -b <branch> -o sdk.schema`
1. `git commit -a -m "Update .schema files"`
1. If you confirm will do `git push`

This will update all of the .schema files and make them accessible through urls like:
 * __component.schema__: `https://raw.githubusercontent.com/Microsoft/botbuilder-dotnet/{branch}/schemas/component.schema` 
 * __sdk.shema__: `https://raw.githubusercontent.com/Microsoft/botbuilder-dotnet/{branch}/schemas/sdk.schema`

