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
        private readonly string alias;
        private readonly string prefix;
        private readonly string postfix;

        public AliasPathResolver(string alias, string prefix, string postfix = null)
        {
            this.alias = alias?.Trim() ?? throw new ArgumentNullException(nameof(alias));
            this.prefix = prefix?.Trim() ?? throw new ArgumentNullException(nameof(prefix));
            this.postfix = postfix ?? string.Empty;
        }

        public virtual string TransformPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var start = path.IndexOf(this.alias);
            if (start == 0)
            {
                // here we only deals with trailing alias, alias in middle be handled in further breakdown
                // $xxx -> path.xxx
                return $"{this.prefix}{path.Substring(start + alias.Length)}{this.postfix}".TrimEnd('.');
            }

            return path;
        }
    }
}
