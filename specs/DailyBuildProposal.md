# Daily Build Propsal for .Net BotBuilder SDK 

This proposal describes our pland for how to publish daily builds for consumption. The goals of this are:
1. Make it easy for developers (1P and 3P) to consume our daily builds. 
2. Exercise our release to Nuget process frequently, so issues don't arise at critical times. 

Use the [ASP.Net Team](https://github.com/dotnet/aspnetcore/blob/master/docs/DailyBuilds.md) as inspiration, and draft off the work they do. 

# Summary - Daily Builds
*Daily Builds* go to Azure Devops feed. ASP.Net core uses the following instructions:
```json
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <clear />
        <add key="aspnetcore" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet5/nuget/v3/index.json" />
        <add key="NuGet.org" value="https://api.nuget.org/v3/index.json" />
    </packageSources>
</configuration>
```

All Nuget packages from our daily build would be pushed to the SDK_Public project at [fuselabs.visualstudio.com](https://fuselabs.visualstudio.com). 

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

## Daily Build Lifecyle
Daily builds older than 90 days are automatically deleted. 

# Summary - Weekly Builds
Once per week, preferably on a Monday, a daily build is pushed to Nuget.org. This serves 2 purposes:

1. Keeps Nuget "Fresh" for people that don't want daily builds.
2. Keeps the release pipelines active and working, and prevents issues. 

These builds will have the "-preview" tag, just like the daily builds. 

This pipeline should be the EXACT same pipeline that releases our production bits. 

## Adding packages to the feed
Our existing Release pipelines would add packages to the feed. From the Azure Artifacts docs, this would be:

```json
nuget.exe push -Source "sdk_public_nuget" -ApiKey az <packagePath>
```

# Versioning
All core packages maintain the basic semver scheme. For production packages, there are no changes. 




# Migration from MyGet
# 