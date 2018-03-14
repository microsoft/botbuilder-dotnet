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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Classic.Internals.Fibers;

namespace Microsoft.Bot.Builder.Classic.Scorables.Internals
{
    public interface IBinder
    {
        bool TryBind(MethodBase method, IResolver resolver, out IBinding binding);
        bool TryBind(Delegate lambda, IResolver resolver, out IBinding binding);
        bool TryBind<R>(MethodInfo method, IResolver resolver, out IBinding<R> binding);
        bool TryBind<R>(Delegate lambda, IResolver resolver, out IBinding<R> binding);
    }

    public static partial class Extensions
    {
        private static readonly ConditionalWeakTable<MethodBase, IReadOnlyList<ParameterInfo>> ParametersByMethod =
            new ConditionalWeakTable<MethodBase, IReadOnlyList<ParameterInfo>>();
        public static IReadOnlyList<ParameterInfo> CachedParameters(this MethodBase method)
        {
            return ParametersByMethod.GetValue(method, m => m.GetParameters());
        }

        private static readonly ConditionalWeakTable<MethodBase, IReadOnlyList<Type>> ParameterTypesByMethod =
            new ConditionalWeakTable<MethodBase, IReadOnlyList<Type>>();
        public static IReadOnlyList<Type> CachedParameterTypes(this MethodBase method)
        {
            return ParameterTypesByMethod.GetValue(method, m => m.GetParameters().ToList(p => p.ParameterType));
        }
    }

    public sealed class Binder : IBinder
    {
        public static readonly IBinder Instance = new Binder();
        private Binder()
        {
        }

        public static bool TryResolveReturnType<R>(MethodInfo method)
        {
            var type = method.ReturnType;
            if (typeof(R).IsAssignableFrom(type))
            {
                return true;
            }

            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();
                if (definition == typeof(Task<>))
                {
                    var arguments = type.GetGenericArguments();
                    if (typeof(R).IsAssignableFrom(arguments[0]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryResolveInstance(IResolver resolver, MethodBase method, object target, out object instance)
        {
            if (target != null)
            {
                var type = target.GetType();
                if (method.DeclaringType.IsAssignableFrom(type))
                {
                    instance = target;
                    return true;
                }
            }

            if (method.IsStatic)
            {
                instance = null;
                return true;
            }

            return resolver.TryResolve(method.DeclaringType, null, out instance);
        }

        public static bool TryResolveArgument(IResolver resolver, ParameterInfo parameter, out object argument)
        {
            var type = parameter.ParameterType;

            var entity = parameter.GetCustomAttribute<EntityAttribute>();
            if (entity != null)
            {
                if (resolver.TryResolve(type, entity.Name, out argument))
                {
                    return true;
                }
            }

            if (resolver.TryResolve(type, parameter.Name, out argument))
            {
                return true;
            }

            return resolver.TryResolve(type, null, out argument);
        }

        public static bool TryResolveArguments(IResolver resolver, MethodBase method, out object[] arguments)
        {
            var parameters = method.CachedParameters();
            if (parameters.Count == 0)
            {
                arguments = Array.Empty<object>();
                return true;
            }

            arguments = null;
            for (int index = 0; index < parameters.Count; ++index)
            {
                var parameter = parameters[index];

                object argument;

                if (!TryResolveArgument(resolver, parameter, out argument))
                {
                    arguments = null;
                    return false;
                }

                if (arguments == null)
                {
                    arguments = new object[parameters.Count];
                }

                arguments[index] = argument;
            }

            return arguments != null;
        }

        public static bool TryBind(MethodBase method, object target, IResolver resolver, out IBinding binding)
        {
            object instance;
            if (!TryResolveInstance(resolver, method, target, out instance))
            {
                binding = null;
                return false;
            }

            object[] arguments;
            if (!TryResolveArguments(resolver, method, out arguments))
            {
                binding = null;
                return false;
            }

            binding = new Binding(method, instance, arguments);
            return true;
        }

        public static bool TryBind<R>(MethodInfo method, object target, IResolver resolver, out IBinding<R> binding)
        {
            if (!TryResolveReturnType<R>(method))
            {
                binding = null;
                return false;
            }

            object instance;
            if (!TryResolveInstance(resolver, method, target, out instance))
            {
                binding = null;
                return false;
            }

            object[] arguments;
            if (!TryResolveArguments(resolver, method, out arguments))
            {
                binding = null;
                return false;
            }

            binding = new Binding<R>(method, instance, arguments);
            return true;
        }

        bool IBinder.TryBind(MethodBase method, IResolver resolver, out IBinding binding)
        {
            return TryBind(method, null, resolver, out binding);
        }

        bool IBinder.TryBind(Delegate lambda, IResolver resolver, out IBinding binding)
        {
            return TryBind(lambda.Method, lambda.Target, resolver, out binding);
        }

        bool IBinder.TryBind<R>(MethodInfo method, IResolver resolver, out IBinding<R> binding)
        {
            return TryBind(method, null, resolver, out binding);
        }

        bool IBinder.TryBind<R>(Delegate lambda, IResolver resolver, out IBinding<R> binding)
        {
            return TryBind(lambda.Method, lambda.Target, resolver, out binding);
        }
    }
}
