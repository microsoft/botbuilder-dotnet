using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Composition.Resources
{
    /// <summary>
    /// Class which managers multiple IResourceSource 
    /// </summary>
    public class BotResourceManager : IBotResourceProvider, IBotResourceWatcher
    {
        public static HashSet<string> ResourceTypes { get; set; } = new HashSet<string>() { "lg", "lu", "cog", "schema" };
        private HashSet<string> watchedSources = new HashSet<string>();

        public BotResourceManager()
        {
        }

        public string Id { get; set; } = "ResourceManager";

        /// <summary>
        /// Resource Providers
        /// </summary>
        public List<IBotResourceProvider> Providers { get; set; } = new List<IBotResourceProvider>();

        /// <summary>
        /// Should monitoring of resources be done?
        /// </summary>
        public bool MonitorChanges { get; set; } = true;

        /// <summary>
        /// Event that fires whenever a resource changes
        /// </summary>
        public event ResourceChangeHandler Changed;

        /// <summary>
        /// get resources of a given type
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        public async Task<IBotResource[]> GetResources(string resourceType)
        {
            List<IBotResource> resources = new List<IBotResource>();

            foreach (var source in this.Providers)
            {
                resources.AddRange(await source.GetResources(resourceType));
                
                // listen to changes for this resources if we aren't already
                if (this.MonitorChanges && source is IBotResourceWatcher watcher && !watchedSources.Contains(source.Id))
                {
                    watcher.Changed += ResourceChanged;
                    watchedSources.Add(source.Id);
                }
            }
            return resources.ToArray();
        }

        private void ResourceChanged(IBotResourceProvider source, IBotResource resource)
        {
            if (Changed != null)
            {
                Changed(source, resource);
            }
        }

    }
}
