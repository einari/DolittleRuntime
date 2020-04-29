// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Dolittle.Concepts;
using Dolittle.Runtime.Events.Store.Streams;

namespace Dolittle.Runtime.Events.Processing.Streams
{
    /// <summary>
    /// Represents the state of an <see cref="ScopedStreamProcessor" />.
    /// </summary>
    public class StreamProcessorState : Value<StreamProcessorState>, IStreamProcessorState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreamProcessorState"/> class.
        /// </summary>
        /// <param name="streamPosition">The <see cref="StreamPosition"/>position of the stream.</param>
        public StreamProcessorState(StreamPosition streamPosition)
        {
            IsFailing = false;
            Position = streamPosition;
            RetryTime = DateTimeOffset.UtcNow;
            FailureReason = string.Empty;
            ProcessingAttempts = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamProcessorState"/> class.
        /// </summary>
        /// <param name="streamPosition">The <see cref="StreamPosition"/>position of the stream.</param>
        /// <param name="failureReason">The reason for failing.</param>
        /// <param name="retryTime">The <see cref="DateTimeOffset" /> for when to retry processing.</param>
        /// <param name="processingAttempts">The number of times it has processed the Event at <see cref="Position" />.</param>
        public StreamProcessorState(StreamPosition streamPosition, string failureReason, DateTimeOffset retryTime, uint processingAttempts)
        {
            IsFailing = true;
            Position = streamPosition;
            RetryTime = retryTime;
            FailureReason = failureReason;
            ProcessingAttempts = processingAttempts;
        }

        /// <summary>
        /// Gets a new, initial, <see cref="StreamProcessorState" />.
        /// </summary>
        public static StreamProcessorState New =>
            new StreamProcessorState(StreamPosition.Start);

        /// <inheritdoc/>
        public bool Partitioned => false;

        /// <inheritdoc/>
        public StreamPosition Position { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ScopedStreamProcessor" /> is failing.
        /// </summary>
        public bool IsFailing { get; }

        /// <summary>
        /// Gets the <see cref="DateTimeOffset" /> for when to retry processing.
        /// </summary>
        public DateTimeOffset RetryTime { get; }

        /// <summary>
        /// Gets the reason for failure.
        /// </summary>
        public string FailureReason { get; }

        /// <summary>
        /// Gets the number of times that the event at position has been attempted processed.
        /// </summary>
        public uint ProcessingAttempts { get; }
    }
}
