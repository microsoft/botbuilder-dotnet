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

using Autofac;
using Autofac.Builder;
using System;

namespace Microsoft.Bot.Builder.Classic.Autofac.Base
{
    public static partial class Extensions
    {
        /// <summary>
        /// Register a component to be created through reflection, and
        /// provide a key that can be used to retrieve the component.
        /// </summary>
        /// <remarks>
        /// This method leverages Autofac's autowiring of components through reflection,
        /// providing a key that directly reflects that component type, so that it might
        /// be retrieved by that key and possibly replaced in an adapter chain.
        /// </remarks>
        /// <typeparam name="TImplementer">The type of the component implementation.</typeparam>
        /// <typeparam name="TLimit">The service type provided by the component.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static
            IRegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterKeyedType<TImplementer, TLimit>(this ContainerBuilder builder)
        {
            // use Autofac to autowire the implementer of the service
            // with a key of that implementer type so that it can be overridden
            return
                builder
                .RegisterType<TImplementer>()
                .Keyed<TLimit>(typeof(TImplementer));
        }

        /// <summary>
        /// Register an adapter chain of components, exposing a shared service interface.
        /// </summary>
        /// <remarks>
        /// This registers a factory method to create a adapter chain of components, based on wrapping each
        /// inner component with an adapter outer component.
        /// </remarks>
        /// <typeparam name="TLimit">The service type provided by the component.</typeparam>
        /// <param name="builder">Container builder.</param>
        /// <param name="types">The services type keys that can be used to retrieve the components in the chain.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        public static
            IRegistrationBuilder<TLimit, SimpleActivatorData, SingleRegistrationStyle>
            RegisterAdapterChain<TLimit>(this ContainerBuilder builder, params Type[] types)
        {
            return
                builder
                .Register(c =>
                {
                    // http://stackoverflow.com/questions/23406641/how-to-mix-decorators-in-autofac
                    TLimit service = default(TLimit);
                    for (int index = 0; index < types.Length; ++index)
                    {
                        // resolve the keyed adapter, passing the previous service as the inner parameter
                        service = c.ResolveKeyed<TLimit>(types[index], TypedParameter.From(service));
                    }

                    return service;
                })
                .As<TLimit>();
        }
    }
}
