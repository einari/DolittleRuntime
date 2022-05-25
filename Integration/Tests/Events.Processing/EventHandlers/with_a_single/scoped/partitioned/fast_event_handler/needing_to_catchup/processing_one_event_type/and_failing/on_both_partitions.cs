// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Dolittle.Runtime.Events.Processing.EventHandlers;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events.Store.Streams;
using Integration.Tests.Events.Processing.EventHandlers.given;
using Machine.Specifications;

namespace Integration.Tests.Events.Processing.EventHandlers.with_a_single.scoped.partitioned.fast_event_handler.needing_to_catchup.processing_one_event_type.and_failing;

class on_both_partitions : given.single_tenant_and_event_handlers
{
    static IEventHandler event_handler;
    static string failure_reason;
    static PartitionId first_failing_partition;
    static PartitionId second_failing_partition;

    Establish context = () =>
    {
        first_failing_partition = "some event source";
        second_failing_partition = "some other event source";
        failure_reason = "some reason";
        commit_events_for_each_event_type(new (int number_of_events, EventSourceId event_source, ScopeId scope_id)[]
        {
            (2, first_failing_partition.Value, "bcb87bbf-f495-4f72-9795-d5e8864add5f"),
            (2, second_failing_partition.Value, "bcb87bbf-f495-4f72-9795-d5e8864add5f")
        }).GetAwaiter().GetResult();
        fail_for_event_sources(new EventSourceId[]{first_failing_partition.Value, second_failing_partition.Value}, failure_reason);
        with_event_handlers_filtering_number_of_event_types((true, 1, "bcb87bbf-f495-4f72-9795-d5e8864add5f", true, false));
        event_handler = event_handlers_to_run.First();
    };

    Because of = () =>
    {
        run_event_handlers_until_completion_and_commit_events_after_starting_event_handlers(
            (2, first_failing_partition.Value, "bcb87bbf-f495-4f72-9795-d5e8864add5f"),
            (2, second_failing_partition.Value, "bcb87bbf-f495-4f72-9795-d5e8864add5f")).GetAwaiter().GetResult();
    };

    It should_the_correct_number_of_events_in_stream = () => expect_number_of_filtered_events(event_handler, scope_events_for_event_types(event_handler.Info.Id.Scope, 1).LongCount());
    It should_have_persisted_correct_stream = () => expect_stream_definition(
        event_handler,
        partitioned: true, 
        public_stream: false, 
        max_handled_event_types: 1);
    It should_have_the_correct_stream_processor_states = () => expect_stream_processor_state(
        event_handler,
        implicit_filter: false,
        partitioned: true,
        num_events_to_handle: scope_events_for_event_types(event_handler.Info.Id.Scope, 1).Count(),
        failing_partitioned_state: new failing_partitioned_state(new Dictionary<PartitionId, StreamPosition>{
            {
                first_failing_partition,
                get_partitioned_events_in_stream(event_handler, first_failing_partition).First().Position
            },
            {
                second_failing_partition,
                get_partitioned_events_in_stream(event_handler, second_failing_partition).First().Position
            }
        }),
        failing_unpartitioned_state: null);
}