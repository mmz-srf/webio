namespace WebIO.DataAccess.EntityFrameworkCore;

using Entities;

public class InterfaceInfoQueryResult
{
    public required DeviceEntity Device { get; init; }
    public required InterfaceEntity Interface { get; init; }
    public required InterfaceDenormalizedProperties Denormalized { get; init; }
}