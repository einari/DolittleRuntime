// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Logging;
using Dolittle.Runtime.Events.Streams;
using Machine.Specifications;

namespace Dolittle.Runtime.Events.Store.MongoDB.Processing.for_EventsFromStreamsFetcher.when_finding_next_position.in_the_all_stream
{
    public class and_there_are_no_events : given.all_dependencies
    {
        static EventsFromStreamsFetcher events_from_streams_fetcher;
        static StreamPosition result;

        Establish context = () => events_from_streams_fetcher = new EventsFromStreamsFetcher(an_event_store_connection, Moq.Mock.Of<ILogger>());

        Because of = () => result = events_from_streams_fetcher.FindNext(StreamId.AllStreamId, PartitionId.NotSet, 0U).GetAwaiter().GetResult();

        It should_return_the_max_value_of_uint = () => result.Value.ShouldEqual(uint.MaxValue);
    }
}