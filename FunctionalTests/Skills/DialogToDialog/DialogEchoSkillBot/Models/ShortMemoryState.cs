using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace Microsoft.BotBuilderSamples.DialogEchoSkillBot.Models
{
    public class ShortMemoryState : BotStateExtended
    {
        public const string ShortMemoryPropertyName = "ShortMemory";
        private readonly string _contextServiceKey;
        private readonly ITurnContextAwareStorage _storageFactory;

        public ShortMemoryState(ITurnContextAwareStorage turnContextAwareStorage)
            : base(turnContextAwareStorage, nameof(ShortMemoryState))
        {
            _storageFactory = turnContextAwareStorage;
            _contextServiceKey = nameof(ShortMemoryState);
        }

        /// <summary>
        /// Creates a named state property within the scope of a <see cref="BotState"/> and returns
        /// an accessor for the property.
        /// </summary>
        /// <typeparam name="T">The value type of the property.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <returns>An accessor for the property.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
        public IStatePropertyAccessor<T> CreateProperty<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new ShortMemoryStatePropertyAccessor<T>(this, name);
        }
    }
}
