namespace WebIO.Api.Nevion;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Model;
using WebIO.Model;

public class NevionApi
{
  private readonly HttpClient _httpClient;

  private readonly Dictionary<string, string> _deviceNameCache = new();

  private readonly Dictionary<string, List<StreamMapping>> _streamMappingCache = new();

  public NevionApi(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<string> GetDeviceName(string dbDeviceName, bool useCache = true)
  {
    if (useCache && _deviceNameCache.TryGetValue(dbDeviceName, out var vipDeviceName))
      return vipDeviceName;

    var requestUri = $"/rest/v1/data/config/devman/devices/* where config.desc.label='{dbDeviceName}'/**";
    var json = await _httpClient.GetStringAsync(requestUri);
    if (json == null)
    {
      throw ApiNotReachableException();
    }

    var response = JsonSerializer.Deserialize<Response<DevmanConfig>>(json)!;
    vipDeviceName = response.Data!.Config!.Devman!.Devices!.Keys.SingleOrDefault() ?? string.Empty;

    if (useCache)
      _deviceNameCache.Add(dbDeviceName, vipDeviceName);

    return vipDeviceName;
  }

  public async Task<List<StreamMapping>?> GetStreamInfos(
    string dbDeviceName,
    bool useCache = true)
  {
    if (useCache && _streamMappingCache.TryGetValue(dbDeviceName, out var results))
      return results;

    results = new();

    var deviceId = await GetDeviceName(dbDeviceName, useCache);

    var requestUri = $"/rest/v1/data/config/network/nGraphElements/* where value.deviceId='{deviceId}'/**";
    var json = await _httpClient.GetStringAsync(requestUri);
    if (json == null)
    {
      throw ApiNotReachableException();
    }

    var response = JsonSerializer.Deserialize<Response<NetworkConfig>>(json);
    foreach (var (streamKey, stream) in response!.Data!.Config!.Network!.NGraphElements)
    {
      const int slotIndex = 2;
      const int channelIndex = 3;
      results.Add(new()
      {
        DestinationVipStreamKey = streamKey,
        DestinationVipSlot = stream.Value!.Gpid!.PointId[slotIndex],
        DestinationVipChannel = stream.Value.Gpid.PointId[channelIndex],
        DestinationVipCodecFormat = stream.Value.CodecFormat,
        DestinationVipCodecVertexType = stream.Value.VertexType,
        DestinationVipStreamLabel = stream.Value.FDescriptor!.Label,
        SourceWebIoStreamName = stream.Value.Descriptor!.Label,
      });
    }

    if (useCache)
      _streamMappingCache.Add(dbDeviceName, results);

    return results;
  }

  private static Exception ApiNotReachableException()
    => new("Cannot connect to nevion API!");
}