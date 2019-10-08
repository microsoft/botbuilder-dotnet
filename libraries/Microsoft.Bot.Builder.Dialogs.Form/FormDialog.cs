// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Microsoft.Bot.Builder.Dialogs.Form.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            DialogEvent evt;
            var queues = EventQueues.Read(sequenceContext);
            var changed = queues.DequeueEvent(sequenceContext.State.GetValue<string>("dialog.lastEvent"));
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

                sequenceContext.State.SetValue($"{TurnPath.RECOGNIZED}.entities.{val.Entity.Name}", entity);
            }
            else if (queues.UnknownEntity.Any())
            {
                evt = new DialogEvent() { Name = FormEvents.UnknownEntity, Value = queues.UnknownEntity[0], Bubble = false };
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

            sequenceContext.State.SetValue($"dialog.lastEvent", evt.Name);
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

        // ChooseEntity: Which entity for a particular property.
        // TODO: I think we can remove this one--a property can decide which entity interpretation to use.
        //
        // ChooseMapping: Which property should consume alternative entities.
        // By "none" do you mean meat or cheese.
        // Expected: one of property names in order to resolve
        // 
        // ChooseProperty: Which property should consume an entity.
        // By "seattle" did you mean an origin or destination?
        // TODO: I think we can remove this.  From a property standpoint it looks just like ChooseMapping.
        // 
        // ClarifyEntity: Clarify which entity value is intended.
        // By "peppers" did you mean green peppers or red peppers?
        // Expected: entity value for filling a particular property
        // 
        // ClearProperty: Clear a particular property.
        // TODO: This is probably created by a composite remove.
        //
        // SetProperty: Set a property to an entity.
        // No expectations
        //
        // UnknownEntity: Do not know how to map entity.
        //
        // Expected is true if property is in expectedProperties or it has the appropriate value from above.
        protected override async Task<bool> ProcessEventAsync(SequenceContext sequenceContext, DialogEvent dialogEvent, bool preBubble, CancellationToken cancellationToken = default)
        {
            bool handled;
            // Save schema into turn
            sequenceContext.State.SetValue(TurnPath.SCHEMA, this.Schema.Schema);
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
            if (!context.State.TryGetValue<string[]>("$expectedProperties", out var properties))
            {
                properties = new string[0];
            }

            if (properties.Contains("utterance"))
            {
                entities["utterance"] = new List<EntityInfo> { new EntityInfo { Priority = int.MaxValue, Coverage = 1.0, Start = 0, End = utterance.Length, Name = "utterance", Score = 0.0, Type = "string", Value = utterance, Text = utterance } };
            }

            UpdateLastEvent(context, queues, entities);
            var newQueues = new EventQueues();
            AssignEntities(entities, properties, newQueues);
            queues.Merge(newQueues);
            var turn = context.State.GetValue<int>("this.turn");
            CombineOldEntityToPropertys(queues, turn);
            queues.Write(context);
        }

        private void UpdateLastEvent(SequenceContext context, EventQueues queues, Dictionary<string, List<EntityInfo>> entities)
        {
            if (context.State.TryGetValue<string>("dialog.lastEvent", out var evt))
            {
                switch (evt)
                {
                    case FormEvents.ClarifyEntity:
                        {
                            context.State.RemoveValue("dialog.lastEvent");
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
                            context.State.RemoveValue("dialog.lastEvent");
                            // NOTE: This assumes the existance of a property entity which contains the normalized
                            // names of the properties.
                            if (entities.TryGetValue("PROPERTYNAME", out var infos) && infos.Count() == 1)
                            {
                                var info = infos[0];
                                var choices = queues.ChooseProperty[0];
                                var choice = choices.Find(p => p.Property == (info.Value as JArray)[0].ToObject<string>());
                                if (choice != null)
                                {
                                    // Resolve and move to SetProperty
                                    infos.Clear();
                                    queues.ChooseProperty.Dequeue();
                                    choice.Expected = true;
                                    queues.SetProperty.Add(choice);
                                    // TODO: This seems a little draconian, but we don't want property names to trigger help
                                    context.State.SetValue("turn.recognized.intent", "None");
                                    context.State.SetValue("turn.recognized.score", 1.0);
                                }
                            }

                            break;
                        }
                }
            }
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
            foreach (var schema in Schema.Property.Children)
            {
                foreach (var entityName in schema.Mappings)
                {
                    if (entities.TryGetValue(entityName, out var matches))
                    {
                        foreach (var entity in matches)
                        {
                            yield return new EntityToProperty
                            {
                                Entity = entity,
                                Schema = schema,
                                Property = schema.Path,
                                // TODO: Eventually we should be able to pick up an add/remove composite here as an alternative
                                Operation = Operations.Add,
                                Expected = expected.Contains(schema.Name)
                            };
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
                    } while (candidate != null);
                }
            }
        }

        private void AddToQueues(Dictionary<string, List<EntityInfo>> entities, string[] expected, EventQueues queues)
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

                if (alternatives.Count() == 1)
                {
                    AddMappingToQueue(candidate, queues);
                }
                else
                {
                    queues.ChooseProperty.Add(alternatives);
                }
            }

            // Collect unknown entities
            foreach (var entity in entities.Values.SelectMany(e => e))
            {
                if (!usedEntities.Contains(entity))
                {
                    queues.UnknownEntity.Add(entity);
                }
            }
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

        private void CombineOldEntityToPropertys(EventQueues queues, int turn)
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
        private void AssignEntities(Dictionary<string, List<EntityInfo>> entities, string[] expected, EventQueues queues)
        {
            AddToQueues(entities, expected, queues);
            CombineNewEntityToPropertys(queues);
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
