// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using AdaptiveExpressions.Memory;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Runtime
{
    /// <summary>
    /// Provides a wrapper around <see cref="IConfiguration"/> that adheres to the <see cref="IMemory"/> interface,
    /// enabling evaluation of adaptive expressions defined within the runtime settings against the constructed
    /// application configuration object.
    /// </summary>
    public class ConfigurationMemory : IMemory
    {
        private const string SupportedSeparator = ":";

        private static readonly Regex UnsupportedSeparators = new Regex("(\\.|_)+");

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationMemory"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance to be wrapped.</param>
        public ConfigurationMemory(IConfiguration configuration)
        {
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// DO NOT USE. Sets the specified value within the wrapped <see cref="IConfiguration"/> instance at the specified path.
        /// </summary>
        /// <remarks>
        /// This function is required to bind to the <see cref="IMemory"/> interface, which is required to evaluate adaptive expressions.
        /// However, as <see cref="IConfiguration"/> is read-only, we do not support write operations against this object.
        /// <see cref="ConfigurationMemory"/> should be utilized in a read-only capacity.
        /// </remarks>
        /// <param name="path">The configuration path to set the specified value at.</param>
        /// <param name="value">The value to be set at the specified configuration path.</param>
        // SetValue function with below signature is required to bind to IMemory interface, which is required
        // to evaluate adaptive expressions.
#pragma warning disable CA1801 // Review unused parameters
        public void SetValue(string path, object value)
#pragma warning restore CA1801 // Review unused parameters
        {
            throw new InvalidOperationException("Assignment expressions are not supported.");
        }

        /// <summary>
        /// Try to retrieve a value from the wrapped <see cref="IConfiguration"/> instance using the specified path.
        /// </summary>
        /// <param name="path">The configuration path to attempt to retrieve the corresponding value for.</param>
        /// <param name="value">The configuration value mapped to the provided path.</param>
        /// <returns>Boolean indicating whether a value was successfully found for the specified path.</returns>
        public bool TryGetValue(string path, out object value)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            string configurationPath = UnsupportedSeparators.Replace(path, SupportedSeparator);

            IConfigurationSection section = this._configuration.GetSection(configurationPath);
            if (section.Exists())
            {
                value = section.Value;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Returns the version of the memory object. This will always return "1".
        /// </summary>
        /// <returns>The version of the memory object. This will always return "1".</returns>
        public string Version()
        {
            return 1.ToString(CultureInfo.InvariantCulture);
        }
    }
}
