namespace WebIO.Api.Controllers;

using Application;
using DataAccess;
using Dto;
using Model;
using Model.Readonly;

public class DeviceValueGetter : ValueGetter
{
  private readonly IMetadataRepository _metadataRepository;
  private readonly DeviceInfo _device;

  public DeviceValueGetter(DeviceInfo device, IMetadataRepository metadataRepository)
  {
    _metadataRepository = metadataRepository;
    _device = device;
  }

  public FieldValueDto GetFieldValueDto(string columnProperty)
  {
    string? value = null;
    switch (columnProperty)
    {
      case PseudoProperties.Name:
        value = _device.Name;
        break;
      case PseudoProperties.DeviceType:
        var deviceType = _metadataRepository.DeviceTypes.FirstOrDefault(d
          => string.Equals(d.Name, _device.DeviceType));
        value = deviceType?.DisplayName ?? _device.DeviceType;
        break;
      case PseudoProperties.InterfaceCount:
        value = _device.InterfacesCount.ToString();
        break;
      case PseudoProperties.Comment:
        value = _device.Comment;
        break;
      case PseudoProperties.Created:
        // value = $"{_device.Modification.Created:g}, {_device.Modification.Creator}";
        break;
      case PseudoProperties.Modified:
        // value = $"{_device.Modification.Modified:g}, {_device.Modification.Modifier}";
        break;
      default:
        _device.Properties.TryGetValue(columnProperty, out value);
        break;
    }

    return new()
    {
      Value = value ?? string.Empty,
      Inherited = false,
    };
  }

  public override string GetValue(string property)
    => GetFieldValueDto(property).Value;
}
