namespace WebIO.Api.Controllers.Dto;

public record StreamDto : EntityBaseDto
{
    public required string Type { get; init; }
}