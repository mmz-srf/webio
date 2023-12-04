namespace WebIO.Elastic.Management.Indexing;

/// <summary>
/// The data source is responsible for fetching the data that should be indexed.
/// </summary>
/// <typeparam name="TIndexedEntity">The type of entity that gets fed into the index</typeparam>
/// <typeparam name="TId">The data type of the id to index</typeparam>
public interface IDataSource<TIndexedEntity, in TId> where TIndexedEntity : class, IIndexedEntity<TId>
{
  /// <summary>
  /// Returns all entities that should be in the index.
  /// </summary>
  ///
  /// Try to remember using .AsNoTracking() ;-)
  /// 
  /// <param name="ct">The cancellation token</param>
  /// <returns>A stream of entities</returns>
  IAsyncEnumerable<TIndexedEntity> AllEntitiesAsync(CancellationToken ct);

  /// <summary>
  /// Returns all entities that should be in the index with the given ids. If an entity with the given id does not
  /// exist, it deletes the entity from the index.
  /// </summary>
  ///
  /// Try to remember using .AsNoTracking() ;-)
  /// 
  /// <param name="ids">A list of ids that need reindexing</param>
  /// <param name="ct">The cancellation token</param>
  /// <returns>A stream of entities</returns>
  IAsyncEnumerable<TIndexedEntity> AllEntitiesWithIds(IEnumerable<TId> ids, CancellationToken ct);

  /// <summary>
  /// Returns the entity with the given id. If the entity does not exist, it returns null.
  /// </summary>
  ///
  /// Try to remember using .AsNoTracking() ;-)
  /// 
  /// <param name="id">The id of the entity to fetch</param>
  /// <param name="ct">The cancellation token</param>
  /// <returns>The entity or null</returns>
  Task<TIndexedEntity?> EntityWithId(TId id, CancellationToken ct)
    => AllEntitiesWithIds(new[] {id}, ct).SingleOrDefaultAsync(ct).AsTask();
}
