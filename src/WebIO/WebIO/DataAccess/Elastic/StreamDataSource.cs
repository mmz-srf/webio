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
      .Select(i => ToIndexedStream(i))
      .AsAsyncEnumerable();

  public IAsyncEnumerable<IndexedStream> AllEntitiesWithIds(IEnumerable<Guid> ids, CancellationToken ct)
    => _db.Streams
      .AsNoTracking()
      .Select(i => ToIndexedStream(i))
      .AsAsyncEnumerable();
  
  private static IndexedStream ToIndexedStream(StreamEntity stream)
    => new()
    {
      Id = stream.Id,
      InterfaceId = stream.InterfaceId,
      Name = stream.Name,
      Comment = stream.Comment ?? string.Empty,
      Type = stream.Type,
      Direction = stream.Direction,
      Properties = stream.Properties.ToDictionary(p => p.Key,
        p => p.Value ?? string.Empty),
    };
}
