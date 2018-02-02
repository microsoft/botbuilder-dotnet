# Bot Builder SDK v4

This repository contains code for the .NET version of the [Microsoft Bot Builder SDK](https://github.com/Microsoft/BotBuilder).
The 4.x version of the SDK is being actively developed and should therefore be used for **EXPERIMENTATION PURPOSES ONLY**.
Production bots should continue to be developed using the [v3 SDK](https://github.com/Microsoft/BotBuilder/tree/master/CSharp).

In addition to the .NET SDK, Bot Builder supports creating bots in other popular programming languages:

- The [v4 JavaScript SDK](https://github.com/Microsoft/botbuilder-js) has a high degree of parity with the .NET SDK 
  and lets you build rich bots using C# for the Microsoft Bot Framework.
- The [Python Connector](https://github.com/Microsoft/botbuilder-python) provides basic connectivity to the Microsoft Bot Framework 
  and lets developers build bots using Python. **v4 SDK coming soon**.
- The [Java Connector](https://github.com/Microsoft/botbuilder-java) provides basic connectivity to the Microsoft Bot Framework 
  and lets developers build bots using Java. **v4 SDK coming soon**.

## Getting Started

The v4 SDK consists of a series of [libraries](/libraries) which can be installed.

<!--Include detailed instructions on how to install the libraries.-->

### Create a "Hello World" bot

Create a new **ASP.NET Core Web Application**:
- Target **.NET Core** **ASP.NET Core 2.0**.
- Use the **Empty** project template.
- Select **No Authentication**.
- Do not enable Docker support.

Add references to the following v4 preview libraries to your project:
```
Microsoft.Bot.Builder
Microsoft.Bot.Builder.BotFramework
Microsoft.Bot.Connector
```

Add the following NuGet packages to your project:
```
Microsoft.Bot.Connector.AspNetCore
Newtonsoft.Json
```

Edit your **Properties/launchSettings.json** file:
- Add a `profiles.IIS Express.launchUrl` property, and set it to "default.html".

To your wwwroot folder:
- Add a default.html file.
  This will be the page that IIS Express displays when you start your bot service.

Within the Startup.cs file:
- Update the using statements:

  ```csharp
  using Microsoft.AspNetCore.Authentication.JwtBearer;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Bot.Connector;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  ```

- Replace the implementation of the Startup class with the following definition:

  ```csharp
  public class Startup
  {
      public Startup(IHostingEnvironment env)
      {
          var builder = new ConfigurationBuilder()
              .SetBasePath(env.ContentRootPath)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
              .AddEnvironmentVariables();
          Configuration = builder.Build();
      }

      public IConfiguration Configuration { get; }

      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services)
      {
          services.AddSingleton(_ => Configuration);
          var credentialProvider = new StaticCredentialProvider(
              Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value,
              Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value);

          services.AddAuthentication(
                  // This can be removed after https://github.com/aspnet/IISIntegration/issues/371 
                  options =>
                  {
                      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                  }
              )
              .AddBotAuthentication(credentialProvider);

          services.AddSingleton(typeof(ICredentialProvider), credentialProvider);
          services.AddMvc(options =>
          {
              options.Filters.Add(typeof(TrustServiceUrlAttribute));
          });
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(
          IApplicationBuilder app, IHostingEnvironment env)
      {
          app.UseStaticFiles();
          app.UseAuthentication();
          app.UseMvc();
      }
  }
  ```

Add a **Controllers** folder and add a **MessagesController** class to the folder.
This is the class that defines your bot's behavior.
- Update the using statements:
  ```csharp
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Bot.Builder;
  using Microsoft.Bot.Builder.Adapters;
  using Microsoft.Bot.Connector;
  using Microsoft.Extensions.Configuration;
  using System.Threading.Tasks;
  ```
- Replace the definition of the class with the following code:
  ```csharp
  [Route("api/[controller]"), Produces("application/json")]
  public class MessagesController : Controller
  {
      Bot _bot;
      BotFrameworkAdapter _adapter;

      public MessagesController(IConfiguration configuration)
      {
          string appId = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value ?? string.Empty;
          string appKey = configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value ?? string.Empty;

          _adapter = new BotFrameworkAdapter(appId, appKey);

          // Create the bot using the adapter. 
          _bot = new Bot(_adapter)
              .OnReceive(async (context, _) =>
              {
                  if (context.Request.Type.Equals(ActivityTypes.Message))
                  {
                      context.Reply("Hello World");
                  }

                  await Task.Yield();
              });
      }

      [HttpPost, Authorize(Roles = "Bot")]
      public async void Post([FromBody]Activity activity)
      {
          await _adapter.Receive(HttpContext.Request.Headers, activity);
      }
  }
  ```

Now start your bot (with or without debugging).

To interact with your bot:
- Download the [Bot Framework Emulator](https://github.com/Microsoft/BotFramework-Emulator).
- The start the emulator, connect to your bot, and say "hello".
  The bot will respond with "Hello World" to every message.

<!--
## Building

_Instructions on how to build and test the libraries yourself._
-->

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Reporting Security Issues
Security issues and bugs should be reported privately, via email, to the Microsoft Security Response Center (MSRC) at [secure@microsoft.com](mailto:secure@microsoft.com). You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your original message. Further information, including the [MSRC PGP](https://technet.microsoft.com/en-us/security/dn606155) key, can be found in the [Security TechCenter](https://technet.microsoft.com/en-us/security/default).

## License

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](https://github.com/Microsoft/vscode/blob/master/LICENSE.txt) License.


## Current Build Status
| Project  | Status |
| --- | --- |
| Microsoft.Bot.Connector | ![Build Status](https://fuselabs.visualstudio.com/_apis/public/build/definitions/86659c66-c9df-418a-a371-7de7aed35064/212/badge) |
