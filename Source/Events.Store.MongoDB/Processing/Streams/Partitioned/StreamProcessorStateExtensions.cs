// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Dolittle.Runtime.Events.Store.Streams;

namespace Dolittle.Runtime.Events.Store.MongoDB.Processing.Streams.Partitioned
{
    /// <summary>
    /// Extension methods for stream processor.
    /// </summary>
    public static class StreamProcessorStateExtensions
    {
        /// <summary>
        /// Converts the <see cref="FailingPartitionState" /> to the runtime representation of <see cref="Runtime.Events.Processing.Streams.Partitioned.FailingPartitionState" />.
        /// </summary>
        /// <param name="state">The <see cref="FailingPartitionState" />.</param>
        /// <returns>The converted <see cref="Runtime.Events.Processing.Streams.Partitioned.FailingPartitionState" />.</returns>
        public static Runtime.Events.Processing.Streams.Partitioned.FailingPartitionState ToRuntimeRepresentation(this FailingPartitionState state) =>
            new Runtime.Events.Processing.Streams.Partitioned.FailingPartitionState(state.Position, state.RetryTime, state.Reason, state.ProcessingAttempts, state.LastFailed);
    }
}
