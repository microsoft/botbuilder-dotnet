-- Microsoft.Bot.PublishValidation --

This is a tool for validating a bot configuration prior to publishing it. By default, the validation checks the following:
- Existence of a '.bot' file
- Internal structure of the configuration file:
    - No spaces in Project's name
    - Production endpoint existence
    - No Development endpoint exists

You can also set to check the following:
- Existence of LUIS key
- Existence of QnA Maker key

In order to change the behavior of this validations, you can create your own properties in your '.csproj' file by adding the following code:

------------------------------------------------------------------------------
<PropertyGroup>
    <RequireEndpoints>Production</RequireEndpoints>
    <ForbidEndpoints>Development</ForbidEndpoints>
    <ForbidSpacesInProjectName>True</ForbidSpacesInProjectName>
    <RequireBotFile>True</RequireBotFile>
    <RequireLuisKey>True</RequireLuisKey>
    <RequireQnAMakerKey>True</RequireQnAMakerKey>
</PropertyGroup>
------------------------------------------------------------------------------
