namespace WebIO.Api.UseCases;

using Model;

public class UpdateDeviceChangeLogEntry : IChangeLogDetails
{
  public Guid DeviceId { get; set; }
  public List<string> OldInterfaces { get; set; } = new();
  public List<string> NewInterfaces { get; set; } = new();
}
