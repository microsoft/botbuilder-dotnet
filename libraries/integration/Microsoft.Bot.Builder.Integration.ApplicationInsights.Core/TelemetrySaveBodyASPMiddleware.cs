// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Integration.ApplicationInsights.Core
{
    /// <summary>
    /// Middleware to store the incoming activity request body into the HttpContext items collection.
    /// This class has been deprecated in favor of using TelemetryInitializerMiddleware in
    /// Microsoft.Bot.Integration.ApplicationInsights.Core and Microsoft.Bot.Integration.ApplicationInsights.WebApi.
    /// </summary>
    [Obsolete("This class is deprecated. Please add TelemetryInitializerMiddleware to your adapter's middleware pipeline instead.")]
    [SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "This class is deprecated, we won't fix FxCop issues")]
    [SuppressMessage("AsyncUsage.CSharp.Naming", "UseAsyncSuffix:Use Async suffix", Justification = "This class is deprecated, we won't fix FxCop issues")]
    public class TelemetrySaveBodyASPMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetrySaveBodyASPMiddleware"/> class.
        /// </summary>
        /// <param name="next">The delegate to the next piece of middleware in the pipeline.</param>
        public TelemetrySaveBodyASPMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        /// <summary>
        /// Invokes the TelemetrySaveBodyASPMiddleware middleware.
        /// </summary>
        /// <param name="httpContext">The HttpContext.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods (can't change this without breaking binary compat)
        public async Task Invoke(HttpContext httpContext)
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            var request = httpContext.Request;

            if (request.Method == "POST"
                && !string.IsNullOrEmpty(request.ContentType)
                && request.ContentType.StartsWith("application/json"))
            {
                var items = httpContext.Items;
                request.EnableBuffering();
                try
                {
                    using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 4096, true))
                    {
                        var body = await reader.ReadToEndAsync().ConfigureAwait(false);
                        var jsonObject = JObject.Parse(body);

                        // Save data in cache.
                        items.Add(TelemetryBotIdInitializer.BotActivityKey, jsonObject);
                    }
                }
                catch (JsonReaderException)
                {
                    // Request not json.
                }
                finally
                {
                    // rewind for next middleware.
                    request.Body.Position = 0;
                }
            }

            await _next(httpContext).ConfigureAwait(false);
        }
    }
}
