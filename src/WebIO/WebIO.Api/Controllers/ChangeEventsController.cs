namespace WebIO.Api.Controllers;

using Auth;
using DataAccess;
using Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Model.Readonly;
using UseCases;

[Route("api/[controller]")]
[Authorize]
[RequiredScope(Claims.CanRead)]
[ApiController]
public class ChangeEventsController : ControllerBase
{
    private readonly UseCaseFactory _useCases;
    private readonly IChangeLogRepository _changeLogRepository;
    private string UsernameFromToken => TokenHelper.GetUserNameFromBearerToken(Request.Headers["Authorization"]!);

    public ChangeEventsController(
        UseCaseFactory useCases,
        IChangeLogRepository changeLogRepository)
    {
        _useCases = useCases;
        _changeLogRepository = changeLogRepository;
    }

    [HttpGet("history")]
    public ActionResult<QueryResultDto<ChangeHistoryDto>> GetHistory(int start = 0, int count = 100)
    {
        var history = _changeLogRepository.Query(start, count);
        var result = new QueryResultDto<ChangeHistoryDto>
        {
            State = QueryResultState.Success,
            Count = history.Count,
            Start = history.StartIndex,
            TotalCount = _changeLogRepository.QueryCount(),
            Data = history.Data.Select(MapToDto).ToList(),
        };
        return Ok(result);
    }

    [Authorize(Policy = Claims.CanEdit)]
    [HttpPost]
    public IActionResult Post([FromBody] PropertiesChangedSummaryDto modifications)
    {
        var useCase = _useCases.Create<SaveModificationsUseCase>();
        useCase.Initialize(modifications, UsernameFromToken);

        if (useCase.Validate())
        {
            useCase.Execute();
            return Ok();
        }

        return BadRequest("Device type or name is not valid or provided invalid values for fields");
    }

    [Authorize(Policy = Claims.CanEdit)]
    [HttpPost("createDevice")]
    public IActionResult CreateDevice([FromBody] DeviceAddedEventDto deviceAdded)
    {
        var useCase = _useCases.Create<CreateDeviceUseCase>();
        useCase.Initialize(deviceAdded, UsernameFromToken);

        if (useCase.Validate())
        {
            useCase.Execute();
            return Ok();
        }

        return BadRequest("Device with that name already exists!");
    }

    [Authorize(Policy = Claims.CanEdit)]
    [HttpPost("deleteDevice")]
    public IActionResult DeleteDevice([FromBody] DeviceDeletedDto deleted)
    {
        var useCase = _useCases.Create<DeleteDeviceUseCase>();
        useCase.Initialize(deleted, UsernameFromToken);

        if (useCase.Validate())
        {
            useCase.Execute();
            return Ok();
        }

        return BadRequest("Device type or name is not valid");
    }

    [Authorize(Policy = Claims.CanEdit)]
    [HttpPost("updateDevice")]
    public IActionResult UpdateDevice([FromBody] DeviceUpdatedEventDto devicetoUpdate)
    {
        var useCase = _useCases.Create<UpdateDeviceUseCase>();
        useCase.Initialize(devicetoUpdate, UsernameFromToken);

        if (useCase.Validate())
        {
            useCase.Execute();
            return Ok();
        }

        return BadRequest("Device type or name is not valid");
    }

    private static ChangeHistoryDto MapToDto(ChangeLogEntryInfo info)
        => new()
        {
            Timestamp = info.Timestamp,
            Comment = info.Comment,
            Summary = info.Summary,
            Username = info.Username,
            Details = info.Details,
        };
}