namespace WebIO.Elastic.Hosting;

using Management;
using Microsoft.Extensions.DependencyInjection;
using Nest;

public static class HostingExtensions
{
  /// <summary>
  /// Registers a single elastic client with the given configuration. Currently this only registers a single node
  /// connection pool. This is according to the best practice of elasticsearch, as per documentation.
  /// </summary>
  /// <param name="builder">The autofac builder to configure</param>
  /// <param name="config">The connection configuration</param>
  /// <returns>The configured autofac builder</returns>
  public static IServiceCollection RegisterElastic(
    this IServiceCollection builder,
    ElasticConfiguration config,
    Action<ConnectionSettings>? configure = null)
  {
    builder.AddSingleton(config);

    var connectionSettings = new ConnectionSettings(config.Url);
    if (config.Proxy != null)
    {
      connectionSettings.Proxy(config.Proxy, config.ProxyUser, config.ProxyPassword);
    }

    if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
    {
      connectionSettings.BasicAuthentication(config.Username, config.Password);
    }

    connectionSettings.EnableApiVersioningHeader();

    configure?.Invoke(connectionSettings);

    builder.AddSingleton<IElasticClient>(new ElasticClient(connectionSettings));
    builder.AddTransient<IElasticStartup, ElasticStartup>();
    builder.AddTransient<IReindexEverything, ReindexEverything>();
    return builder;
  }

  /// <summary>
  /// Automatically sets up an index manager and indexer for the given entity type. If you don't provide a searcher
  /// in the following setup builder, nothing happens. If you're not searching for anything in the code, you don't
  /// need an index now, do you?
  /// </summary>
  /// <param name="builder">The autofac container builder to configure</param>
  /// <typeparam name="TIndexedEntity">The indexed type to manage</typeparam>
  /// <returns>A builder object to configure </returns>
  public static ElasticModelBuilder<TIndexedEntity, TId> ElasticModel<TIndexedEntity, TId>(
    this IServiceCollection builder)
    where TIndexedEntity : class, IIndexedEntity<TId>, new()
    => new(builder);
}
