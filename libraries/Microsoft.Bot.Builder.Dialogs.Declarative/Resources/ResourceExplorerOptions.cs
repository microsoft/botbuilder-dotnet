// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative.Converters;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// Configuration options for <see cref="ResourceExplorer"/>.
    /// </summary>
    public class ResourceExplorerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceExplorerOptions"/> class.
        /// </summary>
        /// <param name="resourceProviders">Initial collection of <see cref="ResourceProvider"/> to bootstrap the <see cref="ResourceExplorer" />.</param>
        /// <param name="declarativeTypes">Initial collection of <see cref="DeclarativeType"/> to bootstrap the <see cref="ResourceExplorer" />.</param>
        /// <param name="converterFactories">Initial collection of <see cref="JsonConverterFactory"/> to bootstrap the <see cref="ResourceExplorer" />.</param>
        public ResourceExplorerOptions(
            IEnumerable<ResourceProvider> resourceProviders = default, 
            IEnumerable<DeclarativeType> declarativeTypes = default, 
            IEnumerable<JsonConverterFactory> converterFactories = default)
        {
            TypeRegistrations = declarativeTypes;
            Providers = resourceProviders;
            ConverterFactories = converterFactories;
        }

        /// <summary>
        /// Gets or sets a value indicating whether whether cycles are allowed in resources managed by the <see cref="ResourceExplorer"/>.
        /// </summary>
        /// <value>
        /// Whether cycles are allowed in resources managed by the <see cref="ResourceExplorer"/>.
        /// </value>
        public bool AllowCycles { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of resource providers to initialize the current the <see cref="ResourceExplorer"/>.
        /// </summary>
        /// <value>
        /// The list of resource providers to initialize the current the <see cref="ResourceExplorer"/>.
        /// </value>
        public IEnumerable<ResourceProvider> Providers { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="DeclarativeType"/> registrations to initialize the current the <see cref="ResourceExplorer"/>.
        /// </summary>
        /// <value>
        /// The list of <see cref="DeclarativeType"/> registrations to initialize the current the <see cref="ResourceExplorer"/>.
        /// </value>
        public IEnumerable<DeclarativeType> TypeRegistrations { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="JsonConverterFactory"/> registrations to initialize the current the <see cref="ResourceExplorer"/>.
        /// </summary>
        /// <value>
        /// The list of <see cref="JsonConverterFactory"/> registrations to initialize the current the <see cref="ResourceExplorer"/>.
        /// </value>
        public IEnumerable<JsonConverterFactory> ConverterFactories { get; set; }

        /// <summary>
        /// Gets or sets the list of declarative types to use. Falls back to <see cref="ComponentRegistration.Components" /> if set to null.
        /// </summary>
        /// <value>
        /// The list of declarative types to use. Falls back to <see cref="ComponentRegistration.Components" /> if set to null.
        /// </value>
        [Obsolete("Register `DeclarativeType` instances directly through the TypeRegistrations property.`")]
        public IEnumerable<IComponentDeclarativeTypes> DeclarativeTypes { get; set; }
    }   
}
