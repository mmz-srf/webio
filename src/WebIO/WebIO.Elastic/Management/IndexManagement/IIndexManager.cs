namespace WebIO.Elastic.Management.IndexManagement;

using Nest;

/// <summary>
/// The index manager is responsible for everything that has to do with the index itself, like creating a new index,
/// switching the alias to the new index, etc. It is also responsible for ensuring that the mapping is up to date.
/// </summary>
///
/// The manager is not responsible for indexing the data, that is the job of the
/// <see cref="IIndexer{TIndexedEntity,TId}"/>.
public interface IIndexManager
{
  /// <summary>
  /// This makes sure, the index is set up and a correct mapping is applied.
  /// </summary>
  /// <param name="ct">The cancellation token</param>
  Task EnsureValidIndexAsync(CancellationToken ct);

  /// <summary>
  /// Creates a new index with a datetime suffix.
  /// </summary>
  /// <param name="ct">The cancellation token</param>
  Task<string> CreateNewIndexAsync(CancellationToken ct);

  /// <summary>
  /// Moves an alias to a new index
  /// </summary>
  /// <param name="newIndex">The name of the index that will have the alias</param>
  /// <param name="ct">The cancellation token</param>
  Task SwitchToNewIndex(IndexName newIndex, CancellationToken ct);

  /// <summary>
  /// Gives you the alias of the index that contains the entities.
  /// </summary>
  /// <returns>The alias</returns>
  string GetAliasName();

  /// <summary>
  /// Refreshes an index in elasticsearch.
  /// </summary>
  /// <param name="ct">The cancellation token</param>
  Task RefreshAsync(CancellationToken ct);
}

/// <summary>
/// <see cref="IIndexManager{TIndexedEntity,TId}"/>
/// </summary>
/// <typeparam name="TIndexedEntity">The data type the index is filled with</typeparam>
/// <typeparam name="TId">The type of the id</typeparam>
public interface IIndexManager<TIndexedEntity, TId> : IIndexManager where TIndexedEntity : class, IIndexedEntity<TId>
{
}
