// ReSharper disable ClassNeverInstantiated.Global

// ReSharper disable CollectionNeverUpdated.Global

namespace WebIO.Api.Nevion.Model;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class NetworkConfig
{
  [JsonPropertyName("network")] public Network? Network { get; set; }
}

public class Network
{
  [JsonPropertyName("nGraphElements")] public Dictionary<string, NGraphElement> NGraphElements { get; set; } = new();
}

public class NGraphElement
{
  [JsonPropertyName("key")] public string? Key { get; set; }

  [JsonPropertyName("rev")] public string? Rev { get; set; }

  [JsonPropertyName("value")] public Value? Value { get; set; }
}

public class Value
{
  [JsonPropertyName("active")] public bool Active { get; set; }

  [JsonPropertyName("bidirPartnerId")] public object? BidirPartnerId { get; set; }

  [JsonPropertyName("codecFormat")] public string? CodecFormat { get; set; }

  [JsonPropertyName("configPriority")] public string? ConfigPriority { get; set; }

  [JsonPropertyName("control")] public string? Control { get; set; }

  [JsonPropertyName("custom")] public Custom? Custom { get; set; }

  [JsonPropertyName("descriptor")] public Descriptor? Descriptor { get; set; }

  [JsonPropertyName("deviceId")] public string? DeviceId { get; set; }

  [JsonPropertyName("extraAlertFilters")]
  public List<object> ExtraAlertFilters { get; set; } = new();

  [JsonPropertyName("fDescriptor")] public Descriptor? FDescriptor { get; set; }

  [JsonPropertyName("gpid")] public Gpid? Gpid { get; set; }

  [JsonPropertyName("imgUrl")] public string? ImgUrl { get; set; }

  [JsonPropertyName("isIgmpSource")] public bool IsIgmpSource { get; set; }

  [JsonPropertyName("isVirtual")] public bool IsVirtual { get; set; }

  [JsonPropertyName("mainDstIp")] public object? MainDstIp { get; set; }

  [JsonPropertyName("mainDstMac")] public object? MainDstMac { get; set; }

  [JsonPropertyName("mainDstPort")] public object? MainDstPort { get; set; }

  [JsonPropertyName("mainDstVlan")] public object? MainDstVlan { get; set; }

  [JsonPropertyName("mainSrcGateway")] public object? MainSrcGateway { get; set; }

  [JsonPropertyName("mainSrcIp")] public object? MainSrcIp { get; set; }

  [JsonPropertyName("mainSrcMac")] public object? MainSrcMac { get; set; }

  [JsonPropertyName("mainSrcNetmask")] public object? MainSrcNetmask { get; set; }

  [JsonPropertyName("maps")] public List<object> Maps { get; set; } = new();

  [JsonPropertyName("partnerConfig")] public object? PartnerConfig { get; set; }

  [JsonPropertyName("public")] public bool Public { get; set; }

  [JsonPropertyName("sdpSupport")] public bool SdpSupport { get; set; }

  [JsonPropertyName("serviceId")] public object? ServiceId { get; set; }

  [JsonPropertyName("sipsMode")] public string? SipsMode { get; set; }

  [JsonPropertyName("spareDstIp")] public object? SpareDstIp { get; set; }

  [JsonPropertyName("spareDstMac")] public object? SpareDstMac { get; set; }

  [JsonPropertyName("spareDstPort")] public object? SpareDstPort { get; set; }

  [JsonPropertyName("spareDstVlan")] public object? SpareDstVlan { get; set; }

  [JsonPropertyName("spareSrcGateway")] public object? SpareSrcGateway { get; set; }

  [JsonPropertyName("spareSrcIp")] public object? SpareSrcIp { get; set; }

  [JsonPropertyName("spareSrcMac")] public object? SpareSrcMac { get; set; }

  [JsonPropertyName("spareSrcNetmask")] public object? SpareSrcNetmask { get; set; }

  [JsonPropertyName("tags")] public List<string> Tags { get; set; } = new();

  [JsonPropertyName("type")] public string? Type { get; set; }

  [JsonPropertyName("useAsEndpoint")] public bool UseAsEndpoint { get; set; }

  [JsonPropertyName("vertexType")] public string? VertexType { get; set; }
}

public class Custom
{
}

public class Descriptor
{
  [JsonPropertyName("desc")] public string? Desc { get; set; }

  [JsonPropertyName("label")] public string? Label { get; set; }
}

public class Gpid
{
  [JsonPropertyName("component")] public long Component { get; set; }

  [JsonPropertyName("pointId")] public List<string> PointId { get; set; } = new();
}

public class Meta
{
  [JsonPropertyName("info")] public Info? Info { get; set; }

  [JsonPropertyName("type")] public string? Type { get; set; }
}

public class Info
{
  [JsonPropertyName("auth")] public bool Auth { get; set; }

  [JsonPropertyName("code")] public long Code { get; set; }

  [JsonPropertyName("desc")] public string? Desc { get; set; }

  [JsonPropertyName("internalDetails")] public List<object> InternalDetails { get; set; } = new();

  [JsonPropertyName("msg")] public string? Msg { get; set; }

  [JsonPropertyName("ok")] public bool Ok { get; set; }

  [JsonPropertyName("requestDetails")] public List<object> RequestDetails { get; set; } = new();

  [JsonPropertyName("user")] public string? User { get; set; }
}
