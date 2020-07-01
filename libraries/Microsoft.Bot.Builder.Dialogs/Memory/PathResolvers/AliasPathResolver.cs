// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers
{
    /// <summary>
    /// Maps aliasXXX -> path.xxx ($foo => dialog.foo).
    /// </summary>
    public class AliasPathResolver : IPathResolver
    {
        private readonly string _postfix;
        private readonly string _prefix;

        public AliasPathResolver(string alias, string prefix, string postfix = null)
        {
            Alias = alias?.Trim() ?? throw new ArgumentNullException(nameof(alias));
            _prefix = prefix?.Trim() ?? throw new ArgumentNullException(nameof(prefix));
            _postfix = postfix ?? string.Empty;
        }

        /// <summary>
        /// Gets the alias name.
        /// </summary>
        /// <value>
        /// The alias name.
        /// </value>
        public string Alias { get; }

        public virtual string TransformPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            path = path.Trim();
            if (path.StartsWith(Alias, StringComparison.Ordinal) && path.Length > Alias.Length && IsPathChar(path[Alias.Length]))
            {
                // here we only deals with trailing alias, alias in middle be handled in further breakdown
                // $xxx -> path.xxx
                return $"{_prefix}{path.Substring(Alias.Length)}{_postfix}".TrimEnd('.');
            }

            return path;
        }

#pragma warning disable CA1822 // Mark members as static (we can't change this without breaking binary compat)
        protected bool IsPathChar(char ch)
#pragma warning restore CA1822 // Mark members as static
        {
            return char.IsLetter(ch) || ch == '_';
        }
    }
}
