// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Runtime.Builders
{
    public interface IBuilder<out T>
    {
        T Build(IServiceProvider services, IConfiguration configuration);
    }
}
