// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// interface to parse lg content into an <see cref="LGFile"/>.
    /// </summary>
    public interface ILGParser
    {
        /// <summary>
        /// Parser to turn lg file into an <see cref="LGFile"/>.
        /// </summary>
        /// <param name="filePath">LG absolute file path.</param>
        /// <returns>new <see cref="LGFile"/> entity.</returns>
        LGFile ParseFile(string filePath);

        /// <summary>
        /// Parser to turn lg text content into an <see cref="LGFile"/>.
        /// </summary>
        /// <param name="content">LG text content.</param>
        /// <param name="id">id is the content identifier.</param>
        /// <returns>new <see cref="LGFile"/> entity.</returns>
        LGFile ParseContent(string content, string id);
    }
}
