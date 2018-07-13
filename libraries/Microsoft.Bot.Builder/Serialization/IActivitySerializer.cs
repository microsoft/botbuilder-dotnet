// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Serialization
{
    public interface IActivitySerializer
    {
        Task SerializeAsync(Activity activity, Stream stream, CancellationToken cancellationToken = default(CancellationToken));

        Task<Activity> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken));
    }

}
