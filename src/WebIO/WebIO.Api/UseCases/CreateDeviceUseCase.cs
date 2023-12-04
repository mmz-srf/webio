namespace WebIO.Api.UseCases;

using Application;
using Controllers.Dto;
using DataAccess;
using Microsoft.Extensions.Logging;
using Model;
using Model.DeviceTemplates;

public class CreateDeviceUseCase : IUseCase
{
  private readonly ILogger<CreateDeviceUseCase> _log;

  private readonly IMetadataRepository _metadata;
  private readonly IDeviceRepository _deviceRepository;
  private readonly IChangeLogRepository _changeLog;

  private DeviceAddedEventDto? _added;
  private DeviceType? _deviceType;
  private DeviceCreation? _deviceCreation;
  private string? _username;

  public CreateDeviceUseCase(
    IMetadataRepository metadata,
    IDeviceRepository deviceRepository,
    IChangeLogRepository changeLog,
    ILogger<CreateDeviceUseCase> log)
  {
    _metadata = metadata;
    _deviceRepository = deviceRepository;
    _changeLog = changeLog;
    _log = log;
  }

  public void Initialize(DeviceAddedEventDto added, string username)
  {
    using var span = Telemetry.Span();
    _username = username;
    _deviceType = _metadata.DeviceTypes.FirstOrDefault(t => t.Name == added.DeviceType);
    _deviceCreation = new(_metadata, _log);
    _added = added;
  }

  public bool Validate()
  {
    using var span = Telemetry.Span();
    var deviceNameUnique = !_deviceRepository.IsDuplicateDeviceName(_added!.Name, default);

    if (deviceNameUnique)
    {
      return _added != null
             && !string.IsNullOrWhiteSpace(_added.Name)
             && !string.IsNullOrWhiteSpace(_username)
             && _deviceType != null;
    }

    return false;
  }

  public void Execute()
  {
    using var span = Telemetry.Span();
    var modifyArgs = new ModifyArgs(_username, DateTime.Now, _added!.Comment);
    _log.LogInformation("Create Device {DeviceName}, Type {DeviceType}", _added.Name, _added.DeviceType);

    // pass interface templates
    var interfaces = _added.Interfaces.GetTemplateSelections(_deviceType!);

    // create device
    var device = _deviceCreation!.CreateDevice(_deviceType!, modifyArgs, _added.Name, _added.Comment, interfaces,
      _added.UseSt20227);

    var logEntry = new ChangeLogEntry(
      modifyArgs.Timestamp,
      modifyArgs.Username,
      modifyArgs.Comment,
      $"Created device {device.Name}",
      new CreateDeviceChangeLogEntry
      {
        DeviceName = _added.Name,
        DeviceType = _deviceType!.Name ?? string.Empty,
      });

    _deviceRepository.Upsert(device);
    _changeLog.Add(logEntry);
  }
}
