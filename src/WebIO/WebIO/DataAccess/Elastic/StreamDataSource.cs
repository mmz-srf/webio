namespace WebIO.DataAccess.Elastic;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EntityFrameworkCore;
using EntityFrameworkCore.Entities;
using Microsoft.EntityFrameworkCore;
using WebIO.Elastic.Data;
using WebIO.Elastic.Management.Indexing;

public class StreamDataSource : IDataSource<IndexedStream, Guid>
{
  private readonly AppDbContext _db;

  public StreamDataSource(AppDbContext db)
  {
    _db = db;
  }

  public IAsyncEnumerable<IndexedStream> AllEntitiesAsync(CancellationToken ct)
    => _db.Streams
      .AsNoTracking()
      .Select(i => ToIndexedStream(i, _db))
      .AsAsyncEnumerable();

  public IAsyncEnumerable<IndexedStream> AllEntitiesWithIds(IEnumerable<Guid> ids, CancellationToken ct)
    => _db.Streams
      .AsNoTracking()
      .Select(i => ToIndexedStream(i, _db))
      .AsAsyncEnumerable();

  private static IndexedStream ToIndexedStream(StreamEntity stream, AppDbContext db)
  {
    var iface = db.Interfaces.Include(i => i.Properties).First(i => i.Id == stream.InterfaceId);
    var device = db.Devices.Include(d => d.Properties).First(d => d.Id == iface.DeviceId);
    return new()
    {
      Id = stream.Id,
      InterfaceId = stream.InterfaceId,
      Name = stream.Name,
      InterfaceName = iface.Name,
      DeviceName = device.Name,
      Comment = stream.Comment ?? string.Empty,
      Type = stream.Type,
      Direction = stream.Direction,
      DeviceId = device.Id,
      DeviceType = device.DeviceType,
      Properties = stream.Properties.ToDictionary(p => p.Key,
        p => p.Value ?? string.Empty),
      InterfaceProperties = iface.Properties.ToDictionary(p => p.Key,
        p => p.Value ?? string.Empty),
      DeviceProperties = device.Properties.ToDictionary(p => p.Key,
        p => p.Value ?? string.Empty),
      
    };
  }
}
