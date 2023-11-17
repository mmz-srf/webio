namespace WebIO.DataAccess.EntityFrameworkCore;

using Entities;

public class DeviceInfoQueryResult
{
    public required DeviceEntity Device { get; init; }
    public required DeviceDenormalizedProperties Denormalized { get; init; }
}