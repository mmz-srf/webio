namespace WebIO.DataAccess.EntityFrameworkCore.Mappers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Entities;
using Model;
using WebIO.Elastic.Data;

public static class DeviceEntityMapper
{
  public static Device ToModel(this DeviceEntity entity)
  {
    var properties = ToPropertyDictionary(entity);
    return new()
    {
      Id = entity.Id,
      Name = entity.Name,
      DeviceType = entity.DeviceType,
      Comment = entity.Comment ?? string.Empty,
      Properties = new(new(properties)),
      Modification = ToModification(entity),
    };
  }

  private static Dictionary<string, string> ToPropertyDictionary<T>(this IAmAnEntityWithProperties<T> entity)
    where T : IAmAPropertyValueEntity
  {
    try
    {
      return entity.Properties.ToDictionary(p => p.Key, p => p.Value ?? string.Empty);
    }

    // duplicate keys
    catch (ArgumentException)
    {
      return entity.Properties.GroupBy(p => p.Key).Select(g =>
      {
        if (g.Count() != 1)
        {
          throw new ArgumentException("Inconsistent data! duplicate {property}", g.Key);
        }

        return g.First();
      }).ToDictionary(p => p.Key, p => p.Value ?? string.Empty);
    }
  }

  public static DeviceEntity ToEntity(this Device device)
  {
    var deviceEntity = new DeviceEntity
    {
      Id = device.Id,
      Name = device.Name,
      DeviceType = device.DeviceType,
      Comment = device.Comment,
    }.SetModification(device.Modification);

    deviceEntity.Properties = device.Properties.All.Select(kvp => new DevicePropertyValueEntity
    {
      Device = deviceEntity,
      Id = Guid.NewGuid(),
      Key = kvp.Key,
      Value = kvp.Value,
    }).ToList();
    return deviceEntity;
  }

  public static void SyncEntity(
    this Device device,
    DeviceEntity entity)
  {
    entity.Name = device.Name;
    entity.Comment = device.Comment;
    entity.SetModification(device.Modification);

    // update properties.
    UpdateProperties(device, entity);
    device.Id = entity.Id;
  }

  public static DeviceDenormalizedProperties UpdateDenormalized(
    this Device device,
    DeviceDenormalizedProperties entity)
  {
    entity.Id = device.Id;
    entity.InterfaceCount = device.Interfaces.Count;
    return entity;
  }

  public static DeviceDenormalizedProperties ToDenormalizedProperties(this Device device)
  {
    return device.UpdateDenormalized(new());
  }

  public static Interface ToModel(this InterfaceEntity entity)
    => new()
    {
      Id = entity.Id,
      Name = entity.Name,
      Index = entity.Index,
      InterfaceTemplate = entity.InterfaceTemplate,
      Comment = entity.Comment ?? string.Empty,
      Properties = new(new(entity.ToPropertyDictionary())),
      Modification = ToModification(entity),
    };

  public static InterfaceEntity ToEntity(this Interface iface, Guid deviceId)
  {
    var interfaceEntity = new InterfaceEntity
    {
      Id = iface.Id,
      Name = iface.Name,
      Index = iface.Index,
      InterfaceTemplate = iface.InterfaceTemplate!,
      Comment = iface.Comment,
      DeviceId = deviceId,
    }.SetModification(iface.Modification);
    interfaceEntity.Properties = iface.Properties.All.Select(kvp => new InterfacePropertyValueEntity
    {
      Interface = interfaceEntity,
      Id = Guid.NewGuid(),
      Key = kvp.Key,
      Value = kvp.Value,
    }).ToList();

    return interfaceEntity;
  }

  public static void SyncEntity(this Interface iface, InterfaceEntity entity)
  {
    entity.Name = iface.Name;
    entity.Comment = iface.Comment;
    entity.InterfaceTemplate = iface.InterfaceTemplate!;
    entity.SetModification(iface.Modification);

    UpdateProperties(iface, entity);
    iface.Id = entity.Id;
  }

  public static InterfaceDenormalizedProperties ToDenormalizedProperties(this Interface @interface)
    => @interface.UpdateDenormalized(new());

  public static InterfaceDenormalizedProperties UpdateDenormalized(
    this Interface @interface,
    InterfaceDenormalizedProperties entity)
  {
    entity.Id = @interface.Id;
    entity.StreamsCountVideoSend =
      @interface.Streams.Count(s => s is {Type: StreamType.Video, Direction: StreamDirection.Send});
    entity.StreamsCountAudioSend =
      @interface.Streams.Count(s => s is {Type: StreamType.Audio, Direction: StreamDirection.Send});
    entity.StreamsCountAncillarySend =
      @interface.Streams.Count(s => s is {Type: StreamType.Ancillary, Direction: StreamDirection.Send});
    entity.StreamsCountVideoReceive =
      @interface.Streams.Count(s => s is {Type: StreamType.Video, Direction: StreamDirection.Receive});
    entity.StreamsCountAudioReceive =
      @interface.Streams.Count(s => s is {Type: StreamType.Audio, Direction: StreamDirection.Receive});
    entity.StreamsCountAncillaryReceive =
      @interface.Streams.Count(s => s is {Type: StreamType.Ancillary, Direction: StreamDirection.Receive});
    return entity;
  }

  public static Stream ToModel(this StreamEntity entity)
  {
    var properties = entity.ToPropertyDictionary();
    return new()
    {
      Id = entity.Id,
      Name = entity.Name,
      Comment = entity.Comment ?? string.Empty,
      Type = entity.Type,
      Direction = entity.Direction,
      Properties = new(new(properties)),
      Modification = ToModification(entity),
    };
  }

  public static StreamEntity ToEntity(this Stream stream, Guid interfaceId)
  {
    var streamEntity = new StreamEntity
    {
      Id = stream.Id,
      Name = stream.Name,
      Comment = stream.Comment,
      Direction = stream.Direction,
      Type = stream.Type,
      InterfaceId = interfaceId,
    }.SetModification(stream.Modification);

    streamEntity.Properties = stream.Properties.All.Select(kvp => new StreamPropertyValueEntity
    {
      Stream = streamEntity,
      Id = Guid.NewGuid(),
      Key = kvp.Key,
      Value = kvp.Value,
    }).ToList();

    return streamEntity;
  }

  public static StreamEntity SyncEntity(
    this Stream stream,
    StreamEntity entity)
  {
    entity.Name = stream.Name;
    entity.Comment = stream.Comment;
    entity.Direction = stream.Direction;
    entity.Type = stream.Type;

    entity.SetModification(stream.Modification);

    // update properties.
    UpdateProperties(stream, entity);

    stream.Id = entity.Id;
    return entity;
  }

  private static T SetModification<T>(this T entity, Modification modification) where T : IHaveModificationInfo
  {
    entity.Creator = modification.Creator;
    entity.Created = modification.Created;
    entity.Modifier = modification.Modifier;
    entity.Modified = modification.Modified;
    entity.ModificationComment = modification.Comment;
    return entity;
  }

  private static void UpdateProperties<T>(IHaveProperties modelValue, IAmAnEntityWithProperties<T> entity)
    where T : class, IAmAPropertyValueEntity, new()
  {
    foreach (var entityProperty in entity.Properties.ToList())
    {
      // update
      if (modelValue.Properties.All.TryGetValue(entityProperty.Key, out var value))
      {
        entityProperty.Value = value;
      }
      // remove
      else
      {
        entity.Properties.Remove(entityProperty);
      }
    }

    var added = modelValue.Properties.All
      .Where(kvp => entity.Properties.All(p => p.Key != kvp.Key))
      .Select(kvp => new T
      {
        Key = kvp.Key,
        Value = kvp.Value ?? throw new InvalidOperationException(),
      }).ToList();

    entity.Properties.AddRange(added);
  }

  private static Modification ToModification(this IHaveModificationInfo modificationInfo)
    => new(
      modificationInfo.Creator ?? string.Empty,
      modificationInfo.Created,
      modificationInfo.Modifier ?? string.Empty,
      modificationInfo.Modified,
      modificationInfo.ModificationComment ?? string.Empty);

  public static Expression<Func<DeviceEntity, bool>> DeviceEquals(Device device)
    => d => d.Id == device.Id || d.Name == device.Name;

  public static Expression<Func<InterfaceEntity, bool>> InterfaceEquals(
    Interface iface,
    Guid deviceId)
    => i => i.Id == iface.Id || i.Name == iface.Name && i.DeviceId == deviceId;

  public static Expression<Func<StreamEntity, bool>> StreamEquals(
    Stream stream,
    Guid ifaceId)
    => s => s.Id == stream.Id || s.Name == stream.Name && s.InterfaceId == ifaceId;
}
