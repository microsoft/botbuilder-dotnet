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
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public class SlotMapSelector : MostSpecificSelector
    {
        private const string _entityPrefix = "turn.entities";
        private readonly IExpressionParser _parser = new ExpressionEngine(TriggerTree.LookupFunction);
        private DialogSchema _schema;
        private Dictionary<string, List<EntityInfo>> _entityToInfo;
        private List<HandlerInfo> _slotMappers = new List<HandlerInfo>();
        private List<IOnEvent> _other = new List<IOnEvent>();
        private Dictionary<string, List<HandlerInfo>> _entityToMappers = new Dictionary<string, List<HandlerInfo>>();

        public SlotMapSelector(DialogSchema schema)
        {
            _schema = schema;
        }

        public override void Initialize(IEnumerable<IOnEvent> onEvents, bool evaluate = false)
        {
            base.Initialize(onEvents, true);
            var pos = 0u;
            foreach (var ev in onEvents)
            {
                var info = new HandlerInfo(pos++, ev, _parser);
                if (info.Entities.Any())
                {
                    _slotMappers.Add(info);
                    foreach (var entity in info.Entities)
                    {
                        if (!_entityToMappers.TryGetValue(entity, out var mappings))
                        {
                            mappings = new List<HandlerInfo>();
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

        public override async Task<IReadOnlyList<IOnEvent>> Select(SequenceContext context, CancellationToken cancel = default(CancellationToken))
        {
            // Prefer handlers by:
            // * Set & Expected slots
            // * Set & Coverage
            // * Set & Priority
            // * Disambiguation & expected
            // * Disambiguation & coverage
            // * Disambiguation & priority
            // * Prompt
            AnalyzeEntities(context);
            var matches = await base.Select(context, cancel);
            var actions = new List<IDialog>();
            var coverages = new double[_slotMappers.Count];
            foreach (var info in _slotMappers)
            {
                coverages[info.Position] = ComputeCoverage(context, info.Entities);
            }

            // TODO: Need to figure out entities that overlap?
            // What if you get number and something bigger
            // Maybe for a given slot, prefer in order of mappings
            foreach (var mappers in _entityToMappers.Values)
            {
                if (mappers.Count > 1)
                {
                    // Prefer expected 
                    foreach (var mapper in mappers)
                    {

                    }
                }
            }

            // Same entity to multiple slots
            // Different entities to same slot 

            // Add any set property that is only consumer of entity.
            foreach (var mapping in _entityToMappers.Values)
            {
                if (mapping.Count == 1)
                {
                    var ev = mapping[0];
                    if (ev.IsSet)
                    {
                        AddActions(actions, ev);
                    }
                }
            }

            // Status messages 
            foreach (var mapping in _entityToMappers.Values)
            {
                if (mapping.Count == 1)
                {
                    var ev = mapping[0];
                    if (ev.SendsActivity)
                    {
                        AddActions(actions, ev);
                    }
                }
            }

            // ??? Need to know disambiguation instead of prompt ???

            // 1) Only mapping that sets
            // 2) Only mapping that does status
            // 3) Only mapping that does prompt
            return (IReadOnlyList<IOnEvent>)_other;
        }

        private void AnalyzeEntities(SequenceContext context)
        {
            var entities = (Dictionary<string, object>)context.State.GetValue(DialogContextState.TURN_RECOGNIZED + ".entities");
            // TODO: We should have RegexRecognizer return $instance or make this robust to it missing, i.e. assume no entities overlap
            var metaData = (Dictionary<string, InstanceData[]>)entities["$instance"];
            foreach (var entry in entities)
            {
                var name = entry.Key;
                if (!name.StartsWith("$"))
                {
                    var values = (object[])entry.Value;
                    var lastName = name.Substring(name.LastIndexOf("."));
                    var instances = metaData[lastName];
                    for (var i = 0; i < values.Count(); ++i)
                    {
                        var val = values[i];
                        var instance = instances[i];
                        if (!_entityToInfo.TryGetValue(name, out var infos))
                        {
                            infos = new List<EntityInfo>();
                            _entityToInfo[name] = infos;
                        }

                        infos.Add(new EntityInfo(instance.StartIndex, instance.EndIndex, instance.Score ?? 0.0));
                    }
                }
            }
        }

        private void AddActions(List<IDialog> actions, HandlerInfo info)
        {
            foreach (var action in info.Handler.Actions)
            {
                actions.Add(action);
            }
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

        private class HandlerInfo
        {
            public HandlerInfo(uint position, IOnEvent handler, IExpressionParser parser)
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

                    if (action is FormInput)
                    {
                        FormInput = true;
                    }
                    else if (action is SendActivity)
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

            public bool FormInput { get; }

            public bool IsSet => Slots.Any() && !SendsActivity && !FormInput;
        }

        private class EntityInfo
        {

            public EntityInfo(int start, int end, double score = 0.0)
            {
                Start = start;
                End = end;
                Score = score;
            }

            public int Start { get; }

            public int End { get; }

            public double Score { get; }
        }
    }
}
