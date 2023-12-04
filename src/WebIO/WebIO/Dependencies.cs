namespace WebIO;

using System;
using System.Linq;
using System.Net.Http;
using Api.Nevion;
using ConfigFiles;
using DataAccess;
using DataAccess.Configuration;
using DataAccess.Elastic;
using DataAccess.EntityFrameworkCore;
using Elastic.Data;
using Elastic.Hosting;
using Elastic.Management.Search;
using IdentityModel.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Model;

public static class Dependencies
{
  public static IServiceCollection SetupDependencies(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    services
      .SetConfigRepos()
      .SetupRepositories(configuration)
      .AddHttpClient<NevionApi, NevionApi>(c =>
      {
        c.BaseAddress = configuration.GetValue<Uri>("NevionApi:Url");
        c.SetBasicAuthentication(
          configuration.GetValue<string>("NevionApi:Username"),
          configuration.GetValue<string>("NevionApi:Password"));
      }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
      {
        UseProxy = false,
        Proxy = null,
      });
    return services;
  }

  public static IServiceCollection AddAllImplementationsOf<T>(this IServiceCollection services)
    => typeof(T).Assembly.GetTypes()
      .Where(t => !t.IsInterface)
      .Where(t => !t.IsAbstract)
      .Where(t => typeof(T).IsAssignableFrom(t))
      .Aggregate(services, (_, t) => services.AddTransient(typeof(T), t).AddTransient(t));

  private static IServiceCollection SetupRepositories(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("WebIO") ??
                           throw new ArgumentException("No connection string defined!");

    return services
      .RegisterElastic(new())
      .ElasticModel<IndexedDevice, Guid>().WithDataSource<DeviceDataSource>().Register()
      .ElasticModel<IndexedInterface, Guid>().WithDataSource<InterfaceDataSource>().Register()
      .ElasticModel<IndexedStream, Guid>().WithDataSource<StreamDataSource>().Register()
      .AddTransient<ISearcher<IndexedDevice, DeviceSearchRequest, Guid>, DeviceSearcher>()
      .AddTransient<ISearcher<IndexedInterface, InterfaceSearchRequest, Guid>, InterfaceSearcher>()
      .AddTransient<ISearcher<IndexedStream, StreamSearchRequest, Guid>, StreamSearcher>()
      .AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString))
      .AddTransient<IDeviceRepository, DbAndElasticDeviceRepository>()
      .AddTransient<ElasticDeviceRepository>()
      .AddTransient<EfCoreDeviceRepository>()
      .AddTransient<IChangeLogRepository, EfCoreChangeLogRepository>()
      .AddTransient<IConfigFileReader, JsonFileReader>()
      .AddTransient<IMetadataRepository, JsonMetadataRepository>();
  }

  private static IServiceCollection SetConfigRepos(this IServiceCollection services)
    => services
      .AddSingleton<IMetadataRepository, JsonMetadataRepository>()
      .AddSingleton<PseudoProperties>()
      .AddSingleton<IExportConfigurationRepository, JsonExportConfigurationRepository>();
}
