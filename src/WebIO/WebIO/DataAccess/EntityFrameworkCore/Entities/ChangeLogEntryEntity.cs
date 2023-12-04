namespace WebIO.DataAccess.EntityFrameworkCore.Entities;

using System;

public class ChangeLogEntryEntity
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; init; }
    public required string Username { get; init; }
    public required string Comment { get; init; }
    public required string Summary { get; init; }
    public required string FullInfo { get; init; }
}