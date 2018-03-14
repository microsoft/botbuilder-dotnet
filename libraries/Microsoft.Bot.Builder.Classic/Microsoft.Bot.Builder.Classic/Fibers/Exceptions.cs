// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.Classic.Internals.Fibers
{
    [Serializable]
    public abstract class InvalidWaitException : InvalidOperationException
    {
        private readonly IWait wait;

        public IWait Wait { get { return this.wait; } }

        protected InvalidWaitException(string message, IWait wait)
            : base(message)
        {
            SetField.NotNull(out this.wait, nameof(wait), wait);
        }
        protected InvalidWaitException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            SetField.NotNullFrom(out this.wait, nameof(this.wait), info);
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.wait), wait);
        }
    }

    [Serializable]
    public sealed class InvalidNeedException : InvalidWaitException
    {
        private readonly Need need;
        private readonly Need have;
        public Need Need { get { return this.need; } }
        public Need Have { get { return this.have; } }

        public InvalidNeedException(IWait wait, Need need)
            : base($"invalid need: expected {need}, have {wait.Need}", wait)
        {
            this.need = need;
            this.have = wait.Need;
        }
        private InvalidNeedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.need = (Need)info.GetValue(nameof(this.need), typeof(Need));
            this.have = (Need)info.GetValue(nameof(this.have), typeof(Need));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.need), this.need);
            info.AddValue(nameof(this.have), this.have);
        }
    }

    [Serializable]
    public sealed class InvalidTypeException : InvalidWaitException
    {
        private readonly Type type;

        public InvalidTypeException(IWait wait, Type type)
            : base($"invalid type: expected {wait.ItemType}, have {type.Name}", wait)
        {
            SetField.NotNull(out this.type, nameof(type), type);
        }
        private InvalidTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            SetField.NotNullFrom(out this.type, nameof(this.type), info);
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.type), type);
        }
    }

    [Serializable]
    public sealed class InvalidNextException : InvalidWaitException
    {
        public InvalidNextException(IWait wait)
            : base($"invalid next: {wait}", wait)
        {
        }
        private InvalidNextException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public sealed class ClosureCaptureException : SerializationException
    {
        [NonSerialized]
        public readonly object Instance;
        public ClosureCaptureException(object instance)
            : base($"anonymous method closures that capture the environment are not serializable, consider removing environment capture or using a reflection serialization surrogate: {instance}")
        {
            this.Instance = instance;
        }
        private ClosureCaptureException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
