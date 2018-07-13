// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.Serialization
{
    [Serializable]
    public class ActivitySerializationException : Exception
    {
        public ActivitySerializationException(string message) : base(message)
        {
        }

        public ActivitySerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ActivitySerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
