namespace WebIO.Model;

using System;

public class ModifyArgs
{
    public ModifyArgs(string? username, DateTime timestamp, string? comment)
    {
        Username = username ?? string.Empty;
        Timestamp = timestamp;
        Comment = comment ?? string.Empty;
    }

    public string Username { get; }
    public DateTime Timestamp { get; }
    public string Comment { get; }
}