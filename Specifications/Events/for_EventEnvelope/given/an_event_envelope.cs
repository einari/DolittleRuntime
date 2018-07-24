﻿using System;
using Dolittle.Artifacts;
using Dolittle.Runtime.Transactions;
using Machine.Specifications;
using Moq;

namespace Dolittle.Runtime.Events.Specs.for_EventEnvelope.given
{
    public class an_event_envelope
    {
        protected static IEventEnvelope event_envelope;
        protected static Artifact event_identifier;
        protected static Artifact event_source_identifier;
        protected static EventSourceId event_source_id;
        protected static EventSourceVersion version;

        Establish context = () =>
        {
            event_source_id = EventSourceId.New();
            event_identifier = Artifact.New();
            event_source_identifier = Artifact.New();
            version = EventSourceVersion.Zero;
            event_envelope = new EventEnvelope(
                TransactionCorrelationId.NotSet,
                EventId.New(),
                EventSequenceNumber.Zero,
                EventSequenceNumber.Zero,
                EventGeneration.First,
                event_identifier,
                event_source_id,
                event_source_identifier,
                version,
                CausedBy.Unknown,
                DateTimeOffset.UtcNow
            );
        };

    }
}
