// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Runtime.Builders
{
    public interface IBuilder<out TItem>
    {
        TItem Build(IServiceProvider services, IConfiguration configuration);
    }
}
