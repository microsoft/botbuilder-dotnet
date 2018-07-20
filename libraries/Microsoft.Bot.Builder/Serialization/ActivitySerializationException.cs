// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.Serialization
{
    /// <summary>
    /// Thrown by <see cref="IActivitySerializer"/> instances when there is an issue during [de]serialization of an <see cref="Activity"/>.
    /// </summary>
    [Serializable]
    public class ActivitySerializationException : Exception
    {
        public ActivitySerializationException(string message)
            : base(message)
        {
        }

        public ActivitySerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ActivitySerializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
