// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Dolittle.Artifacts;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events.Store.Streams;

namespace Dolittle.Runtime.Events.Processing.Filters
{
    /// <summary>
    /// Represents the definition for <see cref="TypeFilterWithEventSourcePartition"/>.
    /// </summary>
    public class TypeFilterWithEventSourcePartitionDefinition : IPersistableFilterDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeFilterWithEventSourcePartitionDefinition"/> class.
        /// </summary>
        /// <param name="sourceStream">The source <see cref="StreamId" />.</param>
        /// <param name="targetStream">The target <see cref="StreamId" />.</param>
        /// <param name="types"><see cref="IEnumerable{T}"/> of <see cref="ArtifactId"/>.</param>
        /// <param name="partitioned">Whether or not to partition by <see cref="EventSourceId"/>.</param>
        public TypeFilterWithEventSourcePartitionDefinition(StreamId sourceStream, StreamId targetStream, IEnumerable<ArtifactId> types, bool partitioned)
        {
            SourceStream = sourceStream;
            TargetStream = targetStream;
            Types = types;
            Partitioned = partitioned;
        }

        /// <summary>
        /// Gets the artifacts included by the filter.
        /// </summary>
        public IEnumerable<ArtifactId> Types { get; }

        /// <summary>
        /// Gets a value indicating whether or not the filter is partitioned by <see cref="EventSourceId"/>.
        /// </summary>
        public bool Partitioned { get; }

        /// <inheritdoc/>.
        public StreamId SourceStream { get; }

        /// <inheritdoc/>.
        public StreamId TargetStream { get; }
    }
}