-- Microsoft.Bot.PublishValidation --

This is a tool for validating a bot configuration prior to publishing it. At this moment, these are the available validation checks:
- AllowSpacesInProjectName: 
    - Possible values: 'true' or 'false'
    - Description: No spaces in the Project's name (this is due to a bug in .NET/Azure - https://github.com/aspnet/websdk/issues/237)
- NotRequireBotFile: 
    - Possible values: 'true' or 'false'
    - Description: Existence of a '.bot' file
- RequireEndpoints: 
    - Possible values: List of comma separated values, inside double quotes
    - Description: Required endpoints inside the configuration file
- ForbidEndpoints: 
    - Possible values: List of comma separated values, inside double quotes
    - Description: Forbidden endpoints inside the configuration file
- RequireLuisKey: 
    - Possible values: 'true' or 'false'
    - Description: Existence of LUIS key inside the configuration file
- RequireQnAMakerKey: 
    - Possible values: 'true' or 'false'
    - Description: Existence of QnA Maker key inside the configuration file

By default, these validations are all set like this:

AllowSpacesInProjectName    ->  false
NotRequireBotFile           ->  false
RequireEndpoints            ->  production
ForbidEndpoints             ->  [empty]
RequireLuisKey              ->  false
RequireQnAMakerKey          ->  false


In order to change the behavior of this validations, you can create your own properties in your '.csproj' file by adding the following code:

------------------------------------------------------------------------------
<PropertyGroup>
    <AllowSpacesInProjectName>false</AllowSpacesInProjectName>
    <NotRequireBotFile>false</NotRequireBotFile>
    <RequireEndpoints>"production1,production2"</RequireEndpoints>
    <ForbidEndpoints>"development"</ForbidEndpoints>
    <RequireLuisKey>true</RequireLuisKey>
    <RequireQnAMakerKey>true</RequireQnAMakerKey>
</PropertyGroup>
------------------------------------------------------------------------------

> Note: You don't need to add all of them, only the ones you want to modify.

Also, there's an option to disable the validation entirely, and for doing so, all you have to do is add the following property to your .csproj file:

------------------------------------------------------------------------------
<PropertyGroup>
    <DisablePublishValidation>true</DisablePublishValidation>
</PropertyGroup>
------------------------------------------------------------------------------
