// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Microsoft.BotBuilderSamples
{
    public class ProxyPlayService : IHostedService
    {
        private Uri _proxyServer;
        private Task _executingTask;
        private CancellationTokenSource _cts;
        private IApplicationLifetime _appLifetime;

        public ProxyPlayService(IApplicationLifetime appLifeTime)
        {
            this._executingTask = null;
            this._cts = null;
            this._appLifetime = appLifeTime;
            var proxyServer = Environment.GetEnvironmentVariable("PROXY_HOST") ?? "http://localhost:3979";
            this._proxyServer = new Uri(new Uri(proxyServer), "/api/runtests");

        }

        protected async Task ExecuteSingleAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            // Run single test
            using (var client = new HttpClient())
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders
                      .Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var response = await client.GetAsync(this._proxyServer, HttpCompletionOption.ResponseContentRead))
                {
                    using (var content = response.Content)
                    {
                        var result = await content.ReadAsStringAsync();
                        if (result != null && result.Length > 0)
                        {
                            // Validate test result.
                            if (result.Equals("{\"id\":\"1\"}"))
                            {
                                // Completed successfully.
                                this._appLifetime.StopApplication();
                            }
                        }
                    }
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a linked token so we can trigger cancellation outside of this token's cancellation
            this._cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Store the task we're executing
            this._executingTask = this.ExecuteSingleAsync(this._cts.Token);

            // If the task is completed then return it, otherwise it's running
            return this._executingTask.IsCompleted ? this._executingTask : Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (this._executingTask == null)
            {
                return;
            }

            // Signal cancellation to the executing method
            _cts.Cancel();

            // Wait until the task completes or the stop token triggers
            await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

            // Throw if cancellation triggered
            cancellationToken.ThrowIfCancellationRequested();
        }

    }
}
