// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Runtime.Providers
{
    public interface IProvider
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    }
}
