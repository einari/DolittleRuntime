// Copyright (c) Dolittle. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Dolittle.Runtime.Events.Processing.Contracts;
using Dolittle.Runtime.Protobuf;
using Dolittle.Services.Contracts;
using RuntimeProjectionEventSelector = Dolittle.Runtime.Projections.Store.Definition.ProjectionEventSelector;
using Dolittle.Runtime.Projections.Store.Definition;
using Dolittle.Runtime.Projections.Store.Definition.Copies;
using Dolittle.Runtime.Projections.Store.Definition.Copies.MongoDB;
using Dolittle.Runtime.Services;

namespace Dolittle.Runtime.Events.Processing.Projections;

/// <summary>
/// Represents the <see cref="IProjectionsProtocol" />.
/// </summary>
public class ProjectionsProtocol : IProjectionsProtocol
{
    /// <inheritdoc/>
    public ProjectionRegistrationArguments ConvertConnectArguments(ProjectionRegistrationRequest arguments)
        => new(
            arguments.CallContext.ExecutionContext.ToExecutionContext(),
            new ProjectionDefinition(
                arguments.ProjectionId.ToGuid(),
                arguments.ScopeId.ToGuid(),
                arguments.Events.Select(eventSelector => eventSelector.SelectorCase switch
                {
                    Contracts.ProjectionEventSelector.SelectorOneofCase.EventSourceKeySelector => RuntimeProjectionEventSelector.EventSourceId(eventSelector.EventType.Id.ToGuid()),
                    Contracts.ProjectionEventSelector.SelectorOneofCase.PartitionKeySelector => RuntimeProjectionEventSelector.PartitionId(eventSelector.EventType.Id.ToGuid()),
                    Contracts.ProjectionEventSelector.SelectorOneofCase.EventPropertyKeySelector => RuntimeProjectionEventSelector.EventProperty(eventSelector.EventType.Id.ToGuid(), eventSelector.EventPropertyKeySelector.PropertyName),
                    Contracts.ProjectionEventSelector.SelectorOneofCase.StaticKeySelector => RuntimeProjectionEventSelector.Static(eventSelector.EventType.Id.ToGuid(), eventSelector.StaticKeySelector.StaticKey),
                    Contracts.ProjectionEventSelector.SelectorOneofCase.EventOccurredKeySelector => RuntimeProjectionEventSelector.Occurred(eventSelector.EventType.Id.ToGuid(), eventSelector.EventOccurredKeySelector.Format),
                    _ => throw new InvalidProjectionEventSelector(eventSelector.SelectorCase)
                }),
                arguments.InitialState,
                ConvertCopySpecification(arguments.Copies)
            ));

    /// <inheritdoc/>
    public ProjectionRegistrationResponse CreateFailedConnectResponse(FailureReason failureMessage)
        => new() { Failure = new Dolittle.Protobuf.Contracts.Failure { Id = ProjectionFailures.FailedToRegisterProjection.Value.ToProtobuf(), Reason = failureMessage } };

    /// <inheritdoc/>
    public ReverseCallArgumentsContext GetArgumentsContext(ProjectionRegistrationRequest message)
        => message.CallContext;

    /// <inheritdoc/>
    public ProjectionRegistrationRequest GetConnectArguments(ProjectionClientToRuntimeMessage message)
        => message.RegistrationRequest;

    /// <inheritdoc/>
    public Pong GetPong(ProjectionClientToRuntimeMessage message)
        => message.Pong;

    /// <inheritdoc/>
    public ProjectionResponse GetResponse(ProjectionClientToRuntimeMessage message)
        => message.HandleResult;

    /// <inheritdoc/>
    public ReverseCallResponseContext GetResponseContext(ProjectionResponse message)
        => message.CallContext;

    /// <inheritdoc/>
    public void SetConnectResponse(ProjectionRegistrationResponse arguments, ProjectionRuntimeToClientMessage message)
        => message.RegistrationResponse = arguments;

    /// <inheritdoc/>
    public void SetPing(ProjectionRuntimeToClientMessage message, Ping ping)
        => message.Ping = ping;

    /// <inheritdoc/>
    public void SetRequest(ProjectionRequest request, ProjectionRuntimeToClientMessage message)
        => message.HandleRequest = request;

    /// <inheritdoc/>
    public void SetRequestContext(ReverseCallRequestContext context, ProjectionRequest request)
        => request.CallContext = context;

    /// <inheritdoc/>
    public ConnectArgumentsValidationResult ValidateConnectArguments(ProjectionRegistrationArguments arguments)
    {
        foreach (var eventType in arguments.ProjectionDefinition.Events.GroupBy(_ => _.EventType))
        {
            if (eventType.Count() > 1)
            {
                return ConnectArgumentsValidationResult.Failed($"Event {eventType.Key.Value} was specified more than once");
            }
        }
        return ConnectArgumentsValidationResult.Ok;
    }

    static ProjectionCopySpecification ConvertCopySpecification(ProjectionCopies copies)
    {
        var mongoDB = CopyToMongoDBSpecification.Default;
        if (copies?.MongoDB is { } copyToMongoDb)
        {
            mongoDB = new CopyToMongoDBSpecification(true, copyToMongoDb.Collection, ConvertPropertyConversions(copyToMongoDb.Conversions));
        }

        return new ProjectionCopySpecification(mongoDB);
    }

    static PropertyConversion[] ConvertPropertyConversions(IEnumerable<ProjectionCopyToMongoDB.Types.PropertyConversion> conversions)
        => conversions.Select(conversion =>
            new PropertyConversion(
                conversion.PropertyName,
                conversion.ConvertTo switch
                {
                    ProjectionCopyToMongoDB.Types.BSONType.None => ConversionBSONType.None,
                    
                    ProjectionCopyToMongoDB.Types.BSONType.DateAsDate => ConversionBSONType.DateAsDate,
                    ProjectionCopyToMongoDB.Types.BSONType.DateAsArray => ConversionBSONType.DateAsArray,
                    ProjectionCopyToMongoDB.Types.BSONType.DateAsDocument => ConversionBSONType.DateAsDocument,
                    ProjectionCopyToMongoDB.Types.BSONType.DateAsString => ConversionBSONType.DateAsString,
                    ProjectionCopyToMongoDB.Types.BSONType.DateAsInt64 => ConversionBSONType.DateAsInt64,
                    
                    ProjectionCopyToMongoDB.Types.BSONType.GuidasStandardBinary => ConversionBSONType.GuidAsStandardBinary,
                    ProjectionCopyToMongoDB.Types.BSONType.GuidasCsharpLegacyBinary => ConversionBSONType.GuidAsCsharpLegacyBinary,
                    ProjectionCopyToMongoDB.Types.BSONType.GuidasString => ConversionBSONType.GuidAsString,
                    
                    _ => throw new InvalidMongoDBFieldConversion(conversion.PropertyName, conversion.ConvertTo),
                },
                conversion.RenameTo != default,
                conversion.RenameTo ?? "",
                ConvertPropertyConversions(conversion.Children))
            ).ToArray();
}
