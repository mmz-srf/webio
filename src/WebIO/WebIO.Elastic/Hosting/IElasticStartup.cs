namespace WebIO.Elastic.Hosting;

public interface IElasticStartup
{
  Task InitializeAllIndexes(CancellationToken ct);
}
