namespace WebIO.Api.Controllers.Dto;

public record StreamTemplateDto
{
    public int Count { get; init; }

    public required string NameTemplate { get; init; }
}