// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: service.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Common.Grpc {

  /// <summary>Holder for reflection information generated from service.proto</summary>
  public static partial class ServiceReflection {

    #region Descriptor
    /// <summary>File descriptor for service.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ServiceReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cg1zZXJ2aWNlLnByb3RvEgtDb21tb24uR3JwYyIGCgROb29wIrwCCg9TZXJ2",
            "aWNlTWVzc2FnZTISEQoJc2Vzc2lvbklkGAEgASgJEhEKCW1lc3NhZ2VJZBgC",
            "IAEoCRITCgtjb21tQ2hhbm5lbBgDIAEoCRITCgttZXNzYWdlSnNvbhgEIAEo",
            "CRIpCghzdGFtcE9uZRgFIAEoCzIXLkNvbW1vbi5HcnBjLlZpc2l0U3RhbXAS",
            "KQoIc3RhbXBUd28YBiABKAsyFy5Db21tb24uR3JwYy5WaXNpdFN0YW1wEisK",
            "CnN0YW1wVGhyZWUYByABKAsyFy5Db21tb24uR3JwYy5WaXNpdFN0YW1wEioK",
            "CXN0YW1wRm91chgIIAEoCzIXLkNvbW1vbi5HcnBjLlZpc2l0U3RhbXASKgoJ",
            "c3RhbXBGaXZlGAkgASgLMhcuQ29tbW9uLkdycGMuVmlzaXRTdGFtcCIuCgpW",
            "aXNpdFN0YW1wEg8KB3Zpc2l0ZWQYASABKAgSDwoHdGltZU5vdxgCIAEoAzJP",
            "ChJHcnBjTWVzc2FnZVNlcnZpY2USOQoEU2VuZBIcLkNvbW1vbi5HcnBjLlNl",
            "cnZpY2VNZXNzYWdlMhoRLkNvbW1vbi5HcnBjLk5vb3AiAGIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Common.Grpc.Noop), global::Common.Grpc.Noop.Parser, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Common.Grpc.ServiceMessage2), global::Common.Grpc.ServiceMessage2.Parser, new[]{ "SessionId", "MessageId", "CommChannel", "MessageJson", "StampOne", "StampTwo", "StampThree", "StampFour", "StampFive" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Common.Grpc.VisitStamp), global::Common.Grpc.VisitStamp.Parser, new[]{ "Visited", "TimeNow" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Noop : pb::IMessage<Noop> {
    private static readonly pb::MessageParser<Noop> _parser = new pb::MessageParser<Noop>(() => new Noop());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Noop> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Common.Grpc.ServiceReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Noop() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Noop(Noop other) : this() {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Noop Clone() {
      return new Noop(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Noop);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Noop other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Noop other) {
      if (other == null) {
        return;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
        }
      }
    }

  }

  public sealed partial class ServiceMessage2 : pb::IMessage<ServiceMessage2> {
    private static readonly pb::MessageParser<ServiceMessage2> _parser = new pb::MessageParser<ServiceMessage2>(() => new ServiceMessage2());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<ServiceMessage2> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Common.Grpc.ServiceReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ServiceMessage2() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ServiceMessage2(ServiceMessage2 other) : this() {
      sessionId_ = other.sessionId_;
      messageId_ = other.messageId_;
      commChannel_ = other.commChannel_;
      messageJson_ = other.messageJson_;
      StampOne = other.stampOne_ != null ? other.StampOne.Clone() : null;
      StampTwo = other.stampTwo_ != null ? other.StampTwo.Clone() : null;
      StampThree = other.stampThree_ != null ? other.StampThree.Clone() : null;
      StampFour = other.stampFour_ != null ? other.StampFour.Clone() : null;
      StampFive = other.stampFive_ != null ? other.StampFive.Clone() : null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ServiceMessage2 Clone() {
      return new ServiceMessage2(this);
    }

    /// <summary>Field number for the "sessionId" field.</summary>
    public const int SessionIdFieldNumber = 1;
    private string sessionId_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string SessionId {
      get { return sessionId_; }
      set {
        sessionId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "messageId" field.</summary>
    public const int MessageIdFieldNumber = 2;
    private string messageId_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string MessageId {
      get { return messageId_; }
      set {
        messageId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "commChannel" field.</summary>
    public const int CommChannelFieldNumber = 3;
    private string commChannel_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string CommChannel {
      get { return commChannel_; }
      set {
        commChannel_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "messageJson" field.</summary>
    public const int MessageJsonFieldNumber = 4;
    private string messageJson_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string MessageJson {
      get { return messageJson_; }
      set {
        messageJson_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "stampOne" field.</summary>
    public const int StampOneFieldNumber = 5;
    private global::Common.Grpc.VisitStamp stampOne_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Common.Grpc.VisitStamp StampOne {
      get { return stampOne_; }
      set {
        stampOne_ = value;
      }
    }

    /// <summary>Field number for the "stampTwo" field.</summary>
    public const int StampTwoFieldNumber = 6;
    private global::Common.Grpc.VisitStamp stampTwo_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Common.Grpc.VisitStamp StampTwo {
      get { return stampTwo_; }
      set {
        stampTwo_ = value;
      }
    }

    /// <summary>Field number for the "stampThree" field.</summary>
    public const int StampThreeFieldNumber = 7;
    private global::Common.Grpc.VisitStamp stampThree_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Common.Grpc.VisitStamp StampThree {
      get { return stampThree_; }
      set {
        stampThree_ = value;
      }
    }

    /// <summary>Field number for the "stampFour" field.</summary>
    public const int StampFourFieldNumber = 8;
    private global::Common.Grpc.VisitStamp stampFour_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Common.Grpc.VisitStamp StampFour {
      get { return stampFour_; }
      set {
        stampFour_ = value;
      }
    }

    /// <summary>Field number for the "stampFive" field.</summary>
    public const int StampFiveFieldNumber = 9;
    private global::Common.Grpc.VisitStamp stampFive_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Common.Grpc.VisitStamp StampFive {
      get { return stampFive_; }
      set {
        stampFive_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as ServiceMessage2);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(ServiceMessage2 other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (SessionId != other.SessionId) return false;
      if (MessageId != other.MessageId) return false;
      if (CommChannel != other.CommChannel) return false;
      if (MessageJson != other.MessageJson) return false;
      if (!object.Equals(StampOne, other.StampOne)) return false;
      if (!object.Equals(StampTwo, other.StampTwo)) return false;
      if (!object.Equals(StampThree, other.StampThree)) return false;
      if (!object.Equals(StampFour, other.StampFour)) return false;
      if (!object.Equals(StampFive, other.StampFive)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (SessionId.Length != 0) hash ^= SessionId.GetHashCode();
      if (MessageId.Length != 0) hash ^= MessageId.GetHashCode();
      if (CommChannel.Length != 0) hash ^= CommChannel.GetHashCode();
      if (MessageJson.Length != 0) hash ^= MessageJson.GetHashCode();
      if (stampOne_ != null) hash ^= StampOne.GetHashCode();
      if (stampTwo_ != null) hash ^= StampTwo.GetHashCode();
      if (stampThree_ != null) hash ^= StampThree.GetHashCode();
      if (stampFour_ != null) hash ^= StampFour.GetHashCode();
      if (stampFive_ != null) hash ^= StampFive.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (SessionId.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(SessionId);
      }
      if (MessageId.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(MessageId);
      }
      if (CommChannel.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(CommChannel);
      }
      if (MessageJson.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(MessageJson);
      }
      if (stampOne_ != null) {
        output.WriteRawTag(42);
        output.WriteMessage(StampOne);
      }
      if (stampTwo_ != null) {
        output.WriteRawTag(50);
        output.WriteMessage(StampTwo);
      }
      if (stampThree_ != null) {
        output.WriteRawTag(58);
        output.WriteMessage(StampThree);
      }
      if (stampFour_ != null) {
        output.WriteRawTag(66);
        output.WriteMessage(StampFour);
      }
      if (stampFive_ != null) {
        output.WriteRawTag(74);
        output.WriteMessage(StampFive);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (SessionId.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(SessionId);
      }
      if (MessageId.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(MessageId);
      }
      if (CommChannel.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(CommChannel);
      }
      if (MessageJson.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(MessageJson);
      }
      if (stampOne_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(StampOne);
      }
      if (stampTwo_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(StampTwo);
      }
      if (stampThree_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(StampThree);
      }
      if (stampFour_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(StampFour);
      }
      if (stampFive_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(StampFive);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(ServiceMessage2 other) {
      if (other == null) {
        return;
      }
      if (other.SessionId.Length != 0) {
        SessionId = other.SessionId;
      }
      if (other.MessageId.Length != 0) {
        MessageId = other.MessageId;
      }
      if (other.CommChannel.Length != 0) {
        CommChannel = other.CommChannel;
      }
      if (other.MessageJson.Length != 0) {
        MessageJson = other.MessageJson;
      }
      if (other.stampOne_ != null) {
        if (stampOne_ == null) {
          stampOne_ = new global::Common.Grpc.VisitStamp();
        }
        StampOne.MergeFrom(other.StampOne);
      }
      if (other.stampTwo_ != null) {
        if (stampTwo_ == null) {
          stampTwo_ = new global::Common.Grpc.VisitStamp();
        }
        StampTwo.MergeFrom(other.StampTwo);
      }
      if (other.stampThree_ != null) {
        if (stampThree_ == null) {
          stampThree_ = new global::Common.Grpc.VisitStamp();
        }
        StampThree.MergeFrom(other.StampThree);
      }
      if (other.stampFour_ != null) {
        if (stampFour_ == null) {
          stampFour_ = new global::Common.Grpc.VisitStamp();
        }
        StampFour.MergeFrom(other.StampFour);
      }
      if (other.stampFive_ != null) {
        if (stampFive_ == null) {
          stampFive_ = new global::Common.Grpc.VisitStamp();
        }
        StampFive.MergeFrom(other.StampFive);
      }
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
            SessionId = input.ReadString();
            break;
          }
          case 18: {
            MessageId = input.ReadString();
            break;
          }
          case 26: {
            CommChannel = input.ReadString();
            break;
          }
          case 34: {
            MessageJson = input.ReadString();
            break;
          }
          case 42: {
            if (stampOne_ == null) {
              stampOne_ = new global::Common.Grpc.VisitStamp();
            }
            input.ReadMessage(stampOne_);
            break;
          }
          case 50: {
            if (stampTwo_ == null) {
              stampTwo_ = new global::Common.Grpc.VisitStamp();
            }
            input.ReadMessage(stampTwo_);
            break;
          }
          case 58: {
            if (stampThree_ == null) {
              stampThree_ = new global::Common.Grpc.VisitStamp();
            }
            input.ReadMessage(stampThree_);
            break;
          }
          case 66: {
            if (stampFour_ == null) {
              stampFour_ = new global::Common.Grpc.VisitStamp();
            }
            input.ReadMessage(stampFour_);
            break;
          }
          case 74: {
            if (stampFive_ == null) {
              stampFive_ = new global::Common.Grpc.VisitStamp();
            }
            input.ReadMessage(stampFive_);
            break;
          }
        }
      }
    }

  }

  public sealed partial class VisitStamp : pb::IMessage<VisitStamp> {
    private static readonly pb::MessageParser<VisitStamp> _parser = new pb::MessageParser<VisitStamp>(() => new VisitStamp());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<VisitStamp> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Common.Grpc.ServiceReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public VisitStamp() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public VisitStamp(VisitStamp other) : this() {
      visited_ = other.visited_;
      timeNow_ = other.timeNow_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public VisitStamp Clone() {
      return new VisitStamp(this);
    }

    /// <summary>Field number for the "visited" field.</summary>
    public const int VisitedFieldNumber = 1;
    private bool visited_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Visited {
      get { return visited_; }
      set {
        visited_ = value;
      }
    }

    /// <summary>Field number for the "timeNow" field.</summary>
    public const int TimeNowFieldNumber = 2;
    private long timeNow_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public long TimeNow {
      get { return timeNow_; }
      set {
        timeNow_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as VisitStamp);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(VisitStamp other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Visited != other.Visited) return false;
      if (TimeNow != other.TimeNow) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Visited != false) hash ^= Visited.GetHashCode();
      if (TimeNow != 0L) hash ^= TimeNow.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Visited != false) {
        output.WriteRawTag(8);
        output.WriteBool(Visited);
      }
      if (TimeNow != 0L) {
        output.WriteRawTag(16);
        output.WriteInt64(TimeNow);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Visited != false) {
        size += 1 + 1;
      }
      if (TimeNow != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(TimeNow);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(VisitStamp other) {
      if (other == null) {
        return;
      }
      if (other.Visited != false) {
        Visited = other.Visited;
      }
      if (other.TimeNow != 0L) {
        TimeNow = other.TimeNow;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            Visited = input.ReadBool();
            break;
          }
          case 16: {
            TimeNow = input.ReadInt64();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
