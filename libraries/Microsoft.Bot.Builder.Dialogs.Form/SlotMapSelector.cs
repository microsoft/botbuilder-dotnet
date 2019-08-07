using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.TriggerTrees;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public class SlotMapSelector : IEventSelector
    {
        private const string _entityPrefix = "turn.entities";
        private readonly IExpressionParser _parser = new ExpressionEngine(TriggerTree.LookupFunction);
        private List<Info> _slotMappers = new List<Info>();
        private List<IOnEvent> _other = new List<IOnEvent>();
        private Dictionary<string, List<Info>> _entityToMappers = new Dictionary<string, List<Info>>();

        public void Initialize(IEnumerable<IOnEvent> onEvents, bool evaluate = false)
        {
            var pos = 0u;
            foreach (var ev in onEvents)
            {
                var info = new Info(pos++, ev, _parser);
                if (info.Entities.Any())
                {
                    _slotMappers.Add(info);
                    foreach (var entity in info.Entities)
                    {
                        List<Info> mappings;
                        if (!_entityToMappers.TryGetValue(entity, out mappings))
                        {
                            mappings = new List<Info>();
                            _entityToMappers[entity] = mappings;
                        }

                        mappings.Add(info);
                    }
                }
                else
                {
                    _other.Add(ev);
                }
            }
        }

        public Task<IReadOnlyList<IOnEvent>> Select(SequenceContext context, CancellationToken cancel = default(CancellationToken))
        {
            var coverages = new double[_slotMappers.Count];
            foreach (var info in _slotMappers)
            {
                coverages[info.Position] = ComputeCoverage(context, info.Entities);
            }

            // 1) Only mapping that sets
            // 2) Only mapping that does status
            // 3) Only mapping that does prompt
            return Task.FromResult((IReadOnlyList<IOnEvent>)_other);
        }

        private double ComputeCoverage(SequenceContext context, IEnumerable<string> entityNames)
        {
            var covered = 0;
            var text = (string)context.State.GetValue(DialogContextState.TURN_RECOGNIZED + ".text");
            foreach (var name in entityNames)
            {
                var path = DialogContextState.TURN_RECOGNIZED + ".entities." + name;
                var entities = (object[])context.State.GetValue(path);
                var last = name.LastIndexOf(".");
                var instancePath = DialogContextState.TURN_RECOGNIZED + ".entities." + name.Substring(0, last) + "$instance." + name.Substring(last);
                if (context.State.TryGetValue<InstanceData[]>(instancePath, out InstanceData[] instances))
                {
                    foreach (var instance in instances)
                    {
                        covered += instance.EndIndex - instance.StartIndex;
                    }
                }
                else
                {
                    foreach (var entity in entities)
                    {
                        if (entity is string str)
                        {
                            // If string use the value length
                            covered += str.Length;
                        }
                        else
                        {
                            // If not string just count as 1
                            ++covered;
                        }
                    }
                }
            }

            return (double)covered / text.Length;
        }

        private class Info
        {
            public Info(uint position, IOnEvent handler, IExpressionParser parser)
            {
                Position = position;
                Handler = handler;
                var references = handler.GetExpression(parser).References();
                foreach (var reference in references)
                {
                    if (reference.StartsWith(_entityPrefix))
                    {
                        Entities.Add(reference.Substring(_entityPrefix.Length));
                    }
                }

                foreach (var action in handler.Actions)
                {
                    if (action is SetProperty prop)
                    {
                        Slots.Add(prop.Property);
                    }

                    if (action is SendActivity)
                    {
                        SendsActivity = true;
                    }
                }
            }

            public uint Position { get; }

            public IOnEvent Handler { get; }

            public List<string> Entities { get; } = new List<string>();

            public List<string> Slots { get; } = new List<string>();

            public bool SendsActivity { get; }
        }
    }
}
