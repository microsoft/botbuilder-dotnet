// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Connector.Streaming.Application;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Streaming;
using Microsoft.Bot.Streaming.Payloads;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Abstractions;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Tests
{
    public class InvokeResponseBot : IBot
    {
        public Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var response = new Activity();

            // response.SetAsInvokeResponse(new { quite = new { honestly = "im.feeling.really.attacked.right.now" } });
            return turnContext.SendActivityAsync(response, cancellationToken);
        }
    }
}
