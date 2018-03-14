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

using Microsoft.Bot.Builder.Classic.Scorables.Internals;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.Classic.Internals.Fibers
{
    public static class Serialization
    {
        /// <summary>
        /// Extend <see cref="ISerializationSurrogate"/> with a "tester" method used by <see cref="SurrogateSelector"/>.
        /// </summary>
        public interface ISurrogateProvider : ISerializationSurrogate
        {
            /// <summary>
            /// Determine whether this surrogate provider handles this type.
            /// </summary>
            /// <param name="type">The query type.</param>
            /// <param name="context">The serialization context.</param>
            /// <param name="priority">The priority of this provider.</param>
            /// <returns>True if this provider handles this type, false otherwise.</returns>
            bool Handles(Type type, StreamingContext context, out int priority);
        }

        public sealed class StoreInstanceByTypeSurrogate : ISurrogateProvider
        {
            [Serializable]
            public sealed class ObjectReference : IObjectReference
            {
                public readonly Type Type = null;
                object IObjectReference.GetRealObject(StreamingContext context)
                {
                    var resolver = (IResolver)context.Context;
                    var real = resolver.Resolve(this.Type, tag: null);
                    return real;
                }
            }

            private readonly int priority;

            public StoreInstanceByTypeSurrogate(int priority)
            {
                this.priority = priority;
            }

            bool ISurrogateProvider.Handles(Type type, StreamingContext context, out int priority)
            {
                var resolver = (IResolver)context.Context;
                var handles = resolver.CanResolve(type, tag: null);
                priority = handles ? this.priority : 0;
                return handles;
            }

            void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var type = obj.GetType();
                info.SetType(typeof(ObjectReference));
                info.AddValue(nameof(ObjectReference.Type), type);
            }

            object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class StoreInstanceByFieldsSurrogate : ISurrogateProvider
        {
            public const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            private readonly int priority;

            public StoreInstanceByFieldsSurrogate(int priority)
            {
                this.priority = priority;
            }

            bool ISurrogateProvider.Handles(Type type, StreamingContext context, out int priority)
            {
                bool handles = !type.IsSerializable;
                priority = handles ? this.priority : 0;
                return handles;
            }

            void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var type = obj.GetType();
                var fields = type.GetFields(Flags);
                foreach (var field in fields)
                {
                    var value = field.GetValue(obj);
                    info.AddValue(field.Name, value);
                }
            }

            object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                var type = obj.GetType();
                var fields = type.GetFields(Flags);
                foreach (var field in fields)
                {
                    var value = info.GetValue(field.Name, field.FieldType);
                    field.SetValue(obj, value);
                }

                return obj;
            }
        }

        public sealed class ClosureCaptureErrorSurrogate : ISurrogateProvider
        {
            private readonly int priority;
            public ClosureCaptureErrorSurrogate(int priority)
            {
                this.priority = priority;
            }

            bool ISurrogateProvider.Handles(Type type, StreamingContext context, out int priority)
            {
                bool generated = Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute));
                bool handles = generated && !type.IsSerializable;
                priority = handles ? this.priority : 0;
                return handles;
            }

            void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                throw new ClosureCaptureException(obj);
            }

            object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class SurrogateLogDecorator : ISurrogateProvider
        {
            private readonly HashSet<Type> visited = new HashSet<Type>();
            private readonly ISurrogateProvider inner;
            // TOOD: better tracing interface
            private readonly TraceListener trace;

            public SurrogateLogDecorator(ISurrogateProvider inner, TraceListener trace)
            {
                SetField.NotNull(out this.inner, nameof(inner), inner);
                SetField.NotNull(out this.trace, nameof(trace), trace);
            }
            public override string ToString()
            {
                return $"{this.GetType().Name}({this.inner})";
            }
            bool ISurrogateProvider.Handles(Type type, StreamingContext context, out int priority)
            {
                return this.inner.Handles(type, context, out priority);
            }

            void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                this.Visit(obj);
                this.inner.GetObjectData(obj, info, context);
            }

            object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                this.Visit(obj);
                return this.inner.SetObjectData(obj, info, context, selector);
            }

            private void Visit(object obj)
            {
                var type = obj.GetType();
                bool added;
                lock (this.visited)
                {
                    added = this.visited.Add(type);
                }
                if (added)
                {
                    var message = $"{this.inner.GetType().Name}: visiting {type}";
                    this.trace.WriteLine(message);
                }
            }
        }

        public sealed class JObjectSurrogate : ISurrogateProvider
        {
            private readonly int priority;

            public JObjectSurrogate(int priority)
            {
                this.priority = priority;
            }

            bool ISurrogateProvider.Handles(Type type, StreamingContext context, out int priority)
            {
                bool handles = type == typeof(JObject);
                priority = handles ? this.priority : 0;
                return handles;
            }

            void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var instance = (JObject)obj;
                info.AddValue(typeof(JObject).Name, instance.ToString(Newtonsoft.Json.Formatting.None));
            }

            object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                return obj = JObject.Parse((string)info.GetValue(typeof(JObject).Name, typeof(string)));
            }
        }

        public sealed class SurrogateSelector : ISurrogateSelector
        {
            private readonly IReadOnlyList<ISurrogateProvider> providers;
            public SurrogateSelector(IReadOnlyList<ISurrogateProvider> providers)
            {
                SetField.NotNull(out this.providers, nameof(providers), providers);
            }

            void ISurrogateSelector.ChainSelector(ISurrogateSelector selector)
            {
                throw new NotImplementedException();
            }

            ISurrogateSelector ISurrogateSelector.GetNextSelector()
            {
                throw new NotImplementedException();
            }

            ISerializationSurrogate ISurrogateSelector.GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
            {
                int maximumPriority = -int.MaxValue;
                ISurrogateProvider maximumProvider = null;
                for (int index = 0; index < this.providers.Count; ++index)
                {
                    var provider = this.providers[index];
                    int priority;
                    if (provider.Handles(type, context, out priority) && priority > maximumPriority)
                    {
                        maximumPriority = priority;
                        maximumProvider = provider;
                    }
                }

                if (maximumProvider != null)
                {
                    selector = this;
                    return maximumProvider;
                }
                else
                {
                    selector = null;
                    return null;
                }
            }
        }
    }
}
