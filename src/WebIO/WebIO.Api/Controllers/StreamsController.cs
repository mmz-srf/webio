namespace WebIO.Api.Controllers;

using Auth;
using DataAccess;
using Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Model;
using Model.Display;
using Model.Readonly;
using MoreLinq.Extensions;

[Route("api/[controller]")]
[Authorize]
[RequiredScope(Claims.CanRead)]
[ApiController]
public class StreamsController : ControllerBase
{
  private readonly IDeviceRepository _deviceRepository;
  private readonly IMetadataRepository _metadataRepository;

  public StreamsController(IDeviceRepository deviceRepository, IMetadataRepository metadataRepository)
  {
    _deviceRepository = deviceRepository;
    _metadataRepository = metadataRepository;
  }

  [HttpPost]
  public ActionResult<QueryResultDto<StreamDto>> Get(
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

    var streamInfos = _deviceRepository.GetStreamInfos(query);

    query = new Query(0, 0)
      .WithFilter(filter, global);

    var totalCount = _deviceRepository.GetStreamCount(query);

    var result = new QueryResultDto<StreamDto>
    {
      State = QueryResultState.Success,
      Count = streamInfos.Count,
      Start = streamInfos.StartIndex,
      TotalCount = totalCount,
      Data = streamInfos.Data.Select(MapToDto).ToList(),
    };

    return Ok(result);
  }

  [HttpGet("tags")]
  public ActionResult<IEnumerable<TagDto>> GetTags()
    => Ok(_metadataRepository.Tags
      .Select(t => new TagDto {Name = t.Name, StreamType = t.StreamType.ToString()}));

  [HttpGet("fields")]
  public ActionResult<IEnumerable<DataFieldDto>> GetFields()
    => Ok(DtoMapper.MapDisplayConfigurationToDtos(_metadataRepository, FieldLevel.Stream));

  private StreamDto MapToDto(StreamInfo stream)
  {
    var streamDto = new StreamDto
    {
      Id = stream.Id.ToString(),
      DeviceId = stream.DeviceId.ToString(),
      Type = stream.Type.ToString(),
    };
    _metadataRepository.DisplayConfiguration!
      .StreamColumns.SelectMany(g => g.Columns)
      .ForEach(c => streamDto.Properties[c.Property!] = GetFieldValueDto(stream, c));

    return streamDto;
  }

  private FieldValueDto GetFieldValueDto(StreamInfo stream, ColumnDefinition column)
  {
    var valueGetter = new StreamsValueGetter(stream, _metadataRepository);

    if (column.ScriptDelegate != null)
    {
      var value = column.ScriptDelegate.Invoke(valueGetter);
      return new()
      {
        Value = value ?? string.Empty,
      };
    }

    return valueGetter.GetFieldValueDto(column.Property!);
  }
}