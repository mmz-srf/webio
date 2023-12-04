namespace WebIO.Elastic.Hosting;

public interface IReindexEverything
{
  public Task ReindexAllAsync(CancellationToken ct);
}
