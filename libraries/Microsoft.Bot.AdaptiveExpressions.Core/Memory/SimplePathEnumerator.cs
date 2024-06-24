// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Bot.AdaptiveExpressions.Core.Memory
{
    /// <summary>
    /// Helper to parse and walk a simple path syntax for IMemory objects.
    /// </summary>
    internal struct SimplePathEnumerator : IEnumerable<SimplePathEnumerator.PathEntry>
    {
        private string _path;

        public SimplePathEnumerator(string path)
        {
            _path = path;
        }

        public IEnumerator<PathEntry> GetEnumerator()
        {
            var parts = _path.Split(".[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim('\'', '"'))
                .ToArray();

            for (int i = 0; i < parts.Length; i++)
            {
                bool isLast = i == parts.Length - 1;
                var part = parts[i];
                if (int.TryParse(part, out var idx))
                {
                    yield return new PathEntry { IsLast = isLast, Index = idx, Part = part };
                }
                else
                {
                    yield return new PathEntry { IsLast = isLast, Part = part };
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        internal struct PathEntry
        {
            public bool IsLast;
            public int? Index;
            public string Part;
        }
    }
}
