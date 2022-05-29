// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dolittle.Runtime.DependencyInversion.Lifecycle;
using Dolittle.Runtime.DependencyInversion.Scoping;
using Dolittle.Runtime.Events.Processing.Streams;
using Dolittle.Runtime.Events.Store.Actors;
using Dolittle.Runtime.Events.Store.EventHorizon;
using Dolittle.Runtime.Events.Store.Streams;
using Dolittle.Runtime.Execution;
using Microsoft.Extensions.Logging;


namespace Dolittle.Runtime.EventHorizon.Consumer.Processing;

/// <summary>
/// Represents an implementation <see cref="IStreamProcessorFactory" />.
/// </summary>
[Singleton, PerTenant]
public class StreamProcessorFactory : IStreamProcessorFactory
{
    readonly IResilientStreamProcessorStateRepository _streamProcessorStates;
    readonly IWriteEventHorizonEvents _eventHorizonEventsWriter;
    readonly IEventFetcherPolicies _eventsFetcherPolicy;
    readonly IEventProcessorPolicies _eventProcessorPolicy;
    readonly EventStoreClient _eventStoreClient;
    readonly IMetricsCollector _metrics;
    readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes an instance of the <see cref="StreamProcessor" /> class.
    /// </summary>
    /// <param name="streamProcessorStates">The <see cref="IResilientStreamProcessorStateRepository" />.</param>
    /// <param name="eventHorizonEventsWriter">The <see cref="IWriteEventHorizonEvents" />.</param>
    /// <param name="eventsFetcherPolicy">The <see cref="IAsyncPolicyFor{T}" /> <see cref="ICanFetchEventsFromStream" />.</param>
    /// <param name="eventProcessorPolicy">The <see cref="IAsyncPolicyFor{T}" /> <see cref="EventProcessor" />.</param>
    /// <param name="eventStoreClient">The <see cref="EventStoreClient"/>.</param>
    /// <param name="metrics">The system for collecting metrics.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory" />.</param>
    public StreamProcessorFactory(
        IResilientStreamProcessorStateRepository streamProcessorStates,
        IWriteEventHorizonEvents eventHorizonEventsWriter,
        IEventFetcherPolicies eventsFetcherPolicy,
        IEventProcessorPolicies eventProcessorPolicy,
        EventStoreClient eventStoreClient,
        IMetricsCollector metrics,
        ILoggerFactory loggerFactory
    )
    {
        _streamProcessorStates = streamProcessorStates;
        _eventHorizonEventsWriter = eventHorizonEventsWriter;
        _eventsFetcherPolicy = eventsFetcherPolicy;
        _eventProcessorPolicy = eventProcessorPolicy;
        _eventStoreClient = eventStoreClient;
        _metrics = metrics;
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc/>
    public IStreamProcessor Create(
        ConsentId consent,
        SubscriptionId subscription,
        ExecutionContext executionContext,
        EventsFromEventHorizonFetcher eventsFromEventHorizonFetcher)
        => new StreamProcessor(
            subscription,
            executionContext,
            new EventProcessor(
                consent,
                subscription,
                _eventHorizonEventsWriter,
                _eventProcessorPolicy,
                _metrics,
                _eventStoreClient,
                _loggerFactory.CreateLogger<EventProcessor>()),
            eventsFromEventHorizonFetcher,
            _streamProcessorStates,
            _eventsFetcherPolicy,
            _metrics,
            _loggerFactory);
}
