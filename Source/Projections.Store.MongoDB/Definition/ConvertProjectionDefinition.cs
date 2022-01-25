// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Dolittle.Runtime.Events.Store;
using Dolittle.Runtime.Lifecycle;
using MongoDB.Bson;
using System.Collections.Generic;
using Dolittle.Runtime.Projections.Store.Definition.Copies;

namespace Dolittle.Runtime.Projections.Store.MongoDB.Definition;

/// <summary>
/// Represents an implementation of <see cref="IConvertProjectionDefinition" />.
/// </summary>
[Singleton]
public class ConvertProjectionDefinition : IConvertProjectionDefinition
{
    public Store.Definition.ProjectionDefinition ToRuntime(
        ProjectionId projection,
        ScopeId scope,
        IEnumerable<ProjectionEventSelector> eventSelectors,
        Store.State.ProjectionState initialState,
        ProjectionCopies copies)
        => new(
            projection,
            scope,
            eventSelectors.Select(_ => new Store.Definition.ProjectionEventSelector(
                _.EventType,
                _.EventKeySelectorType,
                _.EventKeySelectorExpression)),
            initialState,
            ToRuntimeCopies(copies));

    static Store.Definition.Copies.ProjectionCopySpecification ToRuntimeCopies(ProjectionCopies specification)
        => new(
            ToRuntimeCopyToMongoDB(specification.MongoDB));

    static Store.Definition.Copies.MongoDB.CopyToMongoDBSpecification ToRuntimeCopyToMongoDB(ProjectionCopyToMongoDB specification)
        => new(
            specification.ShouldCopyToMongoDB,
            specification.CollectionName,
            specification.Conversions.ToDictionary(_ => new ProjectionField(_.Key), _ => _.Value));
    
    public ProjectionDefinition ToStored(Store.Definition.ProjectionDefinition definition)
        => new()
        {
            Projection = definition.Projection,
            InitialStateRaw = definition.InitialState,
            InitialState = BsonDocument.Parse(definition.InitialState),
            EventSelectors = definition.Events.Select(_ => new ProjectionEventSelector
            {
                EventKeySelectorType = _.KeySelectorType,
                EventKeySelectorExpression = _.KeySelectorExpression,
                EventType = _.EventType,
            }).ToArray(),
            Copies = ToStoredCopies(definition.Copies),
        };

    static ProjectionCopies ToStoredCopies(Store.Definition.Copies.ProjectionCopySpecification specification)
        => new()
        {
            MongoDB = ToStoredCopyToMongoDB(specification.MongoDB),
        };

    static ProjectionCopyToMongoDB ToStoredCopyToMongoDB(Store.Definition.Copies.MongoDB.CopyToMongoDBSpecification specification)
        => new()
        {
            ShouldCopyToMongoDB = specification.ShouldCopyToMongoDB,
            CollectionName = specification.Collection,
            Conversions = specification.Conversions.ToDictionary(_ => _.Key.Value, _ => _.Value),
        };
}
