# Microsoft Bot Framework Publish Validation for .NET

This tool is used for validating a Bot's Configuration prior to publishing it, in order to prevent common issues like missing endpoints.

## Target Framework

* .NET Core 2.1

## How to Install
  
  ```
  PM> Install-Package Microsoft.Bot.PublishValidation
  ```
  
## How to use

There are several validation checks available at the moment. These are the following:
- **AllowSpacesInProjectName**: 
    - Possible values: 'true' or 'false'
    - Description: No spaces in the Project's name (this is due to a [bug in .NET/Azure](https://github.com/aspnet/websdk/issues/237))
- **AppSecret**:
    - Possible values: If necessary, environment variable name, following the format "$(ENV_VAR)".
    - Description: Environment variable with the secret key to decrypt the .bot file, in case it's necessary.
- **NotRequireBotFile**:
    - Possible values: 'true' or 'false'
    - Description: Existence of a '.bot' file
- **RequireEndpoints**: 
    - Possible values: List of comma separated values, inside double quotes
    - Description: Required endpoints inside the configuration file
- **ForbidEndpoints**: 
    - Possible values: List of comma separated values, inside double quotes
    - Description: Forbidden endpoints inside the configuration file
- **RequireLuisKey**: 
    - Possible values: 'true' or 'false'
    - Description: Existence of LUIS key inside the configuration file
- **RequireQnAMakerKey**: 
    - Possible values: 'true' or 'false'
    - Description: Existence of QnA Maker key inside the configuration file

By default, these validations are all set like this:

> AllowSpacesInProjectName: false
>
> AppSecret: [empty]
>
> NotRequireBotFile: false
>
> RequireEndpoints: production
>
> ForbidEndpoints: [empty]
>
> RequireLuisKey: false
>
> RequireQnAMakerKey: false

In order to change the behavior of this validations, you can create your own properties in your `.csproj` file by adding the following code:

```.csproj
<PropertyGroup>
    <AllowSpacesInProjectName>true</AllowSpacesInProjectName>
    <AppSecret>"$(ENV_VAR)"</AppSecret>
    <NotRequireBotFile>true</NotRequireBotFile>
    <RequireEndpoints>"production1,production2"</RequireEndpoints>
    <ForbidEndpoints>"development"</ForbidEndpoints>
    <RequireLuisKey>true</RequireLuisKey>
    <RequireQnAMakerKey>true</RequireQnAMakerKey>
</PropertyGroup>
```
> **Note:** You don't need to add all of them, only the ones you want to modify.

Also, there's an option to disable the validation entirely, and for doing so, all you have to do is add the following property to your `.csproj` file:

```.csproj
<PropertyGroup>
    <DisablePublishValidation>true</DisablePublishValidation>
</PropertyGroup>
```

## License

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](https://github.com/Microsoft/vscode/blob/master/LICENSE.txt) License.
