namespace WebIO.Elastic.Management.Indexing;

using IndexManagement;
using Microsoft.Extensions.Logging;
using Nest;

public class Indexer<TIndexedEntity, TId> : IIndexer<TIndexedEntity, TId>
  where TIndexedEntity : class, IIndexedEntity<TId>, new()
{
  private readonly ILogger<Indexer<TIndexedEntity, TId>> _log;
  private readonly IElasticClient _client;
  private readonly IIndexManager<TIndexedEntity, TId> _indexManager;
  private readonly ElasticConfiguration _config;

  public Indexer(
    ILogger<Indexer<TIndexedEntity, TId>> log,
    IElasticClient client,
    IIndexManager<TIndexedEntity, TId> indexManager,
    ElasticConfiguration config)
  {
    _log = log;
    _client = client;
    _indexManager = indexManager;
    _config = config;
  }

  public async Task ReindexAllAsync(IAsyncEnumerable<TIndexedEntity> entities, CancellationToken ct)
  {
    var newIndex = await _indexManager.CreateNewIndexAsync(ct);
    await BatchProcess(entities, (batch, ctoken) => IndexAsync(newIndex, batch, ctoken), ct);
    await _indexManager.SwitchToNewIndex(newIndex, ct);
  }

  private async Task BatchProcess(
    IAsyncEnumerable<TIndexedEntity> entities,
    Func<IEnumerable<TIndexedEntity>, CancellationToken, Task> process,
    CancellationToken ct)
  {
    var batch = new List<TIndexedEntity>();
    await foreach (var entity in entities.WithCancellation(ct))
    {
      batch.Add(entity);

      if (batch.Count >= _config.BatchSize)
      {
        await process(batch, ct);
        batch.Clear();
      }
    }

    await process(batch, ct);
  }

  public Task IndexAsync(TIndexedEntity entity, CancellationToken ct)
    => IndexAllAsync(new[] {entity}.ToAsyncEnumerable(), ct);

  public Task IndexAllAsync(
    IAsyncEnumerable<TIndexedEntity> entities,
    CancellationToken ct)
    => BatchProcess(
      entities,
      (batch, ctoken) => IndexAsync(_indexManager.GetAliasName(), batch, ctoken),
      ct);

  public Task DeleteAsync(TIndexedEntity entity, CancellationToken ct)
    => DeleteAllAsync(new[] {entity.Id}, ct);

  public Task DeleteAsync(TId id, CancellationToken ct)
    => DeleteAllAsync(new[] {id}, ct);

  public Task DeleteAllAsync(IAsyncEnumerable<TIndexedEntity> entities, CancellationToken ct)
    => BatchProcess(
      entities,
      (batch, ctoken) => _client.DeleteManyAsync(batch, _indexManager.GetAliasName(), ctoken),
      ct);

  public Task DeleteAllAsync(IEnumerable<TId> ids, CancellationToken ct)
    => DeleteAllAsync(
      ids.Select(id => new TIndexedEntity {Id = id})
        .ToAsyncEnumerable(),
      ct);

  private async Task IndexAsync(
    IndexName index,
    IEnumerable<TIndexedEntity> chunk,
    CancellationToken ct)
  {
    var entities = chunk.ToList();

    if (entities.Any())
    {
      _log.LogInformation("Indexing {Count} documents into index {IndexName}", entities.Count, index);
      var response = await _client.IndexManyAsync(entities, index, ct);

      if (!response.IsValid)
      {
        throw new InvalidOperationException(
          $"Could not index {entities.Count} documents into index {index}:\n" +
          $"{response.ServerError}\n{response.DebugInformation}", response.OriginalException);
      }
    }
  }
}
