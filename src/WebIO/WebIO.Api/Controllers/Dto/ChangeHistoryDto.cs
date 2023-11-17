namespace WebIO.Api.Controllers.Dto;

public record ChangeHistoryDto
{
    public DateTime Timestamp { get; init; }
    public string? Username { get; init; }
    public string? Comment { get; init; }
    public string? Summary { get; init; }
    public object? Details { get; init; }
}