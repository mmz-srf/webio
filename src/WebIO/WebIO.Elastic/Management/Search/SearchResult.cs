namespace WebIO.Elastic.Management.Search;

public record SearchResult<TEntity>(IAsyncEnumerable<TEntity> Documents, long Total);
