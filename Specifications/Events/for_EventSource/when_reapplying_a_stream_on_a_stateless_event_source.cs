﻿using System;
using Dolittle.Runtime.Events;
using Machine.Specifications;
using Moq;
using It = Machine.Specifications.It;

namespace Dolittle.Events.Specs.for_EventSource
{
    public class when_reapplying_a_stream_on_a_stateless_event_source : given.a_stateless_event_source
    {
        static CommittedEventStream event_stream;
        static Exception exception;

        Establish context = () => event_stream = new CommittedEventStream(event_source_id, new[] { new EventAndEnvelope(new Mock<IEnvelope>().Object, new SimpleEvent()) });

        Because of = () => exception = Catch.Exception(() => event_source.ReApply(event_stream));

        It should_not_throw_an_exception = () => exception.ShouldBeNull();
    }
}
