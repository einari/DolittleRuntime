// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dolittle.Runtime.Events.Streams.Processing
{
    /// <summary>
    /// Represents an implementation of <see cref="ICanProcessStreamOfEvents"/> that processes an event.
    /// </summary>
    public class EventStreamProcessor : ICanProcessStreamOfEvents
    {
        readonly EventStreamId _eventStreamId;
        readonly ICanHandleEventProcessing<ProcessingResult> _eventStreamProcessor;
        readonly ICanManageEventStream _eventStreamManager;
        EventStreamState _currentState;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventStreamProcessor"/> class.
        /// </summary>
        /// <param name="eventStreamId">Event stream id.</param>
        /// <param name="eventStreamProcessor">Event stream processor.</param>
        /// <param name="eventStreamManager">Event stream manager.</param>
        public EventStreamProcessor(
            EventStreamId eventStreamId,
            ICanHandleEventProcessing<ProcessingResult> eventStreamProcessor,
            ICanManageEventStream eventStreamManager)
        {
            _eventStreamId = eventStreamId;
            _eventStreamProcessor = eventStreamProcessor;
            _eventStreamManager = eventStreamManager;
        }

        /// <inheritdoc/>
        public async Task Process(IObservable<EventEnvelope> eventStream)
        {
            _currentState = _eventStreamManager.GetState();
            var localStream = eventStream.Skip((int)_currentState.Offset.Value);
            await Task.Run(async () =>
            {
                while (_currentState.StreamState != StreamState.Stop)
                {
                    if (_currentState.StreamState == StreamState.NullState) throw new IllegalEventStreamState(_currentState.StreamState);

                    // TODO: Store ignored event
                    if (_currentState.StreamState == StreamState.Ignore) localStream = localStream.Skip(1);
                    else if (_currentState.StreamState == StreamState.Retry) Thread.Sleep(3000);
                    var @event = await localStream.FirstAsync();
                    var filteringResult = _eventStreamProcessor.Process(_eventStreamId, @event);
                    _currentState = _eventStreamManager.UpdateState(filteringResult.StreamState);
                }
            }).ConfigureAwait(false);
        }
    }
}