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
public class InterfacesController : ControllerBase
{
  private readonly IDeviceRepository _deviceRepository;
  private readonly IMetadataRepository _metadataRepository;

  public InterfacesController(IDeviceRepository deviceRepository, IMetadataRepository metadataRepository)
  {
    _deviceRepository = deviceRepository;
    _metadataRepository = metadataRepository;
  }

  [HttpPost]
  public ActionResult<QueryResultDto<InterfaceDto>> Get(
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

    var interfaceInfos = _deviceRepository.GetInterfaceInfos(query);

    query = new Query(0, 0)
      .WithFilter(filter, global);
    var totalCount = _deviceRepository.GetInterfaceCount(query);

    var result = new QueryResultDto<InterfaceDto>
    {
      State = QueryResultState.Success,
      Count = interfaceInfos.Count,
      Start = interfaceInfos.StartIndex,
      TotalCount = totalCount,
      Data = interfaceInfos.Data.Select(MapToDto).ToList(),
    };

    return Ok(result);
  }

  [HttpGet("fields")]
  public ActionResult<IEnumerable<DataFieldDto>> GetFields()
    => Ok(DtoMapper.MapDisplayConfigurationToDtos(_metadataRepository, FieldLevel.Interface));

  private InterfaceDto MapToDto(InterfaceInfo info)
  {
    var interfaceDto = new InterfaceDto
    {
      Id = info.Id.ToString(),
      DeviceId = info.DeviceId.ToString(),
      InterfaceTemplate = info.InterfaceTemplate,
    };

    _metadataRepository.DisplayConfiguration!
      .InterfaceColumns.SelectMany(g => g.Columns)
      .ForEach(c => interfaceDto.Properties[c.Property!] = GetFieldValueDto(info, c));

    return interfaceDto;
  }

  private FieldValueDto GetFieldValueDto(InterfaceInfo interfaceInfo, ColumnDefinition column)
  {
    var valueGetter = new InterfaceValueGetter(interfaceInfo, _metadataRepository);

    if (column.ScriptDelegate != null)
    {
      var value = column.ScriptDelegate.Invoke(valueGetter);
      return new()
      {
        Value = value ?? string.Empty,
      };
    }

    return valueGetter.GetFieldValueDto(column.Property ?? string.Empty);
  }
}
