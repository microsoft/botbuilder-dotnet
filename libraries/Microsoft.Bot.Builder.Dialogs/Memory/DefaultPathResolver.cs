// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    public class DefaultPathResolver : IPathResolver
    {
        public DefaultPathResolver()
        {
        }

        /// <summary>
        /// Override this method to have your resolver say that it can handle the path.
        /// </summary>
        /// <param name="path">path to inspect.</param>
        /// <returns>true if it will resolve the path.</returns>
        public virtual bool Matches(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return true;
        }

        /// <summary>
        /// Return the value from the state using the transformed path.
        /// </summary>
        /// <typeparam name="T">type of value to return</typeparam>
        /// <param name="dc">dc</param>
        /// <param name="path">path to evaluate</param>
        /// <param name="value">value to return</param>
        /// <returns>true if value is found, false ifnot</returns>
        public virtual bool TryGetValue<T>(DialogContext dc, string path, out T value)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            path = this.TransformPath(path);
            var memoryScope = this.ResolveMemoryScope(dc, ref path);
            var memory = memoryScope.GetMemory(dc);
            return ObjectPath.TryGetValue<T>(memory, path, out value);
        }

        /// <summary>
        /// Remove the value from the state using the transformed path.
        /// </summary>
        /// <param name="dc">dc</param>
        /// <param name="path">path to use.</param>
        public virtual void RemoveValue(DialogContext dc, string path)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            path = this.TransformPath(path);
            var memoryScope = this.ResolveMemoryScope(dc, ref path);
            var memory = memoryScope.GetMemory(dc);
            ObjectPath.RemoveProperty(memory, path);
        }

        /// <summary>
        /// Set the value from the state using the transformed path.
        /// </summary>
        /// <param name="dc">dc</param>
        /// <param name="path">path to use.</param>
        /// <param name="value">value</param>
        public virtual void SetValue(DialogContext dc, string path, object value)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            path = this.TransformPath(path);
            var memoryScope = this.ResolveMemoryScope(dc, ref path);
            if (path == string.Empty)
            {
                memoryScope.SetMemory(dc, value);
            }
            else
            {
                var memory = memoryScope.GetMemory(dc);
                ObjectPath.SetValue(memory, path, value);
            }
        }

        /// <summary>
        /// Method to transform aliases to target path
        /// </summary>
        /// <param name="path">path</param>
        /// <returns>transformed path</returns>
        protected virtual string TransformPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return path;
        }

        /// <summary>
        /// ResolveMemoryScope will find the MemoryScope for root part of path and adjust path to be subpath
        /// </summary>
        /// <param name="dc">dc</param>
        /// <param name="path">incoming path will be resolved to scope and adjusted to subpath</param>
        /// <returns>memoryscope</returns>
        protected virtual MemoryScope ResolveMemoryScope(DialogContext dc, ref string path)
        {
            if (dc == null)
            {
                throw new ArgumentNullException(nameof(dc));
            }

            string scope = path;
            var index = path.IndexOf(".");
            if (index > 0)
            {
                scope = path.Substring(0, index);
                var memoryScope = DialogStateManager.GetMemoryScope(scope);
                if (memoryScope != null)
                {
                    path = path.Substring(index + 1);
                    return memoryScope;
                }
            }

            // could be User[foo] path 
            index = path.IndexOf("[");
            if (index > 0)
            {
                scope = path.Substring(0, index);
                path = path.Substring(index);
                return DialogStateManager.GetMemoryScope(scope) ?? throw new ArgumentOutOfRangeException(GetBadScopeMessage(path));
            }
            else
            {
                path = string.Empty;
                return DialogStateManager.GetMemoryScope(scope) ?? throw new ArgumentOutOfRangeException(GetBadScopeMessage(path));
            }
        }

        private static string GetBadScopeMessage(string path)
        {
            return $"'{path}' does not match memory scopes:{string.Join(",", DialogStateManager.MemoryScopes.Select(ms => ms.Name))}";
        }
    }
}
