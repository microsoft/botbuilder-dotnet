// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Helper methods for escaping keys used for Cosmos DB.
    /// </summary>
    public static class CosmosDbKeyEscape
    {
        /// <summary>
        /// Older libraries had a max key length of 255.
        /// The limit is now 1023. In this library, 255 remains the default for backwards compat.
        /// To override this behavior, and use the longer limit, set CosmosDbPartitionedStorageOptions.CompatibilityMode to false.
        /// https://docs.microsoft.com/en-us/azure/cosmos-db/concepts-limits#per-item-limits.
        /// </summary>
        public const int MaxKeyLength = 255;

        // The list of illegal characters for Cosmos DB Keys comes from this list on
        // the CosmostDB docs: https://docs.microsoft.com/dotnet/api/microsoft.azure.documents.resource.id?view=azure-dotnet#remarks
        // Note: We are also escapting the "*" character, as that what we're using
        // as our escape character.
        private static char[] _illegalKeys = new[] { '\\', '?', '/', '#', '*' };

        // We are escaping illegal characters using a "*{AsciiCodeInHex}" pattern. This
        // means a key of "?test?" would be escaped as "*3ftest*3f".
        private static readonly Dictionary<char, string> _illegalKeyCharacterReplacementMap =
                new Dictionary<char, string>(_illegalKeys.ToDictionary(c => c, c => '*' + ((int)c).ToString("x2", CultureInfo.InvariantCulture)));

        /// <summary>
        /// Converts the key into a DocumentID that can be used safely with Cosmos DB.
        /// The following characters are restricted and cannot be used in the Id property: '/', '\', '?', and '#'.
        /// More information at <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.resource.id?view=azure-dotnet"/>.
        /// </summary>
        /// <param name="key">The key to escape.</param>
        /// <returns>An escaped key that can be used safely with CosmosDB.</returns>
        public static string EscapeKey(string key)
        {
            return EscapeKey(key, string.Empty, true);
        }

        /// <summary>
        /// Converts the key into a DocumentID that can be used safely with Cosmos DB.
        /// The following characters are restricted and cannot be used in the Id property: '/', '\', '?', and '#'.
        /// More information at <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.documents.resource.id?view=azure-dotnet"/>.
        /// </summary>
        /// <param name="key">The key to escape.</param>
        /// <param name="suffix">The string to add at the end of all row keys.</param>
        /// <param name="compatibilityMode">True if running in compatability mode and keys should
        /// be truncated in order to support previous CosmosDb max key length of 255. 
        /// This behavior can be overridden by setting
        /// <see cref="CosmosDbPartitionedStorageOptions.CompatibilityMode"/> to false.</param>
        /// <returns>An escaped key that can be used safely with CosmosDB.</returns>
        public static string EscapeKey(string key, string suffix, bool compatibilityMode)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var firstIllegalCharIndex = key.IndexOfAny(_illegalKeys);

            // If there are no illegal characters, and the key is within length costraints,
            // return immediately and avoid any further processing/allocations
            if (firstIllegalCharIndex == -1)
            {
                return TruncateKeyIfNeeded($"{key}{suffix}", compatibilityMode);
            }

            // Allocate a builder that assumes that all remaining characters might be replaced to avoid any extra allocations
            var sanitizedKeyBuilder = new StringBuilder(key.Length + ((key.Length - firstIllegalCharIndex + 1) * 3));

            // Add all good characters up to the first bad character to the builder first
            for (var index = 0; index < firstIllegalCharIndex; index++)
            {
                sanitizedKeyBuilder.Append(key[index]);
            }

            var illegalCharacterReplacementMap = _illegalKeyCharacterReplacementMap;

            // Now walk the remaining characters, starting at the first known bad character, replacing any bad ones with their designated replacement value from the map
            for (var index = firstIllegalCharIndex; index < key.Length; index++)
            {
                var ch = key[index];

                // Check if this next character is considered illegal and, if so, append its replacement; otherwise just append the good character as is
                if (illegalCharacterReplacementMap.TryGetValue(ch, out var replacement))
                {
                    sanitizedKeyBuilder.Append(replacement);
                }
                else
                {
                    sanitizedKeyBuilder.Append(ch);
                }
            }

            if (!string.IsNullOrWhiteSpace(suffix)) 
            {
                sanitizedKeyBuilder.Append(suffix);
            }

            return TruncateKeyIfNeeded(sanitizedKeyBuilder.ToString(), compatibilityMode);
        }

        private static string TruncateKeyIfNeeded(string key, bool truncateKeysForCompatibility)
        {
            if (!truncateKeysForCompatibility)
            {
                return key;
            }

            if (key.Length > MaxKeyLength)
            {
                var hash = key.GetDeterministicHashCode().ToString("x", CultureInfo.InvariantCulture);
                key = key.Substring(0, MaxKeyLength - hash.Length) + hash;
            }

            return key;
        }

        /// <summary>
        /// Creates a deterministic hash code by iterating through the string two characters at a time,
        /// updating two separate hash values, and then combining them at the end.
        /// This approach helps in reducing hash collisions and provides a consistent hash code for the same string across
        /// different runs and environments.
        /// </summary>
        /// <param name="str">The string to calculate the hash on.</param>
        /// <returns>The hash code.</returns>
        private static int GetDeterministicHashCode(this string str)
        {
            unchecked
            {
                var hash1 = (5381 << 16) + 5381; //shifts 5381 left by 16 bits and adds 5381 to it
                var hash2 = hash1;
                for (var i = 0; i < str.Length; i += 2)
                {
                    // ((hash1 << 5) + hash1) is equivalent to hash1 * 33, which is a common multiplier in hash functions.
                    // The character str[i] is then XORed with this value.
                    hash1 = ((hash1 << 5) + hash1) ^ str[i]; 
                    if (i == str.Length - 1)
                    {
                        break;
                    }

                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                //1566083941 is a large prime number used to mix the two hash values together, ensuring a more uniform distribution of hash codes.
                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
