namespace WebIO.Elastic.Management;

using Indexing;
using Microsoft.Extensions.Logging;

public class IndexService<TIndexedEntity, TId> : IIndexService<TIndexedEntity, TId> where TIndexedEntity : class, IIndexedEntity<TId>
{
  private readonly ILogger<IndexService<TIndexedEntity, TId>> _log;
  private readonly IIndexer<TIndexedEntity, TId> _indexer;
  private readonly IDataSource<TIndexedEntity, TId> _dataSource;

  public IndexService(
    ILogger<IndexService<TIndexedEntity, TId>> log,
    IIndexer<TIndexedEntity, TId> indexer,
    IDataSource<TIndexedEntity, TId> dataSource)
  {
    _log = log;
    _indexer = indexer;
    _dataSource = dataSource;
  }

  public Task ReindexAllAsync(CancellationToken ct)
    => _indexer.ReindexAllAsync(_dataSource.AllEntitiesAsync(ct), ct);

  public async Task IndexAllAsync(IEnumerable<TId> ids, CancellationToken ct)
  {
    var allIds = ids.ToList();
    var processedIds = new List<TId>();
    
    await _indexer.IndexAllAsync(_dataSource.AllEntitiesWithIds(allIds, ct)
      .Select(e =>
      {
        processedIds.Add(e.Id);
        return e;
      }), ct);

    await _indexer.DeleteAllAsync(allIds.Except(processedIds), ct);
  }

  public async Task IndexAsync(TId id, CancellationToken ct)
  {
    var entityWithId = await _dataSource.EntityWithId(id, ct);
    if (entityWithId != null)
    {
      await _indexer.IndexAsync(entityWithId, ct);
    }
    else
    {
      await _indexer.DeleteAsync(id, ct);
    }
  }
}
