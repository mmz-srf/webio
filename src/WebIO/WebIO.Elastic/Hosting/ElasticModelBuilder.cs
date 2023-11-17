namespace WebIO.Elastic.Hosting;

using Management;
using Management.Indexing;
using Management.IndexManagement;
using Microsoft.Extensions.DependencyInjection;

public record ElasticModelBuilder<TIndexedEntity, TId>(IServiceCollection Builder)
  where TIndexedEntity : class, IIndexedEntity<TId>, new()
{
  private Type? DataSourceType { get; init; }
  private Type IndexServiceType { get; init; } = typeof(IndexService<TIndexedEntity, TId>);
  private Type IndexManagerType { get; init; } = typeof(IndexManager<TIndexedEntity, TId>);
  private Type IndexerType { get; init; } = typeof(Indexer<TIndexedEntity, TId>);

  public ElasticModelBuilder<TIndexedEntity, TId> WithDataSource<TDataSource>()
    where TDataSource : class, IDataSource<TIndexedEntity, TId>
    => this with {DataSourceType = typeof(TDataSource)};

  public ElasticModelBuilder<TIndexedEntity, TId> WithIndexService<TIndexingService>()
    where TIndexingService : class, IIndexService<TIndexingService, TId>, IIndexedEntity<TId>
    => this with {IndexServiceType = typeof(TIndexingService)};

  public ElasticModelBuilder<TIndexedEntity, TId> WithIndexManager<TIndexManager>()
    where TIndexManager : class, IIndexManager<TIndexedEntity, TId>
    => this with {IndexManagerType = typeof(TIndexManager)};

  public ElasticModelBuilder<TIndexedEntity, TId> WithIndexer<TIndexer>()
    where TIndexer : class, IIndexer<TIndexedEntity, TId>
    => this with {IndexerType = typeof(TIndexer)};

  public IServiceCollection Register()
  {
    if (DataSourceType == null)
      throw new InvalidOperationException(
        $"No data source configured for elastic model {typeof(TIndexedEntity).Name}!");

    Builder.AddTransient(typeof(IDataSource<TIndexedEntity, TId>), DataSourceType);
    Builder.AddTransient(typeof(IIndexService<TIndexedEntity, TId>), IndexServiceType);
    Builder.AddTransient(typeof(IIndexService), IndexServiceType);
    Builder.AddTransient(typeof(IIndexer<TIndexedEntity, TId>), IndexerType);
    Builder.AddTransient(typeof(IIndexManager<TIndexedEntity, TId>), IndexManagerType);
    Builder.AddTransient(typeof(IIndexManager), IndexManagerType);

    return Builder;
  }
}
