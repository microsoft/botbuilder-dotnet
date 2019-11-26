// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Bot.Streaming.Tests.Utilities
{
    public class AttachmentStream
    {
        public AttachmentStream(string contentType, Stream stream)
        {
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            ContentStream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        public string ContentType { get; }

        public Stream ContentStream { get; }
    }
}
