-- Microsoft.Bot.PublishValidation --

This is a tool for validating a bot configuration prior to publishing it. At this moment, these are the available validation checks:
- ForbidSpacesInProjectName: No spaces in the Project's name (this is due to a bug in .NET/Azure - https://github.com/aspnet/websdk/issues/237)
- RequireBotFile: Existence of a '.bot' file
- Internal structure of the configuration file:
    - RequireEndpoints: Required endpoints  
    - ForbidEndpoints: Forbidden endpoints
    - RequireLuisKey: Existence of LUIS key
    - RequireQnAMakerKey: Existence of QnA Maker key

By default, these validations are all set like this:

ForbidSpacesInProjectName   ->  True
RequireBotFile              ->  True
RequireEndpoints            ->  Production
ForbidEndpoints             ->  Development
RequireLuisKey              ->  True
RequireQnAMakerKey          ->  True


In order to change the behavior of this validations, you can create your own properties in your '.csproj' file by adding the following code:

------------------------------------------------------------------------------
<PropertyGroup>
    <ForbidSpacesInProjectName>True</ForbidSpacesInProjectName>
    <RequireBotFile>True</RequireBotFile>
    <RequireEndpoints>Production</RequireEndpoints>
    <ForbidEndpoints>Development</ForbidEndpoints>
    <RequireLuisKey>True</RequireLuisKey>
    <RequireQnAMakerKey>True</RequireQnAMakerKey>
</PropertyGroup>
------------------------------------------------------------------------------

