namespace WebIO.Elastic.Management.Indexing;

/// <summary>
/// The indexer is responsible for storing, updating and deleting the data in the index.
/// </summary>
/// <typeparam name="TIndexedEntity">The data type that is stored in the index</typeparam>
/// <typeparam name="TId">The type of the id</typeparam>
public interface IIndexer<in TIndexedEntity, in TId> where TIndexedEntity : class, IIndexedEntity<TId>
{
  /// <summary>
  /// Indexes all provided entities into a new index and then deletes the old one. This works without
  /// downtime because the alias is switched after the new index is created.
  /// </summary>
  ///  If possible, use the version that takes an <see cref="IAsyncEnumerable{T}"/> to avoid loading all
  /// entities into memory.
  /// <param name="entities">The entities to index</param>
  /// <param name="ct">The cancellation token</param>
  Task ReindexAllAsync(IEnumerable<TIndexedEntity> entities, CancellationToken ct)
    => ReindexAllAsync(entities.ToAsyncEnumerable(), ct);

  /// <summary>
  /// Indexes all provided entities into a new index and then deletes the old one. This works without
  /// downtime because the alias is switched after the new index is created.
  /// </summary>
  /// entities into memory.
  /// <param name="entities">The entities to index</param>
  /// <param name="ct">The cancellation token</param>
  Task ReindexAllAsync(IAsyncEnumerable<TIndexedEntity> entities, CancellationToken ct);

  /// <summary>
  /// Updates the index with the given entity. If the entity does not exist, it is created.
  /// </summary>
  /// <param name="entity">The entity to index</param>
  /// <param name="ct">The cancellation token</param>
  Task IndexAsync(TIndexedEntity entity, CancellationToken ct);

  /// <summary>
  /// Updates the index with the given entities. If the entity does not exist, it is created.
  /// </summary>
  /// If possible, use the version that takes an &lt;see cref="IAsyncEnumerable{T}"/&gt; to avoid loading all
  /// <param name="entities">The entities to index</param>
  /// <param name="ct">The cancellation token</param>
  Task IndexAllAsync(IEnumerable<TIndexedEntity> entities, CancellationToken ct)
    => IndexAllAsync(entities.ToAsyncEnumerable(), ct);

  /// <summary>
  /// Updates the index with the given entities. If the entity does not exist, it is created.
  /// </summary>
  /// <param name="entities">The entities to index</param>
  /// <param name="ct">The cancellation token</param>
  Task IndexAllAsync(IAsyncEnumerable<TIndexedEntity> entities, CancellationToken ct);

  /// <summary>
  /// Removes the entity from the index.
  /// </summary>
  /// <param name="entity">The entity to remove</param>
  /// <param name="ct">The cancellation token</param>
  Task DeleteAsync(TIndexedEntity entity, CancellationToken ct);

  /// <summary>
  /// Removes the entity with the given id from the index.
  /// </summary>
  /// <param name="entity">The id to remove</param>
  /// <param name="ct">The cancellation token</param>
  Task DeleteAsync(TId id, CancellationToken ct);

  /// <summary>
  /// Removes the entities with the given ids from the index.
  /// </summary>
  /// <param name="entity">The ids to remove</param>
  /// <param name="ct">The cancellation token</param>
  Task DeleteAllAsync(IEnumerable<TId> ids, CancellationToken ct);

  /// <summary>
  /// Removes the entities from the index.
  /// </summary>
  /// If possible, use the version that takes an &lt;see cref="IAsyncEnumerable{T}"/&gt; to avoid loading all
  /// entities into memory at once.
  /// <param name="entity">The entities to remove</param>
  /// <param name="ct">The cancellation token</param>
  Task DeleteAllAsync(IEnumerable<TIndexedEntity> entities, CancellationToken ct)
    => DeleteAllAsync(entities.ToAsyncEnumerable(), ct);

  /// <summary>
  /// Removes the entities from the index.
  /// </summary>
  /// <param name="entity">The entities to remove</param>
  /// <param name="ct">The cancellation token</param>
  Task DeleteAllAsync(IAsyncEnumerable<TIndexedEntity> entities, CancellationToken ct);
}
