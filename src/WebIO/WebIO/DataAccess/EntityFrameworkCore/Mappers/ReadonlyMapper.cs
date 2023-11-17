namespace WebIO.DataAccess.EntityFrameworkCore.Mappers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Application;
using Entities;
using Model.Readonly;

public static class ReadonlyMapper
{
  public static DeviceInfo MapToInfo(this DeviceEntity entity, DeviceDenormalizedProperties denormalized)
  {
    using var span = Telemetry.Span();
    return new(
      entity.Id,
      entity.Name,
      entity.DeviceType,
      entity.Comment ?? string.Empty,
      entity.Properties.ToPropertyDictionary(),
      // entity.MapToModificationInfo(),
      denormalized.InterfaceCount
    );
  }

  private static ImmutableDictionary<string, string> ToPropertyDictionary<T>(this IList<T> list)
    where T : IAmAPropertyValueEntity
  {
    try
    {
      return list.ToImmutableDictionary(p => p.Key, p => p.Value ?? string.Empty);
    }

    // duplicate keys
    catch (ArgumentException)
    {
      return list.GroupBy(p => p.Key).Select(g =>
      {
        if (g.Count() != 1)
        {
          throw new ArgumentException($"Duplicate key {g.Key} in {typeof(T).Name}");
        }

        return g.First();
      }).ToImmutableDictionary(p => p.Key, p => p.Value ?? string.Empty);
    }
  }

  public static InterfaceInfo MapToInfo(
    this InterfaceEntity entity,
    InterfaceDenormalizedProperties denormalized,
    DeviceEntity device)
  {
    return new(
      entity.Id,
      entity.Name,
      entity.Comment ?? string.Empty,
      entity.DeviceId,
      device.DeviceType,
      device.Name,
      entity.InterfaceTemplate,
      new(
        denormalized.StreamsCountVideoSend,
        denormalized.StreamsCountAudioSend,
        denormalized.StreamsCountAncillarySend,
        denormalized.StreamsCountVideoReceive,
        denormalized.StreamsCountAudioReceive,
        denormalized.StreamsCountAncillaryReceive),
      entity.Properties.ToPropertyDictionary(),
      device.Properties.ToPropertyDictionary(),
      entity.MapToModificationInfo());
  }

  public static StreamInfo MapToInfo(
    this StreamEntity entity,
    InterfaceEntity @interface,
    DeviceEntity device)
  {
    return new(
      entity.Id,
      entity.Name,
      entity.Comment ?? string.Empty,
      entity.Type,
      entity.Direction,
      device.Id,
      device.DeviceType,
      device.Name,
      @interface.Name,
      entity.Properties.ToPropertyDictionary(),
      device.Properties.ToPropertyDictionary(),
      @interface.Properties.ToPropertyDictionary(),
      entity.MapToModificationInfo());
  }

  private static ModificationInfo MapToModificationInfo(this IHaveModificationInfo entity)
  {
    return new(
      entity.Creator ?? string.Empty,
      entity.Created,
      entity.Modifier ?? string.Empty,
      entity.Modified,
      entity.ModificationComment ?? string.Empty);
  }
}
