namespace WebIO.Api.Controllers.Dto;

public abstract record ChangeEventDto
{
  public string Comment { get; init; } = string.Empty;
}
