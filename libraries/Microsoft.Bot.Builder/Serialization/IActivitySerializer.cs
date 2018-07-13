// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Serialization
{
    /// <summary>
    /// Represents an implementation of a serializer that can both serialize and deserialize <see cref="Activity"/> instances 
    /// to/from provided <see cref="Stream">Streams</see>.
    /// </summary>
    public interface IActivitySerializer
    {
        /// <summary>
        /// Serializes the given <paramref name="activity" /> to the given <paramref name="stream">Stream</paramref>.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/> to be serialized.</param>
        /// <param name="stream">The <see cref="Stream"/> that the serialized output should be written to.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation (if possible).</param>
        /// <returns>A <see cref="Task"/> that represents the serialization process.</returns>
        Task SerializeAsync(Activity activity, Stream stream, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Deserializes an <see cref="Activity"/> from the given <paramref name="stream">Stream</paramref>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> that contains a serialized representation of an <see cref="Activity"/> that is to be deserialized.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation (if possible).</param>
        /// <returns>A <see cref="Task{Activity}"/> that represents the deserialization process whose result will be the <see cref="Activity"/> that was deserialized from the <paramref name="stream"/>.</returns>
        /// <exception cref="ActivitySerializationException">Thrown when the serializer is unable to deserialize an <see cref="Activity"/> from the given <paramref name="stream">Stream</paramref>.</exception>
        Task<Activity> DeserializeAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken));
    }

}
