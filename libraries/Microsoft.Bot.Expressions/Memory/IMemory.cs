// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Expressions.Memory
{
    public interface IMemory
    {
        /// <summary>
        /// Get value from a given path, it can be a simple indenfiter like "a", or
        /// a combined path like "a.b", "a.b[2]", "a.b[2].c", inside [] is guranteed to be a int number or a string.
        /// </summary>
        /// <param name="path">memory path.</param>
        /// <returns> resovled value and error messsage if any. </returns>
        (object value, string error) GetValue(string path);

        /// <summary>
        /// Set value to a given path.
        /// </summary>
        /// <param name="path">memory path.</param>
        /// <param name="value">value to set.</param>
        /// <returns>value set and error message if any.</returns>
        (object value, string error) SetValue(string path, object value);

        /// <summary>
        /// Version is used to identify whether the a particular memory instance has been updated or not.
        /// If version is not changed, the caller may choose to use the cached result instead of recomputing everything.
        /// </summary>
        /// <returns>A string indicates the version.</returns>
        string Version();
    }
}
