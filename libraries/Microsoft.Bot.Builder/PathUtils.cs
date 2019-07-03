using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Bot.Builder
{
    public static class PathUtils
    {
        private static char[] osChars = { '\\', '/' };

        public static string NormalizePath(string path) => Path.Combine(path.TrimEnd(osChars).Split(osChars));
    }
}
