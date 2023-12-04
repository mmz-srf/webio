namespace WebIO.Model;

using System;

public interface IChangeLogDetails { }

public class ChangeLogEntry
{
    public ChangeLogEntry(DateTime timestamp, string username, string comment, string summary, IChangeLogDetails fullDetails)
    {
        Id = Guid.NewGuid();
        Timestamp = timestamp;
        Username = username;
        Comment = comment;
        Summary = summary;
        FullDetails = fullDetails;
    }
    public Guid Id { get; set; }
    public DateTime Timestamp { get; }
    public string Username { get; }
    public string Comment { get; }
    public string Summary { get; }
    public IChangeLogDetails FullDetails { get; }
}