// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Lifecycle;
using Dolittle.Runtime.MongoDB;
using Dolittle.Runtime.Projections.Store.State;
using Dolittle.Runtime.Rudimentary;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Dolittle.Runtime.Projections.Store.MongoDB.State;

/// <summary>
/// Represents an implementation of <see cref="IProjectionStates" />.
/// </summary>
[SingletonPerTenant]
public class ProjectionStates : IProjectionStates
{
    readonly IProjections _projections;

    /// <summary>
    /// Initializes an instance of the <see cref="ProjectionStates" /> class.
    /// </summary>
    /// <param name="projections">The projections repository.</param>
    public ProjectionStates(IProjections projections)
    {
        _projections = projections;
    }

    /// <inheritdoc/>    
    public async Task<bool> TryDrop(ProjectionId projection, ScopeId scope, CancellationToken token)
    {
        try
        {
            var collection = await _projections.GetStates(scope, projection, token).ConfigureAwait(false);
            var deleteResult = await collection.DeleteManyAsync(Builders<Projection>.Filter.Empty, token).ConfigureAwait(false);
            return deleteResult.IsAcknowledged;
        }
        catch (MongoWaitQueueFullException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<Try<ProjectionState>> TryGet(ProjectionId projection, ScopeId scope, ProjectionKey key, CancellationToken token)
    {
        try
        {
            var collection = await _projections.GetStates(scope, projection, token).ConfigureAwait(false);
            var projectionState = await collection
                .Find(CreateKeyFilter(key))
                .Project(_ => _.ContentRaw)
                .SingleOrDefaultAsync(token)
                .ConfigureAwait(false);
            
            return string.IsNullOrEmpty(projectionState)
                ? Try<ProjectionState>.Failed(new ProjectionStateDoesNotExist(projection, key, scope))
                : new ProjectionState(projectionState);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public async Task<Try<IAsyncEnumerable<(ProjectionState State, ProjectionKey Key)>>> TryGetAll(ProjectionId projection, ScopeId scope, CancellationToken token)
    {
        try
        {
            var collection = await _projections.GetStates(scope, projection, token).ConfigureAwait(false);
            var states = collection
                .Find(Builders<Projection>.Filter.Empty)
                .Project(_ => new Tuple<ProjectionState, ProjectionKey>(_.ContentRaw, _.Key))
                .ToAsyncEnumerable(token);
            
            var result = states.Select(_ =>
            {
                return (_.Item1, _.Item2);
            });

            return Try<IAsyncEnumerable<(ProjectionState State, ProjectionKey Key)>>.Succeeded(result);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> TryRemove(ProjectionId projection, ScopeId scope, ProjectionKey key, CancellationToken token)
    {
        try
        {
            var collection = await _projections.GetStates(scope, projection, token).ConfigureAwait(false);
            var deleteResult = await collection.DeleteOneAsync(CreateKeyFilter(key), token).ConfigureAwait(false);
            return deleteResult.IsAcknowledged;
        }
        catch (MongoWaitQueueFullException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> TryReplace(ProjectionId projection, ScopeId scope, ProjectionKey key, ProjectionState state, CancellationToken token)
    {
        try
        {
            var collection = await _projections.GetStates(scope, projection, token).ConfigureAwait(false);
            var filter = CreateKeyFilter(key);
            var updateDefinition = Builders<Projection>
                .Update
                .Set(_ => _.Content, BsonDocument.Parse(state.Value))
                .Set(_ => _.ContentRaw, state.Value);
            var updateResult = await collection.UpdateOneAsync(
                filter,
                updateDefinition,
                new UpdateOptions { IsUpsert = true },
                token).ConfigureAwait(false);
            return updateResult.IsAcknowledged;
        }
        catch (MongoWaitQueueFullException)
        {
            return false;
        }
    }

    FilterDefinition<Projection> CreateKeyFilter(ProjectionKey key) => Builders<Projection>.Filter.Eq(_ => _.Key, key.Value);
}
