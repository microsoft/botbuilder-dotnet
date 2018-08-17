using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Ai.LanguageGeneration.Engine
{
    internal class ResolverPipeline : IResolverPipeline
    {
        private readonly IList<IResolverPipelineWorker> _workers;
        public ResolverPipeline(IList<IResolverPipelineWorker> workers)
        {
            _workers = workers ?? throw new ArgumentNullException(nameof(workers));
        }
        public async Task ExecuteAsync(Activity activity, IDictionary<string, object> entities)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (var worker in _workers)
            {
                await worker.ExecuteAsync().ConfigureAwait(false);
            }
        }
    }
}
