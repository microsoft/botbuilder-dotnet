// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    /// <summary>
    /// Maps aliasXXX -> path.xxx ($foo => dialog.foo).
    /// </summary>
    public class AliasPathResolver : DefaultPathResolver
    {
        private string alias;
        private string prefix;
        private string postfix;

        public AliasPathResolver(string alias, string prefix, string postfix = null)
        {
            this.alias = alias?.Trim() ?? throw new ArgumentNullException(nameof(alias));
            this.prefix = prefix?.Trim() ?? throw new ArgumentNullException(nameof(prefix));
            this.postfix = postfix ?? string.Empty;
        }

        public override bool Matches(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return path.Trim().StartsWith(this.alias);
        }

        protected override string TransformPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var start = path.IndexOf(this.alias);
            if (start >= 0)
            {
                // $xxx -> path.xxx
                return $"{this.prefix}{path.Substring(start + alias.Length)}{this.postfix}";
            }

            return path;
        }
    }
}
