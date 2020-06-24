# Daily Build Propsal for .Net BotBuilder SDK 

This proposal describes our plan to publish daily builds for consumption. The goals of this are:
1. Make it easy for developers (1P and 3P) to consume our daily builds. 
2. Exercise our release to Nuget process frequently, so issues don't arise at critical times. 
3. Meet Developers where they are.

Use the [ASP.Net Team](https://github.com/dotnet/aspnetcore/blob/master/docs/DailyBuilds.md) as inspiration, and draft off the work they do. 

# Versioning
Move to semver2 and use "." rather than "-" to follow semver2 sorting rules. This [file in ASp.Net Core](https://github.com/dotnet/aspnetcore/blob/3787d7e7f070543cc9368d589a504fa8c4bd4830/eng/Versions.props) can be looked at to learn more. 

The tags we use for preview versions are:
```
-daily.<yyyymmdd>.{incrementing value}
-rc.{incrementing value}
```

Note: We are avoding the "-preview" tag for daily builds as we occasionally release "preview" assemblies that have a final semver moniker of "4.10.0-preview". 

# Daily Builds
Copying what the ASP.Net team does, all our Nuget packages would be pushed to the SDK_Public project at [fuselabs.visualstudio.com](https://fuselabs.visualstudio.com). 

    Note: Only a public project on Devops can have a public feed. The public project on our Enterprise Tenant is [SDK_Public](https://fuselabs.visualstudio.com/SDK_Public). 

This means developers could add the package source in Visual Studio or in their .csproj files. 

```json
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="sdk_public_nuget" value="https://fuselabs.pkgs.visualstudio.com/SDK_Public/_packaging/sdk_public_nuget/nuget/v3/index.json" />
   <add key="NuGet.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```
To connect from Visual Studio is equally simple. [Docs are here](https://fuselabs.visualstudio.com/SDK_Public/_packaging?_a=connect&feed=sdk_public_nuget).

## Debugging
To debug daily builds using Visual Studio
* Enable Source Link support in Visual Studio should be enabled.
* Enable source server support in Visual should be enabled.
* Enable Just My Code should be disabled
* Under Symbols enable the Microsoft Symbol Servers setting.

## Daily Build Lifecyle
Daily builds older than 90 days are automatically deleted. 

# Summary - Weekly Builds
Once per week, preferably on a Monday, a daily build is pushed to Nuget.org. This build happens from master, the same as a standard daily build. This serves 2 purposes:

1. Keeps Nuget "Fresh" for people that don't want daily builds.
2. Keeps the release pipelines active and working, and prevents issues. 

These builds will have the "-preview" tag and ARE the the daily build. 

**This release pipeline should be the EXACT same pipeline that releases our production bits.**

Weekly builds older than 1 year should be automatically delisted. 

## Adding packages to the feed
Our existing Release pipelines would add packages to the feed. From the Azure Artifacts docs, this would be:

```json
nuget.exe push -Source "sdk_public_nuget" -ApiKey az <packagePath>
```

# Migration from MyGet

1. Initially, our daily builds should go to both MyGet and Azure Devops. 
2. Our docs are updated once builds are in both locations. 
3. Towards the end of 2020, we stop publising to MyGet.

# Containers
ASP.Net and .Net Core 5 also publish a container to [Docker Hub](https://hub.docker.com/_/microsoft-dotnet-nightly-aspnet/) as part of their daily feed. We should consider that, along with our samples, in the next iteration of this work. 
