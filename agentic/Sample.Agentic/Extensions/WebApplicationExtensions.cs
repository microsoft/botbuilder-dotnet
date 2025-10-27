// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Sample.Agentic.Extensions;

/// <summary>
/// Extension methods for configuring Bot Framework in WebApplication.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures Bot Framework services and maps the bot messages endpoint.
    /// </summary>
    /// <typeparam name="TBot">The bot implementation type.</typeparam>
    /// <param name="builder">The builder to configure.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>The built and configured web application.</returns>
    public static WebApplication UseBotFramework<TBot>(
        this WebApplicationBuilder builder,
        string pattern = "/api/messages")
        where TBot : class, IBot
    {
        // Configure services
        builder.Services.AddBotFrameworkAuthFromConfiguration(builder.Configuration);
        builder.Services.AddTransient<IBot, TBot>();

        // Build the application
        WebApplication app = builder.Build();

        // Map bot endpoint
        app.MapPost(pattern, async (
            IBotFrameworkHttpAdapter adapter,
            IBot bot,
            HttpRequest request,
            HttpResponse response) =>
                await adapter.ProcessAsync(request, response, bot));

        return app;
    }
}
