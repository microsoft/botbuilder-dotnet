using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Converters
{
    /// <summary>
    /// The DependencyInjectionConverter<typeparamref name="T"/> uses IServiceProvider build an object of T with it's dependencies.
    /// </summary>
    /// <typeparam name="T">class to create.</typeparam>
    public class DependencyInjectionConverter<T> : CustomCreationConverter<T>
        where T : class
    {
        private IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyInjectionConverter{T}"/> class.
        /// </summary>
        /// <param name="serviceProvider">Dependency injection service provider.</param>
        public DependencyInjectionConverter(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public override T Create(Type objectType)
        {
            return ActivatorUtilities.CreateInstance<T>(this.serviceProvider);
        }
    }
}
