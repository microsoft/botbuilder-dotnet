using System.Collections.Generic;
using System.Linq;
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
        private readonly DialogSchema _schema;
        private readonly List<HandlerInfo> _slotMappers = new List<HandlerInfo>();
        private readonly List<IOnEvent> _other = new List<IOnEvent>();
        private readonly Dictionary<string, List<HandlerInfo>> _entityToMappers = new Dictionary<string, List<HandlerInfo>>();

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
            var matches = await base.Select(context, cancel);
            if (matches.Any())
            {
                // A big issue is that we want multiple firings.  We can get this from quantification, but not arrays.
                // If we have a rule for value ambiguity we would want it to fire for each value ambiguity.
                // Possibly:
                // * Iterate through ambiguous text and run rule?
                // * Iterate through each ambiguous entity and collect firing rules.
                // * Run rules on remaining
                // Prefer handlers by:
                // * Set & Expected slots
                // * Set & Coverage
                // * Set & Priority
                // * Disambiguation & expected
                // * Disambiguation & coverage
                // * Disambiguation & priority
                // * Prompt

                // We have four kinds of ambiguity to deal with:
                // * Value: Ambiguous interpretation of entity value: (peppers -> [green peppers, red peppers]  Tell this by entity value is array.  Doesn't matter if slot singleton or not. Ask.
                // * Text: Ambiguous interpretion of text: (3 -> age or number) Identify by overlapping entities. Resolve by greater coverage, expected entity, ask.
                // * Singelton: two different entities which could fill slot singleton.  Could be same type or different types.  Resolve by rule priority.
                // * Slot: Which slot should an entity go to?  Resolve by expected, then ask.
                // Should rules by over entities directly or should we process them first into these forms?
                // This is also complicated by singleton vs. array
                // It would be nice if multiple entities were rolled up into a single entity, i.e. a toppings composite with topping inside of it.
                // Rule for value ambiguity: foreach(entity in @entity) entity is array.    
                // Rule for text ambiguity: info overlaps...
                // Rule for singleton ambiguity: multiple rules fire over different entities
                // Rule for slot ambiguity: multiple rules fire for same entity
                // Preference is for expected slots
                // Want to write rules that:
                // * Allow mapping a slot through steps.
                // * Allow disambiguation
                // * More specific win from trigger tree
                // * Easy to understand
                // How to deal with multiple entities.
                // * Rules are over them all--some of which have ambiguity
                // * Rules are specific to individual entity.  Easier to write, but less powerful and lots of machinery for singleton/array
                //
                // Key assumptions:
                // * A single entity type maps to a single slot.  Otherwise we have to figure out how to name different entity instances.
                if (AnalyzeEntities(context, out var entityToInfo))
                {
                    // There exist some entities
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
                }
            }

            return matches;
        }

        private bool AnalyzeEntities(SequenceContext context, out Dictionary<string, List<EntityInfo>> entityToInfo)
        {
            entityToInfo = new Dictionary<string, List<EntityInfo>>();
            if (context.State.TryGetValue<dynamic>(DialogContextState.TURN_RECOGNIZED + ".entities", out var entities))
            {
                // TODO: We should have RegexRecognizer return $instance or make this robust to it missing, i.e. assume no entities overlap
                var metaData = entities["$instance"];
                foreach (var entry in entities)
                {
                    var name = entry.Name;
                    if (!name.StartsWith("$"))
                    {
                        var values = entry.Value;
                        var instances = metaData[name];
                        for (var i = 0; i < values.Count; ++i)
                        {
                            var val = values[i];
                            var instance = instances[i];
                            if (!entityToInfo.TryGetValue(name, out List<EntityInfo> infos))
                            {
                                infos = new List<EntityInfo>();
                                entityToInfo[name] = infos;
                            }

                            infos.Add(new EntityInfo(val, (int)instance.startIndex, (int)instance.endIndex, (double)(instance.score ?? 0.0d)));
                        }
                    }
                }
            }

            return entityToInfo.Any();
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
            public EntityInfo(object entity, int start, int end, double score = 0.0)
            {
                Entity = entity;
                Start = start;
                End = end;
                Score = score;
            }

            public object Entity { get; }

            public int Start { get; }

            public int End { get; }

            public double Score { get; }
        }
    }
}
