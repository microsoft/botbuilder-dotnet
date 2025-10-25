// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Sample.Agentic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTokenAcquisition(true);

builder.Services.AddAgentIdentities();

builder.Services.AddInMemoryTokenCaches();

builder.Services.AddHttpClient();

CredentialDescription credSecret;

if (!string.IsNullOrEmpty(builder.Configuration["MicrosoftAppClientId"]))
{
    builder.Services.AddSingleton<ServiceClientCredentialsFactory>(provider =>
        new FederatedServiceClientCredentialsFactory(
            provider.GetRequiredService<IAuthorizationHeaderProvider>(),
            builder.Configuration["MicrosoftAppId"],
            builder.Configuration["MicrosoftAppClientId"],
            builder.Configuration["MicrosoftAppTenantId"]));

    credSecret = new CredentialDescription()
    {
        SourceType = CredentialSource.SignedAssertionFromManagedIdentity,
        ManagedIdentityClientId = builder.Configuration["MicrosoftAppClientId"]
    };
}
else
{
    credSecret = new CredentialDescription()
    {
        SourceType = CredentialSource.ClientSecret,
        ClientSecret = builder.Configuration["MicrosoftAppPassword"]
    };
}

builder.Services.Configure<MicrosoftIdentityApplicationOptions>(ops =>
{
    ops.Instance = "https://login.microsoftonline.com/";
    ops.TenantId = builder.Configuration["MicrosoftAppTenantId"];
    ops.ClientId = builder.Configuration["MicrosoftAppId"];
    ops.ClientCredentials = [credSecret];
});

builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(provider =>
    new CloudAdapter(
        provider.GetRequiredService<IAuthorizationHeaderProvider>(),
        provider.GetRequiredService<IConfiguration>(),
        provider.GetRequiredService<IHttpClientFactory>(),
        provider.GetRequiredService<ILogger<CloudAdapter>>()));

builder.Services.AddTransient<IBot, EchoBot>();

var app = builder.Build();

app.MapPost("/api/messages", async (IBotFrameworkHttpAdapter adapter, IBot bot, HttpRequest request, HttpResponse response) =>
    await adapter.ProcessAsync(request, response, bot));

app.Run();
