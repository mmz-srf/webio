namespace WebIO.Api.UseCases;

using System.Text.RegularExpressions;
using Application;
using Controllers.Dto;
using DataAccess;
using Export;
using Microsoft.Extensions.Logging;
using Model;

public partial class SaveModificationsUseCase : IUseCase
{
  private static readonly Regex Ipv4V6Regex = IpRegex();

  private readonly IDeviceRepository _deviceRepository;
  private readonly IMetadataRepository _metadataRepository;
  private readonly IChangeLogRepository _changeLog;
  private PropertiesChangedSummaryDto? _changes;

  private IReadOnlyDictionary<string, DataField> _dataFields = new Dictionary<string, DataField>();
  private readonly ILogger<SaveModificationsUseCase> _log;
  private string? _username;

  public SaveModificationsUseCase(
    IDeviceRepository deviceRepository,
    IMetadataRepository metadataRepository,
    IChangeLogRepository changeLog,
    ILogger<SaveModificationsUseCase> log)
  {
    _deviceRepository = deviceRepository;
    _metadataRepository = metadataRepository;
    _changeLog = changeLog;
    _log = log;
  }

  public SaveModificationsUseCase Initialize(PropertiesChangedSummaryDto changes, string username)
  {
    using var span = Telemetry.Span();
    _username = username;
    _dataFields = _metadataRepository.DataFields.ToDictionary(f => f.Key);
    _changes = changes;
    return this;
  }

  public Task<bool> ValidateAsync(CancellationToken ct)
  {
    using var span = Telemetry.Span();
    return Task.FromResult(_changes != null
           && _changes.ChangedEvents.Any()
           && !string.IsNullOrWhiteSpace(_username)
           && AllFieldsValid());
  }

  private bool AllFieldsValid()
    => _changes!.ChangedEvents.All(IsValid);

  private bool IsValid(PropertyChangedEventDto eventDto)
  {
    if (!_dataFields.TryGetValue(eventDto.Property, out var dataField))
      return true;

    return dataField.FieldType switch
    {
      DataFieldType.IpAddress => Ipv4V6Regex.IsMatch(eventDto.NewValue),
      _ => true,
    };
  }

  public async Task ExecuteAsync(CancellationToken ct)
  {
    using var span = Telemetry.Span();
    var modifyArgs = new ModifyArgs(_username, DateTime.Now, _changes!.Comment);
    var changeInfos = new List<ChangeInfo>();

    var modsPerDevice = _changes.ChangedEvents.GroupBy(m => Guid.Parse(m.Device));

    var devices =
      (await _deviceRepository.GetDevicesByIdsAsync(_changes.ChangedEvents.Select(m => Guid.Parse(m.Device)).Distinct(), ct))
        .ToDictionary(d => d.Id);

    foreach (var deviceModifications in modsPerDevice)
    {
      var deviceId = deviceModifications.Key;
      var device = devices.GetValueOrDefault(deviceId);
      if (device == null)
      {
        _log.LogError("Device with id {DeviceId} not found", deviceId);
        continue;
      }

      foreach (var modification in deviceModifications)
      {
        switch (modification.EntityType)
        {
          case "Device":
            UpdateDevice(device, modification, changeInfos, modifyArgs, FieldLevel.Device);
            break;
          case "Interface":
          {
            if (!Guid.TryParse(modification.Entity, out var interfaceId))
            {
              _log.LogError("Could not parse interface id {InterfaceId}", modification.Entity);
              continue;
            }

            var @interface = device.Interfaces.FirstOrDefault(i => i.Id == interfaceId);
            if (@interface != null)
            {
              UpdateDevice(@interface, modification, changeInfos, modifyArgs, FieldLevel.Interface);
            }
            else
            {
              _log.LogError("Interface with id {InterfaceId} not found", interfaceId);
            }

            break;
          }
          case "Stream":
          {
            if (!Guid.TryParse(modification.Entity, out var streamId))
            {
              _log.LogError("Could not parse stream id {InterfaceId}", modification.Entity);
              continue;
            }

            var stream = device.Interfaces
              .SelectMany(i => i.Streams)
              .FirstOrDefault(s => s.Id == streamId);
            if (stream == null)
            {
              continue;
            }

            UpdateDevice(stream, modification, changeInfos, modifyArgs, FieldLevel.Stream);
            break;
          }
          default:
            throw new NotSupportedException($"invalid entity type {modification.EntityType}");
        }
      }

      await _deviceRepository.UpsertAsync(device, ct);
    }

    var logMessage = changeInfos
      .Select(info
        => $"Changed {info.Property} of {info.EntityType} {info.EntityName} from {info.OldValue} to {info.NewValue}")
      .StringJoin(Environment.NewLine);

    var logEntry = new ChangeLogEntry(
      DateTime.Now,
      _username!,
      _changes.Comment,
      logMessage,
      new UpdateFieldsChangeLogEntry {Changes = changeInfos});
    _changeLog.Add(logEntry);
  }

  private static void AddChange(
    List<ChangeInfo> changeInfos,
    PropertyChangedEventDto modification,
    string oldValue,
    string entityName)
  {
    var changeInfo = new ChangeInfo
    {
      OldValue = oldValue,
      Device = modification.Device,
      Entity = modification.Entity,
      EntityName = entityName,
      Property = modification.Property,
      EntityType = modification.EntityType,
      NewValue = modification.NewValue,
    };
    changeInfos.Add(changeInfo);
  }

  private void UpdateDevice(
    IHaveProperties entity,
    PropertyChangedEventDto modification,
    List<ChangeInfo> changeInfos,
    ModifyArgs modifyArgs,
    FieldLevel level)
  {
    switch (modification.Property)
    {
      case PseudoProperties.Name:
        AddChange(changeInfos, modification, entity.Name, entity.Name);
        entity.Modification.Modify(modifyArgs);
        entity.Name = modification.NewValue;
        break;
      case PseudoProperties.Comment:
        AddChange(changeInfos, modification, entity.Comment, entity.Name);
        entity.Modification.Modify(modifyArgs);
        entity.Comment = modification.NewValue;
        break;
      default:
      {
        if (_dataFields.TryGetValue(modification.Property, out var field))
        {
          if (field.EditLevels.Contains(level))
          {
            AddChange(changeInfos, modification, entity.Properties[field]!, entity.Name);
            entity.Modification.Modify(modifyArgs);
            entity.Properties[field] = modification.NewValue;
          }
          else
          {
            _log.LogError("DataField {Field} cannot be edited on level {Level}", field.DisplayName, level);
          }
        }
        else
        {
          _log.LogError("DataField {Field} not found", modification.Property);
        }

        break;
      }
    }
  }

  [GeneratedRegex(
    @"((^\s*(((\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])\.){3}(\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5]))\s*$)|(^\s*((([\dA-Fa-f]{1,4}:){7}([\dA-Fa-f]{1,4}|:))|(([\dA-Fa-f]{1,4}:){6}(:[\dA-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([\dA-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[\dA-Fa-f]{1,4}){1,7})|((:[\dA-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*$))")]
  private static partial Regex IpRegex();
}
