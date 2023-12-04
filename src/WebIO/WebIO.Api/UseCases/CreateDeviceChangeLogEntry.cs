namespace WebIO.Api.UseCases;

using Model;

public class CreateDeviceChangeLogEntry : IChangeLogDetails
{
  public required string DeviceName { get; init; }
  public required string DeviceType { get; set; }
}
