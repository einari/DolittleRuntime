// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: dolittle/events.relativity/open_tunnel.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Dolittle.Runtime.Events.Relativity.Protobuf {

  /// <summary>Holder for reflection information generated from dolittle/events.relativity/open_tunnel.proto</summary>
  public static partial class OpenTunnelReflection {

    #region Descriptor
    /// <summary>File descriptor for dolittle/events.relativity/open_tunnel.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static OpenTunnelReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cixkb2xpdHRsZS9ldmVudHMucmVsYXRpdml0eS9vcGVuX3R1bm5lbC5wcm90",
            "bxIaZG9saXR0bGUuZXZlbnRzLnJlbGF0aXZpdHkaE2RvbGl0dGxlL2d1aWQu",
            "cHJvdG8aKWRvbGl0dGxlL2V2ZW50cy5yZWxhdGl2aXR5L2FydGlmYWN0LnBy",
            "b3RvIsEBCgpPcGVuVHVubmVsEiMKC2FwcGxpY2F0aW9uGAEgASgLMg4uZG9s",
            "aXR0bGUuZ3VpZBImCg5ib3VuZGVkQ29udGV4dBgCIAEoCzIOLmRvbGl0dGxl",
            "Lmd1aWQSIAoIY2xpZW50SWQYAyABKAsyDi5kb2xpdHRsZS5ndWlkEg4KBm9m",
            "ZnNldBgEIAEoBBI0CgZldmVudHMYBSADKAsyJC5kb2xpdHRsZS5ldmVudHMu",
            "cmVsYXRpdml0eS5BcnRpZmFjdEIuqgIrRG9saXR0bGUuUnVudGltZS5FdmVu",
            "dHMuUmVsYXRpdml0eS5Qcm90b2J1ZmIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::System.Protobuf.GuidReflection.Descriptor, global::Dolittle.Runtime.Events.Relativity.Protobuf.ArtifactReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Dolittle.Runtime.Events.Relativity.Protobuf.OpenTunnel), global::Dolittle.Runtime.Events.Relativity.Protobuf.OpenTunnel.Parser, new[]{ "Application", "BoundedContext", "ClientId", "Offset", "Events" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  /// Open tunnel is the message sent when opening a tunnel from a singularity towards an event horizon
  /// </summary>
  public sealed partial class OpenTunnel : pb::IMessage<OpenTunnel> {
    private static readonly pb::MessageParser<OpenTunnel> _parser = new pb::MessageParser<OpenTunnel>(() => new OpenTunnel());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<OpenTunnel> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Dolittle.Runtime.Events.Relativity.Protobuf.OpenTunnelReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OpenTunnel() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OpenTunnel(OpenTunnel other) : this() {
      Application = other.application_ != null ? other.Application.Clone() : null;
      BoundedContext = other.boundedContext_ != null ? other.BoundedContext.Clone() : null;
      ClientId = other.clientId_ != null ? other.ClientId.Clone() : null;
      offset_ = other.offset_;
      events_ = other.events_.Clone();
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public OpenTunnel Clone() {
      return new OpenTunnel(this);
    }

    /// <summary>Field number for the "application" field.</summary>
    public const int ApplicationFieldNumber = 1;
    private global::System.Protobuf.guid application_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::System.Protobuf.guid Application {
      get { return application_; }
      set {
        application_ = value;
      }
    }

    /// <summary>Field number for the "boundedContext" field.</summary>
    public const int BoundedContextFieldNumber = 2;
    private global::System.Protobuf.guid boundedContext_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::System.Protobuf.guid BoundedContext {
      get { return boundedContext_; }
      set {
        boundedContext_ = value;
      }
    }

    /// <summary>Field number for the "clientId" field.</summary>
    public const int ClientIdFieldNumber = 3;
    private global::System.Protobuf.guid clientId_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::System.Protobuf.guid ClientId {
      get { return clientId_; }
      set {
        clientId_ = value;
      }
    }

    /// <summary>Field number for the "offset" field.</summary>
    public const int OffsetFieldNumber = 4;
    private ulong offset_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong Offset {
      get { return offset_; }
      set {
        offset_ = value;
      }
    }

    /// <summary>Field number for the "events" field.</summary>
    public const int EventsFieldNumber = 5;
    private static readonly pb::FieldCodec<global::Dolittle.Runtime.Events.Relativity.Protobuf.Artifact> _repeated_events_codec
        = pb::FieldCodec.ForMessage(42, global::Dolittle.Runtime.Events.Relativity.Protobuf.Artifact.Parser);
    private readonly pbc::RepeatedField<global::Dolittle.Runtime.Events.Relativity.Protobuf.Artifact> events_ = new pbc::RepeatedField<global::Dolittle.Runtime.Events.Relativity.Protobuf.Artifact>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Dolittle.Runtime.Events.Relativity.Protobuf.Artifact> Events {
      get { return events_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as OpenTunnel);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(OpenTunnel other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Application, other.Application)) return false;
      if (!object.Equals(BoundedContext, other.BoundedContext)) return false;
      if (!object.Equals(ClientId, other.ClientId)) return false;
      if (Offset != other.Offset) return false;
      if(!events_.Equals(other.events_)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (application_ != null) hash ^= Application.GetHashCode();
      if (boundedContext_ != null) hash ^= BoundedContext.GetHashCode();
      if (clientId_ != null) hash ^= ClientId.GetHashCode();
      if (Offset != 0UL) hash ^= Offset.GetHashCode();
      hash ^= events_.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (application_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Application);
      }
      if (boundedContext_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(BoundedContext);
      }
      if (clientId_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(ClientId);
      }
      if (Offset != 0UL) {
        output.WriteRawTag(32);
        output.WriteUInt64(Offset);
      }
      events_.WriteTo(output, _repeated_events_codec);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (application_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Application);
      }
      if (boundedContext_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(BoundedContext);
      }
      if (clientId_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(ClientId);
      }
      if (Offset != 0UL) {
        size += 1 + pb::CodedOutputStream.ComputeUInt64Size(Offset);
      }
      size += events_.CalculateSize(_repeated_events_codec);
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(OpenTunnel other) {
      if (other == null) {
        return;
      }
      if (other.application_ != null) {
        if (application_ == null) {
          application_ = new global::System.Protobuf.guid();
        }
        Application.MergeFrom(other.Application);
      }
      if (other.boundedContext_ != null) {
        if (boundedContext_ == null) {
          boundedContext_ = new global::System.Protobuf.guid();
        }
        BoundedContext.MergeFrom(other.BoundedContext);
      }
      if (other.clientId_ != null) {
        if (clientId_ == null) {
          clientId_ = new global::System.Protobuf.guid();
        }
        ClientId.MergeFrom(other.ClientId);
      }
      if (other.Offset != 0UL) {
        Offset = other.Offset;
      }
      events_.Add(other.events_);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            if (application_ == null) {
              application_ = new global::System.Protobuf.guid();
            }
            input.ReadMessage(application_);
            break;
          }
          case 18: {
            if (boundedContext_ == null) {
              boundedContext_ = new global::System.Protobuf.guid();
            }
            input.ReadMessage(boundedContext_);
            break;
          }
          case 26: {
            if (clientId_ == null) {
              clientId_ = new global::System.Protobuf.guid();
            }
            input.ReadMessage(clientId_);
            break;
          }
          case 32: {
            Offset = input.ReadUInt64();
            break;
          }
          case 42: {
            events_.AddEntriesFrom(input, _repeated_events_codec);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
