namespace WebIO.Api.UseCases;

using Application;
using Controllers.Dto;
using DataAccess;
using Microsoft.Extensions.Logging;
using Model;

public class UpdateDeviceUseCase : IUseCase
{
  private readonly IDeviceRepository _deviceRepository;
  private readonly IChangeLogRepository _changeLog;
  private readonly IMetadataRepository _metadata;
  private readonly ILogger<UpdateDeviceUseCase> _log;
  private DeviceUpdatedEventDto? _update;
  private DeviceCreation? _deviceCreation;
  private string? _username;

  public UpdateDeviceUseCase(
    IDeviceRepository deviceRepository,
    IChangeLogRepository changeLog,
    IMetadataRepository metadata,
    ILogger<UpdateDeviceUseCase> log)
  {
    _deviceRepository = deviceRepository;
    _changeLog = changeLog;
    _metadata = metadata;
    _log = log;
  }

  public UpdateDeviceUseCase Initialize(DeviceUpdatedEventDto update, string username)
  {
    using var span = Telemetry.Span();
    _username = username;
    _update = update;
    _deviceCreation = new(_metadata, _log);
    return this;
  }

  public bool Validate()
  {
    using var span = Telemetry.Span();
    return _update != null
           && !string.IsNullOrWhiteSpace(_username);
  }

  public void Execute()
  {
    using var span = Telemetry.Span();
    var device = _deviceRepository.GetDevice(_update!.DeviceId);
    if (device == null)
    {
      _log.LogWarning("Update Device: Device with id {Device} not found", _update.DeviceId);
      return;
    }

    var deviceType = _metadata.DeviceTypes.FirstOrDefault(t => t.Name == device.DeviceType);
    if (deviceType == null)
    {
      _log.LogError("Update Device: Device Type {DeviceType} not found", device.DeviceType);
      return;
    }

    if (!deviceType.SoftwareDefinedInterfaceCount &&
        deviceType.InterfaceCount != _update.Interfaces.Count)
    {
      _log.LogError("Update Device: Invalid Number of interfaces for Device Type {DeviceType}", device.DeviceType);
      return;
    }

    var interfaceSelections = _update.Interfaces.GetTemplateSelections(deviceType);

    var oldInterfaces = device.Interfaces.Select(i => i.InterfaceTemplate!).ToList();
    var modifyArgs = new ModifyArgs(_username, DateTime.Now, _update.Comment);

    device.Comment = _update.Comment;

    var deviceInterfacesUnmodified = device.Interfaces;
    for (var interfaceIndex = 0;
         interfaceIndex < Math.Max(device.Interfaces.Count, interfaceSelections.Count);
         interfaceIndex++)
    {
      var existingInterface = deviceInterfacesUnmodified.ElementAtOrDefault(interfaceIndex);
      var interfaceSelection = interfaceSelections.ElementAtOrDefault(interfaceIndex);

      if (existingInterface is null)
      {
        _deviceCreation!.CreateInterface(deviceType, modifyArgs, interfaceIndex + 1, device, interfaceSelection!);
        device.Modification.Modify(modifyArgs);
      }
      else if (interfaceSelection is null)
      {
        device.Interfaces.Remove(existingInterface);
        device.Modification.Modify(modifyArgs);
      }
      else
      {
        var templateChanged = existingInterface.InterfaceTemplate != interfaceSelection.Template.Name;
        var streamCardinality = existingInterface.GetStreamCardinality();
        var flexibleStreamsChanged =
          interfaceSelection.FlexibleStreams && !interfaceSelection.Streams!.Equals(streamCardinality);

        if (templateChanged || flexibleStreamsChanged)
        {
          _deviceCreation!.UpdateInterface(existingInterface, modifyArgs, interfaceSelection);
          device.Modification.Modify(modifyArgs);
        }
      }
    }

    var logEntry = new ChangeLogEntry(
      modifyArgs.Timestamp,
      modifyArgs.Username,
      modifyArgs.Comment,
      $"Updated device {device.Name} from {Math.Abs(deviceInterfacesUnmodified.Count)} to {device.Interfaces.Count} interfaces",
      new UpdateDeviceChangeLogEntry
      {
        DeviceId = _update.DeviceId,
        OldInterfaces = oldInterfaces,
        NewInterfaces = device.Interfaces.Select(i => i.InterfaceTemplate!).ToList(),
      });

    _deviceRepository.Upsert(device);
    _changeLog.Add(logEntry);
  }
}
