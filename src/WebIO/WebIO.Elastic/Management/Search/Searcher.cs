namespace WebIO.Elastic.Management.Search;

using System.Runtime.CompilerServices;
using System.Text;
using IndexManagement;
using Microsoft.Extensions.Logging;
using Nest;

public abstract class Searcher<TEntity, TSearchRequest, TId> : ISearcher<TEntity, TSearchRequest, TId>
  where TEntity : class, IIndexedEntity<TId> where TSearchRequest : SearchRequest
{
  private readonly ILogger<Searcher<TEntity, TSearchRequest, TId>> _log;
  private readonly IIndexManager<TEntity, TId> _indexManager;
  private readonly IElasticClient _client;
  private readonly ElasticConfiguration _config;

  protected Searcher(
    ILogger<Searcher<TEntity, TSearchRequest, TId>> log,
    IElasticClient client,
    IIndexManager<TEntity, TId> indexManager,
    ElasticConfiguration config)
  {
    _log = log;
    _indexManager = indexManager;
    _client = client;
    _config = config;
  }

  public async Task<SearchResult<TEntity>> FindAllAsync(TSearchRequest request, CancellationToken ct)
  {
    var searchResponse = await _client.SearchAsync<TEntity>(sd => PrepareQuery(request, sd), ct);
    var json = Encoding.UTF8.GetString(searchResponse.ApiCall.RequestBodyInBytes);
    return new(Documents: IterateResultsAsync(searchResponse, request, ct), Total: searchResponse.Total);
  }

  public async Task<TEntity?> GetAsync(TId id, CancellationToken ct)
    => (await GetAllAsync(new[] { id }, ct)).FirstOrDefault();

  public async Task<IEnumerable<TEntity>> GetAllAsync(IEnumerable<TId> ids, CancellationToken ct)
    => (await _client.GetManyAsync<TEntity>(ids.Select(id => $"{id}"), _indexManager.GetAliasName(), ct)).Select(hit
      => hit.Source);

  private async IAsyncEnumerable<TEntity> IterateResultsAsync(
    ISearchResponse<TEntity> response,
    TSearchRequest request,
    [EnumeratorCancellation] CancellationToken ct)
  {
    foreach (var entity in response.Documents)
    {
      yield return entity;
    }

    var currentPosition = response.Documents.Count;
    while (!ct.IsCancellationRequested && response.Documents.Count > 0)
    {
      response = await _client.SearchAsync<TEntity>(sd => PrepareQuery(request, sd, currentPosition), ct);

      foreach (var entity in response.Documents)
      {
        yield return entity;
      }

      currentPosition += response.Documents.Count;
    }
  }

  protected abstract Func<SearchDescriptor<TEntity>, SearchDescriptor<TEntity>> ToQuery(
    TSearchRequest request);

  private SearchDescriptor<TEntity> PrepareQuery(
    TSearchRequest request,
    SearchDescriptor<TEntity> sd,
    int currentPosition = 0)
    => UseIndex(
      _indexManager.GetAliasName(),
      Sort(request,
        SkipTake(
          currentPosition,
          request.Take ?? _config.BatchSize,
          ToQuery(request)(sd))));

  private static SearchDescriptor<TEntity> UseIndex(string index, SearchDescriptor<TEntity> sd)
  {
    sd.Index(index);
    return sd;
  }

  private static SearchDescriptor<TEntity> SkipTake(
    int skip,
    int take,
    SearchDescriptor<TEntity> sd)
  {
    sd.From(skip);
    sd.Take(take);
    return sd;
  }

  private static SearchDescriptor<TEntity> Sort(
    SearchRequest request,
    SearchDescriptor<TEntity> sd)
  {
    sd.Sort(d => request.Sorting.Aggregate(d,
      (sort, kv) => sort.Field(f
        =>
      {
        var fieldSortDescriptor = f.Field(kv.FullName)
          .Order(kv.SortOrder)
          .UnmappedType(FieldType.Keyword);

        return string.IsNullOrWhiteSpace(kv.Path)
          ? fieldSortDescriptor
          : fieldSortDescriptor.Nested(n => n.Path(kv.Path));
      })));
    return sd;
  }
}
