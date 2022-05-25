// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Dolittle.Runtime.Events.Processing.EventHandlers;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events.Store.Streams;
using Integration.Tests.Events.Processing.EventHandlers.given;
using Machine.Specifications;

namespace Integration.Tests.Events.Processing.EventHandlers.with_a_single.scoped.unpartitioned.fast_event_handler.needing_to_catchup.unprocessing_all_event_types.and_failing;

class after_2_events : given.single_tenant_and_event_handlers
{
    static IEventHandler event_handler;
    static string failure_reason;
    static PartitionId failing_partition;

    Establish context = () =>
    {
        failing_partition = "some event source";
        failure_reason = "some reason";
        commit_events_for_each_event_type(new (int number_of_events, EventSourceId event_source, ScopeId scope_id)[]
        {
            (2, failing_partition.Value, "bcb87bbf-f495-4f72-9795-d5e8864add5f")
        }).GetAwaiter().GetResult();
        fail_after_processing_number_of_events(2, failure_reason);
        with_event_handlers_filtering_number_of_event_types((false, number_of_event_types, "bcb87bbf-f495-4f72-9795-d5e8864add5f", true, false));
        event_handler = event_handlers_to_run.First();
    };

    Because of = () =>
    {
        run_event_handlers_until_completion_and_commit_events_after_starting_event_handlers(
            (2, failing_partition.Value, "bcb87bbf-f495-4f72-9795-d5e8864add5f")).GetAwaiter().GetResult();
    };

    It should_the_correct_number_of_events_in_stream = () => expect_number_of_filtered_events(event_handler, scope_events_for_event_types(event_handler.Info.Id.Scope, number_of_event_types).LongCount());
    It should_have_persisted_correct_stream = () => expect_stream_definition(
        event_handler,
        partitioned: false,
        public_stream: false,
        max_handled_event_types: number_of_event_types);
    It should_have_the_correct_stream_processor_states = () => expect_stream_processor_state(
        event_handler,
        implicit_filter: false,
        partitioned: false,
        num_events_to_handle: committed_events.Count,
        failing_partitioned_state: null,
        failing_unpartitioned_state: new failing_unpartitioned_state(1));
}