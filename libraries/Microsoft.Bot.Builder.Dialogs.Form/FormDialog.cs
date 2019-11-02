// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Dialogs.Form.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    /// <summary>
    /// Form dialog.
    /// </summary>
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
            DialogEvent evt;
            var queues = EventQueues.Read(sequenceContext);
            var changed = queues.DequeueEvent(sequenceContext.GetState().GetValue<string>(DialogPath.LastEvent));
            if (queues.ClearProperty.Any())
            {
                evt = new DialogEvent() { Name = FormEvents.ClearProperty, Value = queues.ClearProperty[0], Bubble = false };
            }
            else if (queues.SetProperty.Any())
            {
                var val = queues.SetProperty[0];
                evt = new DialogEvent() { Name = FormEvents.SetProperty, Value = val, Bubble = false };

                // TODO: For now, I'm going to dereference to a one-level array value.  There is a bug in the current code in the distinction between
                // @ which is supposed to unwrap down to non-array and @@ which returns the whole thing. @ in the curent code works by doing [0] which
                // is not enough.
                var entity = val.Entity.Value;
                if (!(entity is JArray))
                {
                    entity = new object[] { entity };
                }

                sequenceContext.GetState().SetValue($"{TurnPath.RECOGNIZED}.entities.{val.Entity.Name}", entity);
            }
            else if (queues.ChooseProperty.Any())
            {
                evt = new DialogEvent() { Name = FormEvents.ChooseProperty, Value = queues.ChooseProperty[0], Bubble = false };
            }
            else if (queues.ClarifyEntity.Any())
            {
                evt = new DialogEvent() { Name = FormEvents.ClarifyEntity, Value = queues.ClarifyEntity[0], Bubble = false };
            }
            else
            {
                evt = new DialogEvent() { Name = FormEvents.Ask, Bubble = false };
            }

            if (changed)
            {
                queues.Write(sequenceContext);
            }

            sequenceContext.GetState().SetValue(DialogPath.LastEvent, evt.Name);
            var handled = await this.ProcessEventAsync(sequenceContext, dialogEvent: evt, preBubble: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!handled)
            {
                // If event wasn't handled, remove it from queues and keep going if things changed
                if (queues.DequeueEvent(evt.Name))
                {
                    queues.Write(sequenceContext);
                    handled = await this.ProcessFormAsync(sequenceContext, cancellationToken);
                }
            }

            return handled;
        }

        protected override async Task<bool> ProcessEventAsync(SequenceContext sequenceContext, DialogEvent dialogEvent, bool preBubble, CancellationToken cancellationToken = default)
        {
            bool handled;

            // Save schema into turn
            sequenceContext.GetState().SetValue(TurnPath.SCHEMA, this.Schema.Schema);
            if (!sequenceContext.GetState().ContainsKey(DialogPath.RequiredProperties))
            {
                // All properties required by default unless specified.
                sequenceContext.GetState().SetValue(DialogPath.RequiredProperties, this.Schema.Required());
            }

            if (preBubble)
            {
                switch (dialogEvent.Name)
                {
                    case AdaptiveEvents.RecognizedIntent:
                        {
                            ProcessEntities(sequenceContext);
                            handled = await base.ProcessEventAsync(sequenceContext, dialogEvent, preBubble, cancellationToken);
                        }

                        break;

                    case AdaptiveEvents.EndOfActions:
                        // Completed actions so continue processing form queues
                        handled = await ProcessFormAsync(sequenceContext, cancellationToken).ConfigureAwait(false);
                        break;

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

        private void ProcessEntities(SequenceContext context)
        {
            var queues = EventQueues.Read(context);
            var entities = NormalizeEntities(context);
            var utterance = context.Context.Activity?.AsMessageActivity()?.Text;
            if (!context.GetState().TryGetValue<string[]>("$expectedProperties", out var expected))
            {
                expected = new string[0];
            }

            if (expected.Contains("utterance"))
            {
                entities["utterance"] = new List<EntityInfo> { new EntityInfo { Priority = int.MaxValue, Coverage = 1.0, Start = 0, End = utterance.Length, Name = "utterance", Score = 0.0, Type = "string", Value = utterance, Text = utterance } };
            }

            var updated = UpdateLastEvent(context, queues, entities);
            var newQueues = new EventQueues();
            var recognized = AssignEntities(entities, expected, newQueues);
            var unrecognized = SplitUtterance(utterance, recognized);
            recognized.AddRange(updated);

            context.GetState().SetValue(TurnPath.UNRECOGNIZEDTEXT, unrecognized);
            context.GetState().SetValue(TurnPath.RECOGNIZEDENTITIES, recognized);

            // turn.unrecognizedText = [<text not consumed by entities>]
            // turn.consumedEntities = [entityInfo] 
            queues.Merge(newQueues);
            var turn = context.GetState().GetValue<uint>(DialogPath.EventCounter);
            CombineOldEntityToPropertys(queues, turn);
            queues.Write(context);
        }

        private List<string> SplitUtterance(string utterance, List<EntityInfo> recognized)
        {
            var unrecognized = new List<string>();
            var current = 0;
            foreach (var entity in recognized)
            {
                if (entity.Start > current)
                {
                    unrecognized.Add(utterance.Substring(current, entity.Start - current).Trim());
                }

                current = entity.End;
            }

            if (current < utterance.Length)
            {
                unrecognized.Add(utterance.Substring(current));
            }

            return unrecognized;
        }

        private List<EntityInfo> UpdateLastEvent(SequenceContext context, EventQueues queues, Dictionary<string, List<EntityInfo>> entities)
        {
            var recognized = new List<EntityInfo>();
            if (context.GetState().TryGetValue<string>(DialogPath.LastEvent, out var evt))
            {
                switch (evt)
                {
                    case FormEvents.ClarifyEntity:
                        {
                            context.GetState().RemoveValue(DialogPath.LastEvent);
                            var entityToProperty = queues.ClarifyEntity[0];
                            var ambiguousEntity = entityToProperty.Entity;
                            var choices = ambiguousEntity.Value as JArray;

                            // TODO: There could be no way to resolve the ambiguity, i.e. wheat has synonym wheat and
                            // honeywheat has synonym wheat.  For now rely on the model to not have that issue.
                            if (entities.TryGetValue(ambiguousEntity.Name, out var infos) && infos.Count() == 1)
                            {
                                var info = infos.First();
                                var foundValues = info.Value as JArray;
                                var common = choices.Intersect(foundValues);
                                if (common.Count() == 1)
                                {
                                    // Resolve and move to SetProperty
                                    recognized.Add(info);
                                    infos.Clear();
                                    entityToProperty.Entity = info;
                                    entityToProperty.Expected = true;
                                    queues.ClarifyEntity.Dequeue();
                                    queues.SetProperty.Add(entityToProperty);
                                }
                            }

                            break;
                        }

                    case FormEvents.ChooseProperty:
                        {
                            context.GetState().RemoveValue(DialogPath.LastEvent);

                            // NOTE: This assumes the existance of a property entity which contains the normalized
                            // names of the properties.
                            if (entities.TryGetValue("PROPERTYName", out var infos) && infos.Count() == 1)
                            {
                                var info = infos[0];
                                var choices = queues.ChooseProperty[0];
                                var choice = choices.Find(p => p.Property == (info.Value as JArray)[0].ToObject<string>());
                                if (choice != null)
                                {
                                    // Resolve and move to SetProperty
                                    recognized.Add(info);
                                    infos.Clear();
                                    queues.ChooseProperty.Dequeue();
                                    choice.Expected = true;
                                    queues.SetProperty.Add(choice);

                                    // TODO: This seems a little draconian, but we don't want property names to trigger help
                                    context.GetState().SetValue("turn.recognized.intent", "None");
                                    context.GetState().SetValue("turn.recognized.score", 1.0);
                                }
                            }

                            break;
                        }
                }
            }

            return recognized;
        }

        // A big issue is that we want multiple firings.  We can get this from quantification, but not arrays.
        // If we have a rule for value ambiguity we would want it to fire for each value ambiguity.
        // Possibly:
        // * Iterate through ambiguous text and run rule?
        // * Iterate through each ambiguous entity and collect firing rules.
        // * Run rules on remaining
        // Prefer handlers by:
        // * Set & Expected propertys
        // * Set & Coverage
        // * Set & Priority
        // * Disambiguation & expected
        // * Disambiguation & coverage
        // * Disambiguation & priority
        // * Prompt

        // We have four kinds of ambiguity to deal with:
        // * Value: Ambiguous interpretation of entity value: (peppers -> [green peppers, red peppers]  Tell this by entity value is array.  Doesn't matter if property singleton or not. Ask.
        // * Text: Ambiguous interpretion of text: (3 -> age or number) Identify by overlapping entities. Resolve by greater coverage, expected entity, ask.
        // * Singelton: two different entities which could fill property singleton.  Could be same type or different types.  Resolve by rule priority.
        // * Slot: Which property should an entity go to?  Resolve by expected, then ask.
        // Should rules by over entities directly or should we process them first into these forms?
        // This is also complicated by singleton vs. array
        // It would be nice if multiple entities were rolled up into a single entity, i.e. a toppings composite with topping inside of it.
        // Rule for value ambiguity: foreach(entity in @entity) entity is array.    
        // Rule for text ambiguity: info overlaps...
        // Rule for singleton ambiguity: multiple rules fire over different entities
        // Rule for property ambiguity: multiple rules fire for same entity
        // Preference is for expected properties
        // Want to write rules that:
        // * Allow mapping a property through steps.
        // * Allow disambiguation
        // * More specific win from trigger tree
        // * Easy to understand
        // How to deal with multiple entities.
        // * Rules are over them all--some of which have ambiguity
        // * Rules are specific to individual entity.  Easier to write, but less powerful and lots of machinery for singleton/array
        //
        // Key assumptions:
        // * A single entity type maps to a single property.  Otherwise we have to figure out how to name different entity instances.
        //
        // Need to figure out how to handle operations.  They could be done in LUIS as composites which allow putting together multiples ones. 
        // You can imagine doing add/remove, but another scenario would be to do "change seattle to dallas" where you are referring to where 
        // a specific value is found independent of which property has the value.
        //
        // 1) @@entity to entities array
        // 2) Use schema information + expected to assign each entity to one of: choice(property), clarify(property), unknown, properties and remove any overlapping entities.
        // 3) Run rules to pick one rule for doing next.  They are in terms of the processing queues and other memory.
        // On the next cycle go ahead and add to process queues
        // Implied in this is that mapping information consists of simple paths to entities.
        // Choice[property] = [[entity, ...]]
        // Clarify[property] = [entity, ...]
        // Slots = [{entity, [properties]}]
        // Unknown = [entity, ...]
        // Set = [{entity, property, op}]
        // For rules, prefer non-forminput, then forminput.

        // Combine all the information we have about entities
        private Dictionary<string, List<EntityInfo>> NormalizeEntities(SequenceContext context)
        {
            var entityToInfo = new Dictionary<string, List<EntityInfo>>();
            var text = context.GetState().GetValue<string>(TurnPath.RECOGNIZED + ".text");
            if (context.GetState().TryGetValue<dynamic>(TurnPath.RECOGNIZED + ".entities", out var entities))
            {
                // TODO: We should have RegexRecognizer return $instance or make this robust to it missing, i.e. assume no entities overlap
                var turn = context.GetState().GetValue<uint>(DialogPath.EventCounter);
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
                                Value = val,
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

            // TODO: This should not be necessary--LUIS should be picking the maximal match
            foreach (var infos in entityToInfo.Values)
            {
                infos.Sort((e1, e2) =>
                {
                    var val = 0;
                    if (e1.Start == e2.Start)
                    {
                        if (e1.End > e2.End)
                        {
                            val = -1;
                        }
                        else if (e1.End < e2.End)
                        {
                            val = +1;
                        }
                    }
                    else if (e1.Start < e2.Start)
                    {
                        val = -1;
                    }
                    else
                    {
                        val = +1;
                    }

                    return val;
                });
                for (var i = 0; i < infos.Count(); ++i)
                {
                    var current = infos[i];
                    for (var j = i + 1; j < infos.Count();)
                    {
                        var alt = infos[j];
                        if (current.Covers(alt))
                        {
                            _ = infos.Remove(alt);
                        }
                        else
                        {
                            ++j;
                        }
                    }
                }
            }

            return entityToInfo;
        }

        // Generate possible entity to property mappings
        private IEnumerable<EntityToProperty> Candidates(Dictionary<string, List<EntityInfo>> entities, string[] expected)
        {
            var expectedOnly = Schema.Schema["$expectedOnly"]?.ToObject<List<string>>() ?? new List<string>();
            foreach (var propSchema in Schema.Property.Children)
            {
                var isExpected = expected.Contains(propSchema.Path);
                if (isExpected || !expectedOnly.Contains(propSchema.Path))
                {
                    foreach (var entityName in propSchema.Mappings)
                    {
                        if (entities.TryGetValue(entityName, out var matches))
                        {
                            foreach (var entity in matches)
                            {
                                yield return new EntityToProperty
                                {
                                    Entity = entity,
                                    Schema = propSchema,
                                    Property = propSchema.Path,

                                    // TODO: Eventually we should be able to pick up an add/remove composite here as an alternative
                                    Operation = Operations.Add,
                                    Expected = isExpected
                                };
                            }
                        }
                    }
                }
            }
        }

        private void AddMappingToQueue(EntityToProperty mapping, EventQueues queues)
        {
            if (mapping.Entity.Value is JArray arr)
            {
                if (arr.Count > 1)
                {
                    queues.ClarifyEntity.Add(mapping);
                }
                else
                {
                    mapping.Entity.Value = arr[0];
                    queues.SetProperty.Add(mapping);
                }
            }
            else
            {
                queues.SetProperty.Add(mapping);
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

        // Have each property pick which overlapping entity is the best one
        private IEnumerable<EntityToProperty> RemoveOverlappingPerProperty(IEnumerable<EntityToProperty> candidates)
        {
            var perProperty = from candidate in candidates
                              group candidate by candidate.Schema;
            foreach (var propChoices in perProperty)
            {
                var schema = propChoices.Key;
                var choices = propChoices.ToList();

                // Assume preference by order listed in mappings
                // Alternatives would be to look at coverage or other metrices
                foreach (var entity in schema.Mappings)
                {
                    EntityToProperty candidate;
                    do
                    {
                        candidate = null;
                        foreach (var mapping in choices)
                        {
                            if (mapping.Entity.Name == entity)
                            {
                                candidate = mapping;
                                break;
                            }
                        }

                        if (candidate != null)
                        {
                            // Remove any overlapping entities
                            choices.RemoveAll(choice => choice.Entity.Overlaps(candidate.Entity));
                            yield return candidate;
                        }
                    }
                    while (candidate != null);
                }
            }
        }

        private List<EntityInfo> AddToQueues(Dictionary<string, List<EntityInfo>> entities, string[] expected, EventQueues queues)
        {
            var candidates = (from candidate in RemoveOverlappingPerProperty(Candidates(entities, expected))
                              orderby candidate.Expected descending
                              select candidate).ToList();
            var usedEntities = new HashSet<EntityInfo>(from candidate in candidates select candidate.Entity);
            while (candidates.Any())
            {
                var candidate = candidates.First();
                var alternatives = (from alt in candidates where candidate.Entity.Overlaps(alt.Entity) select alt).ToList();
                candidates = candidates.Except(alternatives).ToList();
                if (candidate.Expected)
                {
                    // If expected binds entity, drop alternatives
                    alternatives.RemoveAll(a => !a.Expected);
                }

                foreach (var alternative in alternatives)
                {
                    usedEntities.Add(alternative.Entity);
                }

                if (alternatives.Count() == 1)
                {
                    AddMappingToQueue(candidate, queues);
                }
                else
                {
                    queues.ChooseProperty.Add(alternatives);
                }
            }

            return (from entity in usedEntities orderby entity.Start ascending select entity).ToList();
        }

        private EventQueues PropertyQueues(string path, Dictionary<PropertySchema, EventQueues> slotToQueues)
        {
            var prop = Schema.PathToSchema(path);
            if (!slotToQueues.TryGetValue(prop, out var slotQueues))
            {
                slotQueues = new EventQueues();
                slotToQueues[prop] = slotQueues;
            }

            return slotQueues;
        }

        // Create queues for each property
        private Dictionary<PropertySchema, EventQueues> PerPropertyQueues(EventQueues queues)
        {
            var propertyToQueues = new Dictionary<PropertySchema, EventQueues>();
            foreach (var entry in queues.SetProperty)
            {
                PropertyQueues(entry.Property, propertyToQueues).SetProperty.Add(entry);
            }

            foreach (var entry in queues.ClarifyEntity)
            {
                PropertyQueues(entry.Property, propertyToQueues).ClarifyEntity.Add(entry);
            }

            foreach (var entry in queues.ClearProperty)
            {
                PropertyQueues(entry, propertyToQueues).ClearProperty.Add(entry);
            }

            foreach (var entry in queues.ChooseProperty)
            {
                foreach (var choice in entry)
                {
                    PropertyQueues(choice.Property, propertyToQueues).ChooseProperty.Add(entry);
                }
            }

            return propertyToQueues;
        }

        private void CombineNewEntityToPropertys(EventQueues queues)
        {
            var slotToQueues = PerPropertyQueues(queues);
            foreach (var entry in slotToQueues)
            {
                var property = entry.Key;
                var slotQueues = entry.Value;
                if (!property.IsArray && slotQueues.SetProperty.Count() + slotQueues.ClarifyEntity.Count() > 1)
                {
                    // Singleton with multiple operations
                    var mappings = from mapping in slotQueues.SetProperty.Union(slotQueues.ClarifyEntity) where mapping.Operation != Operations.Remove select mapping;
                    switch (mappings.Count())
                    {
                        case 0:
                            queues.ClearProperty.Add(property.Path);
                            break;
                        case 1:
                            AddMappingToQueue(mappings.First(), queues);
                            break;
                        default:
                            // TODO: Map to multiple entity to property
                            /* queues.ChooseProperty.Add(new EntitiesToProperty
                            {
                                Entities = (from mapping in mappings select mapping.Entity).ToList(),
                                Property = mappings.First().Change
                            }); */
                            break;
                    }
                }
            }

            // TODO: There is a lot more we can do here
        }

        private void CombineOldEntityToPropertys(EventQueues queues, uint turn)
        {
            var slotToQueues = PerPropertyQueues(queues);
            foreach (var entry in slotToQueues)
            {
                var property = entry.Key;
                var slotQueues = entry.Value;
                if (!property.IsArray &&
                    (slotQueues.SetProperty.Any(e => e.Entity.Turn == turn)
                    || slotQueues.ClarifyEntity.Any(e => e.Entity.Turn == turn)
                    || slotQueues.ChooseProperty.Any(c => c.First().Entity.Turn == turn)))
                {
                    // Remove all old operations on property because there is a new one
                    foreach (var mapping in slotQueues.SetProperty)
                    {
                        if (mapping.Entity.Turn != turn)
                        {
                            queues.SetProperty.Remove(mapping);
                        }
                    }

                    foreach (var mapping in slotQueues.ClarifyEntity)
                    {
                        if (mapping.Entity.Turn != turn)
                        {
                            queues.ClarifyEntity.Remove(mapping);
                        }
                    }

                    foreach (var mapping in slotQueues.ChooseProperty)
                    {
                        if (mapping.First().Entity.Turn != turn)
                        {
                            queues.ChooseProperty.Remove(mapping);
                        }
                    }
                }
            }
        }

        // Assign entities to queues
        private List<EntityInfo> AssignEntities(Dictionary<string, List<EntityInfo>> entities, string[] expected, EventQueues queues)
        {
            var recognized = AddToQueues(entities, expected, queues);
            CombineNewEntityToPropertys(queues);
            return recognized;
        }

        // For simple singleton property:
        //  Set values
        //      count(@@foo) == 1 -> foo == @foo
        //      count(@@foo) > 1 -> "Which {@@foo} do you want for {slotName}"
        //  Constraints (which are more specific)
        //      count(@@foo) == 1 && @foo < 0 -> "{@foo} is too small for {slotname}"
        //      count(@@foo) > 1 && count(where(@@foo, foo, foo < 0)) > 0 -> "{where(@@foo, foo, foo < 0)} are too small for {slotname}"
        // For simple array property:
        //  Set values:
        //      @@foo -> foo = @@foo
        //  Constraints: (which are more specific)
        //      @@foo && count(where(@@foo, foo, foo < 0)) > 0 -> "{where(@@foo, foo, foo < 0) are too small for {slotname}"
        //  Modification--based on intent?
        //      add: @@foo && @intent == add -> Append(@@foo, foo)
        //      // This is to make this more specific than both the simple constraint and the intent
        //      add: @@foo && count(where(@@foo, foo, foo < 0)) > 0 && @intent == add -> "{where(@@foo, foo, foo < 0)} are too small for {slotname}"
        //      delete: @@foo @intent == delete -> Delete(@@foo, foo)
        // For structured singleton property
        //  count(@@foo) == 1 -> Apply child constraints, i.e. make a new singleton object to apply child property rule sets to it.
        //  count(@@foo) > 1 -> "Which one did you want?" with replacing @@foo with the singleton selection
        //
        // Children properties can either:
        // * Refer to parent structure which turns into count(first(parent).child) == 1
        // * Refer to independent entity, i.e. count(datetime) > 1
        //
        // Assumptions:
        // * In order to map structured entities to structured properties, parent structures must be singletons before child can map them.
        // * We will only generate a single instance of the form.  (Although there can be multiple ones inside.)
        // * You can map directly, but then must deal with that complexity of structures.  For example if you have multiple flight(origin, destination) and you want to map to hotel(location)
        //   you have to figure out how to deal with multiple flight structures and the underlying entity structures.
        // * For now only leaves can be arrays.  If you need more, I think it is a subform, but we could probably automatically generate a foreach step on top.
        //
        // 1) Find all most specific matches
        // 2) Identify any properties that compete for the same entity.  Select by in expected, then keep as property ambiguous.
        // 3) For each entity either: a) Do its set, b) queue up clarification, c) surface as unhandled
        // 
        // Two cases:
        // 1) Flat entity resolution, treat properties as independent.
        // 2) Hierarchical, the first level you get to count(@@flight) == 1, then for count(first(@@flight).origin) == 1
        // We know which is which by entity path, i.e. flight.origin -> hierarchical whereas origin is flat.
        //
        // In order to robustly handle we need a progression of transformations, i.e. to map @meat to meatSlot singleton:
        // @meat -> meatSlot_choice (m->1) ->
        //                          (1->1) -> foreach meatslot_clarify -> set meat property (clears others)
        // If we get a new @meat, then it would reset them all.
        // Should this be a flat set of rules?

        // If one @@entity then goes to foreach
    }
}
