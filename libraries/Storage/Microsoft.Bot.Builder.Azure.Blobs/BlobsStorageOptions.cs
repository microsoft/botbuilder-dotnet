// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Azure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Bot.Builder.Azure.Blobs
{
    /// <summary>
    /// Blobs Storage options.
    /// </summary>
    public class BlobsStorageOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobsStorageOptions"/> class.
        /// </summary>
        /// <param name="dataConnectionString">Azure Storage connection string.</param>
        /// <param name="containerName">Name of the Blob container where entities will be stored.</param>
        /// <param name="jsonSerializer">If passing in a custom JsonSerializer, we recommend the following settings:
        /// <para>jsonSerializer.TypeNameHandling = TypeNameHandling.None.</para>
        /// <para>jsonSerializer.NullValueHandling = NullValueHandling.Include.</para>
        /// <para>jsonSerializer.ContractResolver = new DefaultContractResolver().</para>
        /// </param>
        public BlobsStorageOptions(string dataConnectionString, string containerName, JsonSerializer jsonSerializer = null)
            : this(dataConnectionString, containerName, default, jsonSerializer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobsStorageOptions"/> class.
        /// </summary>
        /// <param name="dataConnectionString">Azure Storage connection string.</param>
        /// <param name="containerName">Name of the Blob container where entities will be stored.</param>
        /// /// <param name="storageTransferOptions">Used for providing options for parallel transfers <see cref="StorageTransferOptions"/>.</param>
        /// <param name="jsonSerializer">If passing in a custom JsonSerializer, we recommend the following settings:
        /// <para>jsonSerializer.TypeNameHandling = TypeNameHandling.None.</para>
        /// <para>jsonSerializer.NullValueHandling = NullValueHandling.Include.</para>
        /// <para>jsonSerializer.ContractResolver = new DefaultContractResolver().</para>
        /// </param>
        public BlobsStorageOptions(string dataConnectionString, string containerName, StorageTransferOptions storageTransferOptions, JsonSerializer jsonSerializer = null)
        {
            if (string.IsNullOrEmpty(dataConnectionString))
            {
                throw new ArgumentNullException(nameof(dataConnectionString));
            }

            DataConnectionString = dataConnectionString;

            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentNullException(nameof(containerName));
            }

            ContainerName = containerName;

            StorageTransferOptions = storageTransferOptions;

            JsonSerializer = jsonSerializer ?? JsonSerializer.Create(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Include,
                ContractResolver = new DefaultContractResolver()
            });
        }

        /// <summary>
        /// Gets the connection string to use for <see cref="BlobsStorage"/>.
        /// </summary>
        /// <value>
        /// The connection string to use for <see cref="BlobsStorage"/>.
        /// </value>
        public string DataConnectionString { get; private set; }

        /// <summary>
        /// Gets the container name to use for <see cref="BlobsStorage"/>.
        /// </summary>
        /// <value>
        /// The container name to use for <see cref="BlobsStorage"/>.
        /// </value>
        public string ContainerName { get; private set; }

        /// <summary>
        /// Gets the <see cref="JsonSerializer"/> to use for <see cref="BlobsStorage"/>.
        /// </summary>
        /// <value>
        /// The <see cref="JsonSerializer"/> to use for <see cref="BlobsStorage"/>.
        /// </value>
        public JsonSerializer JsonSerializer { get; private set; }

        /// <summary>
        /// Gets the <see cref="StorageTransferOptions"/> to use for <see cref="BlobsStorage"/>.
        /// </summary>
        /// <value>
        /// The StorageTransferOptions to use for <see cref="BlobsStorage"/>.
        /// </value>
        public StorageTransferOptions StorageTransferOptions { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to enforce ETag condition consistency while persisting existing objects.
        /// </summary>
        /// <value>
        /// True if Etag consistency should be enforced, or ignored.
        /// </value>
        public bool EnforceEtag { get; set; } = true;
    }
}
