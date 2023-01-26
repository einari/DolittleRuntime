// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dolittle.Runtime.Rudimentary;
using Dolittle.Runtime.Events.Processing.Streams;
using Dolittle.Runtime.Events.Store.MongoDB.Processing.Streams.EventHorizon;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Dolittle.Runtime.EventHorizon.Consumer;
using Dolittle.Runtime.Events.Store.MongoDB.Legacy;
using MongoSubscriptionState = Dolittle.Runtime.Events.Store.MongoDB.Processing.Streams.EventHorizon.SubscriptionState;
using Dolittle.Runtime.Events.Store.Streams;
using Dolittle.Runtime.MongoDB;

namespace Dolittle.Runtime.Events.Store.MongoDB.Processing.Streams;

/// <summary>
/// Represents an implementation of <see cref="IStreamProcessorStateRepository" />.
/// </summary>
public class StreamProcessorStateRepository : IStreamProcessorStateBatchRepository
{
    readonly FilterDefinitionBuilder<AbstractStreamProcessorState> _streamProcessorFilter;
    readonly FilterDefinitionBuilder<MongoSubscriptionState> _subscriptionFilter;
    readonly IStreamProcessorStates _streamProcessorStates;
    readonly ISubscriptionStates _subscriptionStates;
    readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamProcessorStateRepository"/> class.
    /// </summary>
    /// <param name="streamProcessorStates">The <see cref="IStreamProcessorStates" />.</param>
    /// <param name="subscriptionStates">The <see cref="ISubscriptionStates" />.</param>
    /// <param name="logger">An <see cref="ILogger"/>.</param>
    public StreamProcessorStateRepository(
        IStreamProcessorStates streamProcessorStates,
        ISubscriptionStates subscriptionStates,
        ILogger logger)
    {
        _streamProcessorStates = streamProcessorStates;
        _subscriptionStates = subscriptionStates;
        _streamProcessorFilter = Builders<AbstractStreamProcessorState>.Filter;
        _subscriptionFilter = Builders<MongoSubscriptionState>.Filter;
        _logger = logger;
    }

    /// <summary>
    /// Gets the <see cref="IStreamProcessorState" /> for the given <see cref="IStreamProcessorId" /> from the correct
    /// collection, either <see cref="SubscriptionState" /> or <see cref="StreamProcessorState" />.
    /// </summary>
    /// <param name="id">The unique <see cref="IStreamProcessorId" /> representing either the <see cref="AbstractScopedStreamProcessor"/>
    /// or <see cref="SubscriptionId" />.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken" />.</param>
    /// <returns>A <see cref="Task" /> that, when resolved, returns <see cref="Try{TResult}" />.</returns>
    public async Task<Try<IStreamProcessorState>> TryGet(IStreamProcessorId id, CancellationToken cancellationToken)
    {
        _logger.GettingStreamProcessorState(id);
        try
        {
            switch (id)
            {
                case SubscriptionId subscriptionId:
                {
                    var states = await _subscriptionStates.Get(subscriptionId.ScopeId, cancellationToken).ConfigureAwait(false);
                    var persistedState = await states.Find(CreateFilter(subscriptionId))
                        .FirstOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);

                    return persistedState == default
                        ? new StreamProcessorStateDoesNotExist(subscriptionId)
                        : persistedState.ToRuntimeRepresentation();
                }

                case StreamProcessorId streamProcessorId:
                {
                    var states = await _streamProcessorStates.Get(streamProcessorId.ScopeId, cancellationToken).ConfigureAwait(false);
                    var persistedState = await states.Find(CreateFilter(streamProcessorId))
                        .FirstOrDefaultAsync(cancellationToken)
                        .ConfigureAwait(false);
                    return persistedState == default
                        ? new StreamProcessorStateDoesNotExist(streamProcessorId)
                        : Try<IStreamProcessorState>.Succeeded(persistedState.ToRuntimeRepresentation());
                }

                default:
                    return new StreamProcessorIdOfUnsupportedType(id);
            }
        }
        catch (MongoWaitQueueFullException ex)
        {
            return new EventStoreUnavailable("Mongo wait queue is full", ex);
        }
    }

    public async IAsyncEnumerable<StreamProcessorStateWithId> GetAllNonScoped([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _logger.GettingAllStreamProcessorState();
        var stateCollection = await _streamProcessorStates.Get(ScopeId.Default, cancellationToken).ConfigureAwait(false);
        var states = stateCollection
            .Find(FilterDefinition<AbstractStreamProcessorState>.Empty)
            .ToAsyncEnumerable(cancellationToken: cancellationToken)
            .Select(document => new StreamProcessorStateWithId(new StreamProcessorId(ScopeId.Default, document.EventProcessor, document.SourceStream), document.ToRuntimeRepresentation()));
        await foreach (var state in states.WithCancellation(cancellationToken))
        {
            yield return state;
        }
    }

    public async Task<IEnumerable<Partial<IStreamProcessorId>>> Persist(IReadOnlyDictionary<IStreamProcessorId, IStreamProcessorState> streamProcessorStates, CancellationToken cancellationToken)
    {
        var tasksWithIds = streamProcessorStates.Select(_ => (_.Key, Persist(_.Key, _.Value, cancellationToken)));
        var persistResults = await Task.WhenAll(tasksWithIds.Select(_ => _.Item2)).ConfigureAwait(false);

        return persistResults.Where(_ => !_.Success);
    }
    
    
    async Task<Partial<IStreamProcessorId>> Persist(IStreamProcessorId id, IStreamProcessorState baseStreamProcessorState, CancellationToken cancellationToken)
    {
        _logger.PersistingStreamProcessorState(id);
        try
        {
            if (id is SubscriptionId subscriptionId)
            {
                if (baseStreamProcessorState is Runtime.Events.Processing.Streams.StreamProcessorState streamProcessorState)
                {
                    var replacementState = new MongoSubscriptionState(
                        subscriptionId.ProducerMicroserviceId,
                        subscriptionId.ProducerTenantId,
                        subscriptionId.StreamId,
                        subscriptionId.PartitionId,
                        streamProcessorState.Position,
                        streamProcessorState.RetryTime.UtcDateTime,
                        streamProcessorState.FailureReason,
                        streamProcessorState.ProcessingAttempts,
                        streamProcessorState.LastSuccessfullyProcessed.UtcDateTime,
                        streamProcessorState.IsFailing);
                    var states = await _subscriptionStates.Get(subscriptionId.ScopeId, cancellationToken).ConfigureAwait(false);
                    await states.ReplaceOneAsync(
                            CreateFilter(subscriptionId),
                            replacementState,
                            new ReplaceOptions { IsUpsert = true })
                        .ConfigureAwait(false);
                }
                else
                {
                    return new UnsupportedStreamProcessorStatewithSubscriptionId(subscriptionId, baseStreamProcessorState);
                }
            }
            else if (baseStreamProcessorState is Runtime.Events.Processing.Streams.Partitioned.StreamProcessorState partitionedStreamProcessorState)
            {
                var streamProcessorId = id as StreamProcessorId;
                var states = await _streamProcessorStates.Get(streamProcessorId.ScopeId, cancellationToken).ConfigureAwait(false);
                await states.ReplaceOneAsync(
                        CreateFilter(streamProcessorId),
                        new Partitioned.PartitionedStreamProcessorState(
                            streamProcessorId.EventProcessorId,
                            streamProcessorId.SourceStreamId,
                            partitionedStreamProcessorState.Position,
                            partitionedStreamProcessorState.FailingPartitions.ToDictionary(
                                kvp => kvp.Key.Value.ToString(),
                                kvp => new FailingPartitionState(
                                    kvp.Value.Position,
                                    kvp.Value.RetryTime.UtcDateTime,
                                    kvp.Value.Reason,
                                    kvp.Value.ProcessingAttempts,
                                    kvp.Value.LastFailed.UtcDateTime)),
                            partitionedStreamProcessorState.LastSuccessfullyProcessed.UtcDateTime),
                        new ReplaceOptions { IsUpsert = true })
                    .ConfigureAwait(false);
            }
            else if (baseStreamProcessorState is Runtime.Events.Processing.Streams.StreamProcessorState streamProcessorState)
            {
                var streamProcessorId = id as StreamProcessorId;
                var states = await _streamProcessorStates.Get(streamProcessorId.ScopeId, cancellationToken).ConfigureAwait(false);
                await states.ReplaceOneAsync(
                        CreateFilter(streamProcessorId),
                        new StreamProcessorState(
                            streamProcessorId.EventProcessorId,
                            streamProcessorId.SourceStreamId,
                            streamProcessorState.Position,
                            streamProcessorState.RetryTime.UtcDateTime,
                            streamProcessorState.FailureReason,
                            streamProcessorState.ProcessingAttempts,
                            streamProcessorState.LastSuccessfullyProcessed.UtcDateTime,
                            streamProcessorState.IsFailing),
                        new ReplaceOptions { IsUpsert = true })
                    .ConfigureAwait(false);
            }
            else
            {
                return new StreamProcessorStateOfUnsupportedType(id, baseStreamProcessorState);
            }
        }
        catch (MongoWaitQueueFullException ex)
        {
            return Partial<IStreamProcessorId>.PartialSuccess(id, new EventStoreUnavailable("Mongo wait queue is full", ex));
        }
        return Partial<IStreamProcessorId>.Succeeded(id);
    }
    
    
    FilterDefinition<AbstractStreamProcessorState> CreateFilter(StreamProcessorId id) =>
        _streamProcessorFilter.Eq(_ => _.EventProcessor, id.EventProcessorId.Value)
        & _streamProcessorFilter.Eq(_ => _.SourceStream, id.SourceStreamId.Value);

    FilterDefinition<MongoSubscriptionState> CreateFilter(SubscriptionId id) =>
        _subscriptionFilter.Eq(_ => _.Microservice, id.ProducerMicroserviceId.Value)
        & _subscriptionFilter.Eq(_ => _.Tenant, id.ProducerTenantId.Value)
        & _subscriptionFilter.Eq(_ => _.Stream, id.StreamId.Value)
        & _subscriptionFilter.EqStringOrGuid(_ => _.Partition, id.PartitionId.Value);
}
