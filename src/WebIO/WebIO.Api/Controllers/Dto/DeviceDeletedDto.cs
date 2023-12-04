namespace WebIO.Api.Controllers.Dto;

public record DeviceDeletedDto: ChangeEventDto
{
  public Guid DeviceId { get; init; }
}
