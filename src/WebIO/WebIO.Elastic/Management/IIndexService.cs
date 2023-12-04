namespace WebIO.Elastic.Management;

public interface IIndexService
{
  /// <summary>
  /// Reindexes all entities from the data source.
  /// </summary>
  /// <param name="ct">The cancellation token</param>
  Task ReindexAllAsync(CancellationToken ct);
}

/// <summary>
/// Provides a high level interface for indexing entities. All you need is a data source and the ids of the entities.
/// </summary>
/// <typeparam name="TId">The type of the entities id</typeparam>
public interface IIndexService<in TIindexedEntity, in TId> : IIndexService
  where TIindexedEntity : class, IIndexedEntity<TId>
{
  /// <summary>
  /// Updates, creates and deletes all entities with the given ids in the index. If the data source doesn't return
  /// an entity for a given id, it is deleted from the index.
  /// </summary>
  /// <param name="ids">All ids that need updating in the index</param>
  /// <param name="ct">The cancellation token</param>
  Task IndexAllAsync(IEnumerable<TId> ids, CancellationToken ct);

  /// <summary>
  /// Updates, creates or delete the entity with the given id in the index. If the data source doesn't return the
  /// entity, it is deleted from the index.
  /// </summary>
  /// <param name="id">The id of the entity</param>
  /// <param name="ct">The cancellation token</param>
  Task IndexAsync(TId id, CancellationToken ct);
}
