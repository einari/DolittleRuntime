// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Dolittle.Runtime.Events.Processing.EventHandlers;
using Machine.Specifications;

namespace Integration.Tests.Events.Processing.EventHandlers.with_a_single.scoped.not_partitioned.event_handler.with_implicit_filter.not_processing_one_event_type;

[Ignore("Implicit filter does not work yet with event handlers")]
class without_problems : given.single_tenant_and_event_handlers
{
    static IEventHandler event_handler;

    Establish context = () =>
    {
        event_handler = setup_event_handler();
    };

    Because of = () =>
    {
        commit_events_after_starting_event_handler((2, "some_source"));
    };

    It should_have_persisted_correct_stream = () => expect_stream_definition(event_handler);
    It should_the_correct_number_of_events_in_stream = () => expect_number_of_filtered_events(event_handler, scope_events_for_event_types(event_handler_scope, 1).LongCount());
    It should_have_the_correct_stream_processor_states = () => expect_stream_processor_state_without_failure(event_handler);
}