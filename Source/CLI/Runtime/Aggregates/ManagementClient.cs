// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dolittle.Runtime.Aggregates;
using Dolittle.Runtime.Aggregates.Management;
using Dolittle.Runtime.Aggregates.Management.Contracts;
using Dolittle.Runtime.ApplicationModel;
using Dolittle.Runtime.Artifacts;
using Dolittle.Runtime.CLI.Runtime.EventHandlers;
using Dolittle.Runtime.Events.Contracts;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Microservices;
using Dolittle.Runtime.Protobuf;
using Dolittle.Runtime.Rudimentary;
using static Dolittle.Runtime.Aggregates.Management.Contracts.AggregateRoots;
using AggregateRoot = Dolittle.Runtime.Aggregates.Management.Contracts.AggregateRoot;
using CommittedAggregateEvents = Dolittle.Runtime.Events.Store.CommittedAggregateEvents;

namespace Dolittle.Runtime.CLI.Runtime.Aggregates
{

    /// <summary>
    /// Represents an implementation of <see cref="IManagementClient"/>.
    /// </summary>
    public class ManagementClient : IManagementClient
    {
        readonly ICanCreateClients _clients;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagementClient"/> class.
        /// </summary>
        /// <param name="clients">The client creator to us to create clients that connect to the Runtime.</param>
        public ManagementClient(ICanCreateClients clients)
        {
            _clients = clients;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AggregateRootWithTenantScopedInstances>> GetAll(MicroserviceAddress runtime, TenantId tenant = null)
        {
            var client = _clients.CreateClientFor<AggregateRootsClient>(runtime);
            var request = new GetAllRequest
            {
                TenantId = tenant?.ToProtobuf()
            };

            var response = await client.GetAllAsync(request);

            if (response.Failure is not null)
            {
                throw new GetAllFailed(response.Failure.Reason);
            }
            return response.AggregateRoots.Select(FromProtobuf);
        }

        /// <inheritdoc />
        public async Task<Try<AggregateRootWithTenantScopedInstances>> Get(MicroserviceAddress runtime, ArtifactId aggregateRootId, TenantId tenant = null)
        {
            var client = _clients.CreateClientFor<AggregateRootsClient>(runtime);
            var request = new GetOneRequest()
            {
                TenantId = tenant?.ToProtobuf(),
                AggregateRootId = aggregateRootId.ToProtobuf()
            };

            var response = await client.GetOneAsync(request);

            if (response.Failure is not null)
            {
                return new GetOneFailed(response.Failure.Reason);
            }
            return FromProtobuf(response.AggregateRoot);
        }

        /// <inheritdoc />
        public async Task<CommittedAggregateEvents> GetEvents(MicroserviceAddress runtime, ArtifactId aggregateRootId, EventSourceId eventSourceId, TenantId tenant = null)
        {
            var client = _clients.CreateClientFor<AggregateRootsClient>(runtime);
            var request = new GetEventsRequest()
            {
                TenantId = tenant?.ToProtobuf(),
                Aggregate = new Aggregate
                {
                    AggregateRootId = aggregateRootId.ToProtobuf(),
                    EventSourceId = eventSourceId
                }
            };

            var response = await client.GetEventsAsync(request);

            if (response.Failure is not null)
            {
                throw new GetEventsFailed(response.Failure.Reason);
            }
            return FromProtobuf(response.Events);
        }

        static AggregateRootWithTenantScopedInstances FromProtobuf(AggregateRoot root)
            => new(
                new Dolittle.Runtime.Aggregates.AggregateRoot(
                    root.AggregateRoot_.ToArtifact(),
                    root.Alias),
                root.EventSources.Select(_ => new TenantScopedAggregateRootInstance(_.TenantId.ToGuid(), new AggregateRootInstance(_.EventSourceId, _.AggregateRootVersion))));

        static CommittedAggregateEvents FromProtobuf(Dolittle.Runtime.Events.Contracts.CommittedAggregateEvents events)
            => new(
                events.EventSourceId,
                events.AggregateRootId.ToGuid(),
                events.Events.Select(_ => new CommittedAggregateEvent(
                    new Artifact(events.AggregateRootId.ToGuid(), ArtifactGeneration.First),
                    events.AggregateRootVersion,
                    _.EventLogSequenceNumber,
                    _.Occurred.ToDateTimeOffset(),
                    events.EventSourceId,
                    _.ExecutionContext.ToExecutionContext(),
                    _.EventType.ToArtifact(),
                    _.Public,
                    _.Content)).ToList());
    }
}
