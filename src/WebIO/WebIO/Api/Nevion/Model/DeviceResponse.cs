// ReSharper disable ClassNeverInstantiated.Global

namespace WebIO.Api.Nevion.Model;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Response<T>
{
  [JsonPropertyName("_meta")] public Meta? Meta { get; set; }

  [JsonPropertyName("data")] public Data<T>? Data { get; set; }
}

public class Data<T>
{
  [JsonPropertyName("config")] public T? Config { get; set; }
}

public class DevmanConfig
{
  [JsonPropertyName("devman")] public Devman? Devman { get; set; }
}

public class Devman
{
  [JsonPropertyName("devices")] public Dictionary<string, Device>? Devices { get; set; }
}

public class Device
{
  [JsonPropertyName("active")] public bool Active { get; set; }

  [JsonPropertyName("cinfoOverrides")] public CinfoOverrides? CinfoOverrides { get; set; }

  [JsonPropertyName("config")] public DeviceConfig? Config { get; set; }

  [JsonPropertyName("id")] public string? Id { get; set; }

  [JsonPropertyName("meta")] public MetaClass? Meta { get; set; }
}

public class CinfoOverrides
{
  [JsonPropertyName("http")] public SnmpClass? Http { get; set; }

  [JsonPropertyName("snmp")] public SnmpClass? Snmp { get; set; }
}

public class SnmpClass
{
  [JsonPropertyName("id")] public string? Id { get; set; }

  [JsonPropertyName("useDefault")] public bool UseDefault { get; set; }
}

public class DeviceConfig
{
  [JsonPropertyName("cinfo")] public Cinfo? Cinfo { get; set; }

  [JsonPropertyName("customSettings")] public CustomSettings? CustomSettings { get; set; }

  [JsonPropertyName("desc")] public Descriptor? Desc { get; set; }

  [JsonPropertyName("driver")] public Driver? Driver { get; set; }
}

public class Cinfo
{
  [JsonPropertyName("address")] public string? Address { get; set; }

  [JsonPropertyName("altAddresses")] public List<object> AltAddresses { get; set; } = new();

  [JsonPropertyName("altAddressesWithAuth")]
  public List<object> AltAddressesWithAuth { get; set; } = new();

  [JsonPropertyName("auth")] public object? Auth { get; set; }

  [JsonPropertyName("http")] public CinfoHttp? Http { get; set; }

  [JsonPropertyName("protocols")] public MetaClass? Protocols { get; set; }

  [JsonPropertyName("snmp")] public Snmp? Snmp { get; set; }

  [JsonPropertyName("socketTimeout")] public object? SocketTimeout { get; set; }

  [JsonPropertyName("traps")] public Traps? Traps { get; set; }
}

public class CinfoHttp
{
  [JsonPropertyName("httpAuth")] public long HttpAuth { get; set; }

  [JsonPropertyName("https")] public bool Https { get; set; }

  [JsonPropertyName("trustAllCertificates")]
  public bool TrustAllCertificates { get; set; }
}

public class MetaClass
{
}

public class Snmp
{
  [JsonPropertyName("protocol")] public Protocol? Protocol { get; set; }

  [JsonPropertyName("security")] public Security? Security { get; set; }

  [JsonPropertyName("users")] public MetaClass? Users { get; set; }
}

public class Protocol
{
  [JsonPropertyName("localEngineId")] public string? LocalEngineId { get; set; }

  [JsonPropertyName("maxRepetitions")] public long MaxRepetitions { get; set; }

  [JsonPropertyName("preferredVersion")] public long PreferredVersion { get; set; }

  [JsonPropertyName("retries")] public long Retries { get; set; }

  [JsonPropertyName("timeout")] public long Timeout { get; set; }

  [JsonPropertyName("useGetBulk")] public bool UseGetBulk { get; set; }
}

public class Security
{
  [JsonPropertyName("read")] public Read? Read { get; set; }

  [JsonPropertyName("write")] public Read? Write { get; set; }
}

public class Read
{
  [JsonPropertyName("community")] public string? Community { get; set; }

  [JsonPropertyName("user")] public string? User { get; set; }
}

public class Traps
{
  [JsonPropertyName("trapDestinations")] public List<object> TrapDestinations { get; set; } = new();

  [JsonPropertyName("trapType")] public string? TrapType { get; set; }

  [JsonPropertyName("user")] public string? User { get; set; }
}

public class CustomSettings
{
  [JsonPropertyName("com.nevion.NMOS.always_enable_rtp")]
  public bool ComNevionNmosAlwaysEnableRtp { get; set; }

  [JsonPropertyName("com.nevion.NMOS.disable_rx_sdp")]
  public bool ComNevionNmosDisableRxSdp { get; set; }

  [JsonPropertyName("com.nevion.NMOS.disable_rx_sdp_with_null")]
  public bool ComNevionNmosDisableRxSdpWithNull { get; set; }

  [JsonPropertyName("com.nevion.NMOS.enable_bulk_config")]
  public bool ComNevionNmosEnableBulkConfig { get; set; }

  [JsonPropertyName("com.nevion.NMOS.port")]
  public long ComNevionNmosPort { get; set; }
}

public class Driver
{
  [JsonPropertyName("name")] public string? Name { get; set; }

  [JsonPropertyName("organization")] public string? Organization { get; set; }

  [JsonPropertyName("version")] public string? Version { get; set; }
}
