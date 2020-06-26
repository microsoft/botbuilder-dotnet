using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    public class DialogStateManagerConfiguration
    {
        public DialogStateManagerConfiguration()
        {
            if (!ComponentRegistration.Components.Contains(typeof(DialogsComponentRegistration)))
            {
                ComponentRegistration.Add(new DialogsComponentRegistration());
            }

            // get all of the component memory scopes
            foreach (var component in ComponentRegistration.Components.OfType<IComponentMemoryScopes>())
            {
                foreach (var memoryScope in component.GetMemoryScopes())
                {
                    this.MemoryScopes.Add(memoryScope);
                }
            }

            // get all of the component path resolvers
            foreach (var component in ComponentRegistration.Components.OfType<IComponentPathResolvers>())
            {
                foreach (var pathResolver in component.GetPathResolvers())
                {
                    this.PathResolvers.Add(pathResolver);
                }
            }
        }

        /// <summary>
        /// Gets or sets PathResolvers.
        /// </summary>
        /// <value>
        /// PathResolvers (aka shortcuts) to load into the dialog state manager context.
        /// </value>
        public List<IPathResolver> PathResolvers { get; set; } = new List<IPathResolver>();

        /// <summary>
        /// Gets or sets MemoryScopes.
        /// </summary>
        /// <value>
        /// MemoryScopes to load into the dialog state manager context.
        /// </value>
        public List<MemoryScope> MemoryScopes { get; set; } = new List<MemoryScope>();
    }
}
