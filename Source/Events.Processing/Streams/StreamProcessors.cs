// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Threading;
using Dolittle.DependencyInversion;
using Dolittle.Lifecycle;
using Dolittle.Logging;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Events.Store.Streams;
using Dolittle.Runtime.Tenancy;

namespace Dolittle.Runtime.Events.Processing.Streams
{
    /// <summary>
    /// Represents an implementation of <see cref="IStreamProcessors" />.
    /// </summary>
    [Singleton]
    public class StreamProcessors : IStreamProcessors
    {
        readonly IPerformActionOnAllTenants _onAllTenants;
        readonly FactoryFor<IStreamProcessorStateRepository> _getStreamProcessorStates;
        readonly ConcurrentDictionary<StreamProcessorId, StreamProcessor> _streamProcessors;
        readonly FactoryFor<IEventFetchers> _getEventFetchers;
        readonly ILoggerManager _loggerManager;
        readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamProcessors"/> class.
        /// </summary>
        /// <param name="onAllTenants">The <see cref="IPerformActionOnAllTenants" />.</param>
        /// <param name="getStreamProcessorStates">The <see cref="FactoryFor{T}" /> <see cref="IStreamProcessorStateRepository" />.</param>
        /// <param name="getEventFetchers">The <see cref="FactoryFor{T}" /> <see cref="IEventFetchers" />.</param>
        /// <param name="loggerManager">The <see cref="ILoggerManager" />.</param>
        public StreamProcessors(
            IPerformActionOnAllTenants onAllTenants,
            FactoryFor<IStreamProcessorStateRepository> getStreamProcessorStates,
            FactoryFor<IEventFetchers> getEventFetchers,
            ILoggerManager loggerManager)
        {
            _onAllTenants = onAllTenants;
            _getStreamProcessorStates = getStreamProcessorStates;
            _streamProcessors = new ConcurrentDictionary<StreamProcessorId, StreamProcessor>();
            _getEventFetchers = getEventFetchers;
            _loggerManager = loggerManager;
            _logger = loggerManager.CreateLogger<StreamProcessors>();
        }

        /// <inheritdoc />
        public bool TryRegister(
            ScopeId scopeId,
            EventProcessorId eventProcessorId,
            IStreamDefinition streamDefinition,
            Func<IEventProcessor> getEventProcessor,
            CancellationToken cancellationToken,
            out StreamProcessor streamProcessor)
        {
            streamProcessor = default;
            var streamProcessorId = new StreamProcessorId(scopeId, eventProcessorId, streamDefinition.StreamId);
            if (_streamProcessors.ContainsKey(streamProcessorId))
            {
                _logger.Warning("Stream Processor with Id: '{streamProcessorId}' already registered", streamProcessorId);
                return false;
            }

            streamProcessor = new StreamProcessor(
                streamProcessorId,
                _onAllTenants,
                streamDefinition,
                getEventProcessor,
                () => _streamProcessors.TryRemove(streamProcessorId, out var _),
                _getStreamProcessorStates,
                _getEventFetchers,
                _loggerManager,
                cancellationToken);
            if (!_streamProcessors.TryAdd(streamProcessorId, streamProcessor))
            {
                _logger.Warning("Stream Processor with Id: '{streamProcessorId}' already registered", streamProcessorId);
                streamProcessor = default;
                return false;
            }

            _logger.Trace("Stream Processor with Id: '{streamProcessorId}' registered for Tenant: '{tenant}'", streamProcessorId);
            return true;
        }
    }
}