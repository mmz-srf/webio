namespace WebIO.Model.Readonly;

using System;

public class ChangeLogEntryInfo
{
    public ChangeLogEntryInfo(DateTime timestamp, string username, string comment, string summary, object details)
    {
        Timestamp = timestamp;
        Username = username;
        Comment = comment;
        Summary = summary;
        Details = details;
    }

    public DateTime Timestamp { get; }
    public string Username { get; }
    public string Comment { get; }
    public string Summary { get; }

    public object Details { get; }
}