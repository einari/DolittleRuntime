// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: tunnel.proto
#pragma warning disable 1591
#region Designer generated code

using System;
using System.Threading;
using System.Threading.Tasks;
using grpc = global::Grpc.Core;

namespace Dolittle.Runtime.Events.Relativity {
  public static partial class QuantumTunnelService
  {
    static readonly string __ServiceName = "relativity.QuantumTunnelService";

    static readonly grpc::Marshaller<global::Dolittle.Runtime.Events.Relativity.OpenTunnelMessage> __Marshaller_OpenTunnelMessage = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Dolittle.Runtime.Events.Relativity.OpenTunnelMessage.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Dolittle.Runtime.Events.Relativity.CommittedEventStreamParticleMessage> __Marshaller_CommittedEventStreamParticleMessage = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Dolittle.Runtime.Events.Relativity.CommittedEventStreamParticleMessage.Parser.ParseFrom);

    static readonly grpc::Method<global::Dolittle.Runtime.Events.Relativity.OpenTunnelMessage, global::Dolittle.Runtime.Events.Relativity.CommittedEventStreamParticleMessage> __Method_Open = new grpc::Method<global::Dolittle.Runtime.Events.Relativity.OpenTunnelMessage, global::Dolittle.Runtime.Events.Relativity.CommittedEventStreamParticleMessage>(
        grpc::MethodType.ServerStreaming,
        __ServiceName,
        "Open",
        __Marshaller_OpenTunnelMessage,
        __Marshaller_CommittedEventStreamParticleMessage);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Dolittle.Runtime.Events.Relativity.TunnelReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of QuantumTunnelService</summary>
    public abstract partial class QuantumTunnelServiceBase
    {
      public virtual global::System.Threading.Tasks.Task Open(global::Dolittle.Runtime.Events.Relativity.OpenTunnelMessage request, grpc::IServerStreamWriter<global::Dolittle.Runtime.Events.Relativity.CommittedEventStreamParticleMessage> responseStream, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for QuantumTunnelService</summary>
    public partial class QuantumTunnelServiceClient : grpc::ClientBase<QuantumTunnelServiceClient>
    {
      /// <summary>Creates a new client for QuantumTunnelService</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public QuantumTunnelServiceClient(grpc::Channel channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for QuantumTunnelService that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public QuantumTunnelServiceClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected QuantumTunnelServiceClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected QuantumTunnelServiceClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual grpc::AsyncServerStreamingCall<global::Dolittle.Runtime.Events.Relativity.CommittedEventStreamParticleMessage> Open(global::Dolittle.Runtime.Events.Relativity.OpenTunnelMessage request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return Open(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncServerStreamingCall<global::Dolittle.Runtime.Events.Relativity.CommittedEventStreamParticleMessage> Open(global::Dolittle.Runtime.Events.Relativity.OpenTunnelMessage request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncServerStreamingCall(__Method_Open, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override QuantumTunnelServiceClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new QuantumTunnelServiceClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(QuantumTunnelServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_Open, serviceImpl.Open).Build();
    }

  }
}
#endregion
