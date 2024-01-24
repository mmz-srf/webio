namespace WebIO.Api.Controllers;

using Auth;
using Dto;
using Export;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

[Route("api/[controller]")]
[Authorize]
[RequiredScope(Claims.CanRead)]
[ApiController]
public class ExportController : ControllerBase
{
  private readonly IExportFactory _exportFactory;
  private static readonly Dictionary<string, ExportResult> ExportResults = new();

  public ExportController(IExportFactory exportFactory)
  {
    _exportFactory = exportFactory;
  }

  [HttpGet("types")]
  public ActionResult<IEnumerable<ExportTypeDto>> GetTypes()
  {
    return Ok(
      _exportFactory.AvailableTypes
        .Select(t => new ExportTypeDto
        {
          Name = t.Name,
          DisplayName = t.DisplayName,
        }));
  }

  // [HttpPost("export")]
  // public IActionResult Get([FromBody] ExportRequest request)
  // {
  //   request.Filter ??= new Dictionary<string, string>();
  //
  //   var export = _exportFactory.GetExport(request.ExportType);
  //   if (export == null)
  //   {
  //     return NotFound();
  //   }
  //
  //   var result = export.Export(request.Filter);
  //   if (result.Failed)
  //   {
  //     return BadRequest();
  //   }
  //
  //   var contentType = GetContentType(result.FileType);
  //   return File(result.Data, contentType, result.Filename);
  // }

  [HttpPost]
  public async Task<IActionResult> CreateExportFile([FromBody] ExportArgs exportArgs, CancellationToken ct)
  {
    var export = _exportFactory.GetExport(exportArgs.ExportTargetName);

    SanitizeFilters(exportArgs, "Name");
    SanitizeFilters(exportArgs, "Name_1");

    var result = await export.Export(exportArgs, ct);
    if (result.Failed || result.Data == null)
    {
      return BadRequest();
    }

    return File(result.Data, GetContentType(result.FileType), result.Filename);
  }

  private static void SanitizeFilters(ExportArgs exportArgs, string filterKey)
  {
    if (exportArgs.Filters.TryGetValue(filterKey, out var value))
    {
      exportArgs.Filters.TryAdd("DeviceName", value);
      exportArgs.Filters.Remove(filterKey);
    }
  }
  //
  // [AllowAnonymous] // TODO: the client just goes there via location.href, should be authorized as well!
  // [HttpGet("downloadfile/{fileId}")]
  // public IActionResult DownloadFile(string fileId)
  // {
  //   if (!ExportResults.TryGetValue(fileId, out var exportResult))
  //   {
  //     return NotFound();
  //   }
  //
  //   ExportResults.Remove(fileId);
  //   var contentType = GetContentType(exportResult.FileType);
  //
  //   return File(exportResult.Data, contentType, exportResult.Filename);
  // }

  private static string GetContentType(ExportFileType? fileType)
    => fileType switch
    {
      ExportFileType.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
      ExportFileType.Csv => "text/csv",
      ExportFileType.Zip => "application/zip",
      _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null)
    };
}
