// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Dolittle.Logging;
using Machine.Specifications;

namespace Dolittle.Runtime.Events.Store.MongoDB.Processing.for_StreamProcessorStateRepository.when_incrementing_position
{
    public class and_state_is_stored : given.all_dependencies
    {
        static StreamProcessorStateRepository repository;
        static Runtime.Events.Processing.Streams.StreamProcessorId stream_processor_id;
        static Runtime.Events.Processing.Streams.StreamProcessorState initial_state;
        static Runtime.Events.Processing.Streams.StreamProcessorState result;

        Establish context = () =>
        {
            repository = new StreamProcessorStateRepository(an_event_store_connection, Moq.Mock.Of<ILogger>());
            stream_processor_id = new Runtime.Events.Processing.Streams.StreamProcessorId(Guid.NewGuid(), Guid.NewGuid());
            initial_state = repository.GetOrAddNew(stream_processor_id).GetAwaiter().GetResult();
        };

        Because of = () => result = repository.IncrementPosition(stream_processor_id).GetAwaiter().GetResult();

        It should_have_incremented_the_position = () => result.Position.Value.ShouldEqual(initial_state.Position.Value + 1);
        It should_have_the_same_number_of_failing_partitions = () => result.FailingPartitions.Count.ShouldEqual(initial_state.FailingPartitions.Count);
    }
}