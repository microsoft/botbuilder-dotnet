// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Obsolete;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Obsolete
{
    /// <summary>
    /// Bridge class to allow <see cref="BotComponent"/> to be backward compatible with <see cref="ComponentRegistration"/>.
    /// </summary>
    /// <typeparam name="TComponent">The type of <see cref="BotComponent"/> to bridge into legacy <see cref="ComponentRegistration"/>.</typeparam>
    [Obsolete("This class only exists for backward compatibility of legacy `ComponentRegistrations`. Use `BotComponent` for new components.")]
    public class DeclarativeComponentRegistrationBridge<TComponent> 
        : ComponentRegistrationBridge<TComponent>, IComponentDeclarativeTypes
        where TComponent : BotComponent, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeclarativeComponentRegistrationBridge{TComponent}"/> class.
        /// </summary>
        /// <param name="botComponent"><see cref="BotComponent"/> to be exposed as a legacy <see cref="ComponentRegistration"/>.</param>
        /// <param name="configuration">Optional <see cref="IConfiguration"/> for the target <see cref="BotComponent"/>.</param>
        public DeclarativeComponentRegistrationBridge(TComponent botComponent, IConfiguration configuration = null)
            : base(botComponent, configuration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeclarativeComponentRegistrationBridge{TComponent}"/> class.
        /// </summary>
        public DeclarativeComponentRegistrationBridge()
            : base()
        {
        }

        /// <inheritdoc/>
        public IEnumerable<JsonConverter> GetConverters(ResourceExplorer resourceExplorer, SourceContext sourceContext)
        {
            _ = resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer));

            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(resourceExplorer);

            IEnumerable<JsonConverterFactory> converterFactories = GetFromComponent<JsonConverterFactory>(services);

            foreach (var converterFactory in converterFactories)
            {
                yield return converterFactory.Build(resourceExplorer, sourceContext);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<DeclarativeType> GetDeclarativeTypes(ResourceExplorer resourceExplorer)
        {
            _ = resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer));

            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(resourceExplorer);

            return GetFromComponent<DeclarativeType>(services);
        }
    }
}
