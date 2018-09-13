using System.Collections.Generic;
using System.Linq;

namespace Chronic.Handlers
{
    public class HandlerRegistry
    {
        readonly Dictionary<HandlerType, List<ComplexHandler>> _handlers =
            new Dictionary<HandlerType, List<ComplexHandler>>();

        public HandlerRegistry Add(HandlerType type,
                                   IEnumerable<ComplexHandler> handlers)
        {
            _handlers.Add(type, handlers.ToList());
            return this;
        }

        public IList<ComplexHandler> GetHandlers(HandlerType type)
        {
            if (_handlers.ContainsKey(type))
            {
                return _handlers[type].ToList();
            }
            return new List<ComplexHandler>();
        }

        public void MergeWith(HandlerRegistry registry)
        {
            foreach (var type in registry._handlers.Keys)
            {
                if (_handlers.ContainsKey(type) == false)
                {
                    _handlers[type] = new List<ComplexHandler>();
                }
                _handlers[type].AddRange(registry._handlers[type]);
            }
        }
    }
}