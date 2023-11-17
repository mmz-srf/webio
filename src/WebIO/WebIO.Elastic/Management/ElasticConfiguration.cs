// ReSharper disable ClassNeverInstantiated.Global

namespace WebIO.Elastic.Management;

public record ElasticConfiguration
{
  public Uri Url { get; init; } = new("http://localhost:9200");
  public string? Username { get; init; }
  public string? Password { get; init; }

  public string? IndexPrefix { get; init; }
  public int BatchSize { get; init; } = 1000;
  public int NumberOfShards { get; init; } = 1;
  public int NumberOfReplias { get; init; } = 0;
  public int RefreshInterval { get; init; } = 1;

  public Uri? Proxy { get; init; } = null;
  public string? ProxyUser { get; set; }
  public string? ProxyPassword { get; set; }
}
