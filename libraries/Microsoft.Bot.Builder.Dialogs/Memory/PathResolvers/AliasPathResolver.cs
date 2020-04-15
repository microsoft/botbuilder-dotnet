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
        private readonly string prefix;
        private readonly string postfix;

        public AliasPathResolver(string alias, string prefix, string postfix = null)
        {
            this.Alias = alias?.Trim() ?? throw new ArgumentNullException(nameof(alias));
            this.prefix = prefix?.Trim() ?? throw new ArgumentNullException(nameof(prefix));
            this.postfix = postfix ?? string.Empty;
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
            if (path.StartsWith(Alias) && path.Length > Alias.Length && IsPathChar(path[Alias.Length]))
            {
                // here we only deals with trailing alias, alias in middle be handled in further breakdown
                // $xxx -> path.xxx
                return $"{this.prefix}{path.Substring(Alias.Length)}{this.postfix}".TrimEnd('.');
            }

            return path;
        }

        protected bool IsPathChar(char ch)
        {
             return char.IsLetter(ch) || ch == '_';
        }
    }
}
