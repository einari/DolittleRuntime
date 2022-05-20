// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dolittle.Runtime.Artifacts;
using Dolittle.Runtime.Domain.Tenancy;
using Dolittle.Runtime.Events.Processing.Contracts;
using Dolittle.Runtime.Events.Processing.EventHandlers;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events.Store.EventHorizon;
using Dolittle.Runtime.Events.Store.Streams;
using Dolittle.Runtime.Events.Store.Streams.Filters;
using Dolittle.Runtime.Rudimentary;
using Integration.Shared;
using Machine.Specifications;
using Machine.Specifications.Utility;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using CommittedEvent = Dolittle.Runtime.Events.Store.CommittedEvent;
using ExecutionContext = Dolittle.Runtime.Execution.ExecutionContext;
using ReverseCallDispatcher = Dolittle.Runtime.Services.IReverseCallDispatcher<
    Dolittle.Runtime.Events.Processing.Contracts.EventHandlerClientToRuntimeMessage,
    Dolittle.Runtime.Events.Processing.Contracts.EventHandlerRuntimeToClientMessage,
    Dolittle.Runtime.Events.Processing.Contracts.EventHandlerRegistrationRequest,
    Dolittle.Runtime.Events.Processing.Contracts.EventHandlerRegistrationResponse,
    Dolittle.Runtime.Events.Processing.Contracts.HandleEventRequest,
    Dolittle.Runtime.Events.Processing.Contracts.EventHandlerResponse>;
using UncommittedEvent = Dolittle.Runtime.Events.Store.UncommittedEvent;

namespace Integration.Tests.Events.Processing.EventHandlers.given;

class single_tenant_and_event_handlers : Processing.given.a_clean_event_store
{
    protected static IEventHandlers event_handlers;
    protected static IEventHandlerFactory event_handler_factory;
    protected static IEnumerable<IEventHandler> event_handlers_to_run;
    protected static ArtifactId[] event_types;
    protected static Mock<ReverseCallDispatcher> dispatcher;
    protected static int number_of_event_types;
    protected static TaskCompletionSource dispatcher_cancellation_source;
    protected static Func<HandleEventRequest, ExecutionContext, CancellationToken, Task<EventHandlerResponse>> on_handle_event;
    protected static CommittedEvents committed_events;
    protected static Dictionary<ScopeId, CommittedEvents> scoped_committed_events = new();
    protected static IWriteEventHorizonEvents event_horizon_events_writer;
    static int number_of_events_handled = 0;
    static CancellationTokenRegistration? cancel_event_handlers_registration;
    static CancellationTokenSource? cancel_event_handlers_source;

    static Dictionary<EventHandlerInfo, Try<IStreamDefinition>> persisted_stream_definitions;

    Establish context = () =>
    {
        committed_events = new CommittedEvents(Array.Empty<CommittedEvent>());
        dispatcher_cancellation_source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        number_of_event_types = 2;
        event_horizon_events_writer = runtime.Host.Services.GetRequiredService<Func<TenantId, IWriteEventHorizonEvents>>()(tenant);
        event_handlers = runtime.Host.Services.GetRequiredService<IEventHandlers>();
        event_handler_factory = runtime.Host.Services.GetRequiredService<IEventHandlerFactory>();
        event_types = Enumerable.Range(0, number_of_event_types).Select(_ => new ArtifactId(Guid.NewGuid())).ToArray();
        dispatcher = new Mock<ReverseCallDispatcher>();
        on_handle_event = (_, _, _) =>
        {
            var response = new EventHandlerResponse();
            Console.WriteLine($"Handled {number_of_events_handled} events");
            // if (number_of_events_handled == number_of_events)
            // {
            //     Console.WriteLine("Finishing");
            //     dispatcher_cancellation_source.SetResult();
            // }
            Console.WriteLine("Returning response");
            return Task.FromResult(response);
        };
        
        dispatcher
            .Setup(_ => _.Reject(Moq.It.IsAny<EventHandlerRegistrationResponse>(), Moq.It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        dispatcher
            .Setup(_ => _.Accept(Moq.It.IsAny<EventHandlerRegistrationResponse>(), Moq.It.IsAny<CancellationToken>()))
            .Returns(dispatcher_cancellation_source.Task);
        dispatcher
            .Setup(_ => _.Call(Moq.It.IsAny<HandleEventRequest>(), Moq.It.IsAny<ExecutionContext>(), Moq.It.IsAny<CancellationToken>()))
            .Callback(() => Interlocked.Add(ref number_of_events_handled, 1))
            .Returns(on_handle_event);
    };

    Cleanup clean = () =>
    {
        cancel_event_handlers_registration?.Dispose();
        cancel_event_handlers_source?.Dispose();
        foreach (var event_handler in event_handlers_to_run)
        {
            event_handler.Dispose();
        }
    };

    protected static async Task<CommittedEvents> commit_events_for_each_event_type(int number_of_events, EventSourceId event_source)
    {
        var uncommitted_events = event_types.SelectMany(event_type => Enumerable.Range(0, number_of_events)
            .Select(_ => new UncommittedEvent(event_source, new Artifact(event_type, ArtifactGeneration.First), false, "{\"hello\": 42}")));
        var response = await event_store.Commit(new UncommittedEvents(uncommitted_events.ToArray()), Runtime.CreateExecutionContextFor(tenant)).ConfigureAwait(false);
        var newly_committed_events = response.Events.ToCommittedEvents();
        var all_committed_events = committed_events.ToList();
        all_committed_events.AddRange(newly_committed_events);
        committed_events = new CommittedEvents(all_committed_events);
        return newly_committed_events;
    }
    
    protected static async Task<CommittedEvents> commit_events_for_each_event_type_into_scope(int number_of_events, EventSourceId event_source, ScopeId scope)
    {
        var committed_events = await commit_events_for_each_event_type(number_of_events, event_source).ConfigureAwait(false);
        await write_committed_events_to_scoped_event_log(committed_events, scope).ConfigureAwait(false);
        return committed_events;
    }
    
    protected static async Task write_committed_events_to_scoped_event_log(CommittedEvents committed_events, ScopeId scope)
    {
        foreach (var committed_event in committed_events)
        {
            await event_horizon_events_writer.Write(committed_event, Guid.NewGuid(), scope, CancellationToken.None).ConfigureAwait(false);
        }
        if (!scoped_committed_events.TryGetValue(scope, out var previously_committed_events))
        {
            previously_committed_events = new CommittedEvents(Array.Empty<CommittedEvent>());
        }
        var all_committed_events = previously_committed_events.ToList();
        all_committed_events.AddRange(committed_events);
        scoped_committed_events[scope] = new CommittedEvents(all_committed_events);
    }
    
    protected static async Task run_event_handlers_until_completion(IEnumerable<Task> pre_start_tasks = default, Task post_start_task = default)
    {
        await Task.WhenAll(pre_start_tasks ?? Array.Empty<Task>()).ConfigureAwait(false);
        var tasks = new List<Task>();
        tasks.AddRange(event_handlers_to_run.Select(event_handler => event_handlers.RegisterAndStart(
            event_handler,
            (_, _) => dispatcher.Object.Reject(new EventHandlerRegistrationResponse(), CancellationToken.None),
            CancellationToken.None)));
        tasks.Add(post_start_task ?? Task.CompletedTask);
        
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    protected static Task run_event_handlers_until_completion_and_commit_events_after_starting_event_handlers(params (int number_of_events, EventSourceId event_source, ScopeId scope)[] events)
        => run_event_handlers_until_completion(post_start_task: commit_after_delay(events));

    protected static void stop_event_handlers_after(TimeSpan time_span)
    {
        cancel_event_handlers_source = new CancellationTokenSource(time_span);
        cancel_event_handlers_registration = cancel_event_handlers_source.Token.Register(() => dispatcher_cancellation_source?.SetResult());
    }
    
    protected static void with_event_handlers(params (bool partitioned, int max_event_types_to_filter, ScopeId scope, bool fast)[] event_handler_infos)
    {
        event_handlers_to_run = event_handler_infos.Select(_ =>
        {
            var (partitioned, max_event_types_to_filter, scope, fast) = _;
            var registration_arguments = new EventHandlerRegistrationArguments(
                Runtime.CreateExecutionContextFor(tenant), Guid.NewGuid(), event_types.Take(max_event_types_to_filter), partitioned, scope);
            return fast
                ? event_handler_factory.CreateFast(registration_arguments, dispatcher.Object, CancellationToken.None)
                : event_handler_factory.Create(registration_arguments, dispatcher.Object, CancellationToken.None);
        }).ToArray();
    }

    protected static void complete_after_processing_number_of_events(int number_of_events)
    {
        Console.WriteLine($"Completing after processing {number_of_events} events");
        on_handle_event = (_, _, _) =>
        {
            var response = new EventHandlerResponse();
            Console.WriteLine($"Handled {number_of_events_handled} events");
            if (number_of_events_handled == number_of_events)
            {
                Console.WriteLine("Finishing");
                dispatcher_cancellation_source.SetResult();
            }
            Console.WriteLine("Returning response");
            return Task.FromResult(response);
        };
    }

    static async Task commit_after_delay(params (int number_of_events, EventSourceId event_source, ScopeId scope)[] events)
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        await Task.WhenAll(events.Select(_ => _.scope != ScopeId.Default
            ? commit_events_for_each_event_type_into_scope(_.number_of_events, _.event_source, _.scope)
            : commit_events_for_each_event_type(_.number_of_events, _.event_source)));
    }

    protected static Try<IStreamDefinition> get_stream_definition_for(IEventHandler event_handler)
    {
        if (persisted_stream_definitions == default)
        {
            persisted_stream_definitions = new Dictionary<EventHandlerInfo, Try<IStreamDefinition>>();
            foreach (var info in event_handlers_to_run.Select(_ => _.Info))
            {
                var tryGetDefinition = stream_definition_repository.TryGet(info.Id.Scope, info.Id.EventHandler.Value, CancellationToken.None).Result;
                persisted_stream_definitions[info] = tryGetDefinition;
            }
        }
        return persisted_stream_definitions[event_handler.Info];
    }

    protected static IFilterDefinition get_filter_definition_for<TDefinition>(IEventHandler event_handler)
        where TDefinition : class, IFilterDefinition
        => get_stream_definition_for(event_handler).Result.FilterDefinition as TDefinition;
}