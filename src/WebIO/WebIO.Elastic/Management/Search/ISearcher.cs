namespace WebIO.Elastic.Management.Search;

public interface ISearcher<TEntity, in TSearchRequest, TId>
  where TEntity : class, IIndexedEntity<TId>
  where TSearchRequest : SearchRequest
{
  Task<SearchResult<TEntity>> FindAllAsync(TSearchRequest request, CancellationToken ct);
  Task<TEntity?> GetAsync(TId ids, CancellationToken ct);
  Task<IEnumerable<TEntity>> GetAllAsync(IEnumerable<TId> id, CancellationToken ct);
}
