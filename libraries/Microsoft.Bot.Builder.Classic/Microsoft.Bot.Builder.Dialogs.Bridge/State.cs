// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Bot.Builder.Dialogs.Bridge
{
    [Serializable]
    public sealed class State
    {
        public object Dialog { get; set; }

        public MethodInfo Rest { get; set; }

        public Type ItemType
        {
            get
            {
                var paraneters = Rest.GetParameters();
                var awaitable = paraneters[1].ParameterType;
                var arguments = awaitable.GetGenericArguments();
                return arguments.Single();
            }
        }
    }
}
