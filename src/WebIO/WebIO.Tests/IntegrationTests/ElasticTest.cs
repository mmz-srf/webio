namespace WebIO.Tests.IntegrationTests;

using System;
using System.Threading.Tasks;
using Cli;
using DataAccess;
using DataAccess.EntityFrameworkCore;
using Elastic.Data;
using Elastic.Hosting;
using Elastic.Management;
using Elasticsearch.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Nest;
using Xunit;

public class ElasticTest : IDisposable
{
  private readonly Device _testObject = new()
  {
    Name = "testDevice",
    DeviceType = "testDeviceType",
    Comment = "test comment",
    Properties = new(new()),
    Interfaces =
    [
      new()
      {
        Name = "testInterface",
        Index = 1,
        InterfaceTemplate = "sender",
        Comment = "test comment",
        Properties = new(new()),
        Streams =
        [
          new()
          {
            Name = "audio send stream",
            Comment = "test comment",
            Type = StreamType.Audio,
            Direction = StreamDirection.Send,
            Modification = Modification.Empty,
            Properties = new(new()),
          },
        ],
        Modification = Modification.Empty,
      },
    ],
    Modification = Modification.Empty,
  };

  private readonly IServiceProvider _serviceProvider;

  public ElasticTest()
  {
    var host = CliApp.CreateAppBuilder(new[] { "--Elastic:IndexPrefix", "test" });

    var app = host.Build();
    _serviceProvider = app.Services.CreateScope().ServiceProvider;

    var config = _serviceProvider.GetRequiredService<ElasticConfiguration>();
    _serviceProvider.GetRequiredService<IElasticStartup>().InitializeAllIndexes(default).GetAwaiter().GetResult();
    _serviceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
  }

  [Fact]
  public async Task WriteAndLoadEntity()
  {
    var repo = _serviceProvider.GetRequiredService<IDeviceRepository>();
    await repo.UpsertAsync(_testObject, default);

    var entity = await repo.GetDeviceAsync(_testObject.Id, default);

    entity.Should().BeEquivalentTo(_testObject);
  }

  private void ReleaseUnmanagedResources()
  {
    var client = _serviceProvider.GetRequiredService<IElasticClient>();
    var testIndices = client.Indices.GetAlias("test-*").Indices;

    foreach (var (name, _) in testIndices)
    {
      client.Indices.Delete(name);
    }
  }

  public void Dispose()
  {
    ReleaseUnmanagedResources();
    GC.SuppressFinalize(this);
  }

  ~ElasticTest()
  {
    ReleaseUnmanagedResources();
  }
}
