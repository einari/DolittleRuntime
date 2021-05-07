// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dolittle.Runtime.Artifacts;
using Dolittle.Runtime.Embeddings.Store;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Projections.Store.State;
using Dolittle.Runtime.Rudimentary;

namespace Dolittle.Runtime.Embeddings.Processing
{
    /// <summary>
    /// Represents an implementation of <see cref="ICalculateStateTransitionEvents" />.
    /// </summary>
    public class StateTransitionEventsCalculator : ICalculateStateTransitionEvents
    {
        readonly EmbeddingId _identifier;
        readonly IEmbedding _embedding;
        readonly IProjectManyEvents _projector;
        readonly ICompareStates _stateComparer;
        readonly IDetectEmbeddingLoops _loopDetector;

        /// <summary>
        /// Initializes an instance of the <see cref="StateTransitionEventsCalculator" /> class.
        /// </summary>
        /// <param name="identifier">The <see cref="EmbeddingId" />.</param>
        /// <param name="embedding">The <see cref="IEmbedding" />.</param>
        /// <param name="projector">The <see cref="IProjectManyEvents"/>.</param>
        /// <param name="stateComparer">The <see cref="ICompareStates"/>.</param>
        /// <param name="loopDetector">The <see cref="IDetectEmbeddingLoops"/>.</param>
        public StateTransitionEventsCalculator(
            EmbeddingId identifier,
            IEmbedding embedding,
            IProjectManyEvents projector,
            ICompareStates stateComparer,
            IDetectEmbeddingLoops loopDetector
        )
        {
            _identifier = identifier;
            _embedding = embedding;
            _projector = projector;
            _stateComparer = stateComparer;
            _loopDetector = loopDetector;
        }

        /// <inheritdoc/>
        public Task<Try<UncommittedAggregateEvents>> TryConverge(EmbeddingCurrentState current, ProjectionState desired, CancellationToken cancellationToken)
            => DoWork(
                current,
                newCurrent => _stateComparer.TryCheckEquality(newCurrent.State, desired),
                (newCurrent, token) => _embedding.TryCompare(newCurrent, desired, token),
                cancellationToken);

        /// <inheritdoc/>
        public Task<Try<UncommittedAggregateEvents>> TryDelete(EmbeddingCurrentState current, CancellationToken cancellationToken)
            => DoWork(
                current,
                newCurrent => Try<bool>.Do(() => newCurrent.Type is EmbeddingCurrentStateType.Deleted),
                (newCurrent, token) => _embedding.TryDelete(newCurrent, token),
                cancellationToken);

        async Task<Try<UncommittedAggregateEvents>> DoWork(
            EmbeddingCurrentState current,
            Func<EmbeddingCurrentState, Try<bool>> isDesiredState,
            Func<EmbeddingCurrentState, CancellationToken, Task<Try<UncommittedEvents>>> getTransitionEvents,
            CancellationToken cancellationToken)
        {
            try
            {
                var allTransitionEvents = new List<UncommittedEvents>();
                while (true)
                {
                    if (IsDesiredStateOrError(isDesiredState, current, allTransitionEvents, out var eventsToCommit, out var error))
                    {
                        return error == default
                            ? eventsToCommit
                            : error;
                    }

                    var transitionEvents = await getTransitionEvents(current, cancellationToken).ConfigureAwait(false);
                    if (!transitionEvents.Success)
                    {
                        return transitionEvents.Exception;
                    }

                    allTransitionEvents.Add(transitionEvents.Result);

                    var loopDetected = await _loopDetector.TryCheckEventLoops(allTransitionEvents).ConfigureAwait(false);
                    if (!loopDetected.Success)
                    {
                        return loopDetected.Exception;
                    }

                    if (loopDetected.Result)
                    {
                        return new EmbeddingLoopDetected(_identifier);
                    }

                    var intermediateState = await _projector.TryProject(current, transitionEvents.Result, cancellationToken).ConfigureAwait(false);
                    if (!intermediateState.Success)
                    {
                        return intermediateState.IsPartialResult
                            ? new CouldNotProjectAllEvents(_identifier, intermediateState.Exception)
                            : new FailedProjectingEvents(_identifier, intermediateState.Exception);
                    }

                    current = intermediateState.Result;
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        bool IsDesiredStateOrError(
            Func<EmbeddingCurrentState, Try<bool>> isDesiredState,
            EmbeddingCurrentState current,
            List<UncommittedEvents> allTransitionEvents,
            out UncommittedAggregateEvents eventsToCommit,
            out Exception error)
        {
            eventsToCommit = default;
            error = default;
            var isDesiredResult = isDesiredState(current);
            if (!isDesiredResult.Success)
            {
                error = isDesiredResult.Exception;
                return true;
            }

            if (isDesiredResult.Result)
            {
                var events = from uncommittedEvents in allTransitionEvents
                             from @event in uncommittedEvents
                             select @event;

                eventsToCommit = CreateUncommittedAggregateEvents(new UncommittedEvents(events.ToArray()), current);
                return true;
            }
            return false;
        }
        UncommittedAggregateEvents CreateUncommittedAggregateEvents(UncommittedEvents events, EmbeddingCurrentState currentState)
            => new(_identifier.Value, new Artifact(_identifier.Value, ArtifactGeneration.First), currentState.Version, events);
    }
}
