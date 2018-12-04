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
- **ForbidSpacesInProjectName**: 
    - Possible values: 'True' or 'False'
    - Description: No spaces in the Project's name (this is due to a [bug in .NET/Azure](https://github.com/aspnet/websdk/issues/237))
- **RequireBotFile**:
    - Possible values: 'True' or 'False'
    - Description: Existence of a '.bot' file
- Internal structure of the configuration file:
- **RequireEndpoints**: 
    - Possible values: List of comma separated values
    - Description: Required endpoints inside the configuration file
- **ForbidEndpoints**: 
    - Possible values: List of comma separated values
    - Description: Forbidden endpoints inside the configuration file
- **RequireLuisKey**: 
    - Possible values: 'True' or 'False'
    - Description: Existence of LUIS key inside the configuration file
- **RequireQnAMakerKey**: 
    - Possible values: 'True' or 'False'
    - Description: Existence of QnA Maker key inside the configuration file

By default, these validations are all set like this:

**ForbidSpacesInProjectName**: True
**RequireBotFile**: True
**RequireEndpoints**: Production
**ForbidEndpoints**: Development
**RequireLuisKey**: True
**RequireQnAMakerKey**: True


In order to change the behavior of this validations, you can create your own properties in your `.csproj` file by adding the following code:

```.csproj
<PropertyGroup>
    <ForbidSpacesInProjectName>True</ForbidSpacesInProjectName>
    <RequireBotFile>True</RequireBotFile>
    <RequireEndpoints>Production</RequireEndpoints>
    <ForbidEndpoints>Development</ForbidEndpoints>
    <RequireLuisKey>True</RequireLuisKey>
    <RequireQnAMakerKey>True</RequireQnAMakerKey>
</PropertyGroup>
```
> **Note:** You don't need to add all of them, only the ones you want to modify.

Also, there's an option to disable the validation entirely, and for doing so, all you have to do is add the following property to your `.csproj` file:

```.csproj
<PropertyGroup>
    <DisablePublishValidation>True</DisablePublishValidation>
</PropertyGroup>
```

## License

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](https://github.com/Microsoft/vscode/blob/master/LICENSE.txt) License.
