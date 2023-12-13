namespace WebIO.DataAccess.Elastic;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using EntityFrameworkCore;
using EntityFrameworkCore.Entities;
using Microsoft.EntityFrameworkCore;
using WebIO.Elastic.Data;
using WebIO.Elastic.Management.Indexing;

public class InterfaceDataSource : IDataSource<IndexedInterface, Guid>
{
  private readonly AppDbContext _db;

  public InterfaceDataSource(AppDbContext db)
  {
    _db = db;
  }

  public IAsyncEnumerable<IndexedInterface> AllEntitiesAsync(CancellationToken ct)
    => _db.Interfaces
      .AsNoTracking()
      .Select(i => ToIndexedInterface(i, _db))
      .AsAsyncEnumerable();

  public IAsyncEnumerable<IndexedInterface> AllEntitiesWithIds(IEnumerable<Guid> ids, CancellationToken ct)
    => _db.Interfaces
      .AsNoTracking()
      .Where(d => ids.Contains(d.Id))
      .Select(i => ToIndexedInterface(i, _db))
      .AsAsyncEnumerable();

  private static IndexedInterface ToIndexedInterface(InterfaceEntity iface, AppDbContext db)
  {
    var count = CountStreamTypes(iface, db);

    var device = db.Devices.Include(d => d.Properties).First(d => d.Id == iface.DeviceId);
    return new()
    {
      Id = iface.Id,
      DeviceId = iface.DeviceId,
      Name = iface.Name,
      Index = iface.Index,
      InterfaceTemplate = iface.InterfaceTemplate,
      Comment = iface.Comment ?? string.Empty,
      DeviceType = device.DeviceType,
      DeviceName = device.Name,
      InterfaceProperties = iface.Properties.ToDictionary(p => p.Key,
        p => p.Value ?? string.Empty),
      DeviceProperties = device.Properties.ToDictionary(p => p.Key,
        p => p.Value ?? string.Empty),
      StreamsCountVideoSend = CountByTypeAndDirection(count, StreamType.Video, StreamDirection.Send),
      StreamsCountAudioSend = CountByTypeAndDirection(count, StreamType.Audio, StreamDirection.Send),
      StreamsCountAncillarySend = CountByTypeAndDirection(count, StreamType.Ancillary, StreamDirection.Send),
      StreamsCountVideoReceive = CountByTypeAndDirection(count, StreamType.Video, StreamDirection.Receive),
      StreamsCountAudioReceive = CountByTypeAndDirection(count, StreamType.Audio, StreamDirection.Receive),
      StreamsCountAncillaryReceive = CountByTypeAndDirection(count, StreamType.Ancillary, StreamDirection.Receive),
    };
  }

  private static List<(StreamDirection Direction, StreamType Type, int Count)> CountStreamTypes(
    InterfaceEntity iface,
    AppDbContext db)
    => db.Streams
      .Where(s => s.InterfaceId == iface.Id)
      .GroupBy(s => new { s.Direction, s.Type })
      .Select(g => new
      {
        g.Key.Direction,
        g.Key.Type,
        Count = g.Count(),
      }).ToListAsync().GetAwaiter().GetResult()
      .Select(g => (g.Direction, g.Type, g.Count))
      .ToList();

  private static int CountByTypeAndDirection(
    IEnumerable<(StreamDirection direction, StreamType type, int count)> count,
    StreamType type,
    StreamDirection direction)
    => count.FirstOrDefault(s => s.type == type && s.direction == direction).count;
}
