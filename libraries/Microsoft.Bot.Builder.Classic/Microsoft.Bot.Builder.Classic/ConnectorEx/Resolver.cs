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
using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    /// <summary>
    /// A resolver to recover C# type information from Activity schema types.
    /// </summary>
    public sealed class ActivityResolver : DelegatingResolver
    {
        public ActivityResolver(IResolver inner)
            : base(inner)
        {
        }

        public static readonly IReadOnlyDictionary<string, Type> TypeByName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { ActivityTypes.ContactRelationUpdate, typeof(IContactRelationUpdateActivity) },
            { ActivityTypes.ConversationUpdate, typeof(IConversationUpdateActivity) },
            { ActivityTypes.DeleteUserData, typeof(IActivity) },
            { ActivityTypes.Message, typeof(IMessageActivity) },
            { ActivityTypes.Ping, typeof(IActivity) },
            { ActivityTypes.Event, typeof(IEventActivity) },
            { ActivityTypes.Invoke, typeof(IInvokeActivity) },
            { ActivityTypes.Typing, typeof(ITypingActivity) },
        };

        public override bool TryResolve(Type type, object tag, out object value)
        {
            if (tag == null)
            {
                // if type is Activity, we're not delegating to the inner IResolver.
                if (typeof(IActivity).IsAssignableFrom(type))
                {
                    // if we have a registered IActivity
                    IActivity activity;
                    if (this.inner.TryResolve<IActivity>(tag, out activity))
                    {
                        if (activity.Type != null)
                        {
                            // then make sure the IActivity.Type allows the desired type
                            Type allowedType;
                            if (TypeByName.TryGetValue(activity.Type, out allowedType))
                            {
                                if (type.IsAssignableFrom(allowedType))
                                {
                                    // and make sure the actual CLR type also allows the desired type
                                    // (this is true most of the time since Activity implements all of the interfaces)
                                    Type clrType = activity.GetType();
                                    if (allowedType.IsAssignableFrom(clrType))
                                    {
                                        value = activity;
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    // otherwise we were asking for IActivity and it wasn't assignable from the IActivity.Type
                    value = null;
                    return false;
                }
            }

            // delegate to the inner for all remaining type resolutions
            return base.TryResolve(type, tag, out value);
        }
    }

    public abstract class PropertyResolver<T> : DelegatingResolver
    {
        public PropertyResolver(IResolver inner)
            : base(inner)
        {
        }

        protected abstract object PropertyFrom(T item);

        public override bool TryResolve(Type type, object tag, out object value)
        {
            T item;
            if (this.inner.TryResolve(tag, out item))
            {
                var property = PropertyFrom(item);
                if (property != null)
                {
                    var propertyType = property.GetType();
                    if (type.IsAssignableFrom(propertyType))
                    {
                        value = property;
                        return true;
                    }
                }
            }

            return base.TryResolve(type, tag, out value);
        }
    }

    public sealed class EventActivityValueResolver : PropertyResolver<IEventActivity>
    {
        public EventActivityValueResolver(IResolver inner)
            : base(inner)
        {
        }

        protected override object PropertyFrom(IEventActivity item)
        {
            return item.Value;
        }
    }

    public sealed class InvokeActivityValueResolver : PropertyResolver<IInvokeActivity>
    {
        public InvokeActivityValueResolver(IResolver inner)
            : base(inner)
        {
        }

        protected override object PropertyFrom(IInvokeActivity item)
        {
            return item.Value;
        }
    }
}
