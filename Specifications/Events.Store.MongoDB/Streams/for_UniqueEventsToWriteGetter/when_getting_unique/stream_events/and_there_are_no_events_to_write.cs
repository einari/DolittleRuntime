// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Machine.Specifications;

namespace Dolittle.Runtime.Events.Store.MongoDB.Streams.for_UniqueEventsToWriteGetter.when_getting_unique.stream_events;

public class and_there_are_no_events_to_write : given.all_dependencies
{
    Establish context = () =>
    {
        stored_stream_events.Add(given.a_stream_event.at_event_log_position(0));
        stored_stream_events.Add(given.a_stream_event.at_event_log_position(1));
    };

    Because of = get_unique_stream_events;

    It should_be_successful = () => result.ShouldBeTrue();
    It should_have_no_unique_events = () => unique_stream_events.ShouldBeEmpty();
}