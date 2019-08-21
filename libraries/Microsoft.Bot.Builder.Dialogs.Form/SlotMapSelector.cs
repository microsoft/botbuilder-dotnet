using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.TriggerTrees;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;
using Microsoft.Bot.Schema;

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
            //
            // 1) @@entity to entities array
            // 2) Use schema information + expected to assign each entity to one of: choice(slot), clarify(slot), unknown, slots and remove any overlapping entities.
            // 3) Run rules to pick one rule for doing next.  They are in terms of the processing queues and other memory.
            // On the next cycle go ahead and add to process queues
            // Implied in this is that mapping information consists of simple paths to entities.
            // Choice[slot] = [[entity, ...]]
            // Clarify[slot] = [entity, ...]
            // Slots = [{entity, [slots]}]
            // Unknown = [entity, ...]
            // Set = [{entity, slot, op}]
            // For rules, prefer non-forminput, then forminput.
            var entities = NormalizeEntities(context);
            if (entities.Any())
            {
                context.State.Dialog.Add("entities", entities);
            }

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
                //
                // Need to figure out how to handle operations.  They could be done in LUIS as composites which allow putting together multiples ones. 
                // You can imagine doing add/remove, but another scenario would be to do "change seattle to dallas" where you are referring to where 
                // a specific value is found independent of which slot has the value.
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

        // Combine all the information we have about entities
        private Dictionary<string, List<EntityInfo>> NormalizeEntities(SequenceContext context)
        {
            var entityToInfo = new Dictionary<string, List<EntityInfo>>();

            // TODO: In a multiple event world, we only want to do this at the end of the RecognizedIntent round
            var text = (string)context.State.GetValue(DialogContextState.TURN_RECOGNIZED + ".text");
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

                            var info = new EntityInfo
                            {
                                Name = name,
                                Entity = val,
                                Start = (int)instance.startIndex,
                                End = (int)instance.endIndex,
                                Text = (string)instance.text,
                                Type = (string)instance.type,
                                Role = (string)instance.role,
                                Score = (double)(instance.score ?? 0.0d),
                            };
                            // Eventually this could be passed in
                            info.Priority = info.Role == null ? 1 : 0;
                            info.Coverage = (info.End - info.Start) / (double)text.Length;
                            infos.Add(info);
                        }
                    }
                }
            }

            return entityToInfo;
        }

        // Generate possible entity to slot mappings
        private IReadOnlyList<SlotEntityInfo> Candidates(Dictionary<string, List<EntityInfo>> entities)
        {
            var candidates = new List<SlotEntityInfo>();
            foreach (var slot in _schema.Property.Children)
            {
                foreach (var mapping in slot.Mappings)
                {
                    if (entities.TryGetValue(mapping, out var possiblities))
                    {
                        foreach (var possible in possiblities)
                        {
                            var candidate = new SlotEntityInfo
                            {
                                Entity = possible,
                                Slot = slot,
                            };
                        }
                    }
                }
            }

            return candidates;
        }

        private void AddMappingToQueue(SlotMapping mapping, Queues queues)
        {
            if (mapping.Entity.Entity is Array arr && arr.Length > 1)
            {
                queues.Clarify.Add(mapping);
            }
            else
            {
                queues.Set.Add(mapping);
            }
        }

        // Remove any entities that overlap a selected entity
        private void RemoveOverlappingEntities(EntityInfo entity, Dictionary<string, List<EntityInfo>> entities)
        {
            foreach (var infos in entities.Values)
            {
                infos.RemoveAll(e => e.Overlaps(entity));
            }
        }

        private void AddToQueues(Dictionary<string, List<EntityInfo>> entities, Queues queues)
        {
            var candidates = Candidates(entities);

            // Group by specific entity order by priority + coverage then by slots across expected/unexpected
            // Captures the intuition that more specific entities or larger entities are preferred
            var choices = from candidate in candidates
                          group candidate by candidate.Entity into entityGroup
                          orderby entityGroup.Key.Priority ascending, entityGroup.Key.Coverage descending
                          select new
                          {
                              entityGroup.Key,
                              slots = from slot in entityGroup
                                      group slot by slot.Expected into expected
                                      orderby expected.Key descending
                                      select expected
                          };

            foreach (var entityChoices in choices)
            {
                var entity = entityChoices.Key;
                if (entities.TryGetValue(entity.Name, out var entityInfos) && entityInfos.Contains(entity))
                {
                    RemoveOverlappingEntities(entity, entities);
                    foreach (var entitySlots in entityChoices.slots)
                    {
                        var slotOps = from entitySlot in entitySlots
                                      select new SlotOp
                                      {
                                          // TODO: Figure out operation from role?
                                          // If composite do we have role:value, add{role:value}, remove{role:value}, add{type:value}, changeValue, ...
                                          // Would be nice if extensible...
                                          Operation = Operations.Add,
                                          Slot = entitySlot.Slot.Path
                                      };
                        if (entitySlots.Count() == 1)
                        {
                            // Only a single slot consumes entity
                            var slotEntity = entitySlots.First();
                            var slot = slotEntity.Slot;
                            // Send to clarify or set
                            var mapping = new SlotMapping
                            {
                                Entity = entity,
                                Slot = slotOps.First()
                            };
                            AddMappingToQueue(mapping, queues);
                        }
                        else
                        {
                            // Multiple slots want the same entity
                            queues.SlotChoices.Add(new SlotChoices
                            {
                                Entity = entity,
                                Slots = slotOps.ToList()
                            });
                        }
                    }
                }
            }

            // Collect unknown entities
            foreach (var infos in entities.Values)
            {
                foreach (var info in infos)
                {
                    queues.Unknown.Add(info);
                }
            }
        }

        private Queues SlotQueues(string path, Dictionary<PropertySchema, Queues> slotToQueues)
        {
            var prop = _schema.PathToSchema(path);
            if (!slotToQueues.TryGetValue(prop, out var slotQueues))
            {
                slotQueues = new Queues();
                slotToQueues[prop] = slotQueues;
            }
            return slotQueues;
        }

        // Create queues for each slot
        private Dictionary<PropertySchema, Queues> PerSlotQueues(Queues queues)
        {
            var slotToQueues = new Dictionary<PropertySchema, Queues>();
            foreach (var entry in queues.Set)
            {
                SlotQueues(entry.Slot.Slot, slotToQueues).Set.Add(entry);
            }

            foreach (var entry in queues.Clarify)
            {
                SlotQueues(entry.Slot.Slot, slotToQueues).Clarify.Add(entry);
            }

            foreach (var entry in queues.SingletonChoice)
            {
                SlotQueues(entry.Slot.Slot, slotToQueues).SingletonChoice.Add(entry);
            }

            foreach (var entry in queues.Clear)
            {
                SlotQueues(entry, slotToQueues).Clear.Add(entry);
            }

            foreach (var entry in queues.SlotChoices)
            {
                foreach (var slot in entry.Slots)
                {
                    SlotQueues(slot.Slot, slotToQueues).SlotChoices.Add(entry);
                }
            }

            return slotToQueues;
        }

        private void AnalyzeQueues(Queues queues)
        {
            var slotToQueues = PerSlotQueues(queues);
            foreach (var entry in slotToQueues)
            {
                var slot = entry.Key;
                var slotQueues = entry.Value;
                if (!slot.IsArray && slotQueues.Set.Count() + slotQueues.Clarify.Count() > 1)
                {
                    // Singleton with multiple operations
                    var mappings = from mapping in slotQueues.Set.Union(slotQueues.Clarify) where mapping.Slot.Operation != Operations.Remove select mapping;
                    switch (mappings.Count())
                    {
                        case 0:
                            queues.Clear.Add(slot.Path);
                            break;
                        case 1:
                            AddMappingToQueue(mappings.First(), queues);
                            break;
                        default:
                            queues.SingletonChoice.Add(new SingletonChoices
                            {
                                Entities = (from mapping in mappings select mapping.Entity).ToList(),
                                Slot = mappings.First().Slot
                            });
                            break;
                    }
                }
            }

            // TODO: There is a lot more we can do here
        }

        // Assign entities to queues
        private void AssignEntities(Dictionary<string, List<EntityInfo>> entities, Queues queues)
        {
            AddToQueues(entities, queues);
            AnalyzeQueues(queues);
        }

        // Proposed mapping
        private class SlotEntityInfo
        {
            public PropertySchema Slot { get; set; }

            public EntityInfo Entity { get; set; }

            public bool Expected { get; set; }
        }

        // Slot and operation
        private class SlotOp
        {
            public string Slot { get; set; }

            public string Operation { get; set; }
        }

        // Simple mapping
        private class SlotMapping
        {
            public SlotOp Slot { get; set; }

            public EntityInfo Entity { get; set; }
        }

        // Select from multiple entities for singleton
        private class SingletonChoices
        {
            public List<EntityInfo> Entities { get; set; } = new List<EntityInfo>();

            public SlotOp Slot { get; set; }
        }

        // Select which slot entity belongs to
        private class SlotChoices
        {
            public List<SlotOp> Slots { get; set; } = new List<SlotOp>();

            public EntityInfo Entity { get; set; }
        }

        private class Queues
        {
            public List<EntityInfo> Unknown { get; } = new List<EntityInfo>();

            public List<SlotMapping> Set { get; } = new List<SlotMapping>();

            public List<SlotMapping> Clarify { get; } = new List<SlotMapping>();

            public List<SingletonChoices> SingletonChoice { get; } = new List<SingletonChoices>();

            public List<SlotChoices> SlotChoices { get; } = new List<SlotChoices>();

            // Slots to clear
            public List<string> Clear { get; } = new List<string>();
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

                    if (action is Ask)
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
            public string Name { get; set; }

            public object Entity { get; set; }

            public int Start { get; set; }

            public int End { get; set; }

            public double Score { get; set; }

            public string Text { get; set; }

            public string Role { get; set; }

            public string Type { get; set; }

            public int Priority { get; set; }

            public double Coverage { get; set; }

            public bool Overlaps(EntityInfo entity)
                => Start <= entity.End && End >= entity.Start;
        }
    }
}
