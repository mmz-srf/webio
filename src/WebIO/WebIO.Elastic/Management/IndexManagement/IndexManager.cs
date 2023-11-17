namespace WebIO.Elastic.Management.IndexManagement;

using Microsoft.Extensions.Logging;
using Nest;

public class IndexManager<TIndexedEntity, TId> : IIndexManager<TIndexedEntity, TId>
  where TIndexedEntity : class, IIndexedEntity<TId>
{
  private readonly IElasticClient _client;
  private readonly ILogger<IndexManager<TIndexedEntity, TId>> _log;
  private readonly ElasticConfiguration _config;

  public IndexManager(
    ILogger<IndexManager<TIndexedEntity, TId>> log,
    IElasticClient client,
    ElasticConfiguration config)
  {
    _client = client;
    _log = log;
    _config = config;
  }

  public async Task EnsureValidIndexAsync(CancellationToken ct)
  {
    var index = await GetCurrentIndexAsync(ct);
    if (index == null)
    {
      var newIndex = await CreateNewIndexAsync(ct);
      await SwitchToNewIndex(newIndex, ct);
    }

    await EnsureMappingIsUpToDateAsync(ct);
  }

  private async Task EnsureMappingIsUpToDateAsync(CancellationToken ct, string? indexName = null)
  {
    indexName ??= GetAliasName();
    var response = await _client.MapAsync<TIndexedEntity>(
      c => ConfigureMapping(c.Index(indexName)),
      ct);

    if (!response.IsValid)
    {
      throw new InvalidOperationException(
        $"Mapping for {typeof(TIndexedEntity).Name} is not up to date, the index {indexName} needs " +
        $"to be rebuilt!\n" +
        $"{response.ServerError}\n" +
        $"{response.DebugInformation}",
        response.OriginalException);
    }
  }

  protected virtual PutMappingDescriptor<TIndexedEntity> ConfigureMapping(PutMappingDescriptor<TIndexedEntity> putMappingDescriptor)
    => putMappingDescriptor.AutoMap();

  public virtual string GetAliasName()
    => typeof(TIndexedEntity).ToIndexName().PrefixWith(_config.IndexPrefix);

  public async Task<string> CreateNewIndexAsync(CancellationToken ct)
  {
    var newIndexName = CreateNewIndexName();
    _log.LogInformation("Creating new index for alias {AliasName}: {IndexName}", GetAliasName(), newIndexName);
    var response = await _client.Indices.CreateAsync(
      newIndexName,
      ConfigureIndex,
      ct);

    await EnsureMappingIsUpToDateAsync(ct, newIndexName);

    return response.IsValid
      ? response.Index
      : throw new InvalidOperationException(
        $"Could not create new index {newIndexName}: {response.ServerError}" +
        $"\n{response.DebugInformation}", response.OriginalException);
  }

  private CreateIndexDescriptor ConfigureIndex(CreateIndexDescriptor c)
  {
    return c.Settings(
      s => ConfigureAnalysis(s)
        .NumberOfShards(_config.NumberOfShards)
        .NumberOfReplicas(_config.NumberOfReplias)
        .RefreshInterval(_config.RefreshInterval));
  }

  public async Task SwitchToNewIndex(IndexName newIndex, CancellationToken ct)
  {
    var oldIndex = await GetCurrentIndexAsync(ct);

    await MoveAliasAsync(newIndex, ct);
    await RefreshAsync(ct);

    if (oldIndex != null)
    {
      await DeleteIndexAsync(oldIndex, ct);
    }
  }

  private async Task MoveAliasAsync(
    IndexName newIndex,
    CancellationToken ct)
  {
    var aliasName = GetAliasName();
    var exists = await _client.Indices.AliasExistsAsync(aliasName, null, ct);
    if (exists.Exists)
    {
      await _client.Indices.DeleteAliasAsync(Indices.All, aliasName, null, ct);
    }

    var response = await _client.Indices.PutAliasAsync(newIndex, aliasName, null, ct);

    if (!response.IsValid)
    {
      throw new InvalidOperationException(
        $"Could not move alias {aliasName} to index {newIndex}: {response.ServerError}" +
        $"\n{response.DebugInformation}", response.OriginalException);
    }
  }

  public async Task RefreshAsync(CancellationToken ct)
  {
    var response = await _client.Indices.RefreshAsync(GetAliasName(), null, ct);
    if (!response.IsValid)
    {
      throw new InvalidOperationException(
        $"Could not refresh index {GetAliasName()}: {response.ServerError}" +
        $"\n{response.DebugInformation}", response.OriginalException);
    }
  }

  private async Task DeleteIndexAsync(IndexName indexName, CancellationToken ct)
  {
    var response = await _client.Indices.DeleteAsync(indexName, null, ct);
    if (!response.IsValid)
    {
      throw new InvalidOperationException(
        $"Could not delete index {indexName}: {response.ServerError}" +
        $"\n{response.DebugInformation}", response.OriginalException);
    }
  }

  private async Task<IndexName?> GetCurrentIndexAsync(CancellationToken ct)
  {
    var response = await _client.Indices.GetAliasAsync(new GetAliasDescriptor(Names.Parse(GetAliasName())), ct);
    return response.IsValid
      ? response.Indices.Keys.FirstOrDefault()
      : null;
  }

  protected virtual IndexSettingsDescriptor ConfigureAnalysis(IndexSettingsDescriptor s)
    => s.Analysis(
      a => a.Normalizers(
          nd => nd.Custom("lowercase",
            d => d.Filters("lowercase")))
        .Analyzers(
          an => an
            .Custom(
              "default",
              c => c
                .Tokenizer("standard")
                .Filters("lowercase", "asciifolding")))
        .TokenFilters(
          tf => tf
            .AsciiFolding(
              "default",
              af => af.PreserveOriginal())));

  private string CreateNewIndexName()
    => $"{GetAliasName()}-{DateTime.Now:yyyyMMddHHmmssfff}".PrefixWith(_config.IndexPrefix);
}
