// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
namespace Dolittle.Runtime.Events.Store.MongoDB.Migrations
{
    /// <summary>
    /// Defines a system that can migrate a <see cref="IMongoCollection{TDocument}" />
    /// </summary>
    public interface IMongoCollectionMigrator
    {
        Task MigrateCollection<TOld, TNew>(string collectionName, Func<TOld, TNew> converter, CancellationToken cancellationToken);
    }
}