// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers
{
    public interface IRefResolver
    {
        bool IsRef(JToken token);

        Task<JToken> ResolveAsync(JToken refToken);
    }
}
