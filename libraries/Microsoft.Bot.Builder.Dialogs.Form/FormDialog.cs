using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public partial class FormDialog : AdaptiveDialog
    {
        // TODO: This should be wired up to be declarative for the selector and for the schemas
        public FormDialog(DialogSchema schema)
        {
            Schema = schema;
            Selector = new MostSpecificSelector
            {
                Selector = new FirstSelector()
            };
        }

        [JsonProperty("schema")]
        public DialogSchema Schema { get; }

        protected async Task<bool> ProcessFormAsync(SequenceContext sequenceContext, CancellationToken cancellationToken)
        {
            var handled = false;
            var queues = Queues.Read(sequenceContext);
            if (queues.Clear.Any())
            {
                var evt = new DialogEvent() { Name = FormEvents.ClearSlot, Value = queues.Clear.Dequeue(), Bubble = false };
                handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: evt, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else if (queues.Set.Any())
            {
                var evt = new DialogEvent() { Name = FormEvents.SetSlot, Value = queues.Set.Dequeue(), Bubble = false };
                handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: evt, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else if (queues.Unknown.Any())
            {
                var evt = new DialogEvent() { Name = FormEvents.UnknownEntity, Value = queues.Unknown.Dequeue(), Bubble = false };
                handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: evt, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else if (queues.SlotChoices.Any())
            {
                var evt = new DialogEvent() { Name = FormEvents.ChooseSlot, Value = queues.SlotChoices.Dequeue(), Bubble = false };
                handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: evt, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else if (queues.SingletonChoice.Any())
            {
                var evt = new DialogEvent() { Name = FormEvents.ChooseSlotValue, Value = queues.SingletonChoice.Dequeue(), Bubble = false };
                handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: evt, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else if (queues.Clarify.Any())
            {
                var evt = new DialogEvent() { Name = FormEvents.ClarifySlotValue, Value = queues.Clarify.Dequeue(), Bubble = false };
                handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: evt, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (!handled)
            {
                var evt = new DialogEvent() { Name = FormEvents.Ask, Bubble = false };
                handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: evt, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            return handled;
        }

        protected override async Task<bool> ProcessEventAsync(SequenceContext sequenceContext, DialogEvent dialogEvent, bool preBubble, CancellationToken cancellationToken = default)
        {
            var handled = false;
            // Save schema into turn
            sequenceContext.State.SetValue(TurnPath.SCHEMA, this.Schema.Schema);
            if (preBubble)
            {
                switch (dialogEvent.Name)
                {
                    case AdaptiveEvents.RecognizedIntent:
                        {
                            var queues = Queues.Read(sequenceContext);
                            var entities = NormalizeEntities(sequenceContext);
                            var utterance = sequenceContext.Context.Activity?.AsMessageActivity()?.Text;
                            // TODO: Only add utterance if it is in expectedSlots or handle specially when mapping
                            // Build in the whole utterance as a string
                            entities["utterance"] = new List<EntityInfo> { new EntityInfo { Priority = int.MaxValue, Coverage = 1.0, Start = 0, End = utterance.Length, Name = "utterance", Score = 0.0, Type = "string", Entity = utterance, Text = utterance } };
                            var newQueues = new Queues();
                            AssignEntities(entities, newQueues);
                            queues.Merge(newQueues);
                            var turn = sequenceContext.State.GetValue<int>("this.turn");
                            CombineOldSlotMappings(queues, turn);
                            queues.Write(sequenceContext);
                            handled = await ProcessFormAsync(sequenceContext, cancellationToken).ConfigureAwait(false);
                        }

                        break;
                    case FormEvents.FillForm:
                        {
                            handled = await ProcessFormAsync(sequenceContext, cancellationToken).ConfigureAwait(false);
                            break;
                        }

                    default:
                        handled = await base.ProcessEventAsync(sequenceContext, dialogEvent, preBubble, cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
            else
            {
                handled = await base.ProcessEventAsync(sequenceContext, dialogEvent, preBubble, cancellationToken).ConfigureAwait(false);
            }

            return handled;
        }
  
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

        // Combine all the information we have about entities
        private Dictionary<string, List<EntityInfo>> NormalizeEntities(SequenceContext context)
        {
            var entityToInfo = new Dictionary<string, List<EntityInfo>>();

            // TODO: In a multiple event world, we only want to do this at the end of the RecognizedIntent round
            var text = context.State.GetValue<string>(TurnPath.RECOGNIZED + ".text");
            if (context.State.TryGetValue<dynamic>(TurnPath.RECOGNIZED + ".entities", out var entities))
            {
                if (!context.State.TryGetValue<int>("this.turn", out var turn))
                {
                    turn = 0;
                }

                ++turn;
                context.State.SetValue("this.turn", turn);

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
                                Turn = turn,
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
            foreach (var slot in Schema.Property.Children)
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
                            candidates.Add(candidate);
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
                infos.RemoveAll(e => e != entity && e.Overlaps(entity));
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
                                Change = slotOps.First()
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
            var prop = Schema.PathToSchema(path);
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
                SlotQueues(entry.Change.Slot, slotToQueues).Set.Add(entry);
            }

            foreach (var entry in queues.Clarify)
            {
                SlotQueues(entry.Change.Slot, slotToQueues).Clarify.Add(entry);
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

        private void CombineNewSlotMappings(Queues queues)
        {
            var slotToQueues = PerSlotQueues(queues);
            foreach (var entry in slotToQueues)
            {
                var slot = entry.Key;
                var slotQueues = entry.Value;
                if (!slot.IsArray && slotQueues.Set.Count() + slotQueues.Clarify.Count() > 1)
                {
                    // Singleton with multiple operations
                    var mappings = from mapping in slotQueues.Set.Union(slotQueues.Clarify) where mapping.Change.Operation != Operations.Remove select mapping;
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
                                Slot = mappings.First().Change
                            });
                            break;
                    }
                }
            }

            // TODO: There is a lot more we can do here
        }

        private void CombineOldSlotMappings(Queues queues, int turn)
        {
            var slotToQueues = PerSlotQueues(queues);
            foreach (var entry in slotToQueues)
            {
                var slot = entry.Key;
                var slotQueues = entry.Value;
                if (!slot.IsArray &&
                    (slotQueues.Set.Any(e => e.Entity.Turn == turn)
                    || slotQueues.Clarify.Any(e => e.Entity.Turn == turn)
                    || slotQueues.SingletonChoice.Any(c => c.Entities.Any(e => e.Turn == turn))
                    || slotQueues.SlotChoices.Any(c => c.Entity.Turn == turn)))
                {
                    // Remove all old operations on slot because there is a new one
                    foreach (var mapping in slotQueues.Set)
                    {
                        if (mapping.Entity.Turn != turn)
                        {
                            queues.Set.Remove(mapping);
                        }
                    }

                    foreach (var mapping in slotQueues.Clarify)
                    {
                        if (mapping.Entity.Turn != turn)
                        {
                            queues.Clarify.Remove(mapping);
                        }
                    }

                    foreach (var mapping in slotQueues.SingletonChoice)
                    {
                        mapping.Entities.RemoveAll(e => e.Turn != turn);
                        if (mapping.Entities.Count == 0)
                        {
                            slotQueues.SingletonChoice.Remove(mapping);
                        }
                    }

                    foreach (var mapping in slotQueues.SlotChoices)
                    {
                        if (mapping.Entity.Turn != turn)
                        {
                            queues.SlotChoices.Remove(mapping);
                        }
                    }
                }
            }
        }

        // Assign entities to queues
        private void AssignEntities(Dictionary<string, List<EntityInfo>> entities, Queues queues)
        {
            AddToQueues(entities, queues);
            CombineNewSlotMappings(queues);
        }

        public class Queues
        {
            public List<EntityInfo> Unknown { get; } = new List<EntityInfo>();

            public List<SlotMapping> Set { get; } = new List<SlotMapping>();

            public List<SlotMapping> Clarify { get; } = new List<SlotMapping>();

            public List<SingletonChoices> SingletonChoice { get; } = new List<SingletonChoices>();

            public List<SlotChoices> SlotChoices { get; } = new List<SlotChoices>();

            // Slots to clear
            public List<string> Clear { get; } = new List<string>();

            public static Queues Read(SequenceContext context)
            {
                if (!context.State.TryGetValue<Queues>("this.mappings", out var queues))
                {
                    queues = new Queues();
                }

                return queues;
            }

            public void Write(SequenceContext context)
                => context.State.Add("this.mappings", this);

            public void Merge(Queues queues)
            {
                Unknown.AddRange(queues.Unknown);
                Set.AddRange(queues.Set);
                Clarify.AddRange(queues.Clarify);
                SingletonChoice.AddRange(queues.SingletonChoice);
                SlotChoices.AddRange(queues.SlotChoices);
                Clear.AddRange(queues.Clear);
            }
        }

        // For simple singleton slot:
        //  Set values
        //      count(@@foo) == 1 -> foo == @foo
        //      count(@@foo) > 1 -> "Which {@@foo} do you want for {slotName}"
        //  Constraints (which are more specific)
        //      count(@@foo) == 1 && @foo < 0 -> "{@foo} is too small for {slotname}"
        //      count(@@foo) > 1 && count(where(@@foo, foo, foo < 0)) > 0 -> "{where(@@foo, foo, foo < 0)} are too small for {slotname}"
        // For simple array slot:
        //  Set values:
        //      @@foo -> foo = @@foo
        //  Constraints: (which are more specific)
        //      @@foo && count(where(@@foo, foo, foo < 0)) > 0 -> "{where(@@foo, foo, foo < 0) are too small for {slotname}"
        //  Modification--based on intent?
        //      add: @@foo && @intent == add -> Append(@@foo, foo)
        //      // This is to make this more specific than both the simple constraint and the intent
        //      add: @@foo && count(where(@@foo, foo, foo < 0)) > 0 && @intent == add -> "{where(@@foo, foo, foo < 0)} are too small for {slotname}"
        //      delete: @@foo @intent == delete -> Delete(@@foo, foo)
        // For structured singleton slot
        //  count(@@foo) == 1 -> Apply child constraints, i.e. make a new singleton object to apply child property rule sets to it.
        //  count(@@foo) > 1 -> "Which one did you want?" with replacing @@foo with the singleton selection
        //
        // Children slots can either:
        // * Refer to parent structure which turns into count(first(parent).child) == 1
        // * Refer to independent entity, i.e. count(datetime) > 1
        //
        // Assumptions:
        // * In order to map structured entities to structured slots, parent structures must be singletons before child can map them.
        // * We will only generate a single instance of the form.  (Although there can be multiple ones inside.)
        // * You can map directly, but then must deal with that complexity of structures.  For example if you have multiple flight(origin, destination) and you want to map to hotel(location)
        //   you have to figure out how to deal with multiple flight structures and the underlying entity structures.
        // * For now only leaves can be arrays.  If you need more, I think it is a subform, but we could probably automatically generate a foreach step on top.
        //
        // 1) Find all most specific matches
        // 2) Identify any slots that compete for the same entity.  Select by in expected, then keep as slot ambiguous.
        // 3) For each entity either: a) Do its set, b) queue up clarification, c) surface as unhandled
        // 
        // Two cases:
        // 1) Flat entity resolution, treat properties as independent.
        // 2) Hierarchical, the first level you get to count(@@flight) == 1, then for count(first(@@flight).origin) == 1
        // We know which is which by entity path, i.e. flight.origin -> hierarchical whereas origin is flat.
        //
        // In order to robustly handle we need a progression of transformations, i.e. to map @meat to meatSlot singleton:
        // @meat -> meatSlot_choice (m->1) ->
        //                          (1->1) -> foreach meatslot_clarify -> set meat slot (clears others)
        // If we get a new @meat, then it would reset them all.
        // Should this be a flat set of rules?

        // If one @@entity then goes to foreach
    }
}
