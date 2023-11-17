namespace WebIO.Model;

public record DataSelectableValue
{
    public required string Text { get; set; }
    public required string BackgroundColor { get; set; }
    public required string ForegroundColor { get; set; }
}