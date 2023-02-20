// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events.Store.Actors;
using Dolittle.Runtime.Events.Store.Streams;
using Google.Protobuf.WellKnownTypes;

namespace Dolittle.Runtime.Events.Processing.Streams;

/// <summary>
/// Represents the state of an <see cref="ScopedStreamProcessor" />.
/// </summary>
/// <param name="Position">The <see cref="StreamPosition"/>position of the stream.</param>
/// <param name="FailureReason">The reason for failing.</param>
/// <param name="RetryTime">The <see cref="DateTimeOffset" /> for when to retry processing.</param>
/// <param name="ProcessingAttempts">The number of times it has processed the Event at <see cref="Position" />.</param>
/// <param name="LastSuccessfullyProcessed">Timestamp of last successful Stream process.</param>
/// <param name="IsFailing">Whether the stream processor is failing.</param>
public record StreamProcessorState(ProcessingPosition Position, string FailureReason, DateTimeOffset RetryTime,
    uint ProcessingAttempts,
    DateTimeOffset LastSuccessfullyProcessed, bool IsFailing) : IStreamProcessorState
{
    // /// <summary>
    // /// Represents the state of an <see cref="ScopedStreamProcessor" />.
    // /// </summary>
    // /// <param name="position">The <see cref="ProcessingPosition"/>position of the stream.</param>
    // /// <param name="failureReason">The reason for failing.</param>
    // /// <param name="retryTime">The <see cref="DateTimeOffset" /> for when to retry processing.</param>
    // /// <param name="processingAttempts">The number of times it has processed the Event at <see cref="Position" />.</param>
    // /// <param name="lastSuccessfullyProcessed">Timestamp of last successful Stream process.</param>
    // /// <param name="isFailing">Whether the stream processor is failing.</param>
    // public StreamProcessorState(ProcessingPosition position, string failureReason, DateTimeOffset retryTime,
    //     uint processingAttempts,
    //     DateTimeOffset lastSuccessfullyProcessed, bool isFailing) : this(position, failureReason, retryTime,
    //     processingAttempts, lastSuccessfullyProcessed, isFailing)
    // {
    // }


    /// <summary>
    /// Initializes a new instance of the <see cref="StreamProcessorState"/> class.
    /// </summary>
    /// <param name="streamPosition">The <see cref="StreamPosition"/>position of the stream.</param>
    /// <param name="eventLogPosition"></param>
    /// <param name="lastSuccessfullyProcessed">Timestamp of last successful Stream process.</param>
    public StreamProcessorState(StreamPosition streamPosition, EventLogSequenceNumber eventLogPosition, DateTimeOffset lastSuccessfullyProcessed) : this(
        new ProcessingPosition(streamPosition, eventLogPosition), string.Empty,
        lastSuccessfullyProcessed, 0, lastSuccessfullyProcessed, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamProcessorState"/> class.
    /// </summary>
    /// <param name="position">The <see cref="ProcessingPosition"/>position of the stream.</param>
    /// <param name="lastSuccessfullyProcessed">Timestamp of last successful Stream process.</param>
    public StreamProcessorState(ProcessingPosition position, DateTimeOffset lastSuccessfullyProcessed) : this(
        new ProcessingPosition(position.StreamPosition, position.EventLogPosition), string.Empty,
        lastSuccessfullyProcessed, 0, lastSuccessfullyProcessed, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamProcessorState"/> class.
    /// </summary>
    /// <param name="streamPosition">The <see cref="StreamPosition"/>position of the stream.</param>
    /// <param name="eventLogPosition"></param>
    StreamProcessorState(StreamPosition streamPosition, EventLogSequenceNumber eventLogPosition) : this(
        new ProcessingPosition(streamPosition, eventLogPosition), string.Empty,
        DateTimeOffset.UtcNow, 0, DateTimeOffset.MinValue, false)
    {
    }

    // public ProcessingPosition ProcessingPosition => new(Position, EventLogPosition);

    /// <inheritdoc/>
    public bool Partitioned => false;

    public Bucket ToProtobuf()
    {
        var protobuf = new Bucket()
        {
            BucketId = 0,
            CurrentOffset = Position.StreamPosition.Value,
            CurrentEventLogOffset = Position.EventLogPosition.Value,
            LastSuccessfullyProcessed = Timestamp.FromDateTimeOffset(LastSuccessfullyProcessed),
            Partitioned = false
        };
        if (IsFailing)
        {
            protobuf.Failures.Add(new ProcessingFailure()
            {
                EventSourceId = PartitionId.None,
                FailureReason = FailureReason,
                RetryTime = Timestamp.FromDateTimeOffset(RetryTime),
                ProcessingAttempts = ProcessingAttempts,
                LastFailed = Timestamp.FromDateTimeOffset(LastSuccessfullyProcessed)
            });
        }

        return protobuf;
    }

    public ProcessingPosition EarliestProcessingPosition => Position;

    public bool TryGetTimespanToRetry(out TimeSpan timeToRetry)
    {
        timeToRetry = TimeSpan.MaxValue;
        if (IsFailing)
        {
            var retryTime = RetryTime;
            var timeSpan = retryTime.Subtract(DateTimeOffset.UtcNow);
            timeToRetry = timeSpan < TimeSpan.Zero ? TimeSpan.Zero : timeSpan;
            return true;
        }

        return false;
    }

    public IStreamProcessorState WithFailure(IProcessingResult failedProcessing, StreamEvent processedEvent, DateTimeOffset retryAt)
    {
        if (failedProcessing.Succeeded)
        {
            throw new ArgumentException("Processing result cannot be successful when adding a failing partition", nameof(failedProcessing));
        }
        
        return this with
        {
            ProcessingAttempts = ProcessingAttempts + 1,
            FailureReason = failedProcessing.FailureReason,
            RetryTime = retryAt,
            IsFailing = true
        };
    }

    public IStreamProcessorState WithSuccessfullyProcessed(StreamEvent processedEvent, DateTimeOffset timestamp) =>
        new StreamProcessorState(processedEvent.NextProcessingPosition, DateTimeOffset.UtcNow);

    bool RetryTimeIsInThePast(DateTimeOffset retryTime)
        => DateTimeOffset.UtcNow.CompareTo(retryTime) >= 0;
    
    public static StreamProcessorState New => new(StreamPosition.Start, EventLogSequenceNumber.Initial);
}
