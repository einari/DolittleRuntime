// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dolittle.Artifacts;
using Dolittle.Runtime.Async;
using Dolittle.Runtime.Events.Store.Streams;

namespace Dolittle.Runtime.Events.Store.MongoDB.Streams
{
    /// <summary>
    /// Represents an abstract implementation of <see cref="ICanFetchFromWellKnownStreams" />.
    /// </summary>
    public abstract class AbstractEventsFromWellKnownStreamsFetcher : ICanFetchFromWellKnownStreams
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractEventsFromWellKnownStreamsFetcher"/> class.
        /// </summary>
        /// <param name="streams">The streams it can fetch from.</param>
        protected AbstractEventsFromWellKnownStreamsFetcher(IEnumerable<StreamId> streams) => WellKnownStreams = streams;

        /// <inheritdoc/>
        public IEnumerable<StreamId> WellKnownStreams { get; }

        /// <inheritdoc/>
        public bool CanFetchFromStream(StreamId stream) => WellKnownStreams.Contains(stream);

        /// <inheritdoc/>
        public abstract Task<StreamEvent> Fetch(StreamPosition streamPosition, CancellationToken cancellationToken);

        /// <inheritdoc/>
        public abstract Task<IEnumerable<StreamEvent>> FetchRange(StreamPositionRange range, CancellationToken cancellationToken);

        /// <inheritdoc/>
        public abstract Task<IEnumerable<Artifact>> FetchInRange(StreamPositionRange range, CancellationToken cancellationToken);

        /// <inheritdoc/>
        public abstract Task<IEnumerable<Artifact>> FetchInRangeAndPartition(PartitionId partitionId, StreamPositionRange range, CancellationToken cancellationToken);
    }
}