namespace WebIO.Elastic.Hosting;

using Management;

public class ReindexEverything : IReindexEverything
{
  private readonly IEnumerable<IIndexService> _indexers;

  public ReindexEverything(IEnumerable<IIndexService> indexers)
  {
    _indexers = indexers;
  }

  public async Task ReindexAllAsync(CancellationToken ct)
  {
    foreach (var indexService in _indexers)
    {
      await indexService.ReindexAllAsync(ct);
    }
  }
}
