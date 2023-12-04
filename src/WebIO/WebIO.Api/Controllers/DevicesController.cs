namespace WebIO.Api.Controllers;

using Auth;
using DataAccess;
using Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Model;
using Model.DeviceTemplates;
using Model.Display;
using Model.Readonly;
using MoreLinq;

[Route("api/[controller]")]
[Authorize]
[RequiredScope(Claims.CanRead)]
[ApiController]
public class DevicesController : ControllerBase
{
  private readonly IDeviceRepository _deviceRepository;
  private readonly IMetadataRepository _metadataRepository;

  public DevicesController(IDeviceRepository deviceRepository, IMetadataRepository metadataRepository)
  {
    _deviceRepository = deviceRepository;
    _metadataRepository = metadataRepository;
  }

  // Get api/devices/name/details
  [HttpGet("{deviceId:guid}/details")]
  public ActionResult<DeviceDetailsDto> GetDeviceDetails(Guid deviceId)
  {
    var device = _deviceRepository.GetDevice(deviceId);
    if (device == null)
    {
      return NotFound($"Device with id {deviceId} not found");
    }

    var deviceType = _metadataRepository.DeviceTypes.FirstOrDefault(t => t.Name == device.DeviceType);
    if (deviceType == null)
    {
      return base.BadRequest($"Device type {device.DeviceType} not found");
    }

    var result = new DeviceDetailsDto
    {
      Id = deviceId.ToString(),
      Name = device.Name,
      DeviceType = MapToDto(deviceType),
      Interfaces = device.Interfaces
        .Select(i => new InterfaceInfoDto
        {
          Id = i.Id.ToString(),
          Name = i.Name,
          Template = i.InterfaceTemplate!,
          Streams = MapToDto(i.GetStreamCardinality()),
        })
        .ToList(),
      Comment = device.Comment,
    };

    return Ok(result);
  }
  
  [HttpPost]
  public ActionResult<QueryResultDto<DeviceDto>> Get(
    int start = 0,
    int count = 100,
    string? sort = null,
    string? sortOrder = null,
    string? global = null,
    [FromBody] Dictionary<string, string>? filter = null)
  {
    filter ??= new();
      
    var query = new Query(start, count)
      .WithSorting(sort, sortOrder)
      .WithFilter(filter, global);

    var deviceInfos = _deviceRepository.GetDeviceInfos(query);

    query = new Query(0, 0)
      .WithFilter(filter, global);

    var totalCount = _deviceRepository.GetDeviceCount(query);

    var result = new QueryResultDto<DeviceDto>
    {
      State = QueryResultState.Success,
      Count = deviceInfos.Count,
      Start = deviceInfos.StartIndex,
      TotalCount = totalCount,
      Data = deviceInfos.Data.Select(MapToDto).ToList(),
    };

    return Ok(result);
  }

  [HttpGet("isDuplicate")]
  public ActionResult<bool> IsDuplicate(string deviceName, string ownId)
  {
    return _deviceRepository.IsDuplicateDeviceName(deviceName,
      string.IsNullOrWhiteSpace(ownId) ? Guid.Empty : Guid.Parse(ownId));
  }

  [HttpGet("fields")]
  public ActionResult<IEnumerable<DataFieldDto>> GetFields()
  {
    var columnGroups = DtoMapper.MapDisplayConfigurationToDtos(_metadataRepository, FieldLevel.Device);
    return Ok(columnGroups);
  }

  [HttpGet("types")]
  public ActionResult<IEnumerable<DeviceTypeDto>> GetTypes()
  {
    var result = _metadataRepository.DeviceTypes
      .Select(MapToDto)
      .ToList();

    return Ok(result);
  }

  private DeviceDto MapToDto(DeviceInfo device)
  {
    var deviceDto = new DeviceDto
    {
      Id = device.Id.ToString(),
      DeviceId = device.Id.ToString(),
      DeviceType = device.DeviceType,
    };

    _metadataRepository.DisplayConfiguration!
      .DeviceColumns.SelectMany(g => g.Columns)
      .ForEach(c => deviceDto.Properties[c.Property!] = GetFieldValueDto(device, c));

    return deviceDto;
  }

  private FieldValueDto GetFieldValueDto(DeviceInfo device, ColumnDefinition column)
  {
    var valueGetter = new DeviceValueGetter(device, _metadataRepository);

    if (column.ScriptDelegate != null)
    {
      return new()
      {
        Value = column.ScriptDelegate.Invoke(valueGetter) ?? string.Empty,
      };
    }

    return valueGetter.GetFieldValueDto(column.Property!);
  }

  private static DeviceTypeDto MapToDto(DeviceType t)
    => new()
    {
      Name = t.Name!,
      DisplayName = t.DisplayName,
      InterfaceCount = t.InterfaceCount,
      FlexibleStreams = t.InterfaceStreamsFlexible,
      InterfaceNamePrefix = t.InterfaceNameTemplate!.Split('{').First(),
      InterfaceTemplates = t.InterfaceTemplates
        .Select(template => new InterfaceTemplateDto
        {
          Name = template.Name,
          DisplayName = template.DisplayName,
          Streams = template.Streams.Select(
            ts => new StreamTemplateDto
            {
              Count = ts.Count,
              NameTemplate = ts.NameTemplate!.Split('_').First(),
            }).ToList(),
        })
        .ToList(),
      DefaultInterfaces = t.DefaultInterfaces
        .SelectMany(di => Enumerable.Repeat(di.Template, di.Count))
        .ToList(),
    };

  private static StreamCardinalityDto MapToDto(StreamCardinality cardinality)
    => new()
    {
      VideoSend = cardinality.VideoSend,
      VideoReceive = cardinality.VideoReceive,
      AudioSend = cardinality.AudioSend,
      AudioReceive = cardinality.AudioReceive,
      AncillarySend = cardinality.AncillarySend,
      AncillaryReceive = cardinality.AncillaryReceive,
    };
}