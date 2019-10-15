// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Bot.Builder.Dialogs.Memory.PathResolvers
{
    /// <summary>
    /// Maps @ => turn.recognized.entitites.xxx[0].
    /// </summary>
    public class AtPathResolver : AliasPathResolver
    {
        public AtPathResolver()
            : base(alias: "@", prefix: "turn.recognized.entities.", postfix: "[0]")
        {
        }

        public override string TransformPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            // override to make sure it doesn't match @@
            path = path.Trim();
            if (path.StartsWith("@") && !path.StartsWith("@@"))
            {
                return base.TransformPath(path);
            }

            return path;
        }
    }
}
