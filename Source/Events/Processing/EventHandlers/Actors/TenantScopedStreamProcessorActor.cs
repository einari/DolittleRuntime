// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dolittle.Runtime.Actors;
using Dolittle.Runtime.Domain.Tenancy;
using Dolittle.Runtime.Events.Processing.Streams;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events.Store.Streams;
using Dolittle.Runtime.Events.Store.Streams.Filters;
using Dolittle.Runtime.Events.Store.Streams.Legacy;
using Dolittle.Runtime.Rudimentary;
using Microsoft.Extensions.Logging;
using Proto;
using ExecutionContext = Dolittle.Runtime.Execution.ExecutionContext;
using StreamProcessorState = Dolittle.Runtime.Events.Processing.Streams.StreamProcessorState;

namespace Dolittle.Runtime.Events.Processing.EventHandlers.Actors;

public delegate Props CreateTenantScopedStreamProcessorProps(StreamProcessorId streamProcessorId,
    TypeFilterWithEventSourcePartitionDefinition filterDefinition,
    IEventProcessor processor,
    ExecutionContext executionContext,
    ScopedStreamProcessorProcessedEvent onProcessed,
    ScopedStreamProcessorFailedToProcessEvent onFailedToProcess,
    TenantId tenantId);

/// <summary>
/// Represents the basis of system that can process a stream of events.
/// </summary>
public sealed class TenantScopedStreamProcessorActor : IActor, IDisposable
{
    readonly IStreamDefinition _sourceStreamDefinition;
    readonly TenantId _tenantId;
    readonly TypeFilterWithEventSourcePartitionDefinition _filterDefinition;
    readonly IEventProcessor _processor;
    readonly IStreamEventSubscriber _eventSubscriber;
    readonly ExecutionContext _executionContext;
    readonly IStreamProcessorStates _streamProcessorStates;
    readonly IMapStreamPositionToEventLogPosition _eventLogPositionEnricher;
    readonly ScopedStreamProcessorProcessedEvent _onProcessed;
    readonly ScopedStreamProcessorFailedToProcessEvent _onFailedToProcess;

    CancellationTokenSource? _stoppingToken;
    readonly bool _partitioned;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantScopedStreamProcessorActor"/> class.
    /// </summary>
    /// <param name="tenantId">The <see cref="TenantId"/>.</param>
    /// <param name="streamProcessorId">The <see cref="IStreamProcessorId" />.</param>
    /// <param name="filterDefinition"></param>
    /// <param name="processor">An <see cref="IEventProcessor" /> to process the event.</param>
    /// <param name="streamProcessorStates"></param>
    /// <param name="executionContext">The <see cref="ExecutionContext"/> of the stream processor.</param>
    /// <param name="eventLogPositionEnricher"></param>
    /// <param name="logger">An <see cref="ILogger" /> to log messages.</param>
    /// <param name="eventSubscriber"></param>
    /// <param name="onProcessed"></param>
    /// <param name="onFailedToProcess"></param>
    public TenantScopedStreamProcessorActor(
        StreamProcessorId streamProcessorId,
        TypeFilterWithEventSourcePartitionDefinition filterDefinition,
        IEventProcessor processor,
        IStreamEventSubscriber eventSubscriber,
        IStreamProcessorStates streamProcessorStates,
        ExecutionContext executionContext,
        IMapStreamPositionToEventLogPosition eventLogPositionEnricher,
        ILogger<TenantScopedStreamProcessorActor> logger,
        ScopedStreamProcessorProcessedEvent onProcessed,
        ScopedStreamProcessorFailedToProcessEvent onFailedToProcess,
        TenantId tenantId)
    {
        Identifier = streamProcessorId;
        Logger = logger;
        _onProcessed = onProcessed;
        _onFailedToProcess = onFailedToProcess;
        _tenantId = tenantId;
        _eventLogPositionEnricher = eventLogPositionEnricher;
        _eventSubscriber = eventSubscriber;
        _streamProcessorStates = streamProcessorStates;
        _filterDefinition = filterDefinition;
        _processor = processor;
        _executionContext = executionContext;
        _partitioned = filterDefinition.Partitioned;
    }

    public static CreateTenantScopedStreamProcessorProps CreateFactory(ICreateProps createProps)
        => (streamProcessorId, filterDefinition, processor, executionContext, onProcessed, onFailedToProcess, tenantId) =>
            PropsFor(createProps, streamProcessorId, filterDefinition, processor, executionContext, onProcessed, onFailedToProcess, tenantId);

    static Props PropsFor(ICreateProps createProps,
        StreamProcessorId streamProcessorId,
        TypeFilterWithEventSourcePartitionDefinition filterDefinition,
        IEventProcessor processor,
        ExecutionContext executionContext,
        ScopedStreamProcessorProcessedEvent onProcessed,
        ScopedStreamProcessorFailedToProcessEvent onFailedToProcess,
        TenantId tenantId
    )
    {
        return createProps.PropsFor<TenantScopedStreamProcessorActor>(streamProcessorId, filterDefinition, processor, executionContext, onProcessed,
            onFailedToProcess, tenantId);
    }

    public async Task ReceiveAsync(IContext context)
    {
        try
        {
            switch (context.Message)
            {
                case Started:
                    await Init(context);
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("Cancelled processing of {Message}", context.Message);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to process message {Message}", context.Message);
            throw;
        }
    }


    async Task Init(IContext context)
    {
        var processingPosition = await LoadProcessingPosition(context);
        if (!processingPosition.Success)
        {
            Logger.LogError(processingPosition.Exception, "Failed to load processing position for {StreamProcessorId}", Identifier);
            throw processingPosition.Exception;
        }

        var initialState = processingPosition.Result;

        var from = initialState.EarliestProcessingPosition;
        var cts = new CancellationTokenSource();
        var events = StartSubscription(from, cts.Token);
        var firstEventReady = events.WaitToReadAsync(cts.Token).AsTask();
        context.ReenterAfter(firstEventReady, _ => StartProcessing(initialState, events,context));
        _stoppingToken = cts;
    }

    async Task StartProcessing(IStreamProcessorState streamProcessorState, ChannelReader<StreamEvent> events, IContext context)
    {
        try
        {
            if (_partitioned)
            {
                var processor = new PartitionedProcessor(
                    Identifier,
                    _processor,
                    _streamProcessorStates,
                    _executionContext,
                    _onProcessed,
                    _onFailedToProcess,
                    _tenantId,
                    Logger);

                await processor.Process(events, streamProcessorState, context.CancellationToken);
            }
            else
            {
                var processor = new NonPartitionedProcessor(
                    Identifier,
                    _filterDefinition,
                    _processor,
                    _streamProcessorStates,
                    _executionContext,
                    _onProcessed,
                    _onFailedToProcess,
                    _tenantId,
                    Logger);

                await processor.Process(events, streamProcessorState, context.CancellationToken);
            }
        }
        finally
        {
            context.Stop(context.Self);
        }
    }

    ChannelReader<StreamEvent> StartSubscription(ProcessingPosition from, CancellationToken token)
    {
        return _eventSubscriber.Subscribe(Identifier.ScopeId, _filterDefinition.Types, from, _filterDefinition.Partitioned,
            token);
    }



    /// <summary>
    /// Loads processing position from storage, optionally enriching it with the event log position.
    /// If no processing position is found, it will return a new <see cref="Streams.StreamProcessorState"/> with the initial position.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    async Task<Try<IStreamProcessorState>> LoadProcessingPosition(IContext context)
    {
        var position = await _streamProcessorStates.TryGetFor(Identifier, context.CancellationToken);
        if (position is { Success: false, Exception: StreamProcessorStateDoesNotExist })
        {
            return Try<IStreamProcessorState>.Succeeded(new StreamProcessorState(ProcessingPosition.Initial, DateTimeOffset.UtcNow));
        }

        return await position.ReduceAsync(WithEventLogPosition);

        Task<Try<IStreamProcessorState>> WithEventLogPosition(IStreamProcessorState state)
        {
            return _eventLogPositionEnricher
                .WithEventLogSequence(new StreamProcessorStateWithId<StreamProcessorId, IStreamProcessorState>(Identifier, state), context.CancellationToken);
        }
    }

    /// <summary>
    /// Gets the <see cref="StreamProcessorId">identifier</see> for the <see cref="TenantScopedStreamProcessorActor"/>.
    /// </summary>
    public StreamProcessorId Identifier { get; }

    /// <summary>
    /// Gets the <see cref="ILogger" />.
    /// </summary>
    protected ILogger Logger { get; }

    public void Dispose()
    {
        _stoppingToken?.Cancel();
        _stoppingToken?.Dispose();
    }
}
