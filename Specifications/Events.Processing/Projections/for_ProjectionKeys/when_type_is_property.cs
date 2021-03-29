using System;
// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events.Store.Streams;
using Dolittle.Runtime.Projections.Store;
using Dolittle.Runtime.Projections.Store.Definition;
using Machine.Specifications;

namespace Dolittle.Runtime.Events.Processing.Projections.for_ProjectionKeys
{
    public class when_type_is_property : given.all_dependencies
    {
        static ProjectionDefinition definition;
        static CommittedEvent committed_event;
        static PartitionId partition;
        static bool result;
        static ProjectionKey key;

        Establish context = () =>
        {
            committed_event = committed_events.single("{\"property\": \"value\"}");
            partition = Guid.Parse("8fe25727-b922-4907-aeaa-06497b0ba80e");
            definition = given.projection_definition_builder.create()
                            .with_selector(new ProjectionEventSelector(committed_event.Type, ProjectEventKeySelectorType.Property, "property"))
                            .Build();
        };
        Because of = () => result = projection_keys.TryGetFor(definition, committed_event, partition, out key);

        It should_get_key = () => result.ShouldBeTrue();
        It should_have_the_correct_key = () => key.Value.ShouldEqual("value");
    }
}