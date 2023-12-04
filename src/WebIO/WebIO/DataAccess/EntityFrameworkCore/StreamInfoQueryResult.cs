namespace WebIO.DataAccess.EntityFrameworkCore;

using Entities;

public class StreamInfoQueryResult
{
    public required DeviceEntity Device { get; init; }
    public required InterfaceEntity Interface { get; init; }
    public required StreamEntity Stream { get; init; }
}