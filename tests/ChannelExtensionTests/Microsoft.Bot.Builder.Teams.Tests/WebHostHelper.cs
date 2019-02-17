using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams.Middlewares;
using Microsoft.Bot.Builder.Teams.StateStorage;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Teams.Tests
{
    internal class WebHostHelper
    {
        /// <summary>
        /// A test JWT token.
        /// </summary>
        private const string TestToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IktwSVdSVWxnZmlObGQxRFR4WkFoZTRpTm1rQSIsInR5cCI6IkpXVCIsIng1dCI6IktwSVdSVWxnZmlObGQxRFR4WkFoZTRpTm1rQSJ9.eyJzZXJ2aWNldXJsIjoiaHR0cHM6Ly9jYW5hcnkuYm90YXBpLnNreXBlLmNvbS9hbWVyLyIsIm5iZiI6MTU1MDM2Nzg1MCwiZXhwIjoxNTUwMzcxNDUwLCJpc3MiOiJodHRwczovL2FwaS5ib3RmcmFtZXdvcmsuY29tIiwiYXVkIjoiMzZjMzMxYTAtNTgwMC00MDNjLWJmZmYtYzlhM2JlYzJhM2M1In0.Mrgm_uH5HsC7cJoiW1KvcewVvKPtKBn1uvWrzGEKqtKMPagj492V5hLhuzUR4Wxn89eAUHiUJFB_PneKSNHCMJ4AA_eSTv174pzqM-iLLlzVZgEPcEMe9gBdMRHNPsmry_I5KpsPiOsDUnvy1r99NkFFb1OBULM1f_oCZck8-CNOCxxOHYi14jbqQqAmqwcVCV3MRX6hBlDb5y43MU9l0u9S9ExSAUJz6DhPv87p88QC3-73X-MpNwzT0pXc48gro581M469wwQ9tdM5LBcDVAJguZs0RCHVw2tJPsP6A0aBbtQXYtC4Zv-0zPte9oXHMAnq8M04wqMjAHNbPcR1EQ";

        /// <summary>
        /// Test App Id.
        /// </summary>
        private const string TestAppId = "36c331a0-5800-403c-bfff-c9a3bec2a3c5";

        /// <summary>
        /// The next port number to use. Starting with a random one and then increasing by 1 everytime.
        /// </summary>
        private static int nextPortNumberToUse = new Random(Process.GetCurrentProcess().Id).Next(8567, 13000);

        /// <summary>
        /// Object used to synchronize server startups.
        /// </summary>
        private static object lockingObject = new object();

        /// <summary>
        /// Port number this server instance is using.
        /// </summary>
        private int portInUseByInstance;

        /// <summary>
        /// Currently active webhost.
        /// </summary>
        private IWebHost webHost;

        private WebHostHelper()
        {
        }

        public static WebHostHelper GetWebHostHelper(
            Func<ITurnContext, CancellationToken, Task> botFunction,
            Func<HttpRequestMessage, HttpResponseMessage> delegatingHandlerFunction,
            Action<BotFrameworkOptions> configureBotFrameworkOptionsAction = null,
            Action<IServiceCollection> configureServices = null,
            Action<IApplicationBuilder> configure = null)
        {
            WebHostHelper webHostHelper = new WebHostHelper();

            // Trying to ensure the token above is never marked as expired.
            ChannelValidation.ToBotFromChannelTokenValidationParameters.ClockSkew = TimeSpan.FromDays(365000);

            lock (lockingObject)
            {
                webHostHelper.portInUseByInstance = nextPortNumberToUse;
                nextPortNumberToUse++;
            }

            webHostHelper.webHost = WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .UseKestrel((serverOptions) =>
                {
                    serverOptions.Listen(IPAddress.IPv6Loopback, webHostHelper.portInUseByInstance);
                })
                .ConfigureServices(services =>
                {
                    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

                    Func<ITurnContext, CancellationToken, Task> requesExecutionFunction = (turnContext, cancellationToken) =>
                    {
                        // Setting the Service url expiry time to past to ensure we never try to get auth tokens.
                        MicrosoftAppCredentials.TrustServiceUrl(turnContext.Activity.ServiceUrl, DateTime.UtcNow.Subtract(TimeSpan.FromDays(10)));
                        return botFunction.Invoke(turnContext, cancellationToken);
                    };

                    services.AddSingleton(requesExecutionFunction);

                    services.AddBot<TestBot>(options =>
                    {
                        IStorage dataStore = new MemoryStorage();

                        configureBotFrameworkOptionsAction?.Invoke(options);

                        // Drop all activities not coming from Microsoft Teams.
                        options.Middleware.Add(new DropNonTeamsActivitiesMiddleware());

                        // --> Add Teams Middleware.
                        options.Middleware.Add(
                            new TeamsMiddleware(
                                new SimpleCredentialProvider(TestAppId, "AppPassword"),
                                null,
                                new TestDelegatingHandler(delegatingHandlerFunction)));

                        options.CredentialProvider = new SimpleCredentialProvider(TestAppId, "AppPassword");

                        options.HttpClient = new HttpClient(new TestDelegatingHandler(delegatingHandlerFunction));
                    });

                    configureServices?.Invoke(services);
                })
                .Configure((app) =>
                {
                    app.UseDeveloperExceptionPage();
                    app.UseBotFramework();

                    configure?.Invoke(app);
                })
                .Build();

            webHostHelper.webHost.Start();

            return webHostHelper;
        }

        public async Task<HttpResponseMessage> SendRequestAsync(string payload)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json"),
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"http://localhost:{this.portInUseByInstance}/api/messages"),
                };

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", WebHostHelper.TestToken);

                return await httpClient.SendAsync(requestMessage);
            }
        }


        private class Startup
        {

        }

        private class TestBot : IBot
        {
            private readonly Func<ITurnContext, CancellationToken, Task> botFunction;

            public TestBot(Func<ITurnContext, CancellationToken, Task> botFunction)
            {
                this.botFunction = botFunction ?? throw new ArgumentNullException(nameof(botFunction));
            }

            public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
            {
                await this.botFunction(turnContext, cancellationToken);
            }
        }

        private class TestDelegatingHandler : DelegatingHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> delegatingHandlerFunction;

            public TestDelegatingHandler(
                Func<HttpRequestMessage, HttpResponseMessage> delegatingHandlerFunction)
            {
                this.delegatingHandlerFunction = delegatingHandlerFunction ?? throw new ArgumentNullException(nameof(delegatingHandlerFunction));
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Bypassing auth by always returning the same metadata document.
                // This ensures that the token we have never technically expires.
                if (request.RequestUri.AbsoluteUri.Contains("/.well-known/openidconfiguration"))
                {
                    return Task.FromResult(new HttpResponseMessage
                    {
                        Content = new StringContent(File.ReadAllText("MetadataDocument.json"), Encoding.UTF8, "application/json"),
                        StatusCode = HttpStatusCode.OK,
                    });
                }

                if (request.RequestUri.AbsoluteUri.Contains(".well-known/keys"))
                {
                    return Task.FromResult(new HttpResponseMessage
                    {
                        Content = new StringContent(File.ReadAllText("KeysDocument.json"), Encoding.UTF8, "application/json"),
                        StatusCode = HttpStatusCode.OK,
                    });
                }

                return Task.FromResult(this.delegatingHandlerFunction(request));
            }
        }
    }
}
