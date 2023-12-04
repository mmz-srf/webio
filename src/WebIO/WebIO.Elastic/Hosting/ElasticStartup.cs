namespace WebIO.Elastic.Hosting;

using Management.IndexManagement;
using Microsoft.Extensions.DependencyInjection;

public class ElasticStartup : IElasticStartup
{
  private readonly IServiceScope _scope;

  public ElasticStartup(IServiceProvider scope)
  {
    _scope = scope.CreateScope();
  }

  public async Task InitializeAllIndexes(CancellationToken ct)
  {
    foreach (var indexManager in _scope.ServiceProvider.GetServices<IIndexManager>())
    {
      await indexManager.EnsureValidIndexAsync(ct);
    }
  }
}
